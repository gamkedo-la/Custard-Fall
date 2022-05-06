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
using System.Text;
using System.Threading;
using MD = MonoFN.Cecil.Metadata;

namespace MonoFN.Cecil
{
    public struct ArrayDimension
    {
        private int? lower_bound;
        private int? upper_bound;

        public int? LowerBound
        {
            get => lower_bound;
            set => lower_bound = value;
        }

        public int? UpperBound
        {
            get => upper_bound;
            set => upper_bound = value;
        }

        public bool IsSized => lower_bound.HasValue || upper_bound.HasValue;

        public ArrayDimension(int? lowerBound, int? upperBound)
        {
            lower_bound = lowerBound;
            upper_bound = upperBound;
        }

        public override string ToString()
        {
            return !IsSized
                ? string.Empty
                : lower_bound + "..." + upper_bound;
        }
    }

    public sealed class ArrayType : TypeSpecification
    {
        private Collection<ArrayDimension> dimensions;

        public Collection<ArrayDimension> Dimensions
        {
            get
            {
                if (dimensions != null)
                    return dimensions;

                var empty_dimensions = new Collection<ArrayDimension>();
                empty_dimensions.Add(new ArrayDimension());

                Interlocked.CompareExchange(ref dimensions, empty_dimensions, null);

                return dimensions;
            }
        }

        public int Rank => dimensions == null ? 1 : dimensions.Count;

        public bool IsVector
        {
            get
            {
                if (dimensions == null)
                    return true;

                if (dimensions.Count > 1)
                    return false;

                var dimension = dimensions[0];

                return !dimension.IsSized;
            }
        }

        public override bool IsValueType
        {
            get => false;
            set => throw new InvalidOperationException();
        }

        public override string Name => base.Name + Suffix;

        public override string FullName => base.FullName + Suffix;

        private string Suffix
        {
            get
            {
                if (IsVector)
                    return "[]";

                var suffix = new StringBuilder();
                suffix.Append("[");
                for (var i = 0; i < dimensions.Count; i++)
                {
                    if (i > 0)
                        suffix.Append(",");

                    suffix.Append(dimensions[i].ToString());
                }

                suffix.Append("]");

                return suffix.ToString();
            }
        }

        public override bool IsArray => true;

        public ArrayType(TypeReference type)
            : base(type)
        {
            Mixin.CheckType(type);
            etype = MD.ElementType.Array;
        }

        public ArrayType(TypeReference type, int rank)
            : this(type)
        {
            Mixin.CheckType(type);

            if (rank == 1)
                return;

            dimensions = new Collection<ArrayDimension>(rank);
            for (var i = 0; i < rank; i++)
                dimensions.Add(new ArrayDimension());
            etype = MD.ElementType.Array;
        }
    }
}