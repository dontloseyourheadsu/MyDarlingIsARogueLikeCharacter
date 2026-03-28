using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RoguelikeDarling.Core.Skeleton2D
{
    public sealed class SkeletonPartVisual2D
    {
        public Texture2D Texture { get; set; }

        public Rectangle? SourceRect { get; set; }

        public Color Color { get; set; } = Color.White;

        public static SkeletonPartVisual2D CreateSolid(Color color)
        {
            return new SkeletonPartVisual2D
            {
                Texture = null,
                SourceRect = null,
                Color = color,
            };
        }

        public static SkeletonPartVisual2D CreateTexture(Texture2D texture, Rectangle? sourceRect = null, Color? color = null)
        {
            return new SkeletonPartVisual2D
            {
                Texture = texture,
                SourceRect = sourceRect,
                Color = color ?? Color.White,
            };
        }
    }
}
