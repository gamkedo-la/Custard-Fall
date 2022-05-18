using System.Collections.Generic;

namespace Custard
{
    public struct CellUpdate
    {
        public readonly Coords Coords;
        public byte AbsoluteCustardLevel;

        public CellUpdate(byte x, byte y, byte absoluteCustardLevel) : this(Coords.Of(x, y), absoluteCustardLevel)
        {
        }

        public CellUpdate(Coords coords, byte absoluteCustardLevel)
        {
            Coords = coords;
            AbsoluteCustardLevel = absoluteCustardLevel;
        }

        public bool Equals(CellUpdate other)
        {
            return Coords.Equals(other.Coords);
        }

        public override bool Equals(object obj)
        {
            return obj is CellUpdate other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Coords.GetHashCode();
        }

        private sealed class CoordsEqualityComparer : IEqualityComparer<CellUpdate>
        {
            public bool Equals(CellUpdate x, CellUpdate y)
            {
                return x.Coords.Equals(y.Coords);
            }

            public int GetHashCode(CellUpdate obj)
            {
                return obj.Coords.GetHashCode();
            }
        }

        public static IEqualityComparer<CellUpdate> CoordsComparer { get; } = new CoordsEqualityComparer();
    }
}