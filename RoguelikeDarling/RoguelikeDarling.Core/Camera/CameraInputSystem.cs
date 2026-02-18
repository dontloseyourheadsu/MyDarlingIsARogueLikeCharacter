using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using RoguelikeDarling.Core.Ecs;
using RoguelikeDarling.Core.Input;
using RoguelikeDarling.Core.Physics2D;
using RoguelikeDarling.Core.Spatial;

namespace RoguelikeDarling.Core.Camera
{
    public sealed class CameraInputSystem : IGameSystem
    {
        private KeyboardState previousKeyboardState;

        public void Update(World world, GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            KeyboardState keyboardState = Keyboard.GetState();
            bool attachPressedThisFrame = keyboardState.IsKeyDown(Keys.F) && !previousKeyboardState.IsKeyDown(Keys.F);
            bool detachPressedThisFrame = keyboardState.IsKeyDown(Keys.G) && !previousKeyboardState.IsKeyDown(Keys.G);
            Entity playerEntity = FindFirstPlayerRigidBodyEntity(world);

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

                if (attachPressedThisFrame && playerEntity != null)
                {
                    controller.AttachToRigidBody(playerEntity);
                }

                if (detachPressedThisFrame)
                {
                    controller.DetachFromRigidBody();
                }

                if (controller.AttachedRigidBodyEntityId.HasValue
                    && TryGetEntityById(world, controller.AttachedRigidBodyEntityId.Value, out Entity attachedEntity)
                    && attachedEntity.TryGetComponent<Transform2DComponent>(out Transform2DComponent attachedTransform)
                    && attachedEntity.TryGetComponent<RigidBody2DComponent>(out RigidBody2DComponent attachedRigidBody)
                    && attachedTransform != null
                    && attachedRigidBody != null)
                {
                    camera.Position = attachedTransform.Position + controller.FollowOffset;
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

            previousKeyboardState = keyboardState;
        }

        public void Draw(World world, GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
        }

        private static Entity FindFirstPlayerRigidBodyEntity(World world)
        {
            for (int i = 0; i < world.Entities.Count; i++)
            {
                Entity entity = world.Entities[i];
                if (entity.TryGetComponent<PlayerControllerComponent>(out PlayerControllerComponent playerController)
                    && playerController != null
                    && entity.TryGetComponent<RigidBody2DComponent>(out RigidBody2DComponent rigidBody)
                    && rigidBody != null)
                {
                    return entity;
                }
            }

            return null;
        }

        private static bool TryGetEntityById(World world, int entityId, out Entity entity)
        {
            for (int i = 0; i < world.Entities.Count; i++)
            {
                Entity candidate = world.Entities[i];
                if (candidate.Id == entityId)
                {
                    entity = candidate;
                    return true;
                }
            }

            entity = null;
            return false;
        }
    }
}