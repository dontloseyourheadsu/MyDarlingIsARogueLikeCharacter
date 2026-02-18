using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RoguelikeDarling.Core.Collision2D;

namespace RoguelikeDarling.Core.Tilemap
{
    public sealed class TileMapCollisionLayerComponent
    {
        private readonly bool[,] collisionCells;

        public TileMapCollisionLayerComponent(int width, int height, bool debugCollisionEnabled = false, Color? debugCollisionColor = null)
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
            DebugCollisionEnabled = debugCollisionEnabled;
            DebugCollisionColor = debugCollisionColor ?? new Color(220, 30, 30);
        }

        public int Width => collisionCells.GetLength(0);

        public int Height => collisionCells.GetLength(1);

        public CollisionShapeType ShapeType { get; set; }

        public Vector2 ShapeSize { get; set; }

        public IReadOnlyList<Vector2> PolygonPoints { get; set; }

        public bool DebugCollisionEnabled { get; }

        public Color DebugCollisionColor { get; }

        public void SetSolid(int x, int y, bool isSolid)
        {
            collisionCells[x, y] = isSolid;
        }

        public bool IsSolid(int x, int y)
        {
            return collisionCells[x, y];
        }

        public void RenderDebugCollision(SpriteBatch spriteBatch, Texture2D pixelTexture, TileMapLayerComponent tileLayer)
        {
            if (!DebugCollisionEnabled || spriteBatch == null || pixelTexture == null || tileLayer == null)
            {
                return;
            }

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    if (!IsSolid(x, y))
                    {
                        continue;
                    }

                    Vector2[] polygon = BuildTileCollisionPolygon(tileLayer, x, y);
                    if (polygon.Length < 2)
                    {
                        continue;
                    }

                    for (int i = 0; i < polygon.Length; i++)
                    {
                        Vector2 start = polygon[i];
                        Vector2 end = polygon[(i + 1) % polygon.Length];
                        DrawLine(spriteBatch, pixelTexture, start, end, DebugCollisionColor, 2f);
                    }
                }
            }
        }

        private Vector2[] BuildTileCollisionPolygon(TileMapLayerComponent tileLayer, int x, int y)
        {
            float halfWidth = tileLayer.TileStride.X * 0.5f;
            float halfHeight = tileLayer.TileStride.Y * 0.5f;

            float tileBaseX = (x - y) * halfWidth + tileLayer.Offset.X;
            float tileBaseY = (x + y) * halfHeight + tileLayer.Offset.Y;

            switch (ShapeType)
            {
                case CollisionShapeType.Rectangle:
                    {
                        Vector2 size = new Vector2(
                            tileLayer.TileStride.X * ShapeSize.X,
                            tileLayer.TileStride.Y * ShapeSize.Y);
                        Vector2 center = new Vector2(
                            tileBaseX + (tileLayer.TileStride.X * 0.5f),
                            tileBaseY + (tileLayer.TileStride.Y * 0.5f));
                        return BuildRectanglePolygon(center, size);
                    }
                case CollisionShapeType.Oval:
                    {
                        Vector2 size = new Vector2(
                            tileLayer.TileStride.X * ShapeSize.X,
                            tileLayer.TileStride.Y * ShapeSize.Y);
                        Vector2 center = new Vector2(
                            tileBaseX + (tileLayer.TileStride.X * 0.5f),
                            tileBaseY + (tileLayer.TileStride.Y * 0.5f));
                        return BuildOvalPolygon(center, size, 14);
                    }
                case CollisionShapeType.Polygon:
                default:
                    {
                        IReadOnlyList<Vector2> points = PolygonPoints;
                        if (points == null || points.Count < 3)
                        {
                            points = new[]
                            {
                                new Vector2(0.5f, 0f),
                                new Vector2(1f, 0.5f),
                                new Vector2(0.5f, 1f),
                                new Vector2(0f, 0.5f),
                            };
                        }

                        var result = new Vector2[points.Count];
                        for (int i = 0; i < points.Count; i++)
                        {
                            Vector2 p = points[i];
                            result[i] = new Vector2(
                                tileBaseX + (p.X * tileLayer.TileStride.X),
                                tileBaseY + (p.Y * tileLayer.TileStride.Y));
                        }

                        return result;
                    }
            }
        }

        private static Vector2[] BuildRectanglePolygon(Vector2 center, Vector2 size)
        {
            float halfWidth = size.X * 0.5f;
            float halfHeight = size.Y * 0.5f;

            return new[]
            {
                new Vector2(center.X - halfWidth, center.Y - halfHeight),
                new Vector2(center.X + halfWidth, center.Y - halfHeight),
                new Vector2(center.X + halfWidth, center.Y + halfHeight),
                new Vector2(center.X - halfWidth, center.Y + halfHeight),
            };
        }

        private static Vector2[] BuildOvalPolygon(Vector2 center, Vector2 size, int segments)
        {
            float rx = size.X * 0.5f;
            float ry = size.Y * 0.5f;
            var points = new Vector2[segments];

            for (int i = 0; i < segments; i++)
            {
                float t = MathHelper.TwoPi * i / segments;
                points[i] = new Vector2(
                    center.X + (float)System.Math.Cos(t) * rx,
                    center.Y + (float)System.Math.Sin(t) * ry);
            }

            return points;
        }

        private static void DrawLine(SpriteBatch spriteBatch, Texture2D pixelTexture, Vector2 start, Vector2 end, Color color, float thickness)
        {
            Vector2 delta = end - start;
            float length = delta.Length();
            if (length <= 0f)
            {
                return;
            }

            float rotation = (float)System.Math.Atan2(delta.Y, delta.X);
            spriteBatch.Draw(
                pixelTexture,
                position: start,
                sourceRectangle: null,
                color: color,
                rotation: rotation,
                origin: Vector2.Zero,
                scale: new Vector2(length, thickness),
                effects: SpriteEffects.None,
                layerDepth: 0f);
        }
    }
}