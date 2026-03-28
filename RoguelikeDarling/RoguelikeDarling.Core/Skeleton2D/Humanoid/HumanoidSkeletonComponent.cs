using System.Collections.Generic;
using RoguelikeDarling.Core.Ecs;

namespace RoguelikeDarling.Core.Skeleton2D.Humanoid
{
    public sealed class HumanoidSkeletonComponent
    {
        public HumanoidSkeletonComponent(HumanoidSkeleton2D humanoidSkeleton)
        {
            HumanoidSkeleton = humanoidSkeleton;
            PartBindings = new Dictionary<string, HumanoidSkeletonPartBinding>();
        }

        public HumanoidSkeleton2D HumanoidSkeleton { get; }

        public HumanoidSkeletonAnimationKind Animation { get; set; } = HumanoidSkeletonAnimationKind.Idle;

        public float AnimationTime { get; set; }

        public float MovementSpeed01 { get; set; }

        public Microsoft.Xna.Framework.Vector2 SkeletonLocalOffset { get; set; }

        public Microsoft.Xna.Framework.Vector2 CollisionLocalOffset { get; set; }

        public int CollisionLayer { get; set; } = 2;

        public uint CollisionMask { get; set; } = 1u << 1;

        public bool CollisionDebugEnabled { get; set; }

        public Microsoft.Xna.Framework.Color CollisionDebugColor { get; set; } = new Microsoft.Xna.Framework.Color(220, 30, 30);

        public Dictionary<string, HumanoidSkeletonPartBinding> PartBindings { get; }
    }

    public sealed class HumanoidSkeletonPartBinding
    {
        public Entity VisualEntity { get; set; }

        public Entity CollisionEntity { get; set; }
    }
}
