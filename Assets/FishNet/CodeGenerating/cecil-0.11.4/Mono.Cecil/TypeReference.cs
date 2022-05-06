//
// Author:
//   Jb Evain (jbevain@gmail.com)
//
// Copyright (c) 2008 - 2015 Jb Evain
// Copyright (c) 2008 - 2011 Novell, Inc.
//
// Licensed under the MIT/X11 license.
//

using MonoFN.Cecil.Metadata;
using MonoFN.Collections.Generic;
using System;
using System.Threading;

namespace MonoFN.Cecil
{
    public enum MetadataType : byte
    {
        Void = ElementType.Void,
        Boolean = ElementType.Boolean,
        Char = ElementType.Char,
        SByte = ElementType.I1,
        Byte = ElementType.U1,
        Int16 = ElementType.I2,
        UInt16 = ElementType.U2,
        Int32 = ElementType.I4,
        UInt32 = ElementType.U4,
        Int64 = ElementType.I8,
        UInt64 = ElementType.U8,
        Single = ElementType.R4,
        Double = ElementType.R8,
        String = ElementType.String,
        Pointer = ElementType.Ptr,
        ByReference = ElementType.ByRef,
        ValueType = ElementType.ValueType,
        Class = ElementType.Class,
        Var = ElementType.Var,
        Array = ElementType.Array,
        GenericInstance = ElementType.GenericInst,
        TypedByReference = ElementType.TypedByRef,
        IntPtr = ElementType.I,
        UIntPtr = ElementType.U,
        FunctionPointer = ElementType.FnPtr,
        Object = ElementType.Object,
        MVar = ElementType.MVar,
        RequiredModifier = ElementType.CModReqD,
        OptionalModifier = ElementType.CModOpt,
        Sentinel = ElementType.Sentinel,
        Pinned = ElementType.Pinned
    }

    public class TypeReference : MemberReference, IGenericParameterProvider, IGenericContext
    {
        private string @namespace;
        private bool value_type;
        internal IMetadataScope scope;
        internal ModuleDefinition module;

        internal ElementType etype = ElementType.None;

        private string fullname;

        protected Collection<GenericParameter> generic_parameters;

        public override string Name
        {
            get => base.Name;
            set
            {
                if (IsWindowsRuntimeProjection && value != base.Name)
                    throw new InvalidOperationException("Projected type reference name can't be changed.");
                base.Name = value;
                ClearFullName();
            }
        }

        public virtual string Namespace
        {
            get => @namespace;
            set
            {
                if (IsWindowsRuntimeProjection && value != @namespace)
                    throw new InvalidOperationException("Projected type reference namespace can't be changed.");
                @namespace = value;
                ClearFullName();
            }
        }

        public virtual bool IsValueType
        {
            get => value_type;
            set => value_type = value;
        }

        public override ModuleDefinition Module
        {
            get
            {
                if (module != null)
                    return module;

                var declaring_type = DeclaringType;
                if (declaring_type != null)
                    return declaring_type.Module;

                return null;
            }
        }

        internal TypeReferenceProjection WindowsRuntimeProjection
        {
            get => (TypeReferenceProjection) projection;
            set => projection = value;
        }

        IGenericParameterProvider IGenericContext.Type => this;

        IGenericParameterProvider IGenericContext.Method => null;

        GenericParameterType IGenericParameterProvider.GenericParameterType => GenericParameterType.Type;

        public virtual bool HasGenericParameters => !generic_parameters.IsNullOrEmpty();

        public virtual Collection<GenericParameter> GenericParameters
        {
            get
            {
                if (generic_parameters == null)
                    Interlocked.CompareExchange(ref generic_parameters, new GenericParameterCollection(this), null);

                return generic_parameters;
            }
        }

        public virtual IMetadataScope Scope
        {
            get
            {
                var declaring_type = DeclaringType;
                if (declaring_type != null)
                    return declaring_type.Scope;

                return scope;
            }
            set
            {
                var declaring_type = DeclaringType;
                if (declaring_type != null)
                {
                    if (IsWindowsRuntimeProjection && value != declaring_type.Scope)
                        throw new InvalidOperationException("Projected type scope can't be changed.");
                    declaring_type.Scope = value;
                    return;
                }

                if (IsWindowsRuntimeProjection && value != scope)
                    throw new InvalidOperationException("Projected type scope can't be changed.");
                scope = value;
            }
        }

        public bool IsNested => DeclaringType != null;

        public override TypeReference DeclaringType
        {
            get => base.DeclaringType;
            set
            {
                if (IsWindowsRuntimeProjection && value != base.DeclaringType)
                    throw new InvalidOperationException("Projected type declaring type can't be changed.");
                base.DeclaringType = value;
                ClearFullName();
            }
        }

        public override string FullName
        {
            get
            {
                if (fullname != null)
                    return fullname;

                var new_fullname = this.TypeFullName();

                if (IsNested)
                    new_fullname = DeclaringType.FullName + "/" + new_fullname;
                Interlocked.CompareExchange(ref fullname, new_fullname, null);
                return fullname;
            }
        }

        public virtual bool IsByReference => false;

        public virtual bool IsPointer => false;

        public virtual bool IsSentinel => false;

        public virtual bool IsArray => false;

        public virtual bool IsGenericParameter => false;

        public virtual bool IsGenericInstance => false;

        public virtual bool IsRequiredModifier => false;

        public virtual bool IsOptionalModifier => false;

        public virtual bool IsPinned => false;

        public virtual bool IsFunctionPointer => false;

        public virtual bool IsPrimitive => etype.IsPrimitive();

        public virtual MetadataType MetadataType
        {
            get
            {
                switch (etype)
                {
                    case ElementType.None:
                        return IsValueType ? MetadataType.ValueType : MetadataType.Class;
                    default:
                        return (MetadataType) etype;
                }
            }
        }

        protected TypeReference(string @namespace, string name)
            : base(name)
        {
            this.@namespace = @namespace ?? string.Empty;
            token = new MetadataToken(TokenType.TypeRef, 0);
        }

        public TypeReference(string @namespace, string name, ModuleDefinition module, IMetadataScope scope)
            : this(@namespace, name)
        {
            this.module = module;
            this.scope = scope;
        }

        public TypeReference(string @namespace, string name, ModuleDefinition module, IMetadataScope scope,
            bool valueType) :
            this(@namespace, name, module, scope)
        {
            value_type = valueType;
        }

        protected virtual void ClearFullName()
        {
            fullname = null;
        }

        public virtual TypeReference GetElementType()
        {
            return this;
        }

        protected override IMemberDefinition ResolveDefinition()
        {
            return Resolve();
        }

        public new virtual TypeDefinition Resolve()
        {
            var module = Module;
            if (module == null)
                throw new NotSupportedException();

            return module.Resolve(this);
        }
    }

    internal static partial class Mixin
    {
        public static bool IsPrimitive(this ElementType self)
        {
            switch (self)
            {
                case ElementType.Boolean:
                case ElementType.Char:
                case ElementType.I:
                case ElementType.U:
                case ElementType.I1:
                case ElementType.U1:
                case ElementType.I2:
                case ElementType.U2:
                case ElementType.I4:
                case ElementType.U4:
                case ElementType.I8:
                case ElementType.U8:
                case ElementType.R4:
                case ElementType.R8:
                    return true;
                default:
                    return false;
            }
        }

        public static string TypeFullName(this TypeReference self)
        {
            return string.IsNullOrEmpty(self.Namespace)
                ? self.Name
                : self.Namespace + '.' + self.Name;
        }

        public static bool IsTypeOf(this TypeReference self, string @namespace, string name)
        {
            return self.Name == name
                   && self.Namespace == @namespace;
        }

        public static bool IsTypeSpecification(this TypeReference type)
        {
            switch (type.etype)
            {
                case ElementType.Array:
                case ElementType.ByRef:
                case ElementType.CModOpt:
                case ElementType.CModReqD:
                case ElementType.FnPtr:
                case ElementType.GenericInst:
                case ElementType.MVar:
                case ElementType.Pinned:
                case ElementType.Ptr:
                case ElementType.SzArray:
                case ElementType.Sentinel:
                case ElementType.Var:
                    return true;
            }

            return false;
        }

        public static TypeDefinition CheckedResolve(this TypeReference self)
        {
            var type = self.Resolve();
            if (type == null)
                throw new ResolutionException(self);

            return type;
        }
    }
}