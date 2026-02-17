using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using RoguelikeDarling.Core.Ecs;
using RoguelikeDarling.Core.Physics2D;

namespace RoguelikeDarling.Core.Input
{
    public sealed class PlayerMovementInputSystem : IGameSystem
    {
        public void Update(World world, GameTime gameTime)
        {
            KeyboardState keyboardState = Keyboard.GetState();

            for (int i = 0; i < world.Entities.Count; i++)
            {
                Entity entity = world.Entities[i];
                if (!entity.TryGetComponent<PlayerControllerComponent>(out PlayerControllerComponent controller)
                    || !entity.TryGetComponent<RigidBody2DComponent>(out RigidBody2DComponent rigidBody)
                    || controller == null
                    || rigidBody == null
                    || rigidBody.IsStatic)
                {
                    continue;
                }

                Vector2 direction = Vector2.Zero;

                if (keyboardState.IsKeyDown(Keys.A))
                {
                    direction.X -= 1f;
                }

                if (keyboardState.IsKeyDown(Keys.D))
                {
                    direction.X += 1f;
                }

                if (keyboardState.IsKeyDown(Keys.W))
                {
                    direction.Y -= 1f;
                }

                if (keyboardState.IsKeyDown(Keys.S))
                {
                    direction.Y += 1f;
                }

                if (direction != Vector2.Zero)
                {
                    direction.Normalize();
                }

                rigidBody.Velocity = direction * controller.MoveSpeedPixelsPerSecond;
            }
        }

        public void Draw(World world, GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
        }
    }
}