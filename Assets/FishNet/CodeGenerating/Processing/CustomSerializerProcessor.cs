﻿using FishNet.CodeGenerating.Helping;
using FishNet.CodeGenerating.Helping.Extension;
using FishNet.Serializing;
using MonoFN.Cecil;
using MonoFN.Cecil.Cil;
using System.Collections.Generic;
using UnityEngine;

namespace FishNet.CodeGenerating.Processing
{
    internal class CustomSerializerProcessor
    {
        #region Types.

        internal enum ExtensionType
        {
            None,
            Write,
            Read
        }

        #endregion

        internal bool CreateDelegates(TypeDefinition typeDef)
        {
            var modified = false;

            /* Find all declared methods and register delegates to them.
             * After they are all registered create any custom writers
             * needed to complete the declared methods. It's important to
             * make generated writers after so that a generated method
             * isn't made for a type when the user has already made a declared one. */
            foreach (var methodDef in typeDef.Methods)
            {
                var extensionType = GetExtensionType(methodDef);
                if (extensionType == ExtensionType.None)
                    continue;

                var methodRef = CodegenSession.ImportReference(methodDef);
                if (extensionType == ExtensionType.Write)
                {
                    CodegenSession.WriterHelper.AddWriterMethod(methodRef.Parameters[1].ParameterType, methodRef, false,
                        true);
                    modified = true;
                }
                else if (extensionType == ExtensionType.Read)
                {
                    CodegenSession.ReaderHelper.AddReaderMethod(methodRef.ReturnType, methodRef, false, true);
                    modified = true;
                }
            }

            return modified;
        }

        /// <summary>
        /// Creates serializers for any custom types for declared methods.
        /// </summary>
        /// <param name="declaredMethods"></param>
        /// <param name="moduleDef"></param>
        internal bool CreateSerializers(TypeDefinition typeDef)
        {
            var modified = false;

            var declaredMethods = new List<(MethodDefinition, ExtensionType)>();
            /* Go through all custom serializers again and see if 
             * they use any types that the user didn't make a serializer for
             * and that there isn't a built-in type for. Create serializers
             * for these types. */
            foreach (var methodDef in typeDef.Methods)
            {
                var extensionType = GetExtensionType(methodDef);
                if (extensionType == ExtensionType.None)
                    continue;

                declaredMethods.Add((methodDef, extensionType));
                modified = true;
            }

            //Now that all declared are loaded see if any of them need generated serializers.
            foreach ((var methodDef, var extensionType) in declaredMethods)
                CreateSerializers(extensionType, methodDef);

            return modified;
        }


        /// <summary>
        /// Creates a custom serializer for any types not handled within users declared.
        /// </summary>
        /// <param name="extensionType"></param>
        /// <param name="moduleDef"></param>
        /// <param name="methodDef"></param>
        /// <param name="diagnostics"></param>
        private void CreateSerializers(ExtensionType extensionType, MethodDefinition methodDef)
        {
            for (var i = 0; i < methodDef.Body.Instructions.Count; i++)
                CheckToModifyInstructions(extensionType, methodDef, ref i);
        }

        /// <summary>
        /// Checks if instructions need to be modified and does so.
        /// </summary>
        /// <param name="methodDef"></param>
        /// <param name="instructionIndex"></param>
        private void CheckToModifyInstructions(ExtensionType extensionType, MethodDefinition methodDef,
            ref int instructionIndex)
        {
            var instruction = methodDef.Body.Instructions[instructionIndex];
            //Fields.
            if (instruction.OpCode == OpCodes.Ldsfld || instruction.OpCode == OpCodes.Ldfld)
                CheckFieldReferenceInstruction(extensionType, methodDef, ref instructionIndex);
            //Method calls.
            else if (instruction.OpCode == OpCodes.Call || instruction.OpCode == OpCodes.Callvirt)
                CheckCallInstruction(extensionType, methodDef, ref instructionIndex,
                    (MethodReference) instruction.Operand);
        }


        /// <summary>
        /// Checks if a reader or writer must be generated for a field type.
        /// </summary>
        /// <param name="methodDef"></param>
        /// <param name="instructionIndex"></param>
        private void CheckFieldReferenceInstruction(ExtensionType extensionType, MethodDefinition methodDef,
            ref int instructionIndex)
        {
            var instruction = methodDef.Body.Instructions[instructionIndex];
            var field = (FieldReference) instruction.Operand;
            var type = field.DeclaringType;

            if (type.IsType(typeof(GenericWriter<>)) ||
                (type.IsType(typeof(GenericReader<>)) && type.IsGenericInstance))
            {
                var typeGenericInst = (GenericInstanceType) type;
                var parameterType = typeGenericInst.GenericArguments[0];
                CreateReaderOrWriter(extensionType, methodDef, ref instructionIndex, parameterType);
            }
        }


        /// <summary>
        /// Checks if a reader or writer must be generated for a call type.
        /// </summary>
        /// <param name="extensionType"></param>
        /// <param name="moduleDef"></param>
        /// <param name="methodDef"></param>
        /// <param name="instructionIndex"></param>
        /// <param name="method"></param>
        private void CheckCallInstruction(ExtensionType extensionType, MethodDefinition methodDef,
            ref int instructionIndex, MethodReference method)
        {
            if (!method.IsGenericInstance)
                return;

            //True if call is to read/write.
            var canCreate = method.Is<Writer>(nameof(Writer.Write)) ||
                            method.Is<Reader>(nameof(Reader.Read));

            if (canCreate)
            {
                var instanceMethod = (GenericInstanceMethod) method;
                var parameterType = instanceMethod.GenericArguments[0];
                if (parameterType.IsGenericParameter)
                    return;

                CreateReaderOrWriter(extensionType, methodDef, ref instructionIndex, parameterType);
            }
        }


        /// <summary>
        /// Creates a reader or writer for parameterType.
        /// </summary>
        /// <param name="extensionType"></param>
        /// <param name="methodDef"></param>
        /// <param name="instructionIndex"></param>
        /// <param name="parameterType"></param>
        private void CreateReaderOrWriter(ExtensionType extensionType, MethodDefinition methodDef,
            ref int instructionIndex, TypeReference parameterType)
        {
            if (!parameterType.IsGenericParameter && parameterType.CanBeResolved())
            {
                var typeDefinition = parameterType.CachedResolve();
                //If class and not value type check for accessible constructor.
                if (typeDefinition.IsClass && !typeDefinition.IsValueType)
                {
                    var constructor = typeDefinition.GetMethod(".ctor");
                    //Constructor is inaccessible, cannot create serializer for type.
                    if (!constructor.IsPublic)
                    {
                        CodegenSession.LogError(
                            $"Unable to generator serializers for {typeDefinition.FullName} because it's constructor is not public.");
                        return;
                    }
                }

                var processor = methodDef.Body.GetILProcessor();

                //Find already existing read or write method.
                var createdMethodRef = extensionType == ExtensionType.Write
                    ? CodegenSession.WriterHelper.GetFavoredWriteMethodReference(parameterType, true)
                    : CodegenSession.ReaderHelper.GetFavoredReadMethodReference(parameterType, true);
                //If a created method already exist nothing further is required.
                if (createdMethodRef != null)
                {
                    //Replace call to generic with already made serializer.
                    var newInstruction = processor.Create(OpCodes.Call, createdMethodRef);
                    methodDef.Body.Instructions[instructionIndex] = newInstruction;
                    return;
                }
                else
                {
                    createdMethodRef = extensionType == ExtensionType.Write
                        ? CodegenSession.WriterGenerator.CreateWriter(parameterType)
                        : CodegenSession.ReaderGenerator.CreateReader(parameterType);
                }

                //If method was created.
                if (createdMethodRef != null)
                {
                    /* If an autopack type then we have to inject the
                     * autopack above the new instruction. */
                    if (CodegenSession.WriterHelper.IsAutoPackedType(parameterType))
                    {
                        var packType = CodegenSession.GeneralHelper.GetDefaultAutoPackType(parameterType);
                        var autoPack = processor.Create(OpCodes.Ldc_I4, (int) packType);
                        methodDef.Body.Instructions.Insert(instructionIndex, autoPack);
                        instructionIndex++;
                    }

                    var newInstruction = processor.Create(OpCodes.Call, createdMethodRef);
                    methodDef.Body.Instructions[instructionIndex] = newInstruction;
                }
            }
        }


        /// <summary>
        /// Returns the RPC attribute on a method, if one exist. Otherwise returns null.
        /// </summary>
        /// <param name="methodDef"></param>
        /// <returns></returns>
        private ExtensionType GetExtensionType(MethodDefinition methodDef)
        {
            var hasExtensionAttribute =
                methodDef.HasCustomAttribute<System.Runtime.CompilerServices.ExtensionAttribute>();
            if (!hasExtensionAttribute)
                return ExtensionType.None;

            var write = methodDef.ReturnType == methodDef.Module.TypeSystem.Void;

            //Return None for Mirror types.
#if MIRROR
            if (write)
            {
                if (methodDef.Parameters.Count > 0 && methodDef.Parameters[0].ParameterType.FullName == "Mirror.NetworkWriter")
                    return ExtensionType.None;                    
            }
            else
            {
                if (methodDef.Parameters.Count > 0 && methodDef.Parameters[0].ParameterType.FullName == "Mirror.NetworkReader")
                    return ExtensionType.None;
            }
#endif


            var prefix = write ? WriterHelper.WRITE_PREFIX : ReaderHelper.READ_PREFIX;

            //Does not contain prefix.
            if (methodDef.Name.Length < prefix.Length || methodDef.Name.Substring(0, prefix.Length) != prefix)
                return ExtensionType.None;

            //Make sure first parameter is right.
            if (methodDef.Parameters.Count >= 1)
            {
                var tr = methodDef.Parameters[0].ParameterType;
                if (tr.FullName != CodegenSession.WriterHelper.Writer_TypeRef.FullName &&
                    tr.FullName != CodegenSession.ReaderHelper.Reader_TypeRef.FullName)
                    return ExtensionType.None;
            }

            if (write && methodDef.Parameters.Count < 2)
            {
                CodegenSession.LogError(
                    $"{methodDef.FullName} must have at least two parameters, the first being PooledWriter, and second value to write.");
                return ExtensionType.None;
            }
            else if (!write && methodDef.Parameters.Count < 1)
            {
                CodegenSession.LogError(
                    $"{methodDef.FullName} must have at least one parameters, the first being PooledReader.");
                return ExtensionType.None;
            }

            return write ? ExtensionType.Write : ExtensionType.Read;
        }
    }
}