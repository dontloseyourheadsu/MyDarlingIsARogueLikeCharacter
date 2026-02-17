using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RoguelikeDarling.Core.Tilemap
{
    public interface ITileVisual
    {
        void Draw(SpriteBatch spriteBatch, Texture2D pixelTexture, Vector2 screenPosition, Point tileSize, float layerDepth);
    }
}
