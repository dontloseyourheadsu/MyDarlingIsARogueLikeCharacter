using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace RoguelikeDarling.Core.RigidBody2D
{
    public sealed class RigidBody2DColliderComponent
    {
        private RigidBody2DColliderComponent(CollisionShapeType shapeType)
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

        public static RigidBody2DColliderComponent CreateRectangle(Vector2 size)
        {
            var collider = new RigidBody2DColliderComponent(CollisionShapeType.Rectangle);
            collider.Size = size;
            return collider;
        }

        public static RigidBody2DColliderComponent CreateOval(Vector2 size)
        {
            var collider = new RigidBody2DColliderComponent(CollisionShapeType.Oval);
            collider.Size = size;
            return collider;
        }

        public static RigidBody2DColliderComponent CreatePolygon(IReadOnlyList<Vector2> points)
        {
            var collider = new RigidBody2DColliderComponent(CollisionShapeType.Polygon);
            collider.PolygonPoints = points ?? Array.Empty<Vector2>();
            return collider;
        }
    }
}