using Mapping.Mapping;
using System.Runtime.Serialization;

namespace Mapping.Geometry
{
    [DataContract]
    public class Circle : GeographicShape
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
        /// Gets or sets the radius of the circle, in metres.
        /// </summary>
        [DataMember]
        public double Radius { get; set; }

        /// <summary>
        /// Gets or sets the elevation of the circle, in metres.
        /// </summary>
        [DataMember]
        public double? ElevationInMeters { get; set; }

        public Circle(double latitude, double longitude, double radius)
        {
            this.Latitude = latitude;
            this.Longitude = longitude;
            this.Radius = radius;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;

            if (!(obj is Circle))
                return false;

            return Equals((Circle)obj);
        }

        protected bool Equals(Circle obj)
        {
            if (obj == null)
                return false;

            return this.Latitude.Equals(obj.Latitude) &&
                this.Longitude.Equals(obj.Longitude) &&
                this.Radius.Equals(obj.Radius);
        }

        public override int GetHashCode()
        {
            return this.ComputeHashCode(this.Latitude, this.Longitude, this.Radius);
        }
    }
}
