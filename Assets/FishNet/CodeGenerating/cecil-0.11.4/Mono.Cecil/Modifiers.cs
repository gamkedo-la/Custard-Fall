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
using MD = MonoFN.Cecil.Metadata;

namespace MonoFN.Cecil
{
    public interface IModifierType
    {
        TypeReference ModifierType { get; }
        TypeReference ElementType { get; }
    }

    public sealed class OptionalModifierType : TypeSpecification, IModifierType
    {
        private TypeReference modifier_type;

        public TypeReference ModifierType
        {
            get => modifier_type;
            set => modifier_type = value;
        }

        public override string Name => base.Name + Suffix;

        public override string FullName => base.FullName + Suffix;

        private string Suffix => " modopt(" + modifier_type + ")";

        public override bool IsValueType
        {
            get => false;
            set => throw new InvalidOperationException();
        }

        public override bool IsOptionalModifier => true;

        public override bool ContainsGenericParameter =>
            modifier_type.ContainsGenericParameter || base.ContainsGenericParameter;

        public OptionalModifierType(TypeReference modifierType, TypeReference type)
            : base(type)
        {
            if (modifierType == null)
                throw new ArgumentNullException(Mixin.Argument.modifierType.ToString());
            Mixin.CheckType(type);
            modifier_type = modifierType;
            etype = MD.ElementType.CModOpt;
        }
    }

    public sealed class RequiredModifierType : TypeSpecification, IModifierType
    {
        private TypeReference modifier_type;

        public TypeReference ModifierType
        {
            get => modifier_type;
            set => modifier_type = value;
        }

        public override string Name => base.Name + Suffix;

        public override string FullName => base.FullName + Suffix;

        private string Suffix => " modreq(" + modifier_type + ")";

        public override bool IsValueType
        {
            get => false;
            set => throw new InvalidOperationException();
        }

        public override bool IsRequiredModifier => true;

        public override bool ContainsGenericParameter =>
            modifier_type.ContainsGenericParameter || base.ContainsGenericParameter;

        public RequiredModifierType(TypeReference modifierType, TypeReference type)
            : base(type)
        {
            if (modifierType == null)
                throw new ArgumentNullException(Mixin.Argument.modifierType.ToString());
            Mixin.CheckType(type);
            modifier_type = modifierType;
            etype = MD.ElementType.CModReqD;
        }
    }
}