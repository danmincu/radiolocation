using Mapping.ClipperLib;
using System.Collections.Generic;

namespace Mapping.Radio
{
    using Polygons = List<List<IntPoint>>;

    public static partial class RadioIntersection
    {
        public class RadioIntersectionResponse
        {
            public int Level { get; set; }
            public long TranslationX { get; set; }
            public long TranslationY { get; set; }
            public Polygons Intersection { get; set; }
            public Polygons Clips { get; set; }
        }
    }
}
