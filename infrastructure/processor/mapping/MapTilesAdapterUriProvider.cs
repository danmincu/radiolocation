using System;

namespace Mapping
{
    /// <summary>
    /// An IMapTileUri implementation for getting URIs to the MapTilesAdapter web server.
    /// </summary>
    public class MapTilesAdapterUriProvider : IMapTileUri
    {
        // Our server only serves tiles 256x256
        const int MapTilesAdapterTileWidth = 256;
        const int MapTilesAdapterTileHeight = 256;

        private readonly string requestFormat;

        /// <summary>
        /// Initializes a new instance of the <see cref="MapTilesAdapterUriProvider"/> class.
        /// </summary>
        /// <param name="mapTileRequestFormat">Uri format with placeholders for server number, tile id, and mode.</param>
        public MapTilesAdapterUriProvider(string requestFormat)
        {
            ArgumentValidation.CheckArgumentForNullOrEmpty(requestFormat, "requestFormat");

            this.requestFormat = requestFormat;
        }

        public Uri GetTileUri(int tileLevel, Tile tile)
        {
            //ArgumentValidation.CheckArgumentIsGreaterThanOrEqualToValue(tileLevel, 0, "tileLeveL");
            ArgumentValidation.CheckArgumentForNull(tile, "tile");

            // Stoooopid INLINE overflow check to satisfy CA2233
            if (tileLevel < 0)
                throw new ArgumentOutOfRangeException("tileLevel");

            // NOTE: We are passed the tileLevel + x&y, which we must convert to quad-key LOD, row, col
            var lod = tileLevel;
            var row = (int)tile.Y;
            var col = (int)tile.X;

            // Create request params
            var serverKey = "";
            var quadkey = QuadKey.Create(lod, row, col).Key;
            var mode = "";
            
            // Return URI
            var uriString = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                requestFormat, serverKey, quadkey, mode);
            return new Uri(uriString);
        }

        public int TileWidth
        {
            get { return MapTilesAdapterTileWidth; }
        }

        public int TileHeight
        {
            get { return MapTilesAdapterTileHeight; }
        }
    }
}
