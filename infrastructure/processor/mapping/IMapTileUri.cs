using System;

namespace Mapping
{
    public interface IMapTileUri
    {
        Uri GetTileUri(int tileLevel, Tile tile);
        int TileWidth { get; }
        int TileHeight { get; }
    }
}
