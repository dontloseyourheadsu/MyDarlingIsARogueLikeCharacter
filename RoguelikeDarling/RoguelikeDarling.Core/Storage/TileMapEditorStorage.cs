using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using RoguelikeDarling.Core.Ecs;
using RoguelikeDarling.Core.Tilemap;

namespace RoguelikeDarling.Core.Storage
{
    public sealed class TileMapEditorStorage
    {
        public const string Extension = ".tilemap.bjson";

        private readonly string storageDirectory;

        public TileMapEditorStorage(string storageDirectory = null)
        {
            this.storageDirectory = string.IsNullOrWhiteSpace(storageDirectory)
                ? GetDefaultDirectory()
                : storageDirectory;

            Directory.CreateDirectory(this.storageDirectory);
        }

        public string StorageDirectory => storageDirectory;

        public bool SaveFromWorld(World world, string mapName)
        {
            if (world == null || string.IsNullOrWhiteSpace(mapName))
            {
                return false;
            }

            var layers = CollectLayers(world);
            if (layers.Count == 0)
            {
                return false;
            }

            var document = new TileMapBinaryJsonDocument
            {
                Name = mapName.Trim(),
                LastUpdatedUtc = DateTime.UtcNow,
            };

            for (int i = 0; i < layers.Count; i++)
            {
                document.Layers.Add(ToSerializableLayer(layers[i]));
            }

            string filePath = BuildFilePath(document.Name);
            BinaryJsonFileStore.Save(filePath, document);
            return true;
        }

        public bool TryLoadIntoWorld(World world, string filePath, out string loadedMapName)
        {
            loadedMapName = string.Empty;
            if (world == null || string.IsNullOrWhiteSpace(filePath))
            {
                return false;
            }

            if (!BinaryJsonFileStore.TryLoad(filePath, out TileMapBinaryJsonDocument document) || document == null)
            {
                return false;
            }

            var layersByName = CollectLayersByName(world);
            for (int i = 0; i < document.Layers.Count; i++)
            {
                TileMapBinaryJsonLayer savedLayer = document.Layers[i];
                if (!layersByName.TryGetValue(savedLayer.Name, out TileMapLayerComponent runtimeLayer) || runtimeLayer == null)
                {
                    continue;
                }

                ApplySerializableLayer(savedLayer, runtimeLayer);
            }

            loadedMapName = string.IsNullOrWhiteSpace(document.Name) ? "Untitled" : document.Name;
            return true;
        }

        public bool TryLoadDocument(string filePath, out TileMapBinaryJsonDocument document)
        {
            return BinaryJsonFileStore.TryLoad(filePath, out document);
        }

        public string BuildFilePath(string mapName)
        {
            string safe = SanitizeName(mapName);
            return Path.Combine(storageDirectory, safe + Extension);
        }

        public static string GetDefaultDirectory()
        {
            string local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(local, "RoguelikeDarling", "TileMaps");
        }

        private static string SanitizeName(string name)
        {
            string fallback = string.IsNullOrWhiteSpace(name) ? "Untitled" : name.Trim();
            foreach (char invalid in Path.GetInvalidFileNameChars())
            {
                fallback = fallback.Replace(invalid, '_');
            }

            return fallback;
        }

        private static List<TileMapLayerComponent> CollectLayers(World world)
        {
            var result = new List<TileMapLayerComponent>();
            for (int i = 0; i < world.Entities.Count; i++)
            {
                if (world.Entities[i].TryGetComponent<TileMapLayerComponent>(out TileMapLayerComponent layer) && layer != null)
                {
                    result.Add(layer);
                }
            }

            result.Sort((a, b) => a.ZIndex.CompareTo(b.ZIndex));
            return result;
        }

        private static Dictionary<string, TileMapLayerComponent> CollectLayersByName(World world)
        {
            var result = new Dictionary<string, TileMapLayerComponent>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < world.Entities.Count; i++)
            {
                if (world.Entities[i].TryGetComponent<TileMapLayerComponent>(out TileMapLayerComponent layer) && layer != null)
                {
                    result[layer.Name] = layer;
                }
            }

            return result;
        }

        private static TileMapBinaryJsonLayer ToSerializableLayer(TileMapLayerComponent layer)
        {
            int width = layer.Width;
            int height = layer.Height;
            int[] tileIds = new int[width * height];

            int index = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    tileIds[index++] = layer.GetTile(x, y);
                }
            }

            return new TileMapBinaryJsonLayer
            {
                Name = layer.Name,
                Width = width,
                Height = height,
                TileSizeX = layer.TileSize.X,
                TileSizeY = layer.TileSize.Y,
                TileStrideX = layer.TileStride.X,
                TileStrideY = layer.TileStride.Y,
                ZIndex = layer.ZIndex,
                YSortEnabled = layer.YSortEnabled,
                RenderOrder = (int)layer.RenderOrder,
                OffsetX = layer.Offset.X,
                OffsetY = layer.Offset.Y,
                TileIds = tileIds,
            };
        }

        private static void ApplySerializableLayer(TileMapBinaryJsonLayer source, TileMapLayerComponent target)
        {
            if (source.Width != target.Width || source.Height != target.Height)
            {
                return;
            }

            target.TileSize = new Point(source.TileSizeX, source.TileSizeY);
            target.TileStride = new Point(source.TileStrideX, source.TileStrideY);
            target.ZIndex = source.ZIndex;
            target.YSortEnabled = source.YSortEnabled;
            target.RenderOrder = (TileRenderOrder)source.RenderOrder;
            target.Offset = new Vector2(source.OffsetX, source.OffsetY);

            int expectedCount = source.Width * source.Height;
            if (source.TileIds == null || source.TileIds.Length != expectedCount)
            {
                return;
            }

            int index = 0;
            for (int y = 0; y < source.Height; y++)
            {
                for (int x = 0; x < source.Width; x++)
                {
                    target.SetTile(x, y, source.TileIds[index++]);
                }
            }
        }
    }
}
