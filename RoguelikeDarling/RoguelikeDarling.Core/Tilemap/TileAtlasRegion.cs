using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RoguelikeDarling.Core.Tilemap
{
    public sealed class TileAtlasRegion
    {
        public TileAtlasRegion(Texture2D texture, string assetPath, Rectangle sourceRect)
        {
            Texture = texture;
            AssetPath = assetPath;
            SourceRect = sourceRect;
        }

        public Texture2D Texture { get; }

        public string AssetPath { get; }

        public Rectangle SourceRect { get; }
    }
}
