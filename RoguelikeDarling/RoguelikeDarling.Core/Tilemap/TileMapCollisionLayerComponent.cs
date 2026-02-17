using System.Collections.Generic;
using Microsoft.Xna.Framework;
using RoguelikeDarling.Core.Collision2D;

namespace RoguelikeDarling.Core.Tilemap
{
    public sealed class TileMapCollisionLayerComponent
    {
        private readonly bool[,] collisionCells;

        public TileMapCollisionLayerComponent(int width, int height)
        {
            collisionCells = new bool[width, height];
            ShapeType = CollisionShapeType.Polygon;
            PolygonPoints = new[]
            {
                new Vector2(0.5f, 0f),
                new Vector2(1f, 0.5f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, 0.5f),
            };
            ShapeSize = new Vector2(1f, 1f);
        }

        public int Width => collisionCells.GetLength(0);

        public int Height => collisionCells.GetLength(1);

        public CollisionShapeType ShapeType { get; set; }

        public Vector2 ShapeSize { get; set; }

        public IReadOnlyList<Vector2> PolygonPoints { get; set; }

        public void SetSolid(int x, int y, bool isSolid)
        {
            collisionCells[x, y] = isSolid;
        }

        public bool IsSolid(int x, int y)
        {
            return collisionCells[x, y];
        }
    }
}