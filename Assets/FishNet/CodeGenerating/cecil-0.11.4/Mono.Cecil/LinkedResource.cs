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
    public sealed class LinkedResource : Resource
    {
        internal byte[] hash;
        private string file;

        public byte[] Hash => hash;

        public string File
        {
            get => file;
            set => file = value;
        }

        public override ResourceType ResourceType => ResourceType.Linked;

        public LinkedResource(string name, ManifestResourceAttributes flags)
            : base(name, flags)
        {
        }

        public LinkedResource(string name, ManifestResourceAttributes flags, string file)
            : base(name, flags)
        {
            this.file = file;
        }
    }
}