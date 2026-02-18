using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RoguelikeDarling.Core.Spatial;

namespace RoguelikeDarling.Core.Collision2D
{
    public sealed class Collider2DComponent
    {
        private Collider2DComponent(CollisionShapeType shapeType, bool debugCollisionEnabled = false, Color? debugCollisionColor = null)
        {
            ShapeType = shapeType;
            LocalScale = Vector2.One;
            Size = new Vector2(24f, 24f);
            PolygonPoints = Array.Empty<Vector2>();
            DebugCollisionEnabled = debugCollisionEnabled;
            DebugCollisionColor = debugCollisionColor ?? new Color(220, 30, 30);
        }

        public CollisionShapeType ShapeType { get; }

        public Vector2 LocalPosition { get; set; }

        public Vector2 LocalScale { get; set; }

        public Vector2 Size { get; set; }

        public IReadOnlyList<Vector2> PolygonPoints { get; set; }

        public bool DebugCollisionEnabled { get; }

        public Color DebugCollisionColor { get; }

        public static Collider2DComponent CreateRectangle(Vector2 size, bool debugCollisionEnabled = false, Color? debugCollisionColor = null)
        {
            var collider = new Collider2DComponent(CollisionShapeType.Rectangle, debugCollisionEnabled, debugCollisionColor);
            collider.Size = size;
            return collider;
        }

        public static Collider2DComponent CreateOval(Vector2 size, bool debugCollisionEnabled = false, Color? debugCollisionColor = null)
        {
            var collider = new Collider2DComponent(CollisionShapeType.Oval, debugCollisionEnabled, debugCollisionColor);
            collider.Size = size;
            return collider;
        }

        public static Collider2DComponent CreatePolygon(IReadOnlyList<Vector2> points, bool debugCollisionEnabled = false, Color? debugCollisionColor = null)
        {
            var collider = new Collider2DComponent(CollisionShapeType.Polygon, debugCollisionEnabled, debugCollisionColor);
            collider.PolygonPoints = points ?? Array.Empty<Vector2>();
            return collider;
        }

        public void RenderDebugCollision(SpriteBatch spriteBatch, Texture2D pixelTexture, Transform2DComponent transform)
        {
            if (!DebugCollisionEnabled || spriteBatch == null || pixelTexture == null || transform == null)
            {
                return;
            }

            Vector2[] polygon = BuildWorldPolygon(transform);
            if (polygon.Length < 2)
            {
                return;
            }

            for (int i = 0; i < polygon.Length; i++)
            {
                Vector2 start = polygon[i];
                Vector2 end = polygon[(i + 1) % polygon.Length];
                DrawLine(spriteBatch, pixelTexture, start, end, DebugCollisionColor, 2f);
            }
        }

        private Vector2[] BuildWorldPolygon(Transform2DComponent transform)
        {
            Vector2 parentScale = transform.Scale;
            Vector2 localScale = LocalScale;
            Vector2 combinedScale = new Vector2(parentScale.X * localScale.X, parentScale.Y * localScale.Y);

            Vector2 worldCenter = transform.Position + new Vector2(
                LocalPosition.X * parentScale.X,
                LocalPosition.Y * parentScale.Y);

            switch (ShapeType)
            {
                case CollisionShapeType.Rectangle:
                    return BuildRectanglePolygon(worldCenter, Size, combinedScale);
                case CollisionShapeType.Oval:
                    return BuildOvalPolygon(worldCenter, Size, combinedScale, 14);
                case CollisionShapeType.Polygon:
                    return BuildCustomPolygon(worldCenter, PolygonPoints, combinedScale);
                default:
                    return Array.Empty<Vector2>();
            }
        }

        private static Vector2[] BuildRectanglePolygon(Vector2 center, Vector2 size, Vector2 scale)
        {
            float halfWidth = size.X * scale.X * 0.5f;
            float halfHeight = size.Y * scale.Y * 0.5f;

            return new[]
            {
                new Vector2(center.X - halfWidth, center.Y - halfHeight),
                new Vector2(center.X + halfWidth, center.Y - halfHeight),
                new Vector2(center.X + halfWidth, center.Y + halfHeight),
                new Vector2(center.X - halfWidth, center.Y + halfHeight),
            };
        }

        private static Vector2[] BuildCustomPolygon(Vector2 center, IReadOnlyList<Vector2> points, Vector2 scale)
        {
            if (points == null || points.Count < 3)
            {
                return Array.Empty<Vector2>();
            }

            var result = new Vector2[points.Count];
            for (int i = 0; i < points.Count; i++)
            {
                Vector2 p = points[i];
                result[i] = center + new Vector2(p.X * scale.X, p.Y * scale.Y);
            }

            return result;
        }

        private static Vector2[] BuildOvalPolygon(Vector2 center, Vector2 size, Vector2 scale, int segments)
        {
            float rx = size.X * scale.X * 0.5f;
            float ry = size.Y * scale.Y * 0.5f;
            var points = new Vector2[segments];

            for (int i = 0; i < segments; i++)
            {
                float t = MathHelper.TwoPi * i / segments;
                points[i] = new Vector2(
                    center.X + (float)Math.Cos(t) * rx,
                    center.Y + (float)Math.Sin(t) * ry);
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

            float rotation = (float)Math.Atan2(delta.Y, delta.X);
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