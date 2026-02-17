using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using RoguelikeDarling.Core.Ecs;
using RoguelikeDarling.Core.Tilemap;

namespace RoguelikeDarling.Core.RigidBody2D
{
    public sealed class RigidBody2DCollisionSystem : IGameSystem
    {
        public void Update(World world, GameTime gameTime)
        {
            var rigidColliders = CollectRigidColliders(world);
            var tileCollisionLayers = CollectTileCollisionLayers(world);

            ResolveRigidBodyPairs(rigidColliders);
            ResolveRigidBodyToTileMap(rigidColliders, tileCollisionLayers);
        }

        public void Draw(World world, GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
        }

        private static List<RigidColliderEntry> CollectRigidColliders(World world)
        {
            var result = new List<RigidColliderEntry>();

            for (int i = 0; i < world.Entities.Count; i++)
            {
                Entity entity = world.Entities[i];
                if (!entity.TryGetComponent<Transform2DComponent>(out Transform2DComponent transform)
                    || !entity.TryGetComponent<RigidBody2DColliderComponent>(out RigidBody2DColliderComponent collider)
                    || !entity.TryGetComponent<CollisionLayerComponent>(out CollisionLayerComponent collisionLayer)
                    || transform == null
                    || collider == null
                    || collisionLayer == null)
                {
                    continue;
                }

                entity.TryGetComponent<RigidBody2DComponent>(out RigidBody2DComponent rigidBody);

                result.Add(new RigidColliderEntry(entity, transform, rigidBody, collider, collisionLayer));
            }

            return result;
        }

        private static List<TileCollisionEntry> CollectTileCollisionLayers(World world)
        {
            var result = new List<TileCollisionEntry>();

            for (int i = 0; i < world.Entities.Count; i++)
            {
                Entity entity = world.Entities[i];
                if (!entity.TryGetComponent<TileMapLayerComponent>(out TileMapLayerComponent tileLayer)
                    || !entity.TryGetComponent<TileMapCollisionLayerComponent>(out TileMapCollisionLayerComponent collisionLayer)
                    || !entity.TryGetComponent<CollisionLayerComponent>(out CollisionLayerComponent layerMask)
                    || tileLayer == null
                    || collisionLayer == null
                    || layerMask == null)
                {
                    continue;
                }

                result.Add(new TileCollisionEntry(tileLayer, collisionLayer, layerMask));
            }

            return result;
        }

        private static void ResolveRigidBodyPairs(List<RigidColliderEntry> rigidColliders)
        {
            for (int i = 0; i < rigidColliders.Count; i++)
            {
                RigidColliderEntry a = rigidColliders[i];
                for (int j = i + 1; j < rigidColliders.Count; j++)
                {
                    RigidColliderEntry b = rigidColliders[j];
                    if (!a.Layer.CanCollideWith(b.Layer))
                    {
                        continue;
                    }

                    Vector2[] aPolygon = BuildWorldPolygon(a.Transform, a.Collider);
                    Vector2[] bPolygon = BuildWorldPolygon(b.Transform, b.Collider);
                    if (!TryComputeMinimumTranslation(aPolygon, bPolygon, out Vector2 minimumTranslation))
                    {
                        continue;
                    }

                    bool aDynamic = a.RigidBody != null && !a.RigidBody.IsStatic;
                    bool bDynamic = b.RigidBody != null && !b.RigidBody.IsStatic;

                    if (aDynamic && bDynamic)
                    {
                        Vector2 half = minimumTranslation * 0.5f;
                        a.Transform.Position -= half;
                        b.Transform.Position += half;
                    }
                    else if (aDynamic)
                    {
                        a.Transform.Position -= minimumTranslation;
                        a.RigidBody.Velocity = Vector2.Zero;
                    }
                    else if (bDynamic)
                    {
                        b.Transform.Position += minimumTranslation;
                        b.RigidBody.Velocity = Vector2.Zero;
                    }
                }
            }
        }

        private static void ResolveRigidBodyToTileMap(List<RigidColliderEntry> rigidColliders, List<TileCollisionEntry> tileCollisionLayers)
        {
            for (int i = 0; i < rigidColliders.Count; i++)
            {
                RigidColliderEntry body = rigidColliders[i];
                if (body.RigidBody == null || body.RigidBody.IsStatic)
                {
                    continue;
                }

                for (int layerIndex = 0; layerIndex < tileCollisionLayers.Count; layerIndex++)
                {
                    TileCollisionEntry tileCollision = tileCollisionLayers[layerIndex];
                    if (!body.Layer.CanCollideWith(tileCollision.Layer))
                    {
                        continue;
                    }

                    bool hadCollision = ResolveBodyAgainstTileLayer(body, tileCollision);
                    if (hadCollision)
                    {
                        body.RigidBody.Velocity = Vector2.Zero;
                    }
                }
            }
        }

        private static bool ResolveBodyAgainstTileLayer(RigidColliderEntry body, TileCollisionEntry tileCollision)
        {
            bool hadCollision = false;
            Vector2[] bodyPolygon = BuildWorldPolygon(body.Transform, body.Collider);

            for (int y = 0; y < tileCollision.CollisionLayer.Height; y++)
            {
                for (int x = 0; x < tileCollision.CollisionLayer.Width; x++)
                {
                    if (!tileCollision.CollisionLayer.IsSolid(x, y))
                    {
                        continue;
                    }

                    Vector2[] tilePolygon = BuildTileCollisionPolygon(tileCollision, x, y);
                    if (!TryComputeMinimumTranslation(bodyPolygon, tilePolygon, out Vector2 minimumTranslation))
                    {
                        continue;
                    }

                    body.Transform.Position -= minimumTranslation;
                    bodyPolygon = BuildWorldPolygon(body.Transform, body.Collider);
                    hadCollision = true;
                }
            }

            return hadCollision;
        }

        private static Vector2[] BuildWorldPolygon(Transform2DComponent transform, RigidBody2DColliderComponent collider)
        {
            Vector2 parentScale = transform.Scale;
            Vector2 localScale = collider.LocalScale;
            Vector2 combinedScale = new Vector2(parentScale.X * localScale.X, parentScale.Y * localScale.Y);

            Vector2 worldCenter = transform.Position + new Vector2(
                collider.LocalPosition.X * parentScale.X,
                collider.LocalPosition.Y * parentScale.Y);

            switch (collider.ShapeType)
            {
                case CollisionShapeType.Rectangle:
                    return BuildRectanglePolygon(worldCenter, collider.Size, combinedScale);
                case CollisionShapeType.Oval:
                    return BuildOvalPolygon(worldCenter, collider.Size, combinedScale, 14);
                case CollisionShapeType.Polygon:
                    return BuildCustomPolygon(worldCenter, collider.PolygonPoints, combinedScale);
                default:
                    return Array.Empty<Vector2>();
            }
        }

        private static Vector2[] BuildTileCollisionPolygon(TileCollisionEntry entry, int x, int y)
        {
            float halfWidth = entry.TileLayer.TileStride.X * 0.5f;
            float halfHeight = entry.TileLayer.TileStride.Y * 0.5f;

            float tileBaseX = (x - y) * halfWidth + entry.TileLayer.Offset.X;
            float tileBaseY = (x + y) * halfHeight + entry.TileLayer.Offset.Y;

            switch (entry.CollisionLayer.ShapeType)
            {
                case CollisionShapeType.Rectangle:
                    {
                        Vector2 size = new Vector2(
                            entry.TileLayer.TileStride.X * entry.CollisionLayer.ShapeSize.X,
                            entry.TileLayer.TileStride.Y * entry.CollisionLayer.ShapeSize.Y);
                        Vector2 center = new Vector2(
                            tileBaseX + (entry.TileLayer.TileStride.X * 0.5f),
                            tileBaseY + (entry.TileLayer.TileStride.Y * 0.5f));
                        return BuildRectanglePolygon(center, size, Vector2.One);
                    }
                case CollisionShapeType.Oval:
                    {
                        Vector2 size = new Vector2(
                            entry.TileLayer.TileStride.X * entry.CollisionLayer.ShapeSize.X,
                            entry.TileLayer.TileStride.Y * entry.CollisionLayer.ShapeSize.Y);
                        Vector2 center = new Vector2(
                            tileBaseX + (entry.TileLayer.TileStride.X * 0.5f),
                            tileBaseY + (entry.TileLayer.TileStride.Y * 0.5f));
                        return BuildOvalPolygon(center, size, Vector2.One, 14);
                    }
                case CollisionShapeType.Polygon:
                default:
                    {
                        IReadOnlyList<Vector2> points = entry.CollisionLayer.PolygonPoints;
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
                                tileBaseX + (p.X * entry.TileLayer.TileStride.X),
                                tileBaseY + (p.Y * entry.TileLayer.TileStride.Y));
                        }

                        return result;
                    }
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

        private static bool TryComputeMinimumTranslation(Vector2[] polygonA, Vector2[] polygonB, out Vector2 minimumTranslation)
        {
            minimumTranslation = Vector2.Zero;
            if (polygonA.Length < 3 || polygonB.Length < 3)
            {
                return false;
            }

            float minOverlap = float.MaxValue;
            Vector2 smallestAxis = Vector2.Zero;

            if (!CheckAxes(polygonA, polygonB, ref minOverlap, ref smallestAxis))
            {
                return false;
            }

            if (!CheckAxes(polygonB, polygonA, ref minOverlap, ref smallestAxis))
            {
                return false;
            }

            Vector2 centerA = ComputeCenter(polygonA);
            Vector2 centerB = ComputeCenter(polygonB);
            Vector2 fromAToB = centerB - centerA;

            if (Vector2.Dot(fromAToB, smallestAxis) < 0f)
            {
                smallestAxis *= -1f;
            }

            minimumTranslation = smallestAxis * minOverlap;
            return true;
        }

        private static bool CheckAxes(Vector2[] polygonA, Vector2[] polygonB, ref float minOverlap, ref Vector2 smallestAxis)
        {
            for (int i = 0; i < polygonA.Length; i++)
            {
                Vector2 p1 = polygonA[i];
                Vector2 p2 = polygonA[(i + 1) % polygonA.Length];
                Vector2 edge = p2 - p1;

                Vector2 axis = new Vector2(-edge.Y, edge.X);
                if (axis == Vector2.Zero)
                {
                    continue;
                }

                axis.Normalize();

                ProjectPolygon(axis, polygonA, out float minA, out float maxA);
                ProjectPolygon(axis, polygonB, out float minB, out float maxB);

                if (maxA < minB || maxB < minA)
                {
                    return false;
                }

                float overlap = Math.Min(maxA, maxB) - Math.Max(minA, minB);
                if (overlap < minOverlap)
                {
                    minOverlap = overlap;
                    smallestAxis = axis;
                }
            }

            return true;
        }

        private static void ProjectPolygon(Vector2 axis, Vector2[] polygon, out float min, out float max)
        {
            float value = Vector2.Dot(axis, polygon[0]);
            min = value;
            max = value;

            for (int i = 1; i < polygon.Length; i++)
            {
                value = Vector2.Dot(axis, polygon[i]);
                if (value < min)
                {
                    min = value;
                }

                if (value > max)
                {
                    max = value;
                }
            }
        }

        private static Vector2 ComputeCenter(Vector2[] polygon)
        {
            Vector2 total = Vector2.Zero;
            for (int i = 0; i < polygon.Length; i++)
            {
                total += polygon[i];
            }

            return total / polygon.Length;
        }

        private readonly struct RigidColliderEntry
        {
            public RigidColliderEntry(Entity entity, Transform2DComponent transform, RigidBody2DComponent rigidBody, RigidBody2DColliderComponent collider, CollisionLayerComponent layer)
            {
                Entity = entity;
                Transform = transform;
                RigidBody = rigidBody;
                Collider = collider;
                Layer = layer;
            }

            public Entity Entity { get; }

            public Transform2DComponent Transform { get; }

            public RigidBody2DComponent RigidBody { get; }

            public RigidBody2DColliderComponent Collider { get; }

            public CollisionLayerComponent Layer { get; }
        }

        private readonly struct TileCollisionEntry
        {
            public TileCollisionEntry(TileMapLayerComponent tileLayer, TileMapCollisionLayerComponent collisionLayer, CollisionLayerComponent layer)
            {
                TileLayer = tileLayer;
                CollisionLayer = collisionLayer;
                Layer = layer;
            }

            public TileMapLayerComponent TileLayer { get; }

            public TileMapCollisionLayerComponent CollisionLayer { get; }

            public CollisionLayerComponent Layer { get; }
        }
    }
}