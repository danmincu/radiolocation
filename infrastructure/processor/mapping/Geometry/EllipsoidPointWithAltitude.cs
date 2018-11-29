using Mapping.Mapping;
using System.Runtime.Serialization;

namespace Mapping.Geometry
{
    /// <summary>
    /// A shape representing a single point with an associated altitude
    /// </summary>
    [DataContract]
    public class EllipsoidPointWithAltitude : GeographicShape
    {
        /// <summary>
        /// Gets or sets the latitude for the centre of this shape. Assumes WGS84, in degrees.
        /// </summary>
        [DataMember]
        public double Latitude { get; set; }

        /// <summary>
        /// Gets or sets the longitude for the centre of this shape. Assumes WGS84, in degrees.
        /// </summary>
        [DataMember]
        public double Longitude { get; set; }

        /// <summary>
        /// Gets or sets the altitude of this shape
        /// </summary>
        [DataMember]
        public double Altitude { get; set; }

        public EllipsoidPointWithAltitude(double latitude, double longitude, double altitude)
        {
            this.Latitude = latitude;
            this.Longitude = longitude;
            this.Altitude = altitude;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;

            if (!(obj is EllipsoidPointWithAltitude))
                return false;

            return Equals((EllipsoidPointWithAltitude)obj);
        }

        protected bool Equals(EllipsoidPointWithAltitude obj)
        {
            if (obj == null)
                return false;

            return this.Latitude.Equals(obj.Latitude) &&
                   this.Longitude.Equals(obj.Longitude) &&
                   this.Altitude.Equals(obj.Altitude);
        }

        public override int GetHashCode()
        {
            return this.ComputeHashCode(this.Latitude, this.Longitude, this.Altitude);
        }

    }
}
