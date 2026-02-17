using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RoguelikeDarling.Core.RigidBody2D
{
    public sealed class RigidBody2DRenderComponent
    {
        public Texture2D Texture { get; set; }

        public Rectangle SourceRect { get; set; }

        public Vector2 LocalPosition { get; set; }

        public Vector2 LocalScale { get; set; } = Vector2.One;

        public Color Tint { get; set; } = Color.White;

        public int ZIndex { get; set; }
    }
}