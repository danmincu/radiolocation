using Mapping.Mapping;
using System;

namespace Mapping
{
    public static class CoordinateTransformations
    {
        public const double KMPerDegree = 1.853159617481505; // 40 027.604 km circumference
        public const double MilesPerDegree = 1.1515; // 24872 mile circumference

        /// <summary>
        /// Degrees to Radians
        /// </summary>
        /// <param name="deg">degrees</param>
        /// <returns></returns>
        public static double Deg2Rad(double deg)
        {
            return deg * Math.PI / 180.0;
        }

        /// <summary>
        /// radians to degrees
        /// </summary>
        /// <param name="rad">radians</param>
        /// <returns></returns>
        public static double Rad2Deg(double rad)
        {
            return rad / Math.PI * 180.0;
        }

        /// <summary>
        /// Distance (in kilometers) between to points assuming spherical earth
        /// </summary>
        /// <param name="coordinate1"></param>
        /// <param name="coordinate2"></param>
        /// <returns></returns>
        static public double Distance(Coordinate coordinate1, Coordinate coordinate2)
        {
            return Distance(coordinate1.Longitude, coordinate1.Latitude, coordinate2.Longitude, coordinate2.Latitude);
        }

        static public double Distance(double startLongitude, double startLatitude, double endLongitude, double endLatitude)
        {
            double theta = startLongitude - endLongitude;
            double partial = Math.Sin(Deg2Rad(startLatitude)) * Math.Sin(Deg2Rad(endLatitude))
                        + Math.Cos(Deg2Rad(startLatitude)) * Math.Cos(Deg2Rad(endLatitude)) * Math.Cos(Deg2Rad(theta));
            return Rad2Deg(Math.Acos(partial)) * 60.0 * KMPerDegree;
        }

        /// <summary>
        /// Calculations the long lat from given long lat distance and bearing.
        /// Note: Computed assuming spherical earth
        /// </summary>
        /// <param name="coordinate">The coordinate.</param>
        /// <param name="distanceKM">The distance km.</param>
        /// <param name="bearingDegrees">The bearing degrees.</param>
        /// <returns></returns>
        static public Coordinate CalcLongLatFromGivenLongLatDistanceAndBearing(Coordinate coordinate, double distanceKM, double bearingDegrees)
        {
            return CalcLongLatFromGivenLongLatDistanceAndBearing(coordinate.Longitude, coordinate.Latitude, distanceKM, bearingDegrees);
        }

        static public Coordinate CalcLongLatFromGivenLongLatDistanceAndBearing(double longitudeDec, double latitudeDec, double distanceKM, double bearingDegrees)
        {
            const double radian = 180 / Math.PI;
            const double a = 6378.14;
            const double e = 0.08181922;

            double psi = distanceKM / (a * (1 - e * e) / Math.Pow((1 - e * e * Math.Pow(Math.Sin(Deg2Rad(latitudeDec)), 2.0)), 1.5));
            double phi = Math.PI / 2.0 - Deg2Rad(latitudeDec);

            return new Coordinate((Math.PI / 2.0 - Math.Acos(Math.Cos(psi) * Math.Cos(phi) + Math.Sin(psi) * Math.Sin(phi) * Math.Cos(bearingDegrees / radian))) * radian,
                (Deg2Rad(longitudeDec) - Math.Asin(Math.Sin(-bearingDegrees / radian) * Math.Sin(psi) / Math.Sin(phi))) * radian);
        }

        public static double GetRhumbLineBearing(Coordinate fromCoord, Coordinate toCoord)
        {
            // Difference in longitudinal coordinates
            var deltaLong = Deg2Rad(toCoord.Longitude) - Deg2Rad(fromCoord.Longitude);
 
            // Difference in the phi of latitudinal coordinates
            var deltaPhi =
                Math.Log(Math.Tan(Deg2Rad(toCoord.Latitude)/2.0 + Math.PI/4.0)/
                         Math.Tan(Deg2Rad(fromCoord.Latitude)/2.0 + Math.PI/4.0));

            // Return the normalized angle
            return (Rad2Deg(Math.Atan2(deltaLong, deltaPhi)) + 360)%360;
        }
    }

}
