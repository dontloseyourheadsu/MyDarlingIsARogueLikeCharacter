using Microsoft.Xna.Framework;

namespace RoguelikeDarling.Core.RigidBody2D
{
    public sealed class RigidBody2DComponent
    {
        public Vector2 Velocity { get; set; }

        public bool IsStatic { get; set; }
    }
}