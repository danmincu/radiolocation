using System.Runtime.Serialization;

namespace Mapping.Geometry
{
    /// <summary>
    /// Base class for all geographic shapes.
    /// </summary>
    [KnownType(typeof(Annulus))]
    [KnownType(typeof(Circle))]
    [KnownType(typeof(Ellipse))]
    [KnownType(typeof(EllipseWithAltitude))]
    [KnownType(typeof(UncertaintyPolygon))]
    [KnownType(typeof(EllipsoidPointWithAltitude))]
    [DataContract]
    public abstract class GeographicShape
    {
        public abstract override bool Equals(object obj);

        public abstract override int GetHashCode();

        public static bool operator ==(GeographicShape lhs, GeographicShape rhs)
        {
            if (ReferenceEquals(null, lhs) && ReferenceEquals(null, rhs))
                return true;

            if (ReferenceEquals(null, lhs))
                return false;

            return lhs.Equals(rhs);
        }

        public static bool operator !=(GeographicShape lhs, GeographicShape rhs)
        {
            return !(lhs == rhs);
        }
    }
}
