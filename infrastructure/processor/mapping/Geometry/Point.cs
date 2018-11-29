using Mapping.Mapping;
using System;
using System.Collections.Generic;

namespace Mapping.Geometry
{
    public struct Point : IEquatable<Point>
    {
        private readonly double x;
        private readonly double y;

        public static Point Empty()
        {
            return new Point(Double.NaN, Double.NaN);
        }

        public Point(double x, double y) : this()
        {
            this.x = x;
            this.y = y;
        }

        public double X
        {
            get { return this.x; }
        }

        public double Y
        {
            get { return this.y; }
        }

        public bool IsEmpty()
        {
            return Double.IsNaN(this.x) || Double.IsNaN(this.y);
        }

        /// <summary>
        /// Subtracts the specified <see cref="Point" /> from another specified <see cref="Point" /> and returns the difference as a <See cref="Vector" />.
        /// </summary>
        /// <param name="point1">The The point from which point2 is subtracted.</param>
        /// <param name="point2">The point to subtract from point1.</param>
        /// <returns>The difference between point1 and point2.</returns>
        public static Vector Subtract(Point point1, Point point2)
        {
            return new Vector(point1.X - point2.X, point1.Y - point2.Y);
        }

        /// <summary>
        /// Add: Point + Vector
        /// </summary> 
        /// <returns>
        /// Point - The result of the addition 
        /// </returns> 
        /// <param name="point"> The Point to be added to the Vector </param>
        /// <param name="vector"> The Vector to be added to the Point </param> 
        public static Point Add(Point point, Vector vector)
        {
            return new Point(point.X + vector.X, point.Y + vector.Y);
        }

        /// <summary>
        /// Parses a WRT string representing a multipoint into a list of points
        /// </summary>
        /// <param name="value">The multipoint string to parse</param>
        /// <returns>The list of points that make up the multipoint string</returns>
        public static List<Point> FromMultipointString(string value)
        {
            ArgumentValidation.CheckArgumentForNullOrEmpty(value, nameof(value));
            if(!value.Trim().StartsWith("MULTIPOINT", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("value given is not a valid multipoint string", nameof(value));
            var result = new List<Point>();

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
                        result.Add(Point.Empty());
                    else
                    {
                        double lat, lon;
                        var locs = pointStr.Trim().Split(' ');
                        if (locs.Length != 2 || !double.TryParse(locs[0], out lon) || !double.TryParse(locs[1], out lat))
                        {
                            throw new ArgumentException(
                                "The multipoint string '{0}' provided is an invalid format".FormatInvariantCulture(
                                    value));
                        }
                        result.Add(new Point(lon, lat));
                    }
                }
            }

            return result;
        } 

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (!(obj is Point))
                return false;

            return this.Equals((Point) obj);
        }

        /// <summary>
        /// Determines if the given point is equal to this point.
        /// </summary>
        /// <param name="point">The point to check for equality.</param>
        /// <returns><c>True</c> if the points are equal; <c>false</c> otherwise.</returns>
        public bool Equals(Point point)
        {
            return point.X == this.X && point.Y == this.Y;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return this.ComputeHashCode(this.X, this.Y);
        }

        #region Operators

        public static bool operator ==(Point p1, Point p2)
        {
            return p1.Equals(p2);
        }

        public static bool operator !=(Point p1, Point p2)
        {
            return !p1.Equals(p2);
        }

        #endregion
    }
}
