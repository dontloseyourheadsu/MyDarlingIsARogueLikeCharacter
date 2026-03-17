namespace RoguelikeDarling.Core.Editors.TileMapEditor
{
    public sealed class TileMapEditorComponent
    {
        public bool IsEnabled { get; set; }

        public bool HasUnsavedChanges { get; set; }

        public bool ShowLeaveToMenuPopup { get; set; }

        public int SelectedLayerIndex { get; set; }

        public int SelectedTileId { get; set; } = 4;

        public string CurrentMapName { get; set; } = "Untitled";
    }
}
