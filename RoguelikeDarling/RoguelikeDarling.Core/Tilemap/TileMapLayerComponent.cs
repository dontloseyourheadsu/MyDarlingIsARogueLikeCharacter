using Microsoft.Xna.Framework;

namespace RoguelikeDarling.Core.Tilemap
{
    public sealed class TileMapLayerComponent
    {
        private readonly int[,] tileIds;

        public TileMapLayerComponent(string name, int width, int height, Point tileSize, int zIndex)
        {
            Name = name;
            TileSize = tileSize;
            TileStride = tileSize;
            ZIndex = zIndex;
            tileIds = new int[width, height];
            RenderOrder = TileRenderOrder.IsometricDiamondDownHorizontalOffset;
            Offset = Vector2.Zero;
            YSortEnabled = true;
        }

        public string Name { get; }

        public int Width => tileIds.GetLength(0);

        public int Height => tileIds.GetLength(1);

        public Point TileSize { get; set; }

        public Point TileStride { get; set; }

        public int ZIndex { get; set; }

        public bool YSortEnabled { get; set; }

        public TileRenderOrder RenderOrder { get; set; }

        public Vector2 Offset { get; set; }

        public void SetTile(int x, int y, int tileId)
        {
            tileIds[x, y] = tileId;
        }

        public int GetTile(int x, int y)
        {
            return tileIds[x, y];
        }
    }
}
