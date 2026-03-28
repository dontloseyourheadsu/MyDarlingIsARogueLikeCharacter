using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RoguelikeDarling.Core.Skeleton2D.Humanoid
{
    public sealed class SkeletonPartRender2DComponent
    {
        public Texture2D Texture { get; set; }

        public Rectangle? SourceRect { get; set; }

        public Color Color { get; set; } = Color.White;

        public Vector2 Size { get; set; } = new Vector2(10f, 10f);

        public Vector2 Pivot { get; set; } = new Vector2(5f, 5f);

        public int ZIndex { get; set; }

        public bool Visible { get; set; } = true;
    }
}
