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
using System.Threading;

namespace MonoFN.Cecil
{
    public sealed class MethodReturnType : IConstantProvider, ICustomAttributeProvider, IMarshalInfoProvider
    {
        internal IMethodSignature method;
        internal ParameterDefinition parameter;
        private TypeReference return_type;

        public IMethodSignature Method => method;

        public TypeReference ReturnType
        {
            get => return_type;
            set => return_type = value;
        }

        internal ParameterDefinition Parameter
        {
            get
            {
                if (parameter == null)
                    Interlocked.CompareExchange(ref parameter, new ParameterDefinition(return_type, method), null);

                return parameter;
            }
        }

        public MetadataToken MetadataToken
        {
            get => Parameter.MetadataToken;
            set => Parameter.MetadataToken = value;
        }

        public ParameterAttributes Attributes
        {
            get => Parameter.Attributes;
            set => Parameter.Attributes = value;
        }

        public string Name
        {
            get => Parameter.Name;
            set => Parameter.Name = value;
        }

        public bool HasCustomAttributes => parameter != null && parameter.HasCustomAttributes;

        public Collection<CustomAttribute> CustomAttributes => Parameter.CustomAttributes;

        public bool HasDefault
        {
            get => parameter != null && parameter.HasDefault;
            set => Parameter.HasDefault = value;
        }

        public bool HasConstant
        {
            get => parameter != null && parameter.HasConstant;
            set => Parameter.HasConstant = value;
        }

        public object Constant
        {
            get => Parameter.Constant;
            set => Parameter.Constant = value;
        }

        public bool HasFieldMarshal
        {
            get => parameter != null && parameter.HasFieldMarshal;
            set => Parameter.HasFieldMarshal = value;
        }

        public bool HasMarshalInfo => parameter != null && parameter.HasMarshalInfo;

        public MarshalInfo MarshalInfo
        {
            get => Parameter.MarshalInfo;
            set => Parameter.MarshalInfo = value;
        }

        public MethodReturnType(IMethodSignature method)
        {
            this.method = method;
        }
    }
}