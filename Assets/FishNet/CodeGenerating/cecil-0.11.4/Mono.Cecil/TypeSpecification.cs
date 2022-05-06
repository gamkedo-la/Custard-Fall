//
// Author:
//   Jb Evain (jbevain@gmail.com)
//
// Copyright (c) 2008 - 2015 Jb Evain
// Copyright (c) 2008 - 2011 Novell, Inc.
//
// Licensed under the MIT/X11 license.
//

using System;

namespace MonoFN.Cecil
{
    public abstract class TypeSpecification : TypeReference
    {
        private readonly TypeReference element_type;

        public TypeReference ElementType => element_type;

        public override string Name
        {
            get => element_type.Name;
            set => throw new InvalidOperationException();
        }

        public override string Namespace
        {
            get => element_type.Namespace;
            set => throw new InvalidOperationException();
        }

        public override IMetadataScope Scope
        {
            get => element_type.Scope;
            set => throw new InvalidOperationException();
        }

        public override ModuleDefinition Module => element_type.Module;

        public override string FullName => element_type.FullName;

        public override bool ContainsGenericParameter => element_type.ContainsGenericParameter;

        public override MetadataType MetadataType => (MetadataType) etype;

        internal TypeSpecification(TypeReference type)
            : base(null, null)
        {
            element_type = type;
            token = new MetadataToken(TokenType.TypeSpec);
        }

        public override TypeReference GetElementType()
        {
            return element_type.GetElementType();
        }
    }
}