using System.Diagnostics.CodeAnalysis;

namespace Mapping
{
    /// <summary>
    /// Used to convert geographic coordinates into pixel coordinates. 
    /// Since the map width and height is different at each level, so are the pixel coordinates. 
    /// The pixel at the upper-left corner of the map always has pixel coordinates (0, 0). 
    /// The pixel at the lower-right corner of the map has pixel coordinates (width-1, height-1), 
    /// Refer to the following equation (256 * 2^level–1, 256 * 2^level–1). For example, at level 3, the pixel coordinates range from (0, 0) to (2047, 2047),
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Pixel
    {
        public long X { get; set; }
        public long Y { get; set; }

        public Pixel(long x, long y)
        {
            // Just in case someone tries to create a pixel less than 0, we will enforce 0,0 as the default.
            // Otherwise, the application will crash.
            X = (x >= 0) ? x : 0;
            Y = (y >= 0) ? y : 0;
        }
    }
}
