using Mapping.ClipperLib;
using Mapping.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mapping.Radio
{
    using Polygon = List<IntPoint>;
    using Polygons = List<List<IntPoint>>;

    public static partial class RadioIntersection
    {

        public static RadioIntersectionResponse GenerateRadioIntersection(List<RadioInfoGps> info, bool applyOffset = true)
        {
            if (info.FirstOrDefault()?.Latitude == null)
            {
                return null;
            }

            //at this point we should calculate the pixels per km as down the stack we won't have a latitude to refer to
            var pixelsPerKm = (int)(1000L / TileSystem.GroundResolution(info.FirstOrDefault().Latitude, level));

            return GenerateRadioIntersection(new RadioInfoGpsGroup(info).ToRadioInfoList(out long tx, out long ty), pixelsPerKm, tx, ty, applyOffset);
        }

        public static Coordinate CenterOfMass(Polygons polygons, int level, long translationX, long translationY)
        {
            var r = new IntRect(long.MaxValue, long.MaxValue, long.MinValue, long.MinValue);
            //compute bounding box
            foreach (var point in polygons.SelectMany(p => p))
            {
                if (r.left >= point.X)
                    r.left = point.X;
                if (r.right <= point.X)
                    r.right = point.X;

                if (r.top >= point.Y)
                    r.top = point.Y;
                if (r.bottom <= point.Y)
                    r.bottom = point.Y;
            }

            var centerCoordinateX = (long)r.left + (Math.Abs(r.right - r.left) / 2) + translationX;
            var centerCoordinateY = (long)r.top + (Math.Abs(r.bottom - r.top) / 2) + translationY;
            return TileSystem.PixelToCoordinate(new Pixel(centerCoordinateX, centerCoordinateY), level);

        }

        const int level = 18; //at this level you get approximately and on average 1.1943 meters per pixel
        const int paddingOnMap = 20000;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ri"></param>
        /// <returns></returns>
        private static Polygons CellCoverage(RadioInfo ri, int pixelsPerKilometer)
        {
            var rssiRanges = ri.Radio.Equals("lte", StringComparison.OrdinalIgnoreCase) ?
                   new int[][] {
                       new[] { -60, 150, 400 },
                       new[] { -70, 300, 400 },
                       new[] { -80, 600, 400 },
                       new[] { -90, 1200, 800 },
                       new[] { -100, 2400, 1200 },
                       new[] { -110, 4800, 2000 },
                       new[] { -120, 9600, 3000 } } :

                   new int[][] {
                        new[] { -60, 200, 200 },
                        new[] { -70, 600, 300 },
                        new[] { -80, 800, 400 },
                        new[] { -90, 1000, 400 },
                        new[] { -100, 1300, 500 },
                        new[] { -106, 1430, 500 },
                        new[] { -107, 1800, 500 }, // << 
                        new[] { -108, 2450, 600 },
                        new[] { -109, 3000, 600 }, // <<
                        new[] { -110, 3500, 700 },
                        new[] { -111, 4000, 800 }, // <<
                        new[] { -112, 4850, 1000 },
                        new[] { -113, 6000, 1500 },
                        new[] { -114, 10000, 4000 },
                        new[] { -120, 25600, 5000 }

                   };

            var innerRange = 0;
            var outerRange = rssiRanges.Last()[1];

            foreach (var item in rssiRanges)
            {
                if (ri.Rssi <= item[0])
                {
                    innerRange = item[1] - item[2];
                    outerRange = item[1] + item[2];
                }
            }

            outerRange = (int)Math.Truncate(outerRange * ((double)pixelsPerKilometer / 1000));
            innerRange = (int)Math.Truncate(innerRange * ((double)pixelsPerKilometer / 1000));

            return CellCoverage(ri.CenterX, ri.CenterY, innerRange * 2, outerRange * 2);
        }

        /// <summary>
        /// 
        /// </summary>i
        /// <param name="radio"></param>
        /// <param name="rssi"></param>
        /// <param name="centerX"></param>
        /// <param name="centerY"></param>
        /// <returns></returns>
        static Polygons CellCoverage(long centerX, long centerY, long innerRange, long outerRange)
        {
            //this should be 1; greater values for display purposes
            var intScale = 1;
            using (System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath())
            {
                path.Reset();
                path.AddEllipse(centerX - (int)(outerRange / 2), centerY - (int)(outerRange / 2), outerRange, outerRange);

                path.Flatten();
                Polygon elipse = new Polygon(path.PathPoints.Count());
                foreach (System.Drawing.PointF p in path.PathPoints)
                    elipse.Add(new IntPoint((int)(p.X * intScale), (int)(p.Y * intScale)));

                var tempSubjects = new Polygons();

                tempSubjects.Add(elipse);

                var tempClips = new Polygons();

                //donught 
                path.Reset();
                path.AddEllipse(centerX - (int)(innerRange / 2), centerY - (int)(innerRange / 2), innerRange, innerRange);
                path.Flatten();
                var in_elipse = new Polygon(path.PathPoints.Count());
                foreach (System.Drawing.PointF p in path.PathPoints)
                    in_elipse.Add(new IntPoint((int)(p.X * intScale), (int)(p.Y * intScale)));
                tempClips.Add(in_elipse);

                var clipper = new ClipperLib.Clipper();
                clipper.AddPaths(tempSubjects, PolyType.ptSubject, true);
                clipper.AddPaths(tempClips, PolyType.ptClip, true);
                List<List<IntPoint>> sol1 = new Polygons();
                clipper.Execute(ClipType.ctXor, sol1, PolyFillType.pftEvenOdd, PolyFillType.pftEvenOdd);

                return sol1;
            }
        }

        private class RadioInfoGpsGroup
        {
            public RadioInfoGpsGroup(IEnumerable<RadioInfoGps> list)
            {
                this.list = list;
            }

            IEnumerable<RadioInfoGps> list;

            //, out double metersH, out double metersV
            public List<RadioInfo> ToRadioInfoList(out long translationX, out long translationY)
            {
                var radios = from rigps in list ?? Enumerable.Empty<RadioInfoGps>()
                             let xy = TileSystem.CoordinateToPixel(new Mapping.Coordinate(rigps.Latitude, rigps.Longitude), level)
                             select new RadioInfo(rigps.Radio, rigps.Rssi, xy.X, xy.Y);

                var tX = radios.Any() ? radios.Min(r => r.CenterX) - paddingOnMap : 0;
                var tY = radios.Any() ? radios.Min(r => r.CenterY) - paddingOnMap : 0;

                translationX = tX;
                translationY = tY;

                var adjustedRadios = radios.Select(r =>
                    new RadioInfo(
                        r.Radio,
                        r.Rssi,
                        r.CenterX - tX,
                        r.CenterY - tY)).ToList();

                return adjustedRadios;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        internal class RadioInfo
        {
            public RadioInfo(string radio, int rssi, long centerX, long centerY)
            {
                this.Radio = radio;
                this.Rssi = rssi;
                this.CenterX = centerX;
                this.CenterY = centerY;
            }

            public string Radio { get; set; }
            public int Rssi { get; set; }
            public long CenterX { get; set; }
            public long CenterY { get; set; }

            public override string ToString()
            {
                return string.Format($"{this.Radio} {this.Rssi} x:{this.CenterX} y:{this.CenterY}");
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="radios"></param>
        /// <param name="justIntersection"></param>
        /// <returns></returns>
        static RadioIntersectionResponse GenerateRadioIntersection(List<RadioInfo> radios, int pixelsPerKm, long translationX, long translationY, bool applyOffset)
        {
            var subjects = new Polygons();
            radios.Reverse();

            var clipper_offset = 700;

            var stack = new Stack<RadioInfo>(radios);

            Polygons intersection = CellCoverage(stack.Pop(), pixelsPerKm);

            while (stack.Count > 0)
            {
                List<List<IntPoint>> sol1 = new Polygons();
                var clipper = new Clipper();
                clipper.AddPaths(intersection, PolyType.ptSubject, true);
                clipper.AddPaths(CellCoverage(stack.Pop(), pixelsPerKm), PolyType.ptClip, true);
                var previousIntersection = intersection.ToList();
                clipper.Execute(ClipType.ctIntersection, intersection, PolyFillType.pftEvenOdd, PolyFillType.pftEvenOdd);

                //prevent getting nothing out of the intersection
                //the current radio shape is being dismissed
                if (intersection.Count == 0)
                    intersection = previousIntersection;
            }

            if (applyOffset)
            {
                Polygons clippedIntersection = new Polygons();
                ClipperOffset co = new ClipperOffset();
                co.AddPaths(intersection, JoinType.jtRound, EndType.etClosedPolygon);
                co.Execute(ref clippedIntersection, clipper_offset);
                intersection = clippedIntersection;
            }

            return new RadioIntersectionResponse
            {
                Level = level,
                TranslationX = translationX,
                TranslationY = translationY,
                Intersection = intersection,
                Clips = radios.SelectMany(c => CellCoverage(c, pixelsPerKm)).ToList()
            };
        }
    }
}
