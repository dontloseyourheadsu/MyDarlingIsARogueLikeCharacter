using Microsoft.Xna.Framework;

namespace RoguelikeDarling.Core.Camera
{
    public sealed class Camera2DComponent
    {
        public Vector2 Position { get; set; }

        public float Zoom { get; set; } = 1f;

        public Matrix GetViewMatrix()
        {
            return Matrix.CreateTranslation(new Vector3(-Position, 0f)) *
                   Matrix.CreateScale(Zoom, Zoom, 1f);
        }
    }
}