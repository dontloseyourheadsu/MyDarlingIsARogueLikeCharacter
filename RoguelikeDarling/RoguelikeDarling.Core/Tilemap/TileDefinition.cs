namespace RoguelikeDarling.Core.Tilemap
{
    public sealed class TileDefinition
    {
        public TileDefinition(int id, string name, ITileVisual visual)
        {
            Id = id;
            Name = name;
            Visual = visual;
        }

        public int Id { get; }

        public string Name { get; }

        public ITileVisual Visual { get; }
    }
}
