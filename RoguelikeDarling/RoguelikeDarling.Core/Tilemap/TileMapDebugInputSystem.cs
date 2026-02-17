using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using RoguelikeDarling.Core.Ecs;

namespace RoguelikeDarling.Core.Tilemap
{
    public sealed class TileMapDebugInputSystem : IGameSystem
    {
        private KeyboardState previousKeyboard;
        private int selectedLayerIndex;

        public void Update(World world, GameTime gameTime)
        {
            KeyboardState keyboardState = Keyboard.GetState();
            var layers = CollectLayers(world);

            if (layers.Count == 0)
            {
                previousKeyboard = keyboardState;
                return;
            }

            if (selectedLayerIndex >= layers.Count)
            {
                selectedLayerIndex = layers.Count - 1;
            }

            for (int number = 1; number <= 9; number++)
            {
                Keys key = Keys.D0 + number;
                if (WasJustPressed(keyboardState, key) && number - 1 < layers.Count)
                {
                    selectedLayerIndex = number - 1;
                }
            }

            TileMapLayerComponent selectedLayer = layers[selectedLayerIndex];

            Vector2 offsetDelta = Vector2.Zero;
            float nudgeAmount = 20f;

            if (keyboardState.IsKeyDown(Keys.J))
            {
                offsetDelta.X -= nudgeAmount;
            }

            if (keyboardState.IsKeyDown(Keys.L))
            {
                offsetDelta.X += nudgeAmount;
            }

            if (keyboardState.IsKeyDown(Keys.I))
            {
                offsetDelta.Y -= nudgeAmount;
            }

            if (keyboardState.IsKeyDown(Keys.K))
            {
                offsetDelta.Y += nudgeAmount;
            }

            if (offsetDelta != Vector2.Zero)
            {
                selectedLayer.Offset += offsetDelta;
            }

            if (WasJustPressed(keyboardState, Keys.Y))
            {
                selectedLayer.YSortEnabled = !selectedLayer.YSortEnabled;
            }

            bool shiftHeld = keyboardState.IsKeyDown(Keys.LeftShift) || keyboardState.IsKeyDown(Keys.RightShift);

            if (WasJustPressed(keyboardState, Keys.OemPlus) || WasJustPressed(keyboardState, Keys.Add))
            {
                if (shiftHeld)
                {
                    selectedLayer.TileStride = new Point(selectedLayer.TileStride.X + 2, selectedLayer.TileStride.Y + 2);
                }
                else
                {
                    selectedLayer.TileSize = new Point(selectedLayer.TileSize.X + 2, selectedLayer.TileSize.Y + 2);
                }
            }

            if (WasJustPressed(keyboardState, Keys.OemMinus) || WasJustPressed(keyboardState, Keys.Subtract))
            {
                if (shiftHeld)
                {
                    int strideWidth = selectedLayer.TileStride.X - 2;
                    int strideHeight = selectedLayer.TileStride.Y - 2;
                    if (strideWidth >= 4 && strideHeight >= 4)
                    {
                        selectedLayer.TileStride = new Point(strideWidth, strideHeight);
                    }
                }
                else
                {
                    int width = selectedLayer.TileSize.X - 2;
                    int height = selectedLayer.TileSize.Y - 2;
                    if (width >= 8 && height >= 8)
                    {
                        selectedLayer.TileSize = new Point(width, height);
                    }
                }
            }

            if (WasJustPressed(keyboardState, Keys.PageUp))
            {
                selectedLayer.ZIndex += 1;
            }

            if (WasJustPressed(keyboardState, Keys.PageDown))
            {
                selectedLayer.ZIndex -= 1;
            }

            previousKeyboard = keyboardState;
        }

        public void Draw(World world, GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
        }

        private bool WasJustPressed(KeyboardState current, Keys key)
        {
            return current.IsKeyDown(key) && !previousKeyboard.IsKeyDown(key);
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
    }
}
