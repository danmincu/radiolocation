using Mapping.Geometry;
using Mapping.Mapping;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Mapping
{

    /// <summary>  
    /// Converts a coordinate from latitude/longitude WGS-84 coordinates (in degrees) 
    /// into a QuadKey (and visa versa) at a specified level of detail.
    /// <seealso cref="http://msdn.microsoft.com/en-us/library/bb259689.aspx "/>
    /// </summary>
    /// <remarks>
    /// To optimize the performance of map retrieval and display, the rendered map is cut into tiles of 256 x 256 pixels each. 
    /// As the number of pixels differs at each level of detail, so does the number of tiles: map width = map height = 2^level * tiles 
    /// </remarks>
    public static class TileSystem
    {
        private static class MercatorProjection
        {
            private static readonly double scaleX;
            private static readonly double scaleY;
            private static readonly double offsetX;
            private static readonly double offsetY;

            static MercatorProjection()
            {
                scaleX = 0.15915494309189535;
                scaleY = -0.15915494309189535;
                offsetX = 0.5;
                offsetY = 0.5;
            }

            public static Point Project(Point latLong)
            {
                var num = Math.Sin(latLong.Y * 0.017453292519943295);
                return new Point(((latLong.X * 0.017453292519943295) * scaleX) + offsetX, ((0.5 * Math.Log((1.0 + num) / (1.0 - num))) * scaleY) + offsetY);
            }

            public static Point Unproject(Point point)
            {
                return new Point(((point.X - offsetX) * 57.295779513082323) / scaleX, Math.Atan(Math.Sinh((point.Y - offsetY) / scaleY)) * 57.295779513082323);
            }

        }
        
        public const double EarthRadius = 6378137;
        private static Coordinate minLocation = new Coordinate(-85.0511287798066D, -180D);
        private static Coordinate maxLocation = new Coordinate(85.0511287798066D, 180D);
        private static readonly Regex quadKeyRegex = new Regex(@"^[0123]+$", RegexOptions.Compiled);

        /// <summary>
        /// This value is used to set the Map size
        /// A value of 25 will work with lat/longs up to the 8 decimal place
        /// </summary>
        public const int MaxDetailLevel = 30;

        /// <summary>
        /// Mapping starts at zoom level of 1
        /// </summary>
        private const int MinDetailLevel = 1;

        /// <summary>
        /// Gets a cloned copy of the min location.
        /// </summary>
        /// <value>The min location.</value>
        public static Coordinate MinLocation { get { return new Coordinate(minLocation.Latitude, minLocation.Longitude); } }
        /// <summary>
        /// Gets a cloned copy of the max location.
        /// </summary>
        /// <value>The max location.</value>
        public static Coordinate MaxLocation { get { return new Coordinate(maxLocation.Latitude, maxLocation.Longitude); } }

        /// <summary>
        /// Converts a coordinate into absolute Mercator Projection view port values. the range -1 to 1 for both x and y viewport axes.
        /// http://en.wikipedia.org/wiki/Mercator_projection
        /// this function is the inverse of ViewportToCoordinate
        /// </summary>
        /// <param name="coordinate">the input coordinate defined by its latitude and longitude</param>
        /// <returns>the point in the -1 to 1 x and y range</returns>
        public static Point CoordinateToViewport(Coordinate coordinate)
        {
            return MercatorProjection.Project(new Point(coordinate.Longitude, coordinate.Latitude));
        }

        /// <summary>
        /// Converts a Mercator Projection view port point (in the range of -1 to 1 for the x and y axes) into an absolute coordinate
        /// http://en.wikipedia.org/wiki/Mercator_projection
        /// this function is the inverse of CoordinateToViewport
        /// </summary>
        /// <param name="point">the point in the -1 to 1 x and y range</param>
        /// <returns>return the coordinate defined by its latitude and longitude</returns>
        public static Coordinate ViewportToCoordinate(Point point)
        {
            var coordPoint = MercatorProjection.Unproject(point);
            return new Coordinate(coordPoint.Y, coordPoint.X);
        }

        /// <summary>
        /// Coordinates to quad key.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="levelOfDetail">The level of detail.</param>
        /// <returns></returns>
        public static QuadKey CoordinateToQuadKey(Coordinate location, int levelOfDetail)
        {
            return CoordinateToQuadKey(location, levelOfDetail, false);
        }

        /// <summary>
        /// Coordinates to quad key.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="levelOfDetail">The level of detail.</param>
        /// <param name="useLinearQuadkeys">Whether to use a linear quadkey (where lats go to 90 degrees and are divided evenly) or not (where lats go to ~85 and QKs get bigger as you get to the poles)</param>
        /// <returns></returns>
        public static QuadKey CoordinateToQuadKey(Coordinate location, int levelOfDetail, bool useLinearQuadkeys)
        {
            ArgumentValidation.CheckArgumentForNull<Coordinate>(location, "location");

            if (!IsValidLevelOfDetail(levelOfDetail))
                throw new MappingException("Invalid level of detail: " + levelOfDetail.ToString(CultureInfo.InvariantCulture));
            // increase level for increased accuracy
            var tile = CoordinateToTile(location, MaxDetailLevel + 1, useLinearQuadkeys);
            // truncate quad key to requested level
            var key = TileSystem.TileToQuadKey(tile, MaxDetailLevel + 1).Key.Substring(0, levelOfDetail);

            return new QuadKey(key);
        }

        public static Coordinate QuadKeyToCoordinate(string quadkey)
        {
            int levelOfDetail;
            var tile = QuadKeyToTile(new QuadKey(quadkey), out levelOfDetail);
            var pixel = TileToPixel(tile);
            var coord = PixelToCoordinate(pixel, levelOfDetail);
            return coord;
        }

        /// <summary>
        /// Quads the key to lat long.
        /// </summary>
        /// <param name="quadKey">The quad key.</param>      
        public static Quadrangle QuadKeyToQuadrangle(QuadKey quadKey)
        {
            ArgumentValidation.CheckArgumentForNull<QuadKey>(quadKey, "quadKey");

            if (!IsValidQuadKey(quadKey.Key))
                throw new MappingException("Invalid QuadKey: " + quadKey.Key);

            int levelOfDetail;

            var tile = TileSystem.QuadKeyToTile(quadKey, out levelOfDetail);
            var pixel = TileSystem.TileToPixel(tile);

            Coordinate topLeft = TileSystem.PixelToCoordinate(pixel, levelOfDetail);
            // Move (+1,+1) to find the next diagonal tile, and adjust for wrapping            
            long nextTileX = (tile.X + 1 == 1 << levelOfDetail) ? 0 : tile.X + 1;
            long nextTileJ = (tile.Y + 1 == 1 << levelOfDetail) ? 0 : tile.Y + 1;

            var bottomRightTile = new Tile(nextTileX, nextTileJ);
            var bottomRightPixel = TileSystem.TileToPixel(bottomRightTile);

            Coordinate bottomRightDefault = TileSystem.PixelToCoordinate(bottomRightPixel, levelOfDetail);
            // @ Level 1 -- A topLeft of (85, 0) expects the bottomRight to be (0, 180)
            // But because of wrap we get the start of the next tile (0, -180), therefore we adjust the sign
            var bottomRightFinalLongitude = (nextTileX == 0) ? bottomRightDefault.Longitude * -1 : bottomRightDefault.Longitude;
            var bottomRightFinalLatitude = (nextTileJ == 0) ? bottomRightDefault.Latitude * -1 : bottomRightDefault.Latitude;
            return Quadrangle.ReversedQuadrangle(topLeft, new Coordinate(bottomRightFinalLatitude, bottomRightFinalLongitude));

        }

        /// <summary>
        /// Sets the quad key neighbours for the supplied QuadKey. 
        /// Neighbors outside of the North/South lattitudes will be set to null
        /// </summary>
        /// <param name="quadKey">The quad key.</param>
        public static Dictionary<QuadKeyLocation, QuadKey?> GetQuadKeyNeighbours(QuadKey quadKey)
        {
            ArgumentValidation.CheckArgumentForNull<QuadKey>(quadKey, "quadKey");

            if (!IsValidQuadKey(quadKey.Key))
                throw new MappingException("Invalid QuadKey: " + quadKey.Key);

            Dictionary<QuadKeyLocation, QuadKey?> result = new Dictionary<QuadKeyLocation, QuadKey?>();

            int levelOfDetail;
            var tile = QuadKeyToTile(quadKey, out levelOfDetail);
            long mapEdge = (1 << levelOfDetail) - 1;

            // Location enum is a horizontal matrix starting at NorthWest
            int neighbourLocation = 0;
            // Process longitude, then latitude
            for (int y = (int)tile.Y - 1; y <= tile.Y + 1; y++)
            {
                bool invalidLongitude = false;
                long nextTileY = y;
                // Accounting for north and south poles (cut off at +-85 degrees)
                if (y > mapEdge || y < 0)
                    invalidLongitude = true;

                for (int x = (int)tile.X - 1; x <= tile.X + 1; x++)
                {
                    // Accounting for horizontal wrapping
                    long nextTileX = (x > mapEdge) ? 0 : x;
                    nextTileX = (nextTileX < 0) ? (mapEdge) : nextTileX;
                    var location = (QuadKeyLocation)Enum.ToObject(typeof(QuadKeyLocation), neighbourLocation);
                    QuadKey? neighbour = null;

                    if (!invalidLongitude)
                    {
                        var tempTile = new Tile(nextTileX, nextTileY);
                        // increase level for increased accuracy
                        var key = TileSystem.TileToQuadKey(tempTile, levelOfDetail).Key;//MaxDetailLevel + 1).Key.Substring(0, levelOfDetail);
                        neighbour = new QuadKey(key);
                    }
                    result.Add(location, neighbour);
                    neighbourLocation++;
                }
            }
            return result;
        }

        /// <summary>
        /// Determines whether [is valid level of detail] [the specified level of detail].
        /// </summary>
        /// <param name="levelOfDetail">The level of detail.</param>
        /// <returns>
        /// 	<c>true</c> if [is valid level of detail] [the specified level of detail]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsValidLevelOfDetail(int levelOfDetail)
        {
            if (levelOfDetail > MaxDetailLevel)
                return false;

            if (levelOfDetail < MinDetailLevel)
                return false;

            return true;
        }

        /// <summary>
        /// Determines whether [is valid quad key] [the specified quad key].
        /// </summary>
        /// <param name="quadKey">The quad key.</param>
        /// <returns>
        /// 	<c>true</c> if [is valid quad key] [the specified quad key]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsValidQuadKey(string quadKey)
        {
            Match result = quadKeyRegex.Match(quadKey);
            return result.Success;
        }

        /// <summary>
        /// Determines the map scale at a specified latitude, level of detail,
        /// and screen resolution.
        /// </summary>
        /// <param name="latitude">Latitude (in degrees) at which to measure the
        /// map scale.</param>
        /// <param name="levelOfDetail">Level of detail, from 1 (lowest detail)
        /// to 23 (highest detail).</param>
        /// <param name="screenDpi">Resolution of the screen, in dots per inch.</param>
        /// <returns>The map scale, expressed as the denominator N of the ratio 1 : N.</returns>
        public static double MapScale(double latitude, int levelOfDetail, int screenDpi)
        {
            if (!IsValidLevelOfDetail(levelOfDetail))
                throw new MappingException("Invalid levelOfDetail: " + levelOfDetail.ToString(CultureInfo.InvariantCulture));

            return GroundResolution(latitude, levelOfDetail) * screenDpi / 0.0254;
        }


        public static IEnumerable<QuadKey> GetQuadkeysFromQuadrangle(Quadrangle quadrangle)
        {
            var levelOfDetail = Math.Min((int)quadrangle.GetViewportZoomLevel(), MaxDetailLevel);
            var topLeftTile = CoordinateToTile(quadrangle.TopLeft, levelOfDetail, false);
            var bottomRightTile = CoordinateToTile(quadrangle.BottomRight, levelOfDetail, false);
            for (long x = topLeftTile.X; x <= bottomRightTile.X; x++)
            {
                for (long y = topLeftTile.Y; y <= bottomRightTile.Y; y++)
                {
                    yield return TileToQuadKey(new Tile(x, y), levelOfDetail);
                }
            }
        }

        public static Quadrangle ClipToMercatorVisible(Quadrangle quadrangle)
        {
            return new Quadrangle(new Coordinate(Math.Max(TileSystem.minLocation.Latitude, quadrangle.BottomLeft.Latitude), quadrangle.BottomLeft.Longitude),
                new Coordinate(Math.Min(TileSystem.maxLocation.Latitude, quadrangle.TopRight.Latitude), quadrangle.TopRight.Longitude));
        }

        public static Quadrangle MercatorQuadrangle
        {
            get
            {
                return new Quadrangle(MinLocation, MaxLocation);
            }
        }


        /// <summary>
        /// Clips a number to the specified minimum and maximum values.
        /// </summary>
        /// <param name="n">The number to clip.</param>
        /// <param name="minValue">Minimum allowable value.</param>
        /// <param name="maxValue">Maximum allowable value.</param>
        /// <returns>The clipped value.</returns>
        private static double Clip(double n, double minValue, double maxValue)
        {
            return Math.Min(Math.Max(n, minValue), maxValue);
        }

        /// <summary>
        /// Determines the map width and height (in pixels) at a specified level
        /// of detail.
        /// </summary>
        /// <param name="levelOfDetail">Level of detail, from 1 (lowest detail)
        /// to 23 (highest detail).</param>
        /// <returns>The map width and height in pixels.</returns>
        private static long MapSize(int levelOfDetail)
        {
            return (long)256 << levelOfDetail;
        }

        /// <summary>
        /// Determines the ground resolution (in meters per pixel) at a specified
        /// latitude and level of detail.
        /// </summary>
        /// <param name="latitude">Latitude (in degrees) at which to measure the
        /// ground resolution.</param>
        /// <param name="levelOfDetail">Level of detail, from 1 (lowest detail)
        /// to 23 (highest detail).</param>
        /// <returns>The ground resolution, in meters per pixel.</returns>
        public static double GroundResolution(double latitude, int levelOfDetail)
        {
            latitude = Clip(latitude, minLocation.Latitude, maxLocation.Latitude);
            return Math.Cos(latitude * Math.PI / 180) * 2 * Math.PI * EarthRadius / MapSize(levelOfDetail);
        }


        /// <summary>
        /// Converts a point from latitude/longitude WGS-84 coordinates (in degrees)
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="levelOfDetail">Level of detail, from 1 (lowest detail)
        /// <param name="useLinearQuadkeys">Whether to use linear quadkeys or mercator quadkeys.</param>
        /// to 23 (highest detail).</param>
        /// <returns>XY coordinate in pixels.</returns>
        public static Pixel CoordinateToPixel(Coordinate location, int levelOfDetail, bool useLinearQuadkeys = false)
        {
            ArgumentValidation.CheckArgumentForNull<Coordinate>(location, "location");
            double x, y;

            if (useLinearQuadkeys)
            {
                x = (location.Longitude + 180) / 360;
                y = (90 - location.Latitude) / 180;
            }
            else
            {
                double latitude = Clip(location.Latitude, minLocation.Latitude, maxLocation.Latitude);
                double longitude = Clip(location.Longitude, minLocation.Longitude, maxLocation.Longitude);

                double sinLatitude = Math.Sin(latitude * Math.PI / 180);
                x = (longitude + 180) / 360;
                y = 0.5 - Math.Log((1 + sinLatitude) / (1 - sinLatitude)) / (4 * Math.PI);
            }

            long mapSize = MapSize(levelOfDetail);
            long pixelX = (long)Clip(x * mapSize + 0.5, 0, mapSize - 1);
            long pixelY = (long)Clip(y * mapSize + 0.5, 0, mapSize - 1);

            return new Pixel(pixelX, pixelY);
        }

        /// <summary>
        /// Converts a pixel from pixel XY coordinates at a specified level of detail
        /// into latitude/longitude WGS-84 coordinates (in degrees).
        /// </summary>
        /// <param name="pixel">XY coordinates of the point, in pixels.</param>
        /// <param name="levelOfDetail">Level of detail, from 1 (lowest detail)
        /// to 23 (highest detail).</param>
        /// <returns>Coordinate in degrees</returns>
        public static Coordinate PixelToCoordinate(Pixel pixel, int levelOfDetail)
        {
            ArgumentValidation.CheckArgumentForNull<Pixel>(pixel, "pixel");

            double mapSize = MapSize(levelOfDetail);
            double x = (Clip(pixel.X, 0, mapSize - 1) / mapSize) - 0.5;
            double y = 0.5 - (Clip(pixel.Y, 0, mapSize - 1) / mapSize);

            double latitude = 90 - 360 * Math.Atan(Math.Exp(-y * 2 * Math.PI)) / Math.PI;
            double longitude = 360 * x;

            return new Coordinate(latitude, longitude);
        }


        /// <summary>
        /// Converts pixel XY coordinates into tile XY coordinates of the tile containing
        /// the specified pixel.
        /// </summary>
        /// <param name="pixel">The pixel.</param>
        /// <returns>The Tile</returns>
        public static Tile PixelToTile(Pixel pixel)
        {
            ArgumentValidation.CheckArgumentForNull<Pixel>(pixel, "pixel");

            return new Tile((int)(pixel.X / 256), (int)(pixel.Y / 256));
        }


        /// <summary>
        /// Converts tile XY coordinates into pixel XY coordinates of the upper-left pixel
        /// of the specified tile.
        /// </summary>
        /// <param name="tile">The tile.</param>
        /// <returns></returns>
        public static Pixel TileToPixel(Tile tile)
        {
            ArgumentValidation.CheckArgumentForNull<Tile>(tile, "tile");

            return new Pixel(tile.X * 256, tile.Y * 256);
        }


        /// <summary>
        /// Converts tile XY coordinates into a QuadKey at a specified level of detail.
        /// </summary>
        /// <param name="tile">Tile XY coordinate.</param>
        /// <param name="levelOfDetail">Level of detail, from 1 (lowest detail)
        /// to 23 (highest detail).</param>
        /// <returns>A string containing the QuadKey.</returns>
        private static QuadKey TileToQuadKey(Tile tile, int levelOfDetail)
        {
            ArgumentValidation.CheckArgumentForNull<Tile>(tile, "tile");

            var quadKey = new StringBuilder();
            for (int i = levelOfDetail; i > 0; i--)
            {
                char digit = '0';
                long mask = 1 << (i - 1);
                if ((tile.X & mask) != 0)
                {
                    digit++;
                }
                if ((tile.Y & mask) != 0)
                {
                    digit++;
                    digit++;
                }
                quadKey.Append(digit);
            }

            if (!IsValidQuadKey(quadKey.ToString()))
                throw new MappingException("Generated invalid quad key: " + quadKey.ToString());

            return new QuadKey(quadKey.ToString());
        }



        /// <summary>
        /// Converts a QuadKey into tile XY coordinates.
        /// </summary>
        /// <param name="quadKey">QuadKey of the tile.</param>
        /// <param name="levelOfDetail">Output parameter receiving the level of detail.</param>
        /// <returns>The Tile</returns>
        private static Tile QuadKeyToTile(QuadKey quadKey, out int levelOfDetail)
        {
            Tile tile = new Tile(0, 0);
            levelOfDetail = quadKey.Key.Length;
            for (int i = levelOfDetail; i > 0; i--)
            {
                long mask = 1 << (i - 1);
                switch (quadKey.Key[levelOfDetail - i])
                {
                    case '0':
                        break;

                    case '1':
                        tile.X |= mask;
                        break;

                    case '2':
                        tile.Y |= mask;
                        break;

                    case '3':
                        tile.X |= mask;
                        tile.Y |= mask;
                        break;

                    default:
                        throw new MappingException("Invalid QuadKey digit sequence: " + quadKey.Key[levelOfDetail - i]);
                }
            }

            return tile;
        }


        /// <summary>
        /// Toes the tile.
        /// </summary>
        /// <param name="quadKey">The quad key.</param>
        /// <returns></returns>
        public static Tile ToTile(this QuadKey quadKey)
        {
            int levelOfDetail;
            return QuadKeyToTile(quadKey, out levelOfDetail);
        }

        /// <summary>
        /// Converts a location from latitude/longitude WGS-84 coordinates (in degrees) 
        /// into tile coordinate (X,Y) a specified level of detail.
        /// </summary>
        /// <param name="location">Latitude and Longitude of the point, in degrees.</param>
        /// <param name="levelOfDetail">Level of detail, from 1 (lowest detail)
        /// <param name="useLinearQuadkeys">Whether to use linear or mercator quadkeys.</param>
        /// to 23 (highest detail).</param>
        /// <returns>The tile</returns>
        public static Tile CoordinateToTile(Coordinate location, int levelOfDetail, bool useLinearQuadkeys)
        {
            ArgumentValidation.CheckArgumentForNull<Coordinate>(location, "location");
            IsValidLevelOfDetail(levelOfDetail);

            // Convert lat/long to pixel (X,Y) for the given zoomlevel.  This assumes that the whole Earth is
            // present in the image and pixel (0,0) the top left pixel in the image.
            var pixel = TileSystem.CoordinateToPixel(location, levelOfDetail, useLinearQuadkeys);

            // Convert pixel (X,Y) coords to XY coords of tile containing this pixel (at the zoomlevel)
            // Tile (0,0) is the top right tile in the image containing all tiles (the whole Earth map) 
            // at the current zoom level.
            return TileSystem.PixelToTile(pixel);
        }

        /// <summary> 
        /// Converts Pixel (i,j) within Tile (X,Y) at specified level of detail to latitude/longitude WGS-84 
        /// coordinates (in degrees) where pixel (i,j) is the pixel coords with respect to the tile,
        /// and the North-West corner of the tile is pixel (0,0).
        /// </summary>
        /// <returns>The coordinate</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private static Coordinate PixelAndTileToCoordinate(Pixel pixel, Tile tile, int levelOfDetail)
        {
            ArgumentValidation.CheckArgumentForNull<Pixel>(pixel, "pixel");
            ArgumentValidation.CheckArgumentForNull<Tile>(tile, "tile");
            IsValidLevelOfDetail(levelOfDetail);

            long pixelX = tile.X * 256 + pixel.X;
            long pixelY = tile.Y * 256 + pixel.Y;

            return TileSystem.PixelToCoordinate(pixel, levelOfDetail);
        }

        /// <summary>
        /// Clips a coordinate so that it falls with the acceptable minumum and maximum lat/longs.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <returns></returns>
        public static Coordinate ClipToRange(double latitude, double longitude)
        {
            return new Coordinate(
                    Clip(latitude, MinLocation.Latitude,MaxLocation.Latitude),
                    Clip(longitude, MinLocation.Longitude,MaxLocation.Longitude));
        }

    }
}
