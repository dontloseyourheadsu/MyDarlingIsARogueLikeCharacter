using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RoguelikeDarling.Core.Tilemap
{
    public sealed class AtlasTileVisual : ITileVisual
    {
        private readonly TileAtlasRegion atlasRegion;
        private readonly Color tint;

        public AtlasTileVisual(TileAtlasRegion atlasRegion)
            : this(atlasRegion, Color.White)
        {
        }

        public AtlasTileVisual(TileAtlasRegion atlasRegion, Color tint)
        {
            this.atlasRegion = atlasRegion;
            this.tint = tint;
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D pixelTexture, Vector2 screenPosition, Point tileSize, float layerDepth)
        {
            var destination = new Rectangle(
                (int)screenPosition.X,
                (int)screenPosition.Y,
                tileSize.X,
                tileSize.Y);

            spriteBatch.Draw(
                atlasRegion.Texture,
                destination,
                atlasRegion.SourceRect,
                tint,
                0f,
                Vector2.Zero,
                SpriteEffects.None,
                layerDepth);
        }
    }
}
