using Microsoft.Xna.Framework;

namespace RoguelikeDarling.Core.Skeleton2D
{
    public sealed class SkeletonMemberDefinition2D
    {
        public SkeletonMemberDefinition2D(
            string name,
            string parentName,
            Vector2 bindJointOffset,
            Vector2 size,
            Vector2 pivot,
            Vector2 collisionSize,
            int zIndex,
            SkeletonPartVisual2D visual)
        {
            Name = name;
            ParentName = parentName;
            BindJointOffset = bindJointOffset;
            Size = size;
            Pivot = pivot;
            CollisionSize = collisionSize;
            ZIndex = zIndex;
            Visual = visual ?? SkeletonPartVisual2D.CreateSolid(Color.White);
        }

        public string Name { get; }

        public string ParentName { get; }

        public Vector2 BindJointOffset { get; }

        public Vector2 Size { get; }

        public Vector2 Pivot { get; }

        public Vector2 CollisionSize { get; }

        public int ZIndex { get; }

        public SkeletonPartVisual2D Visual { get; set; }
    }
}
