using Mapping.Mapping;
using System.Runtime.Serialization;

namespace Mapping.Geometry
{
    /// <summary>
    /// An annulus is the shape of a donught. An annulus sector is a slice of a donught. Delicious.
    /// </summary>
    [DataContract]
    public class Annulus : GeographicShape
    {
        /// <summary>
        /// Gets or sets the latitude for the centre of this shape. Assumes WGS84, in degrees.
        /// </summary>
        [DataMember]
        public double Latitude { get; set;  }

        /// <summary>
        /// Gets or sets the longitude for the centre of this shape. Assumes WGS84, in degrees.
        /// </summary>
        [DataMember]
        public double Longitude { get; set; }

        /// <summary>
        /// Gets or sets the angle from north at which the annulus is to start, in degress.
        /// </summary>
        [DataMember]
        public double OffsetAngle { get; set; }

        /// <summary>
        /// Gets or sets the angle offset from <see cref="OffsetAngle"/> at which point the annulus sector is to end, in degress.
        /// </summary>
        [DataMember]
        public double IncludedAngle { get; set; }

        /// <summary>
        /// Gets or sets the distance from the center point to the inner part of the annulus, in metres.
        /// </summary>
        [DataMember]
        public double InnerRadius { get; set; }

        /// <summary>
        /// Gets or sets the distance from the center point to the outer part of the annulus, in metres.
        /// </summary>
        [DataMember]
        public double OuterRadius { get; set; }

        public Annulus(double latitude, double longitude, double offsetAngle, double includedAngle, double innerRadius, double outerRadius)
        {
            this.Latitude = latitude;
            this.Longitude = longitude;
            this.OffsetAngle = offsetAngle;
            this.IncludedAngle = includedAngle;
            this.InnerRadius = innerRadius;
            this.OuterRadius = outerRadius;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;

            if (!(obj is Annulus))
                return false;

            return Equals((Annulus)obj);
        }

        protected bool Equals(Annulus obj)
        {
            if (obj == null)
                return false;

            return this.Latitude.Equals(obj.Latitude) &&
                this.Longitude.Equals(obj.Longitude) &&
                this.OffsetAngle.Equals(obj.OffsetAngle) &&
                this.IncludedAngle.Equals(obj.IncludedAngle) &&
                this.InnerRadius.Equals(obj.InnerRadius) &&
                this.OuterRadius.Equals(obj.OuterRadius);
        }

        public override int GetHashCode()
        {
            return this.ComputeHashCode(this.Latitude, this.Longitude, this.OffsetAngle, this.IncludedAngle, this.InnerRadius, this.OuterRadius);
        }
    }
}   
