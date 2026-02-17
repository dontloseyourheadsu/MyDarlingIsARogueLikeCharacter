using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace RoguelikeDarling.Core.Collision2D
{
    public sealed class Collider2DComponent
    {
        private Collider2DComponent(CollisionShapeType shapeType)
        {
            ShapeType = shapeType;
            LocalScale = Vector2.One;
            Size = new Vector2(24f, 24f);
            PolygonPoints = Array.Empty<Vector2>();
        }

        public CollisionShapeType ShapeType { get; }

        public Vector2 LocalPosition { get; set; }

        public Vector2 LocalScale { get; set; }

        public Vector2 Size { get; set; }

        public IReadOnlyList<Vector2> PolygonPoints { get; set; }

        public static Collider2DComponent CreateRectangle(Vector2 size)
        {
            var collider = new Collider2DComponent(CollisionShapeType.Rectangle);
            collider.Size = size;
            return collider;
        }

        public static Collider2DComponent CreateOval(Vector2 size)
        {
            var collider = new Collider2DComponent(CollisionShapeType.Oval);
            collider.Size = size;
            return collider;
        }

        public static Collider2DComponent CreatePolygon(IReadOnlyList<Vector2> points)
        {
            var collider = new Collider2DComponent(CollisionShapeType.Polygon);
            collider.PolygonPoints = points ?? Array.Empty<Vector2>();
            return collider;
        }
    }
}