using System.Collections.Generic;
using Microsoft.Xna.Framework;
using RoguelikeDarling.Core.Collision2D;
using RoguelikeDarling.Core.Ecs;
using RoguelikeDarling.Core.Physics2D;
using RoguelikeDarling.Core.Spatial;

namespace RoguelikeDarling.Core.Skeleton2D.Humanoid
{
    public sealed class HumanoidSkeletonSyncSystem : IGameSystem
    {
        public void Update(World world, GameTime gameTime)
        {
            for (int i = 0; i < world.Entities.Count; i++)
            {
                Entity entity = world.Entities[i];
                if (!entity.TryGetComponent<Transform2DComponent>(out Transform2DComponent rootTransform)
                    || !entity.TryGetComponent<HumanoidSkeletonComponent>(out HumanoidSkeletonComponent humanoid)
                    || rootTransform == null
                    || humanoid == null
                    || humanoid.HumanoidSkeleton == null)
                {
                    continue;
                }

                EnsurePartBindings(world, humanoid);

                IReadOnlyDictionary<string, SkeletonMemberWorldPose2D> visualPoses = humanoid.HumanoidSkeleton.Skeleton.EvaluateWorldPoses(
                    rootTransform.Position + humanoid.SkeletonLocalOffset,
                    rootTransform.Scale);

                IReadOnlyDictionary<string, SkeletonMemberWorldPose2D> collisionPoses = humanoid.HumanoidSkeleton.Skeleton.EvaluateWorldPoses(
                    rootTransform.Position + humanoid.CollisionLocalOffset,
                    rootTransform.Scale);

                foreach (KeyValuePair<string, HumanoidSkeletonPartBinding> bindingEntry in humanoid.PartBindings)
                {
                    string memberName = bindingEntry.Key;
                    HumanoidSkeletonPartBinding binding = bindingEntry.Value;

                    if (binding == null
                        || !visualPoses.TryGetValue(memberName, out SkeletonMemberWorldPose2D visualPose)
                        || !collisionPoses.TryGetValue(memberName, out SkeletonMemberWorldPose2D collisionPose))
                    {
                        continue;
                    }

                    SyncVisualPart(binding.VisualEntity, visualPose);
                    SyncCollisionPart(binding.CollisionEntity, collisionPose, humanoid);
                }
            }
        }

        public void Draw(World world, GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
        }

        private static void EnsurePartBindings(World world, HumanoidSkeletonComponent humanoid)
        {
            IReadOnlyList<SkeletonMemberDefinition2D> members = humanoid.HumanoidSkeleton.Skeleton.Members;

            for (int i = 0; i < members.Count; i++)
            {
                SkeletonMemberDefinition2D member = members[i];
                if (humanoid.PartBindings.ContainsKey(member.Name))
                {
                    continue;
                }

                Entity visualEntity = world.CreateEntity();
                visualEntity
                    .AddComponent(new Transform2DComponent())
                    .AddComponent(new SkeletonPartRender2DComponent());

                Entity collisionEntity = world.CreateEntity();
                collisionEntity
                    .AddComponent(new Transform2DComponent())
                    .AddComponent(new RigidBody2DComponent { IsStatic = false, Velocity = Vector2.Zero })
                    .AddComponent(Collider2DComponent.CreateRectangle(member.CollisionSize, humanoid.CollisionDebugEnabled, humanoid.CollisionDebugColor))
                    .AddComponent(new CollisionLayerComponent(humanoid.CollisionLayer, humanoid.CollisionMask));

                humanoid.PartBindings[member.Name] = new HumanoidSkeletonPartBinding
                {
                    VisualEntity = visualEntity,
                    CollisionEntity = collisionEntity,
                };
            }
        }

        private static void SyncVisualPart(Entity visualEntity, SkeletonMemberWorldPose2D pose)
        {
            if (visualEntity == null
                || !visualEntity.TryGetComponent<Transform2DComponent>(out Transform2DComponent transform)
                || !visualEntity.TryGetComponent<SkeletonPartRender2DComponent>(out SkeletonPartRender2DComponent render)
                || transform == null
                || render == null)
            {
                return;
            }

            transform.Position = pose.JointWorldPosition;
            transform.Scale = pose.WorldScale;
            transform.Rotation = pose.WorldRotation;

            SkeletonMemberDefinition2D definition = pose.Definition;
            SkeletonPartVisual2D visual = definition.Visual;

            render.Texture = visual.Texture;
            render.SourceRect = visual.SourceRect;
            render.Color = visual.Color;
            render.Size = definition.Size;
            render.Pivot = definition.Pivot;
            render.ZIndex = definition.ZIndex;
            render.Visible = true;
        }

        private static void SyncCollisionPart(Entity collisionEntity, SkeletonMemberWorldPose2D pose, HumanoidSkeletonComponent humanoid)
        {
            if (collisionEntity == null
                || !collisionEntity.TryGetComponent<Transform2DComponent>(out Transform2DComponent transform)
                || !collisionEntity.TryGetComponent<Collider2DComponent>(out Collider2DComponent collider)
                || !collisionEntity.TryGetComponent<CollisionLayerComponent>(out CollisionLayerComponent layer)
                || transform == null
                || collider == null
                || layer == null)
            {
                return;
            }

            SkeletonMemberDefinition2D definition = pose.Definition;
            Vector2 collisionCenter = pose.GetCenterWorldPosition(definition.CollisionSize);

            transform.Position = collisionCenter;
            transform.Scale = pose.WorldScale;
            transform.Rotation = pose.WorldRotation;

            collider.Size = definition.CollisionSize;
            collider.LocalPosition = Vector2.Zero;
            collider.LocalScale = Vector2.One;

            layer.CollidesWithMask = humanoid.CollisionMask;
        }
    }
}
