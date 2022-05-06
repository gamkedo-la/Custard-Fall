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

namespace MonoFN.Cecil.Cil
{
    public enum DocumentType
    {
        Other,
        Text
    }

    public enum DocumentHashAlgorithm
    {
        None,
        MD5,
        SHA1,
        SHA256
    }

    public enum DocumentLanguage
    {
        Other,
        C,
        Cpp,
        CSharp,
        Basic,
        Java,
        Cobol,
        Pascal,
        Cil,
        JScript,
        Smc,
        MCpp,
        FSharp
    }

    public enum DocumentLanguageVendor
    {
        Other,
        Microsoft
    }

    public sealed class Document : DebugInformation
    {
        private string url;

        private Guid type;
        private Guid hash_algorithm;
        private Guid language;
        private Guid language_vendor;

        private byte[] hash;
        private byte[] embedded_source;

        public string Url
        {
            get => url;
            set => url = value;
        }

        public DocumentType Type
        {
            get => type.ToType();
            set => type = value.ToGuid();
        }

        public Guid TypeGuid
        {
            get => type;
            set => type = value;
        }

        public DocumentHashAlgorithm HashAlgorithm
        {
            get => hash_algorithm.ToHashAlgorithm();
            set => hash_algorithm = value.ToGuid();
        }

        public Guid HashAlgorithmGuid
        {
            get => hash_algorithm;
            set => hash_algorithm = value;
        }

        public DocumentLanguage Language
        {
            get => language.ToLanguage();
            set => language = value.ToGuid();
        }

        public Guid LanguageGuid
        {
            get => language;
            set => language = value;
        }

        public DocumentLanguageVendor LanguageVendor
        {
            get => language_vendor.ToVendor();
            set => language_vendor = value.ToGuid();
        }

        public Guid LanguageVendorGuid
        {
            get => language_vendor;
            set => language_vendor = value;
        }

        public byte[] Hash
        {
            get => hash;
            set => hash = value;
        }

        public byte[] EmbeddedSource
        {
            get => embedded_source;
            set => embedded_source = value;
        }

        public Document(string url)
        {
            this.url = url;
            hash = Empty<byte>.Array;
            embedded_source = Empty<byte>.Array;
            token = new MetadataToken(TokenType.Document);
        }
    }
}