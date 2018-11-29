using Mapping.Mapping;

namespace Mapping
{
    /// <summary>
    /// Each tile is given X,Y coordinates ranging from (0, 0) in the upper left to (2^level–1, 2^level–1) in the lower right
    /// </summary>
    public class Tile
    {
        public long X { get; set; }
        public long Y { get; set; }

        public Tile(long x, long y)
        {
            if (x < 0) throw new MappingException("Tile must be a positive integer");
            if (y < 0) throw new MappingException("Tile must be a positive integer");

            X = x;
            Y = y;
        }

        

        public bool Equals(Tile other)
        {
            return other.X == this.X && other.Y == this.Y;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (!(obj is Tile)) return false;
            return Equals((Tile)obj);
        }

        public override int GetHashCode()
        {
            return ObjectExtensions.ComputeHashCode(null, this.X, this.Y);
        }

        public static bool operator ==(Tile a, Tile b)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(a, b))
            {
                return true;
            }
            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }
            // Return true if the fields match:
            return a.X == b.X && a.Y == b.Y;
        }

        public static bool operator !=(Tile a, Tile b)
        {
            return !(a == b);
        }


    }
}

