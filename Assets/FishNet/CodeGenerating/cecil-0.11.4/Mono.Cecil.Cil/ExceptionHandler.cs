//
// Author:
//   Jb Evain (jbevain@gmail.com)
//
// Copyright (c) 2008 - 2015 Jb Evain
// Copyright (c) 2008 - 2011 Novell, Inc.
//
// Licensed under the MIT/X11 license.
//

namespace MonoFN.Cecil.Cil
{
    public enum ExceptionHandlerType
    {
        Catch = 0,
        Filter = 1,
        Finally = 2,
        Fault = 4
    }

    public sealed class ExceptionHandler
    {
        private Instruction try_start;
        private Instruction try_end;
        private Instruction filter_start;
        private Instruction handler_start;
        private Instruction handler_end;

        private TypeReference catch_type;
        private ExceptionHandlerType handler_type;

        public Instruction TryStart
        {
            get => try_start;
            set => try_start = value;
        }

        public Instruction TryEnd
        {
            get => try_end;
            set => try_end = value;
        }

        public Instruction FilterStart
        {
            get => filter_start;
            set => filter_start = value;
        }

        public Instruction HandlerStart
        {
            get => handler_start;
            set => handler_start = value;
        }

        public Instruction HandlerEnd
        {
            get => handler_end;
            set => handler_end = value;
        }

        public TypeReference CatchType
        {
            get => catch_type;
            set => catch_type = value;
        }

        public ExceptionHandlerType HandlerType
        {
            get => handler_type;
            set => handler_type = value;
        }

        public ExceptionHandler(ExceptionHandlerType handlerType)
        {
            handler_type = handlerType;
        }
    }
}