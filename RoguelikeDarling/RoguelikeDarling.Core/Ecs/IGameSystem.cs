using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RoguelikeDarling.Core.Ecs
{
    public interface IGameSystem
    {
        void Update(World world, GameTime gameTime);

        void Draw(World world, GameTime gameTime, SpriteBatch spriteBatch);
    }
}
