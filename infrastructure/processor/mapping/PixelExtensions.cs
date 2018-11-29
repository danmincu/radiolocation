using Mapping.Geometry;

namespace Mapping
{
    public static class PixelExtensions
    {
        public static Pixel Translate(this Pixel pixel, Point point)
        {
            return new Pixel(pixel.X + (int)point.X, pixel.Y + (int)point.Y);
        }

        public static Point ProjectionToRect(this Pixel pixel, Pixel bottomLeft, Pixel topRight)
        {
            return new Point(pixel.X - bottomLeft.X, pixel.Y - topRight.Y);
        }

    }
}
