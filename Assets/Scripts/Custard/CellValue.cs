using System.Collections.Generic;

namespace Custard
{
    public struct CellValue
    {
        public readonly Coords Coords;
        public int Value;

        public CellValue(int x, int y, int absoluteCustardLevel) : this(Coords.Of(x, y), absoluteCustardLevel)
        {
        }

        public CellValue(Coords coords, int value)
        {
            Coords = coords;
            Value = value;
        }

        public static CellValue Of(int x, int y, int value)
        {
            return new CellValue(Coords.Of(x,y), value);
        }
        
        public static CellValue Of(Coords coords, int value)
        {
            return new CellValue(coords, value);
        }

        public bool Equals(CellValue other)
        {
            return Coords.Equals(other.Coords);
        }

        public override bool Equals(object obj)
        {
            return obj is CellValue other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Coords.GetHashCode();
        }

        private sealed class CoordsEqualityComparer : IEqualityComparer<CellValue>
        {
            public bool Equals(CellValue x, CellValue y)
            {
                return x.Coords.Equals(y.Coords);
            }

            public int GetHashCode(CellValue obj)
            {
                return obj.Coords.GetHashCode();
            }
        }

        public static IEqualityComparer<CellValue> CoordsComparer { get; } = new CoordsEqualityComparer();
    }
}