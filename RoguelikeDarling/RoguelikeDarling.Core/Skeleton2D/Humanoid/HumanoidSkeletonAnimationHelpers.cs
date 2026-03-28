using System;
using Microsoft.Xna.Framework;

namespace RoguelikeDarling.Core.Skeleton2D.Humanoid
{
    public static class HumanoidSkeletonAnimationHelpers
    {
        public static void ApplyIdle(HumanoidSkeleton2D humanoid, float time)
        {
            if (humanoid == null)
            {
                return;
            }

            Skeleton2DBase skeleton = humanoid.Skeleton;
            skeleton.ResetAllPoses();

            float sway = (float)Math.Sin(time * 1.4f) * 0.025f;
            float bob = (float)Math.Sin(time * 1.1f) * 1.1f;

            skeleton.SetPose(HumanoidSkeleton2D.Body, new Vector2(0f, bob), sway);
            skeleton.SetPose(HumanoidSkeleton2D.Head, new Vector2(0f, -0.6f * bob), -sway * 1.25f);
        }

        public static void ApplyWalk(HumanoidSkeleton2D humanoid, float time, float speedMultiplier = 1f)
        {
            if (humanoid == null)
            {
                return;
            }

            Skeleton2DBase skeleton = humanoid.Skeleton;
            skeleton.ResetAllPoses();

            float wave = time * MathHelper.Lerp(5.2f, 7.4f, MathHelper.Clamp(speedMultiplier, 0f, 1f));
            float legAngle = (float)Math.Sin(wave) * 0.45f;
            float armAngle = -legAngle * 0.8f;
            float kneeBend = Math.Abs((float)Math.Sin(wave)) * 0.35f;
            float bodyBob = Math.Abs((float)Math.Sin(wave)) * 1.8f;

            skeleton.SetPose(HumanoidSkeleton2D.Body, new Vector2(0f, bodyBob), 0f);
            skeleton.SetPose(HumanoidSkeleton2D.Head, new Vector2(0f, -0.6f * bodyBob), (float)Math.Sin(wave * 0.5f) * 0.08f);

            skeleton.SetPose(HumanoidSkeleton2D.LeftUpperArm, Vector2.Zero, armAngle);
            skeleton.SetPose(HumanoidSkeleton2D.RightUpperArm, Vector2.Zero, -armAngle);
            skeleton.SetPose(HumanoidSkeleton2D.LeftLowerArm, Vector2.Zero, armAngle * 0.55f);
            skeleton.SetPose(HumanoidSkeleton2D.RightLowerArm, Vector2.Zero, -armAngle * 0.55f);

            skeleton.SetPose(HumanoidSkeleton2D.LeftUpperLeg, Vector2.Zero, legAngle);
            skeleton.SetPose(HumanoidSkeleton2D.RightUpperLeg, Vector2.Zero, -legAngle);
            skeleton.SetPose(HumanoidSkeleton2D.LeftLowerLeg, Vector2.Zero, Math.Max(0f, -legAngle) * 0.8f + kneeBend * 0.4f);
            skeleton.SetPose(HumanoidSkeleton2D.RightLowerLeg, Vector2.Zero, Math.Max(0f, legAngle) * 0.8f + kneeBend * 0.4f);
        }

        public static void ApplyRun(HumanoidSkeleton2D humanoid, float time)
        {
            if (humanoid == null)
            {
                return;
            }

            Skeleton2DBase skeleton = humanoid.Skeleton;
            skeleton.ResetAllPoses();

            float wave = time * 9.5f;
            float legAngle = (float)Math.Sin(wave) * 0.85f;
            float armAngle = -legAngle * 0.95f;
            float kneeBend = Math.Abs((float)Math.Sin(wave)) * 0.6f;
            float bodyBob = Math.Abs((float)Math.Sin(wave * 0.5f)) * 2.5f;

            skeleton.SetPose(HumanoidSkeleton2D.Body, new Vector2(0f, bodyBob), 0.08f);
            skeleton.SetPose(HumanoidSkeleton2D.Head, new Vector2(0f, -1.2f), -0.12f + (float)Math.Sin(wave * 0.5f) * 0.06f);

            skeleton.SetPose(HumanoidSkeleton2D.LeftUpperArm, Vector2.Zero, armAngle);
            skeleton.SetPose(HumanoidSkeleton2D.RightUpperArm, Vector2.Zero, -armAngle);
            skeleton.SetPose(HumanoidSkeleton2D.LeftLowerArm, Vector2.Zero, armAngle * 0.7f - 0.1f);
            skeleton.SetPose(HumanoidSkeleton2D.RightLowerArm, Vector2.Zero, -armAngle * 0.7f - 0.1f);

            skeleton.SetPose(HumanoidSkeleton2D.LeftUpperLeg, Vector2.Zero, legAngle);
            skeleton.SetPose(HumanoidSkeleton2D.RightUpperLeg, Vector2.Zero, -legAngle);
            skeleton.SetPose(HumanoidSkeleton2D.LeftLowerLeg, Vector2.Zero, Math.Max(0f, -legAngle) + kneeBend * 0.5f);
            skeleton.SetPose(HumanoidSkeleton2D.RightLowerLeg, Vector2.Zero, Math.Max(0f, legAngle) + kneeBend * 0.5f);
        }

        public static void ApplyJump(HumanoidSkeleton2D humanoid, float time, float height = 16f)
        {
            if (humanoid == null)
            {
                return;
            }

            Skeleton2DBase skeleton = humanoid.Skeleton;
            skeleton.ResetAllPoses();

            float jumpWave = (float)Math.Sin(time * 3f);
            float lift = Math.Max(0f, jumpWave) * height;
            float tuck = Math.Max(0f, -jumpWave);

            skeleton.SetPose(HumanoidSkeleton2D.Body, new Vector2(0f, -lift), 0f);
            skeleton.SetPose(HumanoidSkeleton2D.Head, new Vector2(0f, -2f - (lift * 0.15f)), -0.05f);

            skeleton.SetPose(HumanoidSkeleton2D.LeftUpperArm, Vector2.Zero, -1.15f + tuck * 0.2f);
            skeleton.SetPose(HumanoidSkeleton2D.RightUpperArm, Vector2.Zero, 1.15f - tuck * 0.2f);
            skeleton.SetPose(HumanoidSkeleton2D.LeftLowerArm, Vector2.Zero, -0.45f + tuck * 0.2f);
            skeleton.SetPose(HumanoidSkeleton2D.RightLowerArm, Vector2.Zero, 0.45f - tuck * 0.2f);

            skeleton.SetPose(HumanoidSkeleton2D.LeftUpperLeg, Vector2.Zero, 0.55f - tuck * 0.35f);
            skeleton.SetPose(HumanoidSkeleton2D.RightUpperLeg, Vector2.Zero, -0.55f + tuck * 0.35f);
            skeleton.SetPose(HumanoidSkeleton2D.LeftLowerLeg, Vector2.Zero, 0.95f + tuck * 0.25f);
            skeleton.SetPose(HumanoidSkeleton2D.RightLowerLeg, Vector2.Zero, 0.95f + tuck * 0.25f);
        }
    }
}
