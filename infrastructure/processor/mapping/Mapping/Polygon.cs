using System.Collections.Generic;
using System;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Drawing;

namespace Mapping.Mapping
{
    /// <summary>
    /// Class representing a geographical polygon
    /// </summary>
    [DataContract]
    public class Polygon
    {
        private const string polygonWKTPrefix = "POLYGON ((";
        private const string polygonWKTSuffix = "))";

        /// <summary>
        /// Creates a new empty instance
        /// </summary>
        public Polygon() : this(new List<Coordinate>()) { }

        /// <summary>
        /// Creates a new instance from a list of coordinates making up the polygon
        /// </summary>
        /// <param name="coordinates">The coordinates that make up the polygon</param>
        public Polygon(List<Coordinate> coordinates)
        {
            ArgumentValidation.CheckArgumentForNull(coordinates, nameof(coordinates));

            this.Coordinates = coordinates;
        }

        /// <summary>
        /// Creates a new instance from a WKT string defining the polygon
        /// </summary>
        /// <param name="wktPolygon">The WKT string defining the polygon</param>
        /// <remarks>See http://en.wikipedia.org/wiki/Well-known_text for WKT string formats</remarks>
        public Polygon(string wktPolygon)
        {
            ArgumentValidation.CheckArgumentForNull(wktPolygon, nameof(wktPolygon));

            this.Coordinates = new List<Coordinate>();

            if (!IsFormattedInWKT(wktPolygon))
                throw new ArgumentException("Expected that the polygon string begin with '" + polygonWKTPrefix + "' and end with '" + polygonWKTSuffix + "', not: " + wktPolygon);

            var listOfCoordinates = wktPolygon.Substring(polygonWKTPrefix.Length, wktPolygon.Length - polygonWKTSuffix.Length - polygonWKTPrefix.Length);

            if (string.IsNullOrEmpty(listOfCoordinates))
                return;

            var arrayOfCoordinates = listOfCoordinates.Split(',');

            foreach (var coord in arrayOfCoordinates)
            {
                if (string.IsNullOrEmpty(coord))
                    throw new ArgumentException("Expected coordinates and received none");

                var directions = coord.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (directions.Length != 2)
                    throw new ArgumentException("Expected 2 direction identifiers between commas, but received: " + coord);

                if (!(double.TryParse(directions[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var longitude) &&
                      double.TryParse(directions[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var latitude)))
                {
                    throw new ArgumentException("Expected a number for coordinate, but recived, Longitude: " +
                                                directions[0] + " Latitude:" + directions[1]);
                }

                this.Coordinates.Add(new Coordinate(latitude, longitude));
            }
        }

        /// <summary>
        /// Returns a polygon that encompasses the globe
        /// </summary>
        /// <returns>A polygon that wraps the entire globe</returns>
        public static Polygon EntireGlobe()
        {
            return new Polygon("POLYGON ((-180 -90,180 -90,180 90, -180 90, -180 -90))");
        }

        /// <summary>
        /// Converts the polygon into a WKT string
        /// </summary>
        /// <param name="p">The polygon to convert to WKT</param>
        /// <returns>The WKT string that represents the current polygon</returns>
        /// <remarks>See http://en.wikipedia.org/wiki/Well-known_text for WKT format info</remarks>
        public static string ConvertToWKT(Polygon p)
        {
            StringBuilder wktPolygon = new StringBuilder(polygonWKTPrefix);
            var firstIteration = true;
            foreach (Coordinate point in p.Coordinates)
            {
                if (firstIteration) firstIteration = false;
                else wktPolygon = wktPolygon.Append(", ");
                wktPolygon.AppendFormat(CultureInfo.InvariantCulture, "{0} {1}", point.Longitude, point.Latitude);
            }
            wktPolygon.Append(polygonWKTSuffix);
            return wktPolygon.ToString();

        }

        /// <summary>
        /// Checks that the string is in a wkt format for a polygon.
        /// </summary>
        /// <param name="wkt">The WKT string to test</param>
        /// <returns><c>true</c> if the given string is a valid WKT polygon, <c>false</c> otherwise</returns>
        public static bool IsFormattedInWKT(string wkt)
        {
            return (wkt.StartsWith(polygonWKTPrefix, StringComparison.Ordinal) && wkt.EndsWith(polygonWKTSuffix, StringComparison.Ordinal));
        }

        /// <inheritdoc cref="object"/>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("[");
            var firstIteration = true;
            foreach (Coordinate point in this.Coordinates)
            {
                if (firstIteration) firstIteration = false;
                else sb = sb.Append(", ");
                sb.AppendFormat(CultureInfo.InvariantCulture, "({0}, {1})", point.Latitude, point.Longitude);
            }
            sb.Append("]");
            return sb.ToString();
        }

        /// <summary>
        /// Due to serialization, we make the getter and setter public on this list. 
        /// </summary>
        [DataMember]
        public List<Coordinate> Coordinates { get; set; }

        /// <inheritdoc cref="object"/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (!(obj is Polygon)) return false;
            return Equals((Polygon)obj);
        }

        /// <summary>
        /// Checks to see if two polygons represent the same shape. If a polygon has coordinates A,B,C,D then polygons C,D,A,B (starting at different coord) and A,D,C,B (anticlockwise instead of clockwise) are the exact same polygon.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(Polygon other)
        {
            if (other == null)
                return false;

            if ((this.Coordinates == null) && other.Coordinates == null)
                return true;

            if (((this.Coordinates != null) && (other.Coordinates == null)) || ((this.Coordinates == null) && (other.Coordinates != null)))
                return false;

            var thisCoord = this.Coordinates.ToList();
            var otherCoord = other.Coordinates.ToList();

            if (thisCoord.Count != otherCoord.Count)
                return false;

            if (thisCoord.Count == 0)
                return true;

            var firstPoint = thisCoord.First();

            // Guaranteed to have non-empty lists at this point
            if (!thisCoord.First().Equals(otherCoord.First()))
            {
                // Check to see if other polygon starts its coordinates at a different corner.                
                var otherIndex = otherCoord.IndexOf(firstPoint);
                if (otherIndex < 0)
                    return false;
                var backupOtherCoord = new List<Coordinate>(otherCoord);
                otherCoord.Clear();
                otherCoord.AddRange(backupOtherCoord.GetRange(otherIndex, backupOtherCoord.Count - otherIndex));
                otherCoord.AddRange(backupOtherCoord.GetRange(0, otherIndex));
            }

            if (thisCoord.SequenceEqual(otherCoord))
                return true;

            // Check for the reversed polygon
            otherCoord.RemoveAt(0);
            otherCoord.Reverse();
            otherCoord.Insert(0, firstPoint);

            return thisCoord.SequenceEqual(otherCoord);
        }

        /// <summary>
        /// Convert a set of coordinates given to a rectangle enclosing all the points
        /// </summary>
        public static RectangleF ConvertToBoundingRectangle(List<Coordinate> coordinates)
        {
            if (coordinates.Count < 1)
                throw new ArgumentOutOfRangeException("coordinates");
            double maxLongitude, minLongitude, maxLatitude, minLatitude;
            maxLongitude = minLongitude = coordinates[0].Longitude;
            maxLatitude = minLatitude = coordinates[0].Latitude;
            // Finds the max and min longitudes and latitudes in the set of coordinates
            foreach (var coord in coordinates)
            {
                var lat = coord.Latitude;
                var lng = coord.Longitude;
                if (minLongitude > lng)
                    minLongitude = lng;
                else if (maxLongitude < lng)
                    maxLongitude = lng;
                if (minLatitude > lat)
                    minLatitude = lat;
                else if (maxLatitude < lat)
                    maxLatitude = lat;
            }

            // Returns the rectangle that contains the max and min longitudes and latitudes
            return new RectangleF(Convert.ToSingle(minLongitude), Convert.ToSingle(minLatitude),
                 Convert.ToSingle(maxLongitude - minLongitude), Convert.ToSingle(maxLatitude - minLatitude));
        }

        /// <inheritdoc cref="object"/>
        public override int GetHashCode()
        {
            var hash = 0;

            // unfortunately, List<Coordinate> generates different hash codes 
            // (even if both are empty) so we need to calc hashcodes manually.
            if (this.Coordinates.SafeAny())
            {
                foreach (var coord in this.Coordinates)
                    hash ^= coord.GetHashCode();
            }
            return hash;
        }

        /// <summary>
        /// Creates a new polygon instance with the same coordinates as the current polygon
        /// </summary>
        /// <returns>A clone of the current polygon</returns>
        public Polygon Clone()
        {
            return new Polygon(new List<Coordinate>(this.Coordinates));
        }

    }
}

