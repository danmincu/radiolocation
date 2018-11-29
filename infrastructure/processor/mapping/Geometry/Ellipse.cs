using Mapping.Mapping;
using System.Runtime.Serialization;

namespace Mapping.Geometry
{
    /// <summary>
    /// A shape representing a stretched circle 
    /// </summary>
    [DataContract]
    public class Ellipse : GeographicShape
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
        /// the length of the longest point from the center of the ellipse to the edge of the ellipse
        /// </summary>
        [DataMember]
        public double UncertaintySemiMajor { get; set; }

        /// <summary>
        /// the length of the shortest point from the center of the ellipse to the edge of the ellipse
        /// </summary>
        [DataMember]
        public double UncertaintySemiMinor { get; set; }

        /// <summary>
        /// the orientation angle of the ellipse
        /// </summary>
        [DataMember]
        public double OrientationOfMajorAxis { get; set; }

        public Ellipse(double latitude, double longitude, double uncertaintySemiMajor, double uncertaintySemiMinor, double orientationOfMajorAxis)
        {
            this.Latitude = latitude;
            this.Longitude = longitude;
            this.UncertaintySemiMajor = uncertaintySemiMajor;
            this.UncertaintySemiMinor = uncertaintySemiMinor;
            this.OrientationOfMajorAxis = orientationOfMajorAxis;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;

            if (!(obj is Ellipse))
                return false;

            return Equals((Ellipse)obj);
        }

        protected bool Equals(Ellipse obj)
        {
            if (obj == null)
                return false;

            return this.Latitude.Equals(obj.Latitude) &&
                   this.Longitude.Equals(obj.Longitude) &&
                   this.UncertaintySemiMajor.Equals(obj.UncertaintySemiMajor) &&
                   this.UncertaintySemiMinor.Equals(obj.UncertaintySemiMinor) &&
                   this.OrientationOfMajorAxis.Equals(obj.OrientationOfMajorAxis);
        }

        public override int GetHashCode()
        {
            return this.ComputeHashCode(this.Latitude, this.Longitude, this.UncertaintySemiMajor, this.UncertaintySemiMinor, this.OrientationOfMajorAxis);
        }
    }
}
