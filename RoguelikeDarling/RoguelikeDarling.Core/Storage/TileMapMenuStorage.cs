using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RoguelikeDarling.Core.Storage
{
    public sealed class TileMapMenuStorage
    {
        private readonly TileMapEditorStorage editorStorage;

        public TileMapMenuStorage(TileMapEditorStorage editorStorage)
        {
            this.editorStorage = editorStorage ?? throw new ArgumentNullException(nameof(editorStorage));
        }

        public IReadOnlyList<TileMapMenuItem> GetTileMaps()
        {
            if (!Directory.Exists(editorStorage.StorageDirectory))
            {
                return Array.Empty<TileMapMenuItem>();
            }

            var items = new List<TileMapMenuItem>();
            string[] files = Directory.GetFiles(editorStorage.StorageDirectory, "*" + TileMapEditorStorage.Extension, SearchOption.TopDirectoryOnly);

            for (int i = 0; i < files.Length; i++)
            {
                string filePath = files[i];
                if (!editorStorage.TryLoadDocument(filePath, out TileMapBinaryJsonDocument document) || document == null)
                {
                    continue;
                }

                TileMapBinaryJsonLayer previewLayer = document.Layers.FirstOrDefault();
                int[] preview = previewLayer?.TileIds ?? Array.Empty<int>();

                items.Add(new TileMapMenuItem
                {
                    Name = string.IsNullOrWhiteSpace(document.Name) ? Path.GetFileNameWithoutExtension(filePath) : document.Name,
                    FilePath = filePath,
                    LastUpdatedUtc = document.LastUpdatedUtc,
                    Width = previewLayer?.Width ?? 0,
                    Height = previewLayer?.Height ?? 0,
                    PreviewTileIds = preview,
                });
            }

            items.Sort((a, b) => b.LastUpdatedUtc.CompareTo(a.LastUpdatedUtc));
            return items;
        }
    }
}
