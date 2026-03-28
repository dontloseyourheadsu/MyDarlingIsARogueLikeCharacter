using Microsoft.Xna.Framework;

namespace RoguelikeDarling.Core.Skeleton2D
{
    public readonly struct SkeletonMemberWorldPose2D
    {
        public SkeletonMemberWorldPose2D(SkeletonMemberDefinition2D definition, Vector2 jointWorldPosition, float worldRotation, Vector2 worldScale)
        {
            Definition = definition;
            JointWorldPosition = jointWorldPosition;
            WorldRotation = worldRotation;
            WorldScale = worldScale;
        }

        public SkeletonMemberDefinition2D Definition { get; }

        public Vector2 JointWorldPosition { get; }

        public float WorldRotation { get; }

        public Vector2 WorldScale { get; }

        public Vector2 GetCenterWorldPosition()
        {
            return GetCenterWorldPosition(Definition.Size);
        }

        public Vector2 GetCenterWorldPosition(Vector2 sizeOverride)
        {
            Vector2 sourceSize = Definition.Size;
            Vector2 pivot = Definition.Pivot;

            if (sourceSize.X > 0f && sourceSize.Y > 0f
                && sizeOverride.X > 0f && sizeOverride.Y > 0f)
            {
                Vector2 normalizedPivot = new Vector2(pivot.X / sourceSize.X, pivot.Y / sourceSize.Y);
                pivot = new Vector2(normalizedPivot.X * sizeOverride.X, normalizedPivot.Y * sizeOverride.Y);
            }

            Vector2 localCenterOffset = (sizeOverride * 0.5f) - pivot;
            Vector2 scaledCenterOffset = new Vector2(localCenterOffset.X * WorldScale.X, localCenterOffset.Y * WorldScale.Y);

            return JointWorldPosition + Rotate(scaledCenterOffset, WorldRotation);
        }

        private static Vector2 Rotate(Vector2 value, float radians)
        {
            float cos = (float)System.Math.Cos(radians);
            float sin = (float)System.Math.Sin(radians);
            return new Vector2(
                (value.X * cos) - (value.Y * sin),
                (value.X * sin) + (value.Y * cos));
        }
    }
}
