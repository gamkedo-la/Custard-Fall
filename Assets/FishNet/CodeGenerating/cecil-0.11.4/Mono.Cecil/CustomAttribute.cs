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
using System.Diagnostics;
using System.Threading;

namespace MonoFN.Cecil
{
    public struct CustomAttributeArgument
    {
        private readonly TypeReference type;
        private readonly object value;

        public TypeReference Type => type;

        public object Value => value;

        public CustomAttributeArgument(TypeReference type, object value)
        {
            Mixin.CheckType(type);
            this.type = type;
            this.value = value;
        }
    }

    public struct CustomAttributeNamedArgument
    {
        private readonly string name;
        private readonly CustomAttributeArgument argument;

        public string Name => name;

        public CustomAttributeArgument Argument => argument;

        public CustomAttributeNamedArgument(string name, CustomAttributeArgument argument)
        {
            Mixin.CheckName(name);
            this.name = name;
            this.argument = argument;
        }
    }

    public interface ICustomAttribute
    {
        TypeReference AttributeType { get; }

        bool HasFields { get; }
        bool HasProperties { get; }
        bool HasConstructorArguments { get; }
        Collection<CustomAttributeNamedArgument> Fields { get; }
        Collection<CustomAttributeNamedArgument> Properties { get; }
        Collection<CustomAttributeArgument> ConstructorArguments { get; }
    }

    [DebuggerDisplay("{AttributeType}")]
    public sealed class CustomAttribute : ICustomAttribute
    {
        internal CustomAttributeValueProjection projection;
        internal readonly uint signature;
        internal bool resolved;
        private MethodReference constructor;
        private byte[] blob;
        internal Collection<CustomAttributeArgument> arguments;
        internal Collection<CustomAttributeNamedArgument> fields;
        internal Collection<CustomAttributeNamedArgument> properties;

        public MethodReference Constructor
        {
            get => constructor;
            set => constructor = value;
        }

        public TypeReference AttributeType => constructor.DeclaringType;

        public bool IsResolved => resolved;

        public bool HasConstructorArguments
        {
            get
            {
                Resolve();

                return !arguments.IsNullOrEmpty();
            }
        }

        public Collection<CustomAttributeArgument> ConstructorArguments
        {
            get
            {
                Resolve();

                if (arguments == null)
                    Interlocked.CompareExchange(ref arguments, new Collection<CustomAttributeArgument>(), null);

                return arguments;
            }
        }

        public bool HasFields
        {
            get
            {
                Resolve();

                return !fields.IsNullOrEmpty();
            }
        }

        public Collection<CustomAttributeNamedArgument> Fields
        {
            get
            {
                Resolve();

                if (fields == null)
                    Interlocked.CompareExchange(ref fields, new Collection<CustomAttributeNamedArgument>(), null);

                return fields;
            }
        }

        public bool HasProperties
        {
            get
            {
                Resolve();

                return !properties.IsNullOrEmpty();
            }
        }

        public Collection<CustomAttributeNamedArgument> Properties
        {
            get
            {
                Resolve();

                if (properties == null)
                    Interlocked.CompareExchange(ref properties, new Collection<CustomAttributeNamedArgument>(), null);

                return properties;
            }
        }

        internal bool HasImage => constructor != null && constructor.HasImage;

        internal ModuleDefinition Module => constructor.Module;

        internal CustomAttribute(uint signature, MethodReference constructor)
        {
            this.signature = signature;
            this.constructor = constructor;
            resolved = false;
        }

        public CustomAttribute(MethodReference constructor)
        {
            this.constructor = constructor;
            resolved = true;
        }

        public CustomAttribute(MethodReference constructor, byte[] blob)
        {
            this.constructor = constructor;
            resolved = false;
            this.blob = blob;
        }

        public byte[] GetBlob()
        {
            if (blob != null)
                return blob;

            if (!HasImage)
                throw new NotSupportedException();

            return Module.Read(ref blob, this,
                (attribute, reader) => reader.ReadCustomAttributeBlob(attribute.signature));
        }

        private void Resolve()
        {
            if (resolved || !HasImage)
                return;

            lock (Module.SyncRoot)
            {
                if (resolved)
                    return;

                Module.Read(this, (attribute, reader) =>
                {
                    try
                    {
                        reader.ReadCustomAttributeSignature(attribute);
                        resolved = true;
                    }
                    catch (ResolutionException)
                    {
                        if (arguments != null)
                            arguments.Clear();
                        if (fields != null)
                            fields.Clear();
                        if (properties != null)
                            properties.Clear();

                        resolved = false;
                    }
                });
            }
        }
    }
}