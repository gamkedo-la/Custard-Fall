﻿
    using System;
    using System.Collections.Generic;
    using Unity.VisualScripting;

    public struct Coords
    {
        public readonly byte X;

        public readonly byte Y;

        public override string ToString()
        {
            return "("+X+","+Y+")";
        }

        public Coords(byte x, byte y)
        {
            this.X = x;
            this.Y = y;
        }

        public Coords Add(byte dx, byte dy)
        {
            return new Coords((byte)(this.X + dx), (byte)(this.Y + dy));
        }        
        public Coords Add(int dx, int dy)
        {
            return new Coords((byte)(this.X + dx), (byte)(this.Y + dy));
        }

        private sealed class XYEqualityComparer : IEqualityComparer<Coords>
        {
            public bool Equals(Coords x, Coords y)
            {
                return x.X == y.X && x.Y == y.Y;
            }

            public int GetHashCode(Coords obj)
            {
                return HashCode.Combine(obj.X, obj.Y);
            }
        }

        public static IEqualityComparer<Coords> XYComparer { get; } = new XYEqualityComparer();

        public bool Equals(Coords other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            return obj is Coords other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

       
    }
