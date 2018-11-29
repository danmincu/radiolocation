using Mapping.Geometry;
using Mapping.Mapping;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Mapping
{
    public struct Quadrangle : IRange<Coordinate>
    {
        public static readonly Quadrangle WholeWorld = new Quadrangle(new Coordinate(-90, -180), new Coordinate(90, 180));
        private static double Epsilon => Math.Pow(10, 8 * -1);

        private readonly Coordinate bottomLeft;
        private readonly Coordinate topRight;
        private readonly Coordinate centre;

        internal const double EndOfTheWorldLongitude = 180;
        internal const int DefaultZoom = 12;
        internal const int MinimumZoom = 1;
        
        public Quadrangle(Coordinate bottomLeft, Coordinate topRight)
        {
            ArgumentValidation.CheckArgumentForNull<Coordinate>(bottomLeft, "bottomLeft");
            ArgumentValidation.CheckArgumentForNull<Coordinate>(topRight, "topRight");

            if (bottomLeft.Latitude > topRight.Latitude) throw new ArgumentException("bottomLeft.Latitude must be less than the topRight.Latitude");
            if (bottomLeft.Longitude > topRight.Longitude) throw new ArgumentException("bottomLeft.Longitude must be less than the topRight.Longitude");

            this.bottomLeft = bottomLeft;
            this.topRight = topRight;

            double latitude = (bottomLeft.Latitude + topRight.Latitude) / 2;
            double longitude = (bottomLeft.Longitude + topRight.Longitude) / 2;
            centre = new Coordinate(latitude, longitude);

            this.hashCode = ObjectExtensions.ComputeHashCode(null, bottomLeft, topRight);
        }
        

        /// <summary>
        /// This constructor takes topLeft - bottomRight coordinates versus the "regular" bottomLeft - topRight
        /// </summary>
        public static Quadrangle ReversedQuadrangle(Coordinate topLeft, Coordinate bottomRight)
        {
            ArgumentValidation.CheckArgumentForNull<Coordinate>(topLeft, "topLeft");
            ArgumentValidation.CheckArgumentForNull<Coordinate>(bottomRight, "bottomRight");
            return new Quadrangle(new Coordinate(bottomRight.Latitude, topLeft.Longitude), new Coordinate(topLeft.Latitude, bottomRight.Longitude));
        }

        /// <summary>
        /// Calculates the intersection point of a line and the quadrangle.
        /// </summary>
        /// <param name="A">Start point of the line.</param>
        /// <param name="B">End point of the line.</param>
        /// <returns>A Point of collision if the line intersects with the quadrangle, default(Coordinate) otherwise.</returns>
        public Coordinate Intersection(Coordinate a, Coordinate b)
        {
            // For each side of the quadrangle, calculate the intersection point.
            Coordinate intersection = default(Coordinate);

            var quadrangleSides = new List<Tuple<Coordinate, Coordinate>>
                {
                    new Tuple<Coordinate, Coordinate>(this.TopLeft, this.TopRight),
                    new Tuple<Coordinate, Coordinate>(this.BottomRight, this.TopRight),
                    new Tuple<Coordinate, Coordinate>(this.BottomLeft, this.BottomRight),
                    new Tuple<Coordinate, Coordinate>(this.BottomLeft, this.TopLeft)
                };

            foreach (var quadrangleSide in quadrangleSides)
            {
                intersection = Intersection(quadrangleSide.Item1, quadrangleSide.Item2, a, b);
                if (intersection != default(Coordinate))
                {
                    // Verify the point is within the quadrangle and if not, keep trying.
                    if (this.Contains(intersection))
                    {
                        break;
                    }

                    intersection = default(Coordinate);
                }
            }
            return intersection;
        }

        /// <summary>
        /// Calculates the point of intersection between two lines defined by coordinate endpoints.
        /// For more info, check out http://stackoverflow.com/questions/563198/how-do-you-detect-where-two-line-segments-intersect.
        /// </summary>
        /// <param name="a">Start coord of first line.</param>
        /// <param name="b">End coord of first line.</param>
        /// <param name="c">Start coord of second line.</param>
        /// <param name="d">End coord of second line.</param>
        private static Coordinate Intersection(Coordinate a, Coordinate b, Coordinate c, Coordinate d)
        {
            Coordinate intersectingCoord = default(Coordinate);

            var pointA = new Point(a.Latitude, a.Longitude);
            var pointB = new Point(b.Latitude, b.Longitude);

            var pointC = new Point(c.Latitude, c.Longitude);
            var pointD = new Point(d.Latitude, d.Longitude);

            var vectorE = Point.Subtract(pointB, pointA);
            var vectorF = Point.Subtract(pointD, pointC);
            var vectorP = new Vector(-vectorE.Y, vectorE.X);

            // We calculate h, which represents the multiplication factor for the line in order to cause
            // it to intersect with our quadrangle (between 0 and 1 means it intersects).
            // Calculated as: h = ((A - C)*P)/(F*P)
            double h = (double)(DotProduct(Point.Subtract(pointA, pointC), vectorP)) / (double)(DotProduct(vectorF, vectorP));

            // The lines intersect if h is between 0 and 1.
            if (0 <= h && h <= 1)
            {
                // intersection = C + F*h
                var intersectingPoint = Point.Add(pointC, Vector.Multiply(vectorF, h));
                intersectingCoord = new Coordinate(intersectingPoint.X, intersectingPoint.Y);
            }

            return intersectingCoord;
        }

        private static double DotProduct(Vector a, Vector b)
        {
            var result = (a.X * b.X) + (a.Y * b.Y);
            return result;
        }

        /// <summary>
        /// Gets the centre coordinate for the quadrangle.
        /// </summary>
        /// <value>The centre.</value>
        public Coordinate Centre
        {
            get { return centre; }
        }

        public Coordinate BottomLeft
        {
            get { return this.bottomLeft; }
        }

        public Coordinate TopRight
        {
            get { return this.topRight; }
        }

        public Coordinate BottomRight
        {
            get { return new Coordinate(this.bottomLeft.Latitude, this.topRight.Longitude); }
        }

        public Coordinate TopLeft
        {
            get { return new Coordinate(this.topRight.Latitude, this.bottomLeft.Longitude); }
        }

        public static bool operator ==(Quadrangle mg1, Quadrangle mg2)
        {
            return mg1.Equals(mg2);
        }

        public static bool operator !=(Quadrangle mg1, Quadrangle mg2)
        {
            return !(mg1 == mg2);
        }

        private readonly int hashCode;
        public override int GetHashCode()
        {
            return hashCode;
        }

        public bool Equals(Quadrangle other)
        {
            return (other.BottomLeft == this.BottomLeft) && (other.TopRight == this.TopRight);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (!(obj is Quadrangle)) return false;
            return Equals((Quadrangle)obj);
        }

        private const string ToStringTemplate = "BottomLeft: {0}; TopRight: {1}";
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return ToStringTemplate.FormatInvariantCulture(this.BottomLeft, this.TopRight);
        }

        public bool OverlapsWith(IRange range)
        {
            if (!(range is Quadrangle)) return false;

            var innerQuad = (Quadrangle)range;

            // If one quad is fully encompassed in another, then they overlap
            if (this.Contains(innerQuad) || innerQuad.Contains(this)) return true;

            // If a vertex from one quad crosses any vertex from another quad, they overlap
            //
            // Assumption:  the map that we're dealing with does not wrap around the international
            //              date line, and thus no quads will span it.           
            return this.ToExtent().IntersectsWith(innerQuad.ToExtent());
        }

        public bool Contains(IRange innerRange)
        {
            if (!(innerRange is Quadrangle)) return false;

            var innerQuad = (Quadrangle)innerRange;
            return this.Contains(innerQuad.BottomLeft) && this.Contains(innerQuad.TopRight);
        }

        /// <summary>
        /// Return true if the specified long/lat coordinate falls within this quad (inclusive), 
        /// or false otherwise.
        /// </summary>
        public bool Contains(Coordinate coord)
        {
            if (!(this.BottomLeft.Latitude <= coord.Latitude && this.TopRight.Latitude >= coord.Latitude))
            {
                return false;
            }
            // this would be true in the case of quadrangle that spans -180 to 180         
            else if (this.bottomLeft.Longitude == -this.TopRight.Longitude
                     && this.TopRight.Longitude == EndOfTheWorldLongitude)
            {
                return true;
            }
            else
            {
                return (this.BottomLeft.Longitude <= coord.Longitude && this.TopRight.Longitude >= coord.Longitude);
            }
        }

        public Quadrangle Combine(Quadrangle other)
        {
            return new Quadrangle(
                new Coordinate(
                    Math.Min(this.BottomLeft.Latitude, other.BottomLeft.Latitude),
                    Math.Min(this.BottomLeft.Longitude, other.BottomLeft.Longitude)),
                new Coordinate(
                    Math.Max(this.TopRight.Latitude, other.TopRight.Latitude),
                    Math.Max(this.TopRight.Longitude, other.TopRight.Longitude)
                    ));
        }

        /// <summary>
        /// This function returns the highest zoom level that contains this quadrangle
        /// </summary>
        /// <returns></returns>
        public double GetViewportZoomLevel()
        {
            // Calculate
            var zoom = GetViewPortZoomLevelInternal();

            // Ensure we don't provide invalid zoom (0)
            return Math.Max(zoom, MinimumZoom);
        }

        private double GetViewPortZoomLevelInternal()
        {
            // Single point; zoom to default
            if (this.topRight == this.bottomLeft) return DefaultZoom;

            var longitudeZoomLevel = 0.0;
            //Get the zoom level of the Longitude edge of the quadrangle.  If the left and right Longitude are equal to within 1.1cm then
            //consider them equal with a zoom level infinite
            if (Math.Abs(this.topRight.Longitude - this.bottomLeft.Longitude) < Epsilon)
            {
                longitudeZoomLevel = double.PositiveInfinity;
            }
            else
            {
                //Get the zoom level of the longitude edge of the quadrangle.
                longitudeZoomLevel = Math.Log(
                    (TileSystem.MaxLocation.Longitude - TileSystem.MinLocation.Longitude)/
                    Math.Abs(((this.topRight.Longitude > this.bottomLeft.Longitude) ? 0 : 360)
                             + this.topRight.Longitude - this.bottomLeft.Longitude), 2
                    );
            }

            var latitudeZoomLevel = 0.0;
            //Get the zoom level of the latitude edge of the quadrangle.  If the top and bottom latitudes are equal to within 1.1cm then
            //consider them equal with a zoom level infinite
            if (Math.Abs(this.topRight.Latitude - this.bottomLeft.Latitude) < Epsilon)
            {
                latitudeZoomLevel = double.PositiveInfinity;
            }
            else
            {
                latitudeZoomLevel = Math.Log(
                        (TileSystem.MaxLocation.Latitude - TileSystem.MinLocation.Latitude)/
                        Math.Abs(this.topRight.Latitude - this.bottomLeft.Latitude), 2);
            
            }

            //Return the zoom level required to accommodate the longest side of the quadrangle.
            return 1 + Math.Min(longitudeZoomLevel, latitudeZoomLevel);
        }

        /// <summary>
        /// Convert the Quadrangle to an <see cref="Extent" />, treating Longitude as X and Latitude as Y
        /// </summary>
        private Extent ToExtent()
        {
            var topLeft = new Point(this.TopLeft.Longitude, this.TopLeft.Latitude);
            var bottomRight = new Point(this.BottomRight.Longitude, this.BottomRight.Latitude);
            return new Extent(topLeft, bottomRight);
        }

        /// <summary>
        /// The method is returning the encompasing quadrangle that coincides with the intersected quadkeys for the computed zoom/level
        /// </summary>
        /// <param name="ceilingZoom">maximum level/zoom to determine the intersected quadkeys</param>
        /// <returns></returns>
        public Quadrangle QuadkeysBoundingQuadrangle(float ceilingZoom)
        {
            ArgumentValidation.CheckArgumentIsInRange(ceilingZoom, 1, 20, "ceilingZoom");
            var zoomLevel = (int)Math.Min(ceilingZoom, GetViewPortZoomLevelInternal() + 1);
            var northWest = QuadKey.ConvertCoordinateToNumericQuadKey(this.TopLeft, zoomLevel);
            var southEast = QuadKey.ConvertCoordinateToNumericQuadKey(this.BottomRight, zoomLevel);
            return ReversedQuadrangle(QuadKey.ConvertQuadKeyToQuadrangle(northWest).TopLeft,
                               QuadKey.ConvertQuadKeyToQuadrangle(southEast).BottomRight);
        }

        /// <summary>
        /// Overloaded method for the CLS compatibility
        /// </summary>
        /// <returns></returns>
        public Quadrangle QuadkeysBoundingQuadrangle()
        {
            return QuadkeysBoundingQuadrangle(14);
        }

        /// <summary>
        /// Gets the approximate Width in km.
        /// </summary>
        public double Width
        {
            get { return CoordinateTransformations.Distance(this.TopLeft, this.TopRight); }
        }

        /// <summary>
        /// Gets the approximate Height in km.
        /// </summary>
        public double Height
        {
            get { return CoordinateTransformations.Distance(this.TopLeft, this.BottomLeft); }
        }

        /// <summary>
        /// Gets the approximate Area in km squared.
        /// </summary>
        public double Area
        {
            get { return Width * Height; }
        }

        public static Quadrangle FromPolygon(Polygon polygon)
        {
            double minX = double.MaxValue, minY = double.MaxValue, maxX = double.MinValue, maxY = double.MinValue;

            foreach (var coord in polygon.Coordinates)
            {
                minX = Math.Min(minX, coord.Longitude);
                minY = Math.Min(minY, coord.Latitude);
                maxX = Math.Max(maxX, coord.Longitude);
                maxY = Math.Max(maxY, coord.Latitude);
            }

            return new Quadrangle(new Coordinate(minY, minX), new Coordinate(maxY, maxX));
        }

        /// <summary>
        /// Return a polygon that represents this quadrangle.
        /// </summary>
        /// <returns></returns>
        public Polygon ToPolygon()
        {
           return new Polygon
            {
                Coordinates = new List<Coordinate>()
                    {
                        this.TopLeft,
                        this.BottomLeft,
                        this.BottomRight,
                        this.TopRight,
                        this.TopLeft
                    }
            };
        }
    }
}
