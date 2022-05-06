//
// Author:
//   Jb Evain (jbevain@gmail.com)
//
// Copyright (c) 2008 - 2015 Jb Evain
// Copyright (c) 2008 - 2011 Novell, Inc.
//
// Licensed under the MIT/X11 license.
//

using MonoFN.Collections.Generic;
using System;
using System.Text;
using System.Threading;

namespace MonoFN.Cecil
{
    public class MethodReference : MemberReference, IMethodSignature, IGenericParameterProvider, IGenericContext
    {
        internal ParameterDefinitionCollection parameters;
        private MethodReturnType return_type;

        private bool has_this;
        private bool explicit_this;
        private MethodCallingConvention calling_convention;
        internal Collection<GenericParameter> generic_parameters;

        public virtual bool HasThis
        {
            get => has_this;
            set => has_this = value;
        }

        public virtual bool ExplicitThis
        {
            get => explicit_this;
            set => explicit_this = value;
        }

        public virtual MethodCallingConvention CallingConvention
        {
            get => calling_convention;
            set => calling_convention = value;
        }

        public virtual bool HasParameters => !parameters.IsNullOrEmpty();

        public virtual Collection<ParameterDefinition> Parameters
        {
            get
            {
                if (parameters == null)
                    Interlocked.CompareExchange(ref parameters, new ParameterDefinitionCollection(this), null);

                return parameters;
            }
        }

        IGenericParameterProvider IGenericContext.Type
        {
            get
            {
                var declaring_type = DeclaringType;
                var instance = declaring_type as GenericInstanceType;
                if (instance != null)
                    return instance.ElementType;

                return declaring_type;
            }
        }

        IGenericParameterProvider IGenericContext.Method => this;

        GenericParameterType IGenericParameterProvider.GenericParameterType => GenericParameterType.Method;

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

        public TypeReference ReturnType
        {
            get
            {
                var return_type = MethodReturnType;
                return return_type != null ? return_type.ReturnType : null;
            }
            set
            {
                var return_type = MethodReturnType;
                if (return_type != null)
                    return_type.ReturnType = value;
            }
        }

        public virtual MethodReturnType MethodReturnType
        {
            get => return_type;
            set => return_type = value;
        }

        public override string FullName
        {
            get
            {
                var builder = new StringBuilder();
                builder.Append(ReturnType.FullName)
                    .Append(" ")
                    .Append(MemberFullName());
                this.MethodSignatureFullName(builder);
                return builder.ToString();
            }
        }

        public virtual bool IsGenericInstance => false;

        public override bool ContainsGenericParameter
        {
            get
            {
                if (ReturnType.ContainsGenericParameter || base.ContainsGenericParameter)
                    return true;

                if (!HasParameters)
                    return false;

                var parameters = Parameters;

                for (var i = 0; i < parameters.Count; i++)
                    if (parameters[i].ParameterType.ContainsGenericParameter)
                        return true;

                return false;
            }
        }

        internal MethodReference()
        {
            return_type = new MethodReturnType(this);
            token = new MetadataToken(TokenType.MemberRef);
        }

        public MethodReference(string name, TypeReference returnType)
            : base(name)
        {
            Mixin.CheckType(returnType, Mixin.Argument.returnType);

            return_type = new MethodReturnType(this);
            return_type.ReturnType = returnType;
            token = new MetadataToken(TokenType.MemberRef);
        }

        public MethodReference(string name, TypeReference returnType, TypeReference declaringType)
            : this(name, returnType)
        {
            Mixin.CheckType(declaringType, Mixin.Argument.declaringType);

            DeclaringType = declaringType;
        }

        public virtual MethodReference GetElementMethod()
        {
            return this;
        }

        protected override IMemberDefinition ResolveDefinition()
        {
            return Resolve();
        }

        public new virtual MethodDefinition Resolve()
        {
            var module = Module;
            if (module == null)
                throw new NotSupportedException();

            return module.Resolve(this);
        }
    }

    internal static partial class Mixin
    {
        public static bool IsVarArg(this IMethodSignature self)
        {
            return self.CallingConvention == MethodCallingConvention.VarArg;
        }

        public static int GetSentinelPosition(this IMethodSignature self)
        {
            if (!self.HasParameters)
                return -1;

            var parameters = self.Parameters;
            for (var i = 0; i < parameters.Count; i++)
                if (parameters[i].ParameterType.IsSentinel)
                    return i;

            return -1;
        }
    }
}