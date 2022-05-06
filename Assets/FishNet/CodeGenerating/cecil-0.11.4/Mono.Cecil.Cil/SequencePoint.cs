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
    public sealed class SequencePoint
    {
        internal InstructionOffset offset;
        private Document document;

        private int start_line;
        private int start_column;
        private int end_line;
        private int end_column;

        public int Offset => offset.Offset;

        public int StartLine
        {
            get => start_line;
            set => start_line = value;
        }

        public int StartColumn
        {
            get => start_column;
            set => start_column = value;
        }

        public int EndLine
        {
            get => end_line;
            set => end_line = value;
        }

        public int EndColumn
        {
            get => end_column;
            set => end_column = value;
        }

        public bool IsHidden => start_line == 0xfeefee && start_line == end_line;

        public Document Document
        {
            get => document;
            set => document = value;
        }

        internal SequencePoint(int offset, Document document)
        {
            if (document == null)
                throw new ArgumentNullException("document");

            this.offset = new InstructionOffset(offset);
            this.document = document;
        }

        public SequencePoint(Instruction instruction, Document document)
        {
            if (document == null)
                throw new ArgumentNullException("document");

            offset = new InstructionOffset(instruction);
            this.document = document;
        }
    }
}