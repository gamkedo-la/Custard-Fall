using FishNet.CodeGenerating.Helping.Extension;
using FishNet.Object.Synchronizing;
using FishNet.Object.Synchronizing.Internal;
using MonoFN.Cecil;
using MonoFN.Cecil.Rocks;
using System;
using System.Collections.Generic;

namespace FishNet.CodeGenerating.Helping
{
    internal class CreatedSyncVarGenerator
    {
        private readonly Dictionary<TypeDefinition, CreatedSyncVar>
            _createdSyncVars = new(new TypeDefinitionComparer());

        #region Relfection references.

        private TypeReference _syncBase_TypeRef;
        internal TypeReference SyncVar_TypeRef;
        private MethodReference _syncVar_Constructor_MethodRef;

        #endregion

        #region Const.

        private const string GETVALUE_NAME = "GetValue";
        private const string SETVALUE_NAME = "SetValue";

        #endregion

        /* //feature add and test the dirty boolean changes
         * eg... instead of base.Dirty()
         * do if (!base.Dirty()) return false;
         * See synclist for more info. */

        /// <summary>
        /// Imports references needed by this helper.
        /// </summary>
        /// <param name="moduleDef"></param>
        /// <returns></returns>
        internal bool ImportReferences()
        {
            SyncVar_TypeRef = CodegenSession.ImportReference(typeof(SyncVar<>));
            var svConstructor = SyncVar_TypeRef.GetFirstConstructor(true);
            _syncVar_Constructor_MethodRef = CodegenSession.ImportReference(svConstructor);

            var syncBaseType = typeof(SyncBase);
            _syncBase_TypeRef = CodegenSession.ImportReference(syncBaseType);

            return true;
        }

        /// <summary>
        /// Gets and optionally creates data for SyncVar<typeOfField>
        /// </summary>
        /// <param name="dataTr"></param>
        /// <returns></returns>
        internal CreatedSyncVar GetCreatedSyncVar(FieldDefinition originalFd, bool createMissing)
        {
            var dataTr = originalFd.FieldType;
            var dataTd = dataTr.CachedResolve();

            if (_createdSyncVars.TryGetValue(dataTd, out var createdSyncVar))
            {
                return createdSyncVar;
            }
            else
            {
                if (!createMissing)
                    return null;

                CodegenSession.ImportReference(dataTd);

                var syncVarGit = SyncVar_TypeRef.MakeGenericInstanceType(new TypeReference[] {dataTr});
                var genericDataTr = syncVarGit.GenericArguments[0];

                //Make sure can serialize.
                var canSerialize = CodegenSession.GeneralHelper.HasSerializerAndDeserializer(genericDataTr, true);
                if (!canSerialize)
                {
                    CodegenSession.LogError(
                        $"SyncVar {originalFd.Name} data type {genericDataTr.FullName} does not support serialization. Use a supported type or create a custom serializer.");
                    return null;
                }

                //Set needed methods from syncbase.
                MethodReference setSyncIndexMr;
                var genericSyncVarCtor = _syncVar_Constructor_MethodRef.MakeHostInstanceGeneric(syncVarGit);

                if (!CodegenSession.NetworkBehaviourSyncProcessor.SetSyncBaseMethods(_syncBase_TypeRef.CachedResolve(),
                        out setSyncIndexMr, out _))
                    return null;

                MethodReference setValueMr = null;
                MethodReference getValueMr = null;
                foreach (var md in SyncVar_TypeRef.CachedResolve().Methods)
                    //GetValue.
                    if (md.Name == GETVALUE_NAME)
                    {
                        var mr = CodegenSession.ImportReference(md);
                        getValueMr = mr.MakeHostInstanceGeneric(syncVarGit);
                    }
                    //SetValue.
                    else if (md.Name == SETVALUE_NAME)
                    {
                        var mr = CodegenSession.ImportReference(md);
                        setValueMr = mr.MakeHostInstanceGeneric(syncVarGit);
                    }

                if (setValueMr == null || getValueMr == null)
                    return null;

                var csv = new CreatedSyncVar(syncVarGit, dataTd, getValueMr, setValueMr, setSyncIndexMr, null,
                    genericSyncVarCtor);
                _createdSyncVars.Add(dataTd, csv);
                return csv;
            }
        }
    }
}