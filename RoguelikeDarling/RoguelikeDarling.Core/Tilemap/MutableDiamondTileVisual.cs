using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RoguelikeDarling.Core.Tilemap
{
    public sealed class MutableDiamondTileVisual : ITileVisual
    {
        public MutableDiamondTileVisual(Color fillColor, Color borderColor)
        {
            FillColor = fillColor;
            BorderColor = borderColor;
        }

        public Color FillColor { get; set; }

        public Color BorderColor { get; set; }

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
                spriteBatch.Draw(pixelTexture, fillRect, null, FillColor, 0f, Vector2.Zero, SpriteEffects.None, layerDepth);

                var leftBorder = new Rectangle(rowStartX, startY + y, 1, 1);
                var rightBorder = new Rectangle(rowStartX + diamondWidth - 1, startY + y, 1, 1);
                spriteBatch.Draw(pixelTexture, leftBorder, null, BorderColor, 0f, Vector2.Zero, SpriteEffects.None, layerDepth);
                spriteBatch.Draw(pixelTexture, rightBorder, null, BorderColor, 0f, Vector2.Zero, SpriteEffects.None, layerDepth);
            }
        }
    }
}
