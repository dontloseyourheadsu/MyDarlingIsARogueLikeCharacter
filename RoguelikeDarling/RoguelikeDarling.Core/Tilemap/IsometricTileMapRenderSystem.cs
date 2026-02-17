using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RoguelikeDarling.Core.Camera;
using RoguelikeDarling.Core.Ecs;

namespace RoguelikeDarling.Core.Tilemap
{
    public sealed class IsometricTileMapRenderSystem : IGameSystem
    {
        private readonly Texture2D pixelTexture;

        public IsometricTileMapRenderSystem(GraphicsDevice graphicsDevice)
        {
            pixelTexture = new Texture2D(graphicsDevice, 1, 1);
            pixelTexture.SetData(new[] { Color.White });
        }

        public void Update(World world, GameTime gameTime)
        {
        }

        public void Draw(World world, GameTime gameTime, SpriteBatch spriteBatch)
        {
            Camera2DComponent camera = null;
            TilePaletteComponent palette = null;
            var layers = new List<TileMapLayerComponent>();

            for (int i = 0; i < world.Entities.Count; i++)
            {
                Entity entity = world.Entities[i];

                if (camera == null && entity.TryGetComponent<Camera2DComponent>(out Camera2DComponent cameraComponent))
                {
                    camera = cameraComponent;
                }

                if (palette == null && entity.TryGetComponent<TilePaletteComponent>(out TilePaletteComponent paletteComponent))
                {
                    palette = paletteComponent;
                }

                if (entity.TryGetComponent<TileMapLayerComponent>(out TileMapLayerComponent layer) && layer != null)
                {
                    layers.Add(layer);
                }
            }

            if (camera == null || palette == null)
            {
                return;
            }

            layers.Sort((a, b) => a.ZIndex.CompareTo(b.ZIndex));

            var drawCommands = new List<TileDrawCommand>();
            for (int layerIndex = 0; layerIndex < layers.Count; layerIndex++)
            {
                BuildLayerCommands(layers[layerIndex], palette, drawCommands);
            }

            drawCommands.Sort((a, b) =>
            {
                int sort = a.SortKey.CompareTo(b.SortKey);
                if (sort != 0)
                {
                    return sort;
                }

                return a.LayerZIndex.CompareTo(b.LayerZIndex);
            });

            spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.PointClamp,
                DepthStencilState.None,
                RasterizerState.CullNone,
                effect: null,
                transformMatrix: camera.GetViewMatrix());

            for (int i = 0; i < drawCommands.Count; i++)
            {
                TileDrawCommand drawCommand = drawCommands[i];
                drawCommand.Visual.Draw(spriteBatch, pixelTexture, drawCommand.ScreenPosition, drawCommand.TileSize, 0f);
            }

            spriteBatch.End();
        }

        private static void BuildLayerCommands(TileMapLayerComponent layer, TilePaletteComponent palette, List<TileDrawCommand> drawCommands)
        {
            if (layer.RenderOrder != TileRenderOrder.IsometricDiamondDownHorizontalOffset)
            {
                return;
            }

            float halfWidth = layer.TileStride.X * 0.5f;
            float halfHeight = layer.TileStride.Y * 0.5f;
            int insertionCounter = 0;

            for (int y = 0; y < layer.Height; y++)
            {
                for (int x = 0; x < layer.Width; x++)
                {
                    int tileId = layer.GetTile(x, y);
                    if (tileId == 0 || !palette.TryGet(tileId, out TileDefinition definition) || definition == null)
                    {
                        continue;
                    }

                    float screenX = (x - y) * halfWidth + layer.Offset.X;
                    float screenY = (x + y) * halfHeight + layer.Offset.Y;

                    float sortKey = layer.YSortEnabled
                        ? screenY + (layer.ZIndex * 100000f)
                        : (layer.ZIndex * 100000f) + insertionCounter;

                    drawCommands.Add(new TileDrawCommand(
                        definition.Visual,
                        new Vector2(screenX, screenY),
                        layer.TileSize,
                        sortKey,
                        layer.ZIndex));

                    insertionCounter++;
                }
            }
        }

        private readonly struct TileDrawCommand
        {
            public TileDrawCommand(ITileVisual visual, Vector2 screenPosition, Point tileSize, float sortKey, int layerZIndex)
            {
                Visual = visual;
                ScreenPosition = screenPosition;
                TileSize = tileSize;
                SortKey = sortKey;
                LayerZIndex = layerZIndex;
            }

            public ITileVisual Visual { get; }

            public Vector2 ScreenPosition { get; }

            public Point TileSize { get; }

            public float SortKey { get; }

            public int LayerZIndex { get; }
        }
    }
}
