using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RoguelikeDarling.Core.Ecs
{
    public sealed class World
    {
        private readonly List<Entity> entities;
        private readonly List<IGameSystem> systems;
        private int nextEntityId;

        public World()
        {
            entities = new List<Entity>();
            systems = new List<IGameSystem>();
            nextEntityId = 1;
        }

        public Entity CreateEntity()
        {
            var entity = new Entity(nextEntityId++);
            entities.Add(entity);
            return entity;
        }

        public IReadOnlyList<Entity> Entities => entities;

        public void AddSystem(IGameSystem system)
        {
            systems.Add(system);
        }

        public void Update(GameTime gameTime)
        {
            for (int i = 0; i < systems.Count; i++)
            {
                systems[i].Update(this, gameTime);
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            for (int i = 0; i < systems.Count; i++)
            {
                systems[i].Draw(this, gameTime, spriteBatch);
            }
        }
    }
}
