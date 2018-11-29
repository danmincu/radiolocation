using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Mapping.Geometry;
using Mapping.Mapping;

namespace Mapping
{

    /// <summary>  
    /// Converts a coordinate from latitude/longitude WGS-84 coordinates (in degrees) 
    /// into a "ABCD" format QuadKey that covers the entire -90, -180 to 90, 180 
    /// </summary>
    /// <remarks>
    /// To optimize the performance of map retrieval and display, the rendered map is cut into tiles of tilePixelSize x tilePixelSize pixels each. 
    /// As the number of pixels differs at each level of detail, so does the number of tiles: map width = map height = 2^level * tiles 
    /// </remarks>
    public static class Spatial4JTileSystem
    {
        const int tilePixelSize = 256;

        private static class LinearDecimalProjection
        {
            public static Point Project(Point latLong)
            {
                return new Point((latLong.X + 180) / 360, (90 - latLong.Y) / 180);
            }

            public static Point Unproject(Point point)
            {
                return new Point(point.X * 360 - 180, 90 - point.Y * 180);
            }
        }

        private const double MinLatitude = -90D;
        private const double MaxLatitude = 90D;
        private const double MinLongitude = -180D;
        private const double MaxLongitude = 180D;

        private static Coordinate minLocation = new Coordinate(MinLatitude, MinLongitude);
        private static Coordinate maxLocation = new Coordinate(MaxLatitude, MaxLongitude);
        private static readonly Regex quadKeyRegex = new Regex(@"^[ABCD]+$", RegexOptions.Compiled);


        /// This value is used to set the Map size
        /// A value of 25 will work with lat/longs up to the 8 decimal place
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
        /// to 30 (highest detail).</param>
        /// <returns>The map width and height in pixels.</returns>
        public static long MapSize(int levelOfDetail)
        {
            return (long)tilePixelSize << levelOfDetail;
        }


        /// <summary>
        /// Converts a point from latitude/longitude WGS-84 coordinates (in degrees)
        /// into pixel XY coordinates at a specified level of detail.
        /// </summary>
        /// <param name="latitude">Latitude of the point, in degrees.</param>
        /// <param name="longitude">Longitude of the point, in degrees.</param>
        /// <param name="levelOfDetail">Level of detail, from 1 (lowest detail)
        /// to 30 (highest detail).</param>
        /// <param name="pixelX">Output parameter receiving the X coordinate in pixels.</param>
        /// <param name="pixelY">Output parameter receiving the Y coordinate in pixels.</param>
        public static void LatLongToPixelXY(double latitude, double longitude, int levelOfDetail, out int pixelX, out int pixelY)
        {
            latitude = Clip(latitude, MinLatitude, MaxLatitude);
            longitude = Clip(longitude, MinLongitude, MaxLongitude);

            var x = (longitude + 180) / 360;
            var y = (90 - latitude) / 180;

            var mapSize = MapSize(levelOfDetail);
            pixelX = (int)Clip(x * mapSize + 0.5, 0, mapSize - 1);
            pixelY = (int)Clip(y * mapSize + 0.5, 0, mapSize - 1);
        }


        /// <summary>
        /// Converts a pixel from pixel XY coordinates at a specified level of detail
        /// into latitude/longitude WGS-84 coordinates (in degrees).
        /// </summary>
        /// <param name="pixelX">X coordinate of the point, in pixels.</param>
        /// <param name="pixelY">Y coordinates of the point, in pixels.</param>
        /// <param name="levelOfDetail">Level of detail, from 1 (lowest detail)
        /// to 30 (highest detail).</param>
        /// <param name="latitude">Output parameter receiving the latitude in degrees.</param>
        /// <param name="longitude">Output parameter receiving the longitude in degrees.</param>
        public static void PixelXYToLatLong(int pixelX, int pixelY, int levelOfDetail, out double latitude, out double longitude)
        {
            double mapSize = MapSize(levelOfDetail);
            var x = (Clip(pixelX, 0, mapSize - 1) / mapSize) - 0.5;
            var y = 0.5 - (Clip(pixelY, 0, mapSize - 1) / mapSize);

            latitude = 180 * y;
            longitude = 360 * x;
        }

        /// <summary>
        /// Converts pixel XY coordinates into tile XY coordinates of the tile containing
        /// the specified pixel.
        /// </summary>
        /// <param name="pixelX">Pixel X coordinate.</param>
        /// <param name="pixelY">Pixel Y coordinate.</param>
        /// <param name="tileX">Output parameter receiving the tile X coordinate.</param>
        /// <param name="tileY">Output parameter receiving the tile Y coordinate.</param>
        public static void PixelXYToTileXY(int pixelX, int pixelY, out int tileX, out int tileY)
        {
            tileX = pixelX / tilePixelSize;
            tileY = pixelY / tilePixelSize;
        }


        /// <summary>
        /// Converts tile XY coordinates into pixel XY coordinates of the upper-left pixel
        /// of the specified tile.
        /// </summary>
        /// <param name="tileX">Tile X coordinate.</param>
        /// <param name="tileY">Tile Y coordinate.</param>
        /// <param name="pixelX">Output parameter receiving the pixel X coordinate.</param>
        /// <param name="pixelY">Output parameter receiving the pixel Y coordinate.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "tileY*256"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "tileX*256")]
        public static void TileXYToPixelXY(int tileX, int tileY, out int pixelX, out int pixelY)
        {
            pixelX = tileX * tilePixelSize;
            pixelY = tileY * tilePixelSize;
        }

        /// <summary>
        /// Converts tile XY coordinates into a Spatial4JQuadKey at a specified level of detail.
        /// </summary>
        /// <param name="tileX">Tile X coordinate.</param>
        /// <param name="tileY">Tile Y coordinate.</param>
        /// <param name="levelOfDetail">Level of detail, from 1 (lowest detail)
        /// to 30 (highest detail).</param>
        /// <returns>A string containing the Spatial4JQuadKey.</returns>
        public static string TileXYToQuadKey(int tileX, int tileY, int levelOfDetail)
        {
            StringBuilder quadKey = new StringBuilder();
            for (int i = levelOfDetail; i > 0; i--)
            {
                char digit = '0';
                int mask = 1 << (i - 1);
                if ((tileX & mask) != 0)
                {
                    digit++;
                }
                if ((tileY & mask) != 0)
                {
                    digit++;
                    digit++;
                }
                quadKey.Append(digit);
            }
            return quadKey.Replace('0', 'A').Replace('1', 'B').Replace('2', 'C').Replace('3', 'D').ToString();
        }



        /// <summary>
        /// Converts a Spatial4JQuadKey into tile XY coordinates.
        /// </summary>
        /// <param name="quadKey">Spatial4JQuadKey of the tile.</param>
        /// <param name="tileX">Output parameter receiving the tile X coordinate.</param>
        /// <param name="tileY">Output parameter receiving the tile Y coordinate.</param>
        /// <param name="levelOfDetail">Output parameter receiving the level of detail.</param>
        public static void QuadKeyToTileXY(string quadKey, out int tileX, out int tileY, out int levelOfDetail)
        {
            tileX = tileY = 0;
            levelOfDetail = quadKey.Length;
            for (int i = levelOfDetail; i > 0; i--)
            {
                int mask = 1 << (i - 1);
                switch (quadKey[levelOfDetail - i])
                {
                    case 'A':
                        break;

                    case 'B':
                        tileX |= mask;
                        break;

                    case 'C':
                        tileY |= mask;
                        break;

                    case 'D':
                        tileX |= mask;
                        tileY |= mask;
                        break;

                    default:
                        throw new ArgumentException("Invalid QuadKey digit sequence.");
                }
            }
        }

        /// <summary>
        /// Converts a coordinate into Linear Projection view port values. the range -1 to 1 for both x and y viewport axes.
        /// this function is the inverse of ViewportToCoordinate
        /// </summary>
        /// <param name="coordinate">the input coordinate defined by its latitude and longitude</param>
        /// <returns>the point in the -1 to 1 x and y range</returns>
        public static Point CoordinateToViewport(Coordinate coordinate)
        {
            return LinearDecimalProjection.Project(new Point(coordinate.Longitude, coordinate.Latitude));
        }

        /// <summary>
        /// Converts a Linear Projection view port point (in the range of -1 to 1 for the x and y axes) into an absolute coordinate
        /// this function is the inverse of CoordinateToViewport
        /// </summary>
        /// <param name="point">the point in the -1 to 1 x and y range</param>
        /// <returns>return the coordinate defined by its latitude and longitude</returns>
        public static Coordinate ViewportToCoordinate(Point point)
        {
            var coordPoint = LinearDecimalProjection.Unproject(point);
            return new Coordinate(coordPoint.Y, coordPoint.X);
        }

        /// <summary>
        /// Coordinates to Spatial4JQuadKey.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="levelOfDetail">The level of detail.</param>
        /// <returns></returns>
        public static Spatial4JQuadKey CoordinateToQuadKey(Coordinate location, int levelOfDetail)
        {
            ArgumentValidation.CheckArgumentForNull<Coordinate>(location, "location");

            if (!IsValidLevelOfDetail(levelOfDetail))
                throw new MappingException("Invalid level of detail: " + levelOfDetail.ToString(CultureInfo.InvariantCulture));
            // increase level for increased accuracy
            var tile = CoordinateToTile(location, MaxDetailLevel + 1);
            // truncate quad key to requested level
            var key = Spatial4JTileSystem.TileToQuadKey(tile, MaxDetailLevel + 1).Key.Substring(0, levelOfDetail);

            return new Spatial4JQuadKey(key);
        }

        public static Coordinate QuadKeyToCoordinate(string quadkey)
        {
            int levelOfDetail;
            var tile = QuadKeyToTile(new Spatial4JQuadKey(quadkey), out levelOfDetail);
            var pixel = TileToPixel(tile);
            var coord = PixelToCoordinate(pixel, levelOfDetail);
            return coord;
        }

        public static Coordinate QuadKeyToCoordinate(string quadkey, double tileOffset)
        {
            ArgumentValidation.CheckArgumentIsInRange(tileOffset, 0, 1, "tileOffset");
            int levelOfDetail;
            var tile = QuadKeyToTile(new Spatial4JQuadKey(quadkey), out levelOfDetail);
            var pixelX = (long)Math.Round((tile.X + tileOffset) * tilePixelSize);
            var pixelY = (long)Math.Round((tile.Y + tileOffset) * tilePixelSize);
            return PixelToCoordinate(new Pixel(pixelX, pixelY), levelOfDetail);
        }

        /// <summary>
        /// Quads the key to lat long.
        /// </summary>
        /// <param name="quadKey">The quad key.</param>      
        public static Quadrangle QuadKeyToQuadrangle(Spatial4JQuadKey quadKey)
        {
            ArgumentValidation.CheckArgumentForNull<Spatial4JQuadKey>(quadKey, "quadKey");

            if (!IsValidQuadKey(quadKey.Key))
                throw new MappingException("Invalid QuadKey: " + quadKey.Key);

            int levelOfDetail;

            var tile = Spatial4JTileSystem.QuadKeyToTile(quadKey, out levelOfDetail);
            var pixel = Spatial4JTileSystem.TileToPixel(tile);

            Coordinate topLeft = Spatial4JTileSystem.PixelToCoordinate(pixel, levelOfDetail);
            // Move (+1,+1) to find the next diagonal tile, and adjust for wrapping            
            long nextTileX = (tile.X + 1 == 1 << levelOfDetail) ? 0 : tile.X + 1;
            long nextTileJ = (tile.Y + 1 == 1 << levelOfDetail) ? 0 : tile.Y + 1;

            var bottomRightTile = new Tile(nextTileX, nextTileJ);
            var bottomRightPixel = Spatial4JTileSystem.TileToPixel(bottomRightTile);

            Coordinate bottomRightDefault = Spatial4JTileSystem.PixelToCoordinate(bottomRightPixel, levelOfDetail);
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
        public static Dictionary<QuadKeyLocation, Spatial4JQuadKey?> GetQuadKeyNeighbours(Spatial4JQuadKey quadKey)
        {
            ArgumentValidation.CheckArgumentForNull<Spatial4JQuadKey>(quadKey, "quadKey");

            if (!IsValidQuadKey(quadKey.Key))
                throw new MappingException("Invalid QuadKey: " + quadKey.Key);

            var result = new Dictionary<QuadKeyLocation, Spatial4JQuadKey?>();

            int levelOfDetail;
            var tile = QuadKeyToTile(quadKey, out levelOfDetail);
            long mapEdge = (1 << levelOfDetail) - 1;

            // Location enum is a horizontal matrix starting at NorthWest
            var neighbourLocation = 0;
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
                    Spatial4JQuadKey? neighbour = null;

                    if (!invalidLongitude)
                    {
                        var tempTile = new Tile(nextTileX, nextTileY);
                        // increase level for increased accuracy
                        var key = Spatial4JTileSystem.TileToQuadKey(tempTile, levelOfDetail).Key;
                        neighbour = new Spatial4JQuadKey(key);
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

        private static double GetViewPortZoomLevel(Quadrangle quadrangle)
        {
            return 1 + Math.Min(
                Math.Log(
                    (Spatial4JTileSystem.MaxLocation.Longitude - Spatial4JTileSystem.MinLocation.Longitude) /
                    Math.Abs(((quadrangle.TopRight.Longitude > quadrangle.BottomLeft.Longitude) ? 0 : 360)
                    + quadrangle.TopRight.Longitude - quadrangle.BottomLeft.Longitude), 2
                ),
                Math.Log(
                    (Spatial4JTileSystem.MaxLocation.Latitude - Spatial4JTileSystem.MinLocation.Latitude) /
                    Math.Abs(quadrangle.TopRight.Latitude - quadrangle.BottomLeft.Latitude), 2
                )
            );
        }

        public static IEnumerable<Spatial4JQuadKey> GetQuadkeysFromQuadrangle(Quadrangle quadrangle)
        {
            var levelOfDetail = Math.Min((int)GetViewPortZoomLevel(quadrangle), MaxDetailLevel);
            var topLeftTile = CoordinateToTile(quadrangle.TopLeft, levelOfDetail);
            var bottomRightTile = CoordinateToTile(quadrangle.BottomRight, levelOfDetail);
            for (var x = topLeftTile.X; x <= bottomRightTile.X; x++)
            {
                for (var y = topLeftTile.Y; y <= bottomRightTile.Y; y++)
                {
                    yield return TileToQuadKey(new Tile(x, y), levelOfDetail);
                }
            }
        }

        /// <summary>
        /// Converts a point from latitude/longitude WGS-84 coordinates (in degrees)
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="levelOfDetail">Level of detail, from 1 (lowest detail)
        /// to 30 (highest detail).</param>
        /// <returns>XY coordinate in pixels.</returns>
        public static Pixel CoordinateToPixel(Coordinate location, int levelOfDetail)
        {
            ArgumentValidation.CheckArgumentForNull<Coordinate>(location, "location");

            var x = (location.Longitude + 180) / 360;
            var y = (90 - location.Latitude) / 180;

            var mapSize = MapSize(levelOfDetail);
            var pixelX = (long)Clip(x * mapSize, 0, mapSize - 1);
            var pixelY = (long)Clip(y * mapSize, 0, mapSize - 1);
            return new Pixel(pixelX, pixelY);
        }

        /// <summary>
        /// Converts a pixel from pixel XY coordinates at a specified level of detail
        /// into latitude/longitude WGS-84 coordinates (in degrees).
        /// </summary>
        /// <param name="pixel">XY coordinates of the point, in pixels.</param>
        /// <param name="levelOfDetail">Level of detail, from 1 (lowest detail)
        /// to 30 (highest detail).</param>
        /// <returns>Coordinate in degrees</returns>
        public static Coordinate PixelToCoordinate(Pixel pixel, int levelOfDetail)
        {
            ArgumentValidation.CheckArgumentForNull<Pixel>(pixel, "pixel");
            double mapSize = MapSize(levelOfDetail);
            var x = (Clip(pixel.X, 0, mapSize - 1) / mapSize) - 0.5;
            var y = 0.5 - (Clip(pixel.Y, 0, mapSize - 1) / mapSize);
            var latitude = 180 * y;
            var longitude = 360 * x;
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

            return new Tile((int)(pixel.X / tilePixelSize), (int)(pixel.Y / tilePixelSize));
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

            return new Pixel(tile.X * tilePixelSize, tile.Y * tilePixelSize);
        }


        /// <summary>
        /// Converts tile XY coordinates into a Spatial4JQuadKey at a specified level of detail.
        /// </summary>
        /// <param name="tile">Tile XY coordinate.</param>
        /// <param name="levelOfDetail">Level of detail, from 1 (lowest detail)
        /// to 30 (highest detail).</param>
        /// <returns>A string containing the Spatial4JQuadKey.</returns>
        private static Spatial4JQuadKey TileToQuadKey(Tile tile, int levelOfDetail)
        {
            ArgumentValidation.CheckArgumentForNull<Tile>(tile, "tile");

            var quadKeySb = new StringBuilder(levelOfDetail);
            long mask = 1 << (levelOfDetail - 1);
            var tileX = tile.X;
            var tileY = tile.Y;
            for (int i = 0; i < levelOfDetail; ++i)
            {
                char digit = 'A';
                if ((tileX & mask) != 0)
                {
                    digit++;
                }
                if ((tileY & mask) != 0)
                {
                    digit++;
                    digit++;
                }
                quadKeySb.Append(digit);
                mask >>= 1;
            }

            return new Spatial4JQuadKey(quadKeySb.ToString());
        }


        /// <summary>
        /// Converts a Spatial4JQuadKey into tile XY coordinates.
        /// </summary>
        /// <param name="quadKey">Spatial4JQuadKey of the tile.</param>
        /// <param name="levelOfDetail">Output parameter receiving the level of detail.</param>
        /// <returns>The Tile</returns>
        private static Tile QuadKeyToTile(Spatial4JQuadKey quadKey, out int levelOfDetail)
        {
            Tile tile = new Tile(0, 0);
            levelOfDetail = quadKey.Key.Length;
            for (int i = levelOfDetail; i > 0; i--)
            {
                long mask = 1 << (i - 1);
                switch (quadKey.Key[levelOfDetail - i])
                {
                    case 'A':
                        break;

                    case 'B':
                        tile.X |= mask;
                        break;

                    case 'C':
                        tile.Y |= mask;
                        break;

                    case 'D':
                        tile.X |= mask;
                        tile.Y |= mask;
                        break;

                    default:
                        throw new MappingException("Invalid Spatial4JQuadKey digit sequence: " + quadKey.Key[levelOfDetail - i]);
                }
            }
            return tile;
        }


        /// <summary>
        /// Converts Spatial4JQuadKey to the tile.
        /// </summary>
        /// <param name="quadKey">The Spatial4J quad key.</param>
        /// <returns></returns>
        public static Tile ToTile(this Spatial4JQuadKey quadKey)
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
        /// to 30 (highest detail).</param>
        /// <returns>The tile</returns>
        public static Tile CoordinateToTile(Coordinate location, int levelOfDetail)
        {
            ArgumentValidation.CheckArgumentForNull<Coordinate>(location, "location");
            IsValidLevelOfDetail(levelOfDetail);

            // Convert lat/long to pixel (X,Y) for the given zoomlevel.  This assumes that the whole Earth is
            // present in the image and pixel (0,0) the top left pixel in the image.
            var pixel = Spatial4JTileSystem.CoordinateToPixel(location, levelOfDetail);

            // Convert pixel (X,Y) coords to XY coords of tile containing this pixel (at the zoomlevel)
            // Tile (0,0) is the top right tile in the image containing all tiles (the whole Earth map) 
            // at the current zoom level.
            return Spatial4JTileSystem.PixelToTile(pixel);
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
                    Clip(latitude, MinLocation.Latitude, MaxLocation.Latitude),
                    Clip(longitude, MinLocation.Longitude, MaxLocation.Longitude));
        }

    }

}
