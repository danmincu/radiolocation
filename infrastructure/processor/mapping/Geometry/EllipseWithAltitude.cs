using System.Runtime.Serialization;

namespace Mapping.Geometry
{
    /// <summary>
    /// Class that descripes an ellipse with an altitude
    /// </summary>
    [DataContract]
    public class EllipseWithAltitude : Ellipse
    {
        /// <summary>
        ///  the altitude of the ellipse shape
        /// </summary>
        [DataMember]
        public double AltitudeInMeters { get; set; }

        /// <summary>
        ///  the uncertainty altitude of the ellipse
        /// </summary>
        [DataMember]
        public double UncertainityAltitude { get; set; }

        public EllipseWithAltitude(double latitude, double longitude, double uncertaintySemiMajor, double uncertaintySemiMinor, double orientationOfMajorAxis, double altitudeInMeters, double uncertainAltitude) 
            :  base(latitude, longitude, uncertaintySemiMajor, uncertaintySemiMinor, orientationOfMajorAxis)
        {
            this.AltitudeInMeters = altitudeInMeters;
            this.UncertainityAltitude = uncertainAltitude;
        }

    }
}
