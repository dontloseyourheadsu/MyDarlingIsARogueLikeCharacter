using System;
using System.Collections.Generic;

namespace RoguelikeDarling.Core.Storage
{
    public sealed class TileMapBinaryJsonDocument
    {
        public string Name { get; set; } = "Untitled";

        public DateTime LastUpdatedUtc { get; set; } = DateTime.UtcNow;

        public List<TileMapBinaryJsonLayer> Layers { get; set; } = new List<TileMapBinaryJsonLayer>();
    }

    public sealed class TileMapBinaryJsonLayer
    {
        public string Name { get; set; } = string.Empty;

        public int Width { get; set; }

        public int Height { get; set; }

        public int TileSizeX { get; set; }

        public int TileSizeY { get; set; }

        public int TileStrideX { get; set; }

        public int TileStrideY { get; set; }

        public int ZIndex { get; set; }

        public bool YSortEnabled { get; set; }

        public int RenderOrder { get; set; }

        public float OffsetX { get; set; }

        public float OffsetY { get; set; }

        public int[] TileIds { get; set; } = Array.Empty<int>();
    }

    public sealed class TileMapMenuItem
    {
        public string Name { get; set; } = string.Empty;

        public string FilePath { get; set; } = string.Empty;

        public DateTime LastUpdatedUtc { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public int[] PreviewTileIds { get; set; } = Array.Empty<int>();
    }
}
