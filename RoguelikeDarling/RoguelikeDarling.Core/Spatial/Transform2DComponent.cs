using Microsoft.Xna.Framework;

namespace RoguelikeDarling.Core.Spatial
{
    public sealed class Transform2DComponent
    {
        public Vector2 Position { get; set; }

        public Vector2 Scale { get; set; } = Vector2.One;
    }
}