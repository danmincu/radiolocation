using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;

namespace Mapping.Mapping
{
    /// <summary>
    /// Class representing a lat/long coordinate
    /// </summary>
#if NET45
    [Serializable]
#endif
    [DataContract]
    public class Coordinate
    {
        private const double EndOfTheWorldLongitude = 180;

        // In constructor, we will normalize the double to a reasonable coordinate value
        // 4th decimal place is up to 11m
        // 5th decimal place is up to 1.1m
        // 6th decimal place is up to 0.11m
        // 7th decimal place is up to 11mm
        // 8th decimal place is up to 1.1mm
        // see http://gis.stackexchange.com/questions/8650/how-to-measure-the-accuracy-of-latitude-and-longitude
        private const int RoundingPlaces = 8;
        private static double Epsilon => Math.Pow(10, 8 * -1);


        /// <summary>
        /// Returns an instance that represents an empty coordinate
        /// </summary>
        public static readonly Coordinate Empty = new Coordinate { Latitude = double.NaN, Longitude = double.NaN };

        /// <summary>
        /// Creates a new coordinate
        /// </summary>
        public Coordinate() { }

        /// <summary>
        /// Creates a new coordinate with validation
        /// </summary>
        /// <param name="latitude">The latitude of the coordinate</param>
        /// <param name="longitude">The longitude of the coordinate</param>
        public Coordinate(double latitude, double longitude)
        {
            this.Latitude = Math.Round(latitude, RoundingPlaces, MidpointRounding.AwayFromZero);
            this.Longitude = Math.Round(longitude, RoundingPlaces, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Creates a new coordinate with validation
        /// </summary>
        /// <param name="latitude">The latitude of the coordinate</param>
        /// <param name="longitude">The longitude of the coordinate</param>
        public Coordinate(float latitude, float longitude)
            : this(Convert.ToDouble(latitude), Convert.ToDouble(longitude))
        {
        }

        /// <summary>
        /// Gets or sets the latitude of the coordinate
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public double Latitude { get; set; }

        /// <summary>
        /// Gets or sets the longitude of the coordinate
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public double Longitude { get; set; }


        /// <summary>
        /// Given string lat and lon, retrieve coordinate and a boolean indicator for success
        /// </summary>
        /// <param name="location">The string location in format "long lat"</param>
        /// <param name="coordinate">The output coordinate, lat lon 90, 0 is the invalid coordinate</param>
        /// <returns>boolean indicating success</returns>
        public static bool TryParseCoordinate(string location, out Coordinate coordinate)
        {
            var locationPair = location?.Split(' ');

            if (locationPair != null && locationPair.Length == 2 &&
                !string.IsNullOrEmpty(locationPair[0]) &&
                !locationPair[0].Equals("NaN", StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrEmpty(locationPair[1]) &&
                !locationPair[1].Equals("NaN", StringComparison.OrdinalIgnoreCase))
            {
                var latOK = double.TryParse(locationPair[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var latitude);
                var lngOK = double.TryParse(locationPair[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var longitude);

                if (latOK && lngOK)
                {
                    if (latitude >= -90.0 && latitude <= 90.0 && longitude >= -180.0 && longitude <= 180.0)
                    {
                        coordinate = new Coordinate(latitude, longitude);
                        return true;
                    }
                }
            }

            coordinate = new Coordinate(90.0, 0.0);
            return false;
        }

        /// <summary>
        /// Parses a WRT string representing a multipoint into a list of coordinates
        /// </summary>
        /// <param name="value">The multipoint string to parse</param>
        /// <returns>The list of coordinates that make up the multipoint string</returns>
        public static List<Coordinate> FromMultipointString(string value)
        {
            ArgumentValidation.CheckArgumentForNullOrEmpty(value, nameof(value));
            if (!value.Trim().StartsWith("MULTIPOINT", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("value given is not a valid multipoint string", nameof(value));
            var result = new List<Coordinate>();

            /* 
             * Mutipoint strings can be in two different formats: 
             *   MULTIPOINT ((10 40), (40 30), (EMPTY), (30 10))
             *   MULTIPOINT (10 40, 40 30, EMPTY, 30 10)
             * We can support either by just parsing out everything but the numbers, spaces & commas, then
             * splitting based on commas & remaining spaces
             */
            value = value.Replace("MULTIPOINT", "")
                .Replace("(", "")
                .Replace(")", "")
                .Trim();

            if (!string.IsNullOrEmpty(value))
            {
                var pointsStrs = value.Split(',');
                foreach (var pointStr in pointsStrs)
                {
                    if (pointStr.Trim().Equals("EMPTY", StringComparison.OrdinalIgnoreCase))
                        result.Add(Coordinate.Empty);
                    else
                    {
                        var locs = pointStr.Trim().Split(' ');
                        if (locs.Length != 2 || !double.TryParse(locs[0], out var lon) || !double.TryParse(locs[1], out var lat))
                        {
                            throw new ArgumentException(
                                "The multipoint string '{0}' provided is an invalid format".FormatInvariantCulture(
                                    value));
                        }
                        result.Add(new Coordinate(lat, lon));
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="lhp">The LHP.</param>
        /// <param name="rhp">The RHP.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(Coordinate lhp, Coordinate rhp)
        {
            if (ReferenceEquals(lhp, rhp))
                return true;

            if (((object)lhp == null) || ((object)rhp == null))
                return false;

            return lhp.Equals(rhp);
        }

        /// <summary>
        /// Implements the operator !=
        /// </summary>
        /// <param name="c1">The LHP</param>
        /// <param name="c2">The RHP</param>
        /// <returns>The result of the operator</returns>
        public static bool operator !=(Coordinate c1, Coordinate c2)
        {
            return !(c1 == c2);
        }

        private const string ToStringTemplate = "({0}, {1})";

        /// <inheritdoc cref="object"/>
        public override string ToString()
        {
            return ToStringTemplate.FormatInvariantCulture(this.Latitude, this.Longitude);
        }

        /// <inheritdoc cref="object"/>
        public override int GetHashCode()
        {
            return ObjectExtensions.ComputeHashCode(null, this.Latitude, this.Longitude);
        }

        /// <summary>
        /// Tests if the current coordinate equals a given coordinate
        /// </summary>
        /// <param name="other">The coordinate to test equality against</param>
        /// <returns><c>true</c> if the two coordinates are equal, <c>false</c> otherwise</returns>
        public bool Equals(Coordinate other)
        {
            //Check if both lat and long are NaN. 
            if (this.Latitude.Equals(double.NaN) && other.Latitude.Equals(double.NaN) &&
                this.Longitude.Equals(double.NaN) && other.Longitude.Equals(double.NaN))
            {
                return true;
            }
            if (Math.Abs(this.Latitude - other.Latitude) < Epsilon)
            {
                if (Math.Abs(Math.Abs(this.Longitude) - EndOfTheWorldLongitude) < Epsilon)
                {
                    return Math.Abs(Math.Abs(other.Longitude) - EndOfTheWorldLongitude) < Epsilon;
                }
                return Math.Abs(this.Longitude - other.Longitude) < Epsilon;
            }
            return false;
        }

        /// <inheritdoc cref="object"/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (!(obj is Coordinate)) return false;
            return Equals((Coordinate)obj);
        }

        /// <summary>
        /// Checks if the given coordinate is empty
        /// </summary>
        /// <returns><c>true</c> if the coordinate is empty, <c>false</c> otherwise</returns>
        public bool IsEmpty()
        {
            return double.IsNaN(this.Latitude) || double.IsNaN(this.Longitude);
        }

        /// <summary>
        /// Checks if the given coordinate is valid (ie. has valid lat/long values)
        /// </summary>
        /// <returns><c>true</c> if the coordinate is valid, <c>false</c> otherwise</returns>
        public bool IsValid()
        {
            var result = true;

            if (Double.IsNaN(this.Latitude) || Double.IsNaN(this.Longitude))
            {
                result = false;
            }
            else if (this.Latitude < -90F || this.Latitude > 90F)
            {
                result = false;
            }
            else if (this.Longitude < -180F || this.Longitude > 180F)
            {
                result = false;
            }

            return result;
        }

    }
}
