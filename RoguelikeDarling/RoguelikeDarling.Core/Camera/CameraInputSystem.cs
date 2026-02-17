using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using RoguelikeDarling.Core.Ecs;

namespace RoguelikeDarling.Core.Camera
{
    public sealed class CameraInputSystem : IGameSystem
    {
        public void Update(World world, GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            KeyboardState keyboardState = Keyboard.GetState();

            for (int i = 0; i < world.Entities.Count; i++)
            {
                Entity entity = world.Entities[i];
                if (!entity.TryGetComponent<Camera2DComponent>(out Camera2DComponent camera)
                    || !entity.TryGetComponent<CameraControllerComponent>(out CameraControllerComponent controller)
                    || camera == null
                    || controller == null)
                {
                    continue;
                }

                Vector2 direction = Vector2.Zero;

                if (keyboardState.IsKeyDown(Keys.Left))
                {
                    direction.X -= 1f;
                }

                if (keyboardState.IsKeyDown(Keys.Right))
                {
                    direction.X += 1f;
                }

                if (keyboardState.IsKeyDown(Keys.Up))
                {
                    direction.Y -= 1f;
                }

                if (keyboardState.IsKeyDown(Keys.Down))
                {
                    direction.Y += 1f;
                }

                if (direction != Vector2.Zero)
                {
                    direction.Normalize();
                }

                float speed = controller.MoveSpeedPixelsPerSecond;
                if (keyboardState.IsKeyDown(Keys.LeftShift) || keyboardState.IsKeyDown(Keys.RightShift))
                {
                    speed *= 2f;
                }

                camera.Position += direction * speed * dt;
            }
        }

        public void Draw(World world, GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
        }
    }
}