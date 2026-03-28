using Microsoft.Xna.Framework;
using RoguelikeDarling.Core.Ecs;

namespace RoguelikeDarling.Core.Skeleton2D.Humanoid
{
    public sealed class HumanoidSkeletonAnimationSystem : IGameSystem
    {
        public void Update(World world, GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            for (int i = 0; i < world.Entities.Count; i++)
            {
                Entity entity = world.Entities[i];
                if (!entity.TryGetComponent<HumanoidSkeletonComponent>(out HumanoidSkeletonComponent humanoid)
                    || humanoid == null
                    || humanoid.HumanoidSkeleton == null)
                {
                    continue;
                }

                humanoid.AnimationTime += dt;
                HumanoidSkeleton2D skeleton = humanoid.HumanoidSkeleton;

                switch (humanoid.Animation)
                {
                    case HumanoidSkeletonAnimationKind.Walk:
                        HumanoidSkeletonAnimationHelpers.ApplyWalk(skeleton, humanoid.AnimationTime, humanoid.MovementSpeed01);
                        break;
                    case HumanoidSkeletonAnimationKind.Run:
                        HumanoidSkeletonAnimationHelpers.ApplyRun(skeleton, humanoid.AnimationTime);
                        break;
                    case HumanoidSkeletonAnimationKind.Jump:
                        HumanoidSkeletonAnimationHelpers.ApplyJump(skeleton, humanoid.AnimationTime);
                        break;
                    case HumanoidSkeletonAnimationKind.Idle:
                    default:
                        HumanoidSkeletonAnimationHelpers.ApplyIdle(skeleton, humanoid.AnimationTime);
                        break;
                }
            }
        }

        public void Draw(World world, GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
        }
    }
}
