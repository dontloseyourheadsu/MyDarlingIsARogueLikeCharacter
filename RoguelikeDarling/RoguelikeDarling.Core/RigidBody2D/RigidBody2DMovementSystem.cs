using Microsoft.Xna.Framework;
using RoguelikeDarling.Core.Ecs;

namespace RoguelikeDarling.Core.RigidBody2D
{
    public sealed class RigidBody2DMovementSystem : IGameSystem
    {
        public void Update(World world, GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            for (int i = 0; i < world.Entities.Count; i++)
            {
                Entity entity = world.Entities[i];
                if (!entity.TryGetComponent<Transform2DComponent>(out Transform2DComponent transform)
                    || !entity.TryGetComponent<RigidBody2DComponent>(out RigidBody2DComponent rigidBody)
                    || transform == null
                    || rigidBody == null
                    || rigidBody.IsStatic)
                {
                    continue;
                }

                transform.Position += rigidBody.Velocity * dt;
            }
        }

        public void Draw(World world, GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
        }
    }
}