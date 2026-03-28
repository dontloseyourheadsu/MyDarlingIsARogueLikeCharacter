using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace RoguelikeDarling.Core.Skeleton2D.Humanoid
{
    public sealed class HumanoidSkeletonSkin2D
    {
        private readonly Dictionary<string, SkeletonPartVisual2D> visualsByMember;

        public HumanoidSkeletonSkin2D()
        {
            visualsByMember = new Dictionary<string, SkeletonPartVisual2D>();
        }

        public void SetVisual(string memberName, SkeletonPartVisual2D visual)
        {
            visualsByMember[memberName] = visual;
        }

        public SkeletonPartVisual2D GetVisualOrDefault(string memberName, Color defaultColor)
        {
            if (visualsByMember.TryGetValue(memberName, out SkeletonPartVisual2D visual) && visual != null)
            {
                return visual;
            }

            return SkeletonPartVisual2D.CreateSolid(defaultColor);
        }

        public static HumanoidSkeletonSkin2D CreateDefaultSolid()
        {
            var skin = new HumanoidSkeletonSkin2D();
            skin.SetVisual(HumanoidSkeleton2D.Body, SkeletonPartVisual2D.CreateSolid(new Color(190, 170, 150)));
            skin.SetVisual(HumanoidSkeleton2D.Head, SkeletonPartVisual2D.CreateSolid(new Color(230, 210, 180)));
            skin.SetVisual(HumanoidSkeleton2D.LeftUpperArm, SkeletonPartVisual2D.CreateSolid(new Color(205, 185, 165)));
            skin.SetVisual(HumanoidSkeleton2D.LeftLowerArm, SkeletonPartVisual2D.CreateSolid(new Color(210, 190, 170)));
            skin.SetVisual(HumanoidSkeleton2D.RightUpperArm, SkeletonPartVisual2D.CreateSolid(new Color(205, 185, 165)));
            skin.SetVisual(HumanoidSkeleton2D.RightLowerArm, SkeletonPartVisual2D.CreateSolid(new Color(210, 190, 170)));
            skin.SetVisual(HumanoidSkeleton2D.LeftUpperLeg, SkeletonPartVisual2D.CreateSolid(new Color(170, 155, 140)));
            skin.SetVisual(HumanoidSkeleton2D.LeftLowerLeg, SkeletonPartVisual2D.CreateSolid(new Color(165, 150, 135)));
            skin.SetVisual(HumanoidSkeleton2D.RightUpperLeg, SkeletonPartVisual2D.CreateSolid(new Color(170, 155, 140)));
            skin.SetVisual(HumanoidSkeleton2D.RightLowerLeg, SkeletonPartVisual2D.CreateSolid(new Color(165, 150, 135)));
            return skin;
        }
    }
}
