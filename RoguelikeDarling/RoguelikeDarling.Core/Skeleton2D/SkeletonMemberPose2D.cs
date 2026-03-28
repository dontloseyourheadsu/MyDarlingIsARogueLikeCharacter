using Microsoft.Xna.Framework;

namespace RoguelikeDarling.Core.Skeleton2D
{
    public sealed class SkeletonMemberPose2D
    {
        public Vector2 LocalJointOffset { get; set; }

        public float LocalRotation { get; set; }

        public Vector2 LocalScale { get; set; } = Vector2.One;
    }
}
