using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace RoguelikeDarling.Core.UI.Buttons
{
    public sealed class IconButton
    {
        public IconButton(string iconText, Rectangle bounds, ButtonStyle style = null)
        {
            IconText = iconText;
            Bounds = bounds;
            Style = style ?? ButtonStyle.Default;
        }

        public string IconText { get; set; }

        public Rectangle Bounds { get; set; }

        public ButtonStyle Style { get; }

        public bool IsHovered(MouseState mouseState)
        {
            return Bounds.Contains(mouseState.Position);
        }

        public bool WasClicked(MouseState current, MouseState previous)
        {
            return IsHovered(current)
                && current.LeftButton == ButtonState.Pressed
                && previous.LeftButton == ButtonState.Released;
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D pixelTexture, SpriteFont font)
        {
            spriteBatch.Draw(pixelTexture, Bounds, Style.FillColor);
            DrawBorder(spriteBatch, pixelTexture, Bounds, Style.BorderColor, Style.BorderThickness);

            Vector2 iconSize = font.MeasureString(IconText);
            var iconPosition = new Vector2(
                Bounds.X + (Bounds.Width - iconSize.X) * 0.5f,
                Bounds.Y + (Bounds.Height - iconSize.Y) * 0.5f);

            spriteBatch.DrawString(font, IconText, iconPosition, Style.ContentColor);
        }

        private static void DrawBorder(SpriteBatch spriteBatch, Texture2D pixelTexture, Rectangle rect, Color color, int thickness)
        {
            spriteBatch.Draw(pixelTexture, new Rectangle(rect.X, rect.Y, rect.Width, thickness), color);
            spriteBatch.Draw(pixelTexture, new Rectangle(rect.X, rect.Bottom - thickness, rect.Width, thickness), color);
            spriteBatch.Draw(pixelTexture, new Rectangle(rect.X, rect.Y, thickness, rect.Height), color);
            spriteBatch.Draw(pixelTexture, new Rectangle(rect.Right - thickness, rect.Y, thickness, rect.Height), color);
        }
    }
}
