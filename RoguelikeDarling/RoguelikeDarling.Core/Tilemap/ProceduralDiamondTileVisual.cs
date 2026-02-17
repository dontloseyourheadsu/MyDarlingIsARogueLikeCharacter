using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RoguelikeDarling.Core.Tilemap
{
    public sealed class ProceduralDiamondTileVisual : ITileVisual
    {
        private readonly Color topColor;
        private readonly Color borderColor;

        public ProceduralDiamondTileVisual(Color topColor)
            : this(topColor, Color.Black)
        {
        }

        public ProceduralDiamondTileVisual(Color topColor, Color borderColor)
        {
            this.topColor = topColor;
            this.borderColor = borderColor;
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D pixelTexture, Vector2 screenPosition, Point tileSize, float layerDepth)
        {
            int width = tileSize.X;
            int height = tileSize.Y;
            int halfWidth = width / 2;
            int halfHeight = height / 2;

            int startX = (int)screenPosition.X;
            int startY = (int)screenPosition.Y;

            for (int y = 0; y < height; y++)
            {
                int diamondWidth = y <= halfHeight
                    ? (y * width) / halfHeight
                    : ((height - y) * width) / halfHeight;

                if (diamondWidth < 2)
                {
                    diamondWidth = 2;
                }

                int rowStartX = startX + halfWidth - (diamondWidth / 2);
                var fillRect = new Rectangle(rowStartX, startY + y, diamondWidth, 1);
                spriteBatch.Draw(pixelTexture, fillRect, null, topColor, 0f, Vector2.Zero, SpriteEffects.None, layerDepth);

                var leftBorder = new Rectangle(rowStartX, startY + y, 1, 1);
                var rightBorder = new Rectangle(rowStartX + diamondWidth - 1, startY + y, 1, 1);
                spriteBatch.Draw(pixelTexture, leftBorder, null, borderColor, 0f, Vector2.Zero, SpriteEffects.None, layerDepth);
                spriteBatch.Draw(pixelTexture, rightBorder, null, borderColor, 0f, Vector2.Zero, SpriteEffects.None, layerDepth);
            }
        }
    }
}
