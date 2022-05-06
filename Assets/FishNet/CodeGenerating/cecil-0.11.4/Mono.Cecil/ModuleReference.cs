//
// Author:
//   Jb Evain (jbevain@gmail.com)
//
// Copyright (c) 2008 - 2015 Jb Evain
// Copyright (c) 2008 - 2011 Novell, Inc.
//
// Licensed under the MIT/X11 license.
//

namespace MonoFN.Cecil
{
    public class ModuleReference : IMetadataScope
    {
        private string name;

        internal MetadataToken token;

        public string Name
        {
            get => name;
            set => name = value;
        }

        public virtual MetadataScopeType MetadataScopeType => MetadataScopeType.ModuleReference;

        public MetadataToken MetadataToken
        {
            get => token;
            set => token = value;
        }

        internal ModuleReference()
        {
            token = new MetadataToken(TokenType.ModuleRef);
        }

        public ModuleReference(string name)
            : this()
        {
            this.name = name;
        }

        public override string ToString()
        {
            return name;
        }
    }
}