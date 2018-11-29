using Mapping.Mapping;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace Mapping.Shapes
{
    public class OmaShape
    {       
        private readonly shape shape;
        private readonly string xmlData;

        #region Constructors

        protected OmaShape(OmaShape omaShape)
        {
            this.shape = omaShape.shape;
        }

        protected OmaShape(string xmlData)
        {
            this.xmlData = xmlData;

            if (string.IsNullOrEmpty(xmlData))
                return;

            var ser = new XmlSerializer(typeof(svc_result));
            using (var stringReader = new StringReader(xmlData))
            {
                using (var xmlReader = new XmlTextReader(stringReader))
                {
                    try
                    {
                        var root = ser.Deserialize(xmlReader) as svc_result;
                        if (root != null && root.slia != null && root.slia.pos != null && root.slia.pos.pd != null)
                        {
                            this.shape = root.slia.pos.pd.shape;
                        }
                    }
                    catch (Exception e)
                    {
                        throw new InvalidDataException(string.Format(CultureInfo.InvariantCulture,
                                                               "Invalid Xml cannot be deserialized into an OmaShape class: {0}",
                                                               e.Message));
                    }
                    xmlReader.Close();
                    stringReader.Close();
                }
            }
        }

        #endregion

        protected string XmlData
        {
            get { return xmlData; }
        }

        public virtual IEnumerable<Coordinate> PolygonCoordinates
        {
            get { return new List<Coordinate>(); }
        }

        protected shape Shape
        {
            get { return this.shape; }
        }

        public override bool Equals(object obj)
        {
            if (obj == null || this.GetType() != obj.GetType())
            {
                return false;
            }

            var omaShape = (OmaShape)obj;
            var count1 = omaShape.PolygonCoordinates.Count();
            var count2 = this.PolygonCoordinates.Count();

            if (count1 > 0 && count1 == count2)
            {
                return this.PolygonCoordinates.ToArray().SequenceEqual(omaShape.PolygonCoordinates.ToArray());
            }

            return false;
        }

        public override int GetHashCode()
        {
            return this.shape.GetHashCode();
        }

        #region Static Helpers

        public static OmaShape GetShape(string xmlData)
        {
            if (string.IsNullOrEmpty(xmlData))
                return null;
            var omaShape = new OmaShape(xmlData);
            if (OmaPolygon.CanCast(omaShape.shape))
                return new OmaPolygon(omaShape);
            if (OmaCircularArcArea.CanCast(omaShape.shape))
                return new OmaCircularArcArea(omaShape);
            if (OmaCircularArea.CanCast(omaShape.shape))
                return new OmaCircularArea(omaShape);
            return omaShape;
        }

        public static OmaShape CreateFromSourceLocationData(string sourceLocationData)
        {
            if (String.IsNullOrEmpty(sourceLocationData))
                return null;

            try
            {
                using (TextReader textReader = new StringReader(sourceLocationData))
                {
                    var doc = new XPathDocument(textReader);
                    XPathNavigator nav = doc.CreateNavigator();
                    var expr = nav.Compile("/Event/OMAMessage");
                    XPathNodeIterator iterator = nav.Select(expr);
                    if (iterator.Count != 0)
                    {
                        iterator.MoveNext();
                        if (iterator.Current != null && iterator.Current.Value != null)
                        {
                            return OmaShape.GetShape(iterator.Current.Value);
                        }
                    }
                }
            }
            catch (Exception)
            {
                // If we cannot properly parse the sourceLocationData, don't return a shape.
                return null;
            }
            return null;
        }

        /// <summary>
        /// Converts an ellipse into a polygon of points representing said ellipse
        /// </summary>
        /// <param name="longitude">the longitude of the center of the ellipse</param>
        /// <param name="latitude">the latitude of the center of the ellipse</param>
        /// <param name="majorRange">the range in meters of the long radius of the ellipse</param>
        /// <param name="minorRange">the range in meters of the shortest</param>
        /// <param name="angleOfMajorAxis">The angle of rotation for the major axis</param>
        /// <param name="anglesPerStep">the number of degrees to step by between every point</param>
        /// <returns>a list of points approximating an ellipse</returns>
        public static IEnumerable<Coordinate> EllipseToPolygon(double longitude, double latitude, double majorRange, double minorRange, double angleOfMajorAxis, double anglesPerStep)
        {
            var result = new List<Coordinate>();

            // from https://math.stackexchange.com/a/2205349
            double a = majorRange;
            double b = minorRange;
            double theta, distance, bCosTheta, aSinTheta, angleIncludingBaseShapeOrientation;

            // Walk the ellipse in steps
            for (double currentPointAngle = 0; currentPointAngle < 360; currentPointAngle += anglesPerStep)
            {
                theta = CoordinateTransformations.Deg2Rad(currentPointAngle );
                bCosTheta = b * Math.Cos(theta);
                aSinTheta = a * Math.Sin(theta);

                // this equation does NOT take into effect our positioning or rotation, that is handled below
                distance = (majorRange * minorRange) / Math.Sqrt(bCosTheta * bCosTheta + aSinTheta * aSinTheta);

                // now apply our rotation based on angleOfMajorAxis
                angleIncludingBaseShapeOrientation = (currentPointAngle + angleOfMajorAxis) % 360;
                result.Add(CoordinateTransformations.CalcLongLatFromGivenLongLatDistanceAndBearing(longitude, latitude, distance / 1000, angleIncludingBaseShapeOrientation));
            }

            result.Add(result.First());

            return result;
        }

        public static IEnumerable<Coordinate> SectorSliceToPolygon(double longitude, double latitude, double minAngle, double maxAngle, double minRange, double maxRange)
        {
            return SectorSliceToPolygon(longitude, latitude, minAngle, maxAngle, minRange, maxRange, 10);
        }

        public static IEnumerable<Coordinate> SectorSliceToPolygon(double longitude, double latitude, double minAngle, double maxAngle, double minRange, double maxRange, int iterationAngle)
        {
            var result = new List<Coordinate>();
            double angle = maxAngle > minAngle ? maxAngle - minAngle : 360 - (minAngle - maxAngle);
            if (angle > 360)
            {
                angle %= 360;
            }
            double direction = maxAngle - (angle / 2);
            if (minRange > 0)
            {
                result.Add(CoordinateTransformations.CalcLongLatFromGivenLongLatDistanceAndBearing(longitude, latitude, minRange / 1000, minAngle));
            }
            else
                if ((maxAngle - minAngle) % 360 != 0)
                {
                    result.Add(new Coordinate(latitude, longitude));
                }

            int iterations = (int)angle / iterationAngle;
            for (int i = 0; i < iterations; i++)
            {
                result.Add(CoordinateTransformations.CalcLongLatFromGivenLongLatDistanceAndBearing(longitude, latitude, maxRange / 1000, direction - angle / 2 + iterationAngle * i));
            };

            result.Add(CoordinateTransformations.CalcLongLatFromGivenLongLatDistanceAndBearing(longitude, latitude, maxRange / 1000, maxAngle));

            if (minRange > 0)
            {
                for (int i = iterations; i > 0; i--)
                {
                    result.Add(CoordinateTransformations.CalcLongLatFromGivenLongLatDistanceAndBearing(longitude, latitude, minRange / 1000, direction - angle / 2 + iterationAngle * i));
                };
            }

            // Close the polygon by duplicating the first point and adding it to the end of the list
            result.Add(result.First());

            return result;
        }

        #endregion
    }
}