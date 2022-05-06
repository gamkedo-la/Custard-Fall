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
    public class FieldReference : MemberReference
    {
        private TypeReference field_type;

        public TypeReference FieldType
        {
            get => field_type;
            set => field_type = value;
        }

        public override string FullName => field_type.FullName + " " + MemberFullName();

        public override bool ContainsGenericParameter =>
            field_type.ContainsGenericParameter || base.ContainsGenericParameter;

        internal FieldReference()
        {
            token = new MetadataToken(TokenType.MemberRef);
        }

        public FieldReference(string name, TypeReference fieldType)
            : base(name)
        {
            Mixin.CheckType(fieldType, Mixin.Argument.fieldType);

            field_type = fieldType;
            token = new MetadataToken(TokenType.MemberRef);
        }

        public FieldReference(string name, TypeReference fieldType, TypeReference declaringType)
            : this(name, fieldType)
        {
            Mixin.CheckType(declaringType, Mixin.Argument.declaringType);

            DeclaringType = declaringType;
        }

        protected override IMemberDefinition ResolveDefinition()
        {
            return Resolve();
        }

        public new virtual FieldDefinition Resolve()
        {
            var module = Module;
            if (module == null)
                throw new NotSupportedException();

            return module.Resolve(this);
        }
    }
}