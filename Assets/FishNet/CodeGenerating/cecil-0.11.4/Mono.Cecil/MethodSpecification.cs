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

namespace MonoFN.Cecil
{
    public abstract class MethodSpecification : MethodReference
    {
        private readonly MethodReference method;

        public MethodReference ElementMethod => method;

        public override string Name
        {
            get => method.Name;
            set => throw new InvalidOperationException();
        }

        public override MethodCallingConvention CallingConvention
        {
            get => method.CallingConvention;
            set => throw new InvalidOperationException();
        }

        public override bool HasThis
        {
            get => method.HasThis;
            set => throw new InvalidOperationException();
        }

        public override bool ExplicitThis
        {
            get => method.ExplicitThis;
            set => throw new InvalidOperationException();
        }

        public override MethodReturnType MethodReturnType
        {
            get => method.MethodReturnType;
            set => throw new InvalidOperationException();
        }

        public override TypeReference DeclaringType
        {
            get => method.DeclaringType;
            set => throw new InvalidOperationException();
        }

        public override ModuleDefinition Module => method.Module;

        public override bool HasParameters => method.HasParameters;

        public override Collection<ParameterDefinition> Parameters => method.Parameters;

        public override bool ContainsGenericParameter => method.ContainsGenericParameter;

        internal MethodSpecification(MethodReference method)
        {
            Mixin.CheckMethod(method);

            this.method = method;
            token = new MetadataToken(TokenType.MethodSpec);
        }

        public sealed override MethodReference GetElementMethod()
        {
            return method.GetElementMethod();
        }
    }
}