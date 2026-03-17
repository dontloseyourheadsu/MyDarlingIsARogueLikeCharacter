using Microsoft.Xna.Framework;

namespace RoguelikeDarling.Core.UI.Buttons
{
    public sealed class ButtonStyle
    {
        public Color FillColor { get; set; } = Color.Black;

        public Color BorderColor { get; set; } = Color.White;

        public Color ContentColor { get; set; } = Color.White;

        public int BorderThickness { get; set; } = 2;

        public static ButtonStyle Default => new ButtonStyle();
    }
}
