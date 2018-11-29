using Mapping.Mapping;
using Mapping.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mapping.Geometry
{
    public static class GeographicShapeExtensions
    {
        /// <summary>
        /// Converts the provided <see cref="GeographicShape"/> to list of  an ESRI <see cref="Coordinate"/> instance. If 
        /// the given shape is not supported, the returned polygon will be <c>null</c>.
        /// </summary>
        public static List<Coordinate> ToCoordinatesList(this GeographicShape shape)
        {
            List<Coordinate> points;

            if (shape is Circle)
                points = GetCoordinates((Circle)shape);
            else if (shape is Annulus)
                points = GetCoordinates((Annulus)shape);
            else if (shape is Ellipse)
                points = GetCoordinates((Ellipse)shape);
            else if (shape is UncertaintyPolygon)
                points = GetCoordinates((UncertaintyPolygon)shape);
            else
                points = null;

            return points;
        }

        private static List<Coordinate> GetCoordinates(UncertaintyPolygon model)
        {
            var pc = new List<Coordinate>(model.Points);

            // if the polygon isn't closed, close it be going back to the first point
            if (!pc.First().Equals(pc.Last()))
            {
                pc.Add(pc.First());
            }

            return pc;
        }


        private static List<Coordinate> GetCoordinates(Ellipse model)
        {
            var coordinates = OmaShape.EllipseToPolygon(
                model.Longitude,
                model.Latitude,
                model.UncertaintySemiMajor,
                model.UncertaintySemiMinor,
                model.OrientationOfMajorAxis,
                GetAngleGranularity(model.UncertaintySemiMajor));

            return coordinates.ToList();
        }

        private static List<Coordinate> GetCoordinates(Circle model)
        {
            var coordinates = OmaShape.SectorSliceToPolygon(
                model.Longitude,
                model.Latitude,
                0,
                360,
                0,
                model.Radius,
                GetAngleGranularity(model.Radius));

            return coordinates.ToList();
        }

        private static List<Coordinate> GetCoordinates(Annulus model)
        {
            var coordinates = OmaShape.SectorSliceToPolygon(
                model.Longitude,
                model.Latitude,
                model.OffsetAngle,
                model.OffsetAngle + model.IncludedAngle,
                model.InnerRadius,
                model.OuterRadius,
                GetAngleGranularity(model.OuterRadius));

            return coordinates.ToList();
        }

        /// <summary>
        /// Calculates at which sweep angle a point should be placed when drawing circular shapes. This takes into account how big the shape is, as larger
        /// shapes should be drawn with tighter points.
        /// </summary>
        private static int GetAngleGranularity(double radius)
        {
            // MAH: Before you ask, no, there is no magic to these numbers. They were picked after some experimenting with rendering of the uncertainty annulus
            // sectors to come up with numbers that worked for the smaller shapes.
            const double maxIterationAngle = 12;
            const double scalingFactor = 1000;

            // In general, the smaller the shape, the greater the number of lines required to make it look smooth.
            // MAH: Astute reader, you may be saying that zoom level comes into play, too. And you'd be right. However, in 4SE, by the time you zoom out far
            // enough to make larger shapes look small, they have been filtered off by the range-based filtering options. If this changes, this calculation will
            // have to changed to take range (zoom) into account.
            return (int)Math.Min(Math.Ceiling(scalingFactor / Math.Abs(radius)), maxIterationAngle);
        }
    }

}
