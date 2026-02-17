using System.Collections.Generic;

namespace RoguelikeDarling.Core.Tilemap
{
    public sealed class TilePaletteComponent
    {
        private readonly Dictionary<int, TileDefinition> definitions;

        public TilePaletteComponent()
        {
            definitions = new Dictionary<int, TileDefinition>();
        }

        public void Register(TileDefinition definition)
        {
            definitions[definition.Id] = definition;
        }

        public bool TryGet(int tileId, out TileDefinition definition)
        {
            if (definitions.TryGetValue(tileId, out TileDefinition result))
            {
                definition = result;
                return true;
            }

            definition = null;
            return false;
        }
    }
}
