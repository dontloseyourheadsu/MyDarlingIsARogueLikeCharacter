using Microsoft.Xna.Framework;

namespace RoguelikeDarling.Core.Skeleton2D.Humanoid
{
    public sealed class HumanoidSkeleton2D
    {
        public const string Body = "Body";
        public const string Head = "Head";

        public const string LeftUpperArm = "LeftUpperArm";
        public const string LeftLowerArm = "LeftLowerArm";
        public const string RightUpperArm = "RightUpperArm";
        public const string RightLowerArm = "RightLowerArm";

        public const string LeftUpperLeg = "LeftUpperLeg";
        public const string LeftLowerLeg = "LeftLowerLeg";
        public const string RightUpperLeg = "RightUpperLeg";
        public const string RightLowerLeg = "RightLowerLeg";

        public HumanoidSkeleton2D(HumanoidSkeletonSkin2D skin = null)
        {
            skin ??= HumanoidSkeletonSkin2D.CreateDefaultSolid();
            Skeleton = new Skeleton2DBase();

            Skeleton.RegisterMember(new SkeletonMemberDefinition2D(
                Body,
                parentName: null,
                bindJointOffset: Vector2.Zero,
                size: new Vector2(28f, 44f),
                pivot: new Vector2(14f, 8f),
                collisionSize: new Vector2(24f, 40f),
                zIndex: 20,
                visual: skin.GetVisualOrDefault(Body, new Color(190, 170, 150))));

            Skeleton.RegisterMember(new SkeletonMemberDefinition2D(
                Head,
                parentName: Body,
                bindJointOffset: new Vector2(0f, -14f),
                size: new Vector2(24f, 22f),
                pivot: new Vector2(12f, 18f),
                collisionSize: new Vector2(20f, 18f),
                zIndex: 30,
                visual: skin.GetVisualOrDefault(Head, new Color(230, 210, 180))));

            Skeleton.RegisterMember(new SkeletonMemberDefinition2D(
                LeftUpperArm,
                parentName: Body,
                bindJointOffset: new Vector2(-12f, -8f),
                size: new Vector2(10f, 24f),
                pivot: new Vector2(5f, 3f),
                collisionSize: new Vector2(8f, 20f),
                zIndex: 16,
                visual: skin.GetVisualOrDefault(LeftUpperArm, new Color(205, 185, 165))));

            Skeleton.RegisterMember(new SkeletonMemberDefinition2D(
                LeftLowerArm,
                parentName: LeftUpperArm,
                bindJointOffset: new Vector2(0f, 19f),
                size: new Vector2(8f, 22f),
                pivot: new Vector2(4f, 2f),
                collisionSize: new Vector2(7f, 18f),
                zIndex: 15,
                visual: skin.GetVisualOrDefault(LeftLowerArm, new Color(210, 190, 170))));

            Skeleton.RegisterMember(new SkeletonMemberDefinition2D(
                RightUpperArm,
                parentName: Body,
                bindJointOffset: new Vector2(12f, -8f),
                size: new Vector2(10f, 24f),
                pivot: new Vector2(5f, 3f),
                collisionSize: new Vector2(8f, 20f),
                zIndex: 18,
                visual: skin.GetVisualOrDefault(RightUpperArm, new Color(205, 185, 165))));

            Skeleton.RegisterMember(new SkeletonMemberDefinition2D(
                RightLowerArm,
                parentName: RightUpperArm,
                bindJointOffset: new Vector2(0f, 19f),
                size: new Vector2(8f, 22f),
                pivot: new Vector2(4f, 2f),
                collisionSize: new Vector2(7f, 18f),
                zIndex: 17,
                visual: skin.GetVisualOrDefault(RightLowerArm, new Color(210, 190, 170))));

            Skeleton.RegisterMember(new SkeletonMemberDefinition2D(
                LeftUpperLeg,
                parentName: Body,
                bindJointOffset: new Vector2(-6f, 22f),
                size: new Vector2(10f, 26f),
                pivot: new Vector2(5f, 2f),
                collisionSize: new Vector2(8f, 22f),
                zIndex: 11,
                visual: skin.GetVisualOrDefault(LeftUpperLeg, new Color(170, 155, 140))));

            Skeleton.RegisterMember(new SkeletonMemberDefinition2D(
                LeftLowerLeg,
                parentName: LeftUpperLeg,
                bindJointOffset: new Vector2(0f, 21f),
                size: new Vector2(9f, 24f),
                pivot: new Vector2(4.5f, 2f),
                collisionSize: new Vector2(8f, 20f),
                zIndex: 10,
                visual: skin.GetVisualOrDefault(LeftLowerLeg, new Color(165, 150, 135))));

            Skeleton.RegisterMember(new SkeletonMemberDefinition2D(
                RightUpperLeg,
                parentName: Body,
                bindJointOffset: new Vector2(6f, 22f),
                size: new Vector2(10f, 26f),
                pivot: new Vector2(5f, 2f),
                collisionSize: new Vector2(8f, 22f),
                zIndex: 13,
                visual: skin.GetVisualOrDefault(RightUpperLeg, new Color(170, 155, 140))));

            Skeleton.RegisterMember(new SkeletonMemberDefinition2D(
                RightLowerLeg,
                parentName: RightUpperLeg,
                bindJointOffset: new Vector2(0f, 21f),
                size: new Vector2(9f, 24f),
                pivot: new Vector2(4.5f, 2f),
                collisionSize: new Vector2(8f, 20f),
                zIndex: 12,
                visual: skin.GetVisualOrDefault(RightLowerLeg, new Color(165, 150, 135))));
        }

        public Skeleton2DBase Skeleton { get; }
    }
}
