using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RoguelikeDarling.Core.Ecs;
using RoguelikeDarling.Core.Editors.TileMapEditor;
using RoguelikeDarling.Core.Storage;
using RoguelikeDarling.Core.Tilemap;
using RoguelikeDarling.Core.UI.Buttons;

namespace RoguelikeDarling.Core.GameSpecific.Menus.TileMap
{
    public sealed class TileMapMenuSystem : IGameSystem
    {
        private const float VirtualWidth = 1280f;
        private const float VirtualHeight = 720f;

        private readonly TileMapEditorStorage editorStorage;
        private readonly TileMapMenuStorage menuStorage;
        private readonly SpriteFont font;
        private readonly Texture2D pixelTexture;
        private readonly GraphicsDevice graphicsDevice;
        private MouseState previousMouse;
        private KeyboardState previousKeyboard;

        public TileMapMenuSystem(TileMapEditorStorage editorStorage, SpriteFont font, GraphicsDevice graphicsDevice)
        {
            this.editorStorage = editorStorage;
            menuStorage = new TileMapMenuStorage(editorStorage);
            this.font = font;
            this.graphicsDevice = graphicsDevice;

            pixelTexture = new Texture2D(graphicsDevice, 1, 1);
            pixelTexture.SetData(new[] { Color.White });
        }

        public void Update(World world, GameTime gameTime)
        {
            MouseState mouse = Mouse.GetState();
            KeyboardState keyboard = Keyboard.GetState();
            Point virtualMouse = ToVirtualPoint(mouse.Position);

            if (!TryGetMenuAndEditor(world, out TileMapMenuComponent menu, out TileMapEditorComponent editor))
            {
                previousMouse = mouse;
                previousKeyboard = keyboard;
                return;
            }

            if (!menu.IsVisible)
            {
                previousMouse = mouse;
                previousKeyboard = keyboard;
                return;
            }

            if (menu.NeedsRefresh || (DateTime.UtcNow - menu.LastRefreshUtc).TotalSeconds >= 2)
            {
                menu.Items.Clear();
                menu.Items.AddRange(menuStorage.GetTileMaps());
                menu.LastRefreshUtc = DateTime.UtcNow;
                menu.NeedsRefresh = false;
            }

            var layout = BuildLayout();

            if (WasVirtualClicked(layout.NewButton, mouse, previousMouse, virtualMouse) || WasJustPressed(keyboard, Keys.N))
            {
                CreateNewMap(world, editor, menu);
            }
            else if (WasVirtualClicked(layout.ContinueButton, mouse, previousMouse, virtualMouse) || WasJustPressed(keyboard, Keys.Enter))
            {
                menu.IsVisible = false;
                editor.IsEnabled = true;
            }
            else if (WasVirtualClicked(layout.RefreshButton, mouse, previousMouse, virtualMouse) || WasJustPressed(keyboard, Keys.R))
            {
                menu.NeedsRefresh = true;
            }

            int startY = layout.ListStartY;
            for (int i = 0; i < menu.Items.Count; i++)
            {
                Rectangle card = new Rectangle(layout.ListX, startY + i * (layout.CardHeight + layout.CardSpacing), layout.ListWidth, layout.CardHeight);
                if (WasVirtualClicked(card, mouse, previousMouse, virtualMouse))
                {
                    TileMapMenuItem selected = menu.Items[i];
                    if (editorStorage.TryLoadIntoWorld(world, selected.FilePath, out string loadedName))
                    {
                        editor.CurrentMapName = loadedName;
                        editor.HasUnsavedChanges = false;
                        editor.ShowLeaveToMenuPopup = false;
                        editor.SelectedLayerIndex = 0;
                        menu.IsVisible = false;
                        editor.IsEnabled = true;
                        break;
                    }
                }
            }

            previousMouse = mouse;
            previousKeyboard = keyboard;
        }

        public void Draw(World world, GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (!TryGetMenuAndEditor(world, out TileMapMenuComponent menu, out TileMapEditorComponent editor) || !menu.IsVisible)
            {
                return;
            }

            float uiScale = GetUiScale();
            var layout = BuildLayout();

            spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.PointClamp,
                DepthStencilState.None,
                RasterizerState.CullNone,
                effect: null,
                transformMatrix: Matrix.CreateScale(uiScale));

            spriteBatch.Draw(pixelTexture, new Rectangle(0, 0, 1280, 720), new Color(4, 4, 6, 255));

            spriteBatch.Draw(pixelTexture, layout.HeaderPanel, new Color(14, 14, 16, 255));
            DrawBorder(spriteBatch, layout.HeaderPanel, 2, Color.White);

            new TextButton("New Tilemap", layout.NewButton).Draw(spriteBatch, pixelTexture, font);
            new TextButton("Continue", layout.ContinueButton).Draw(spriteBatch, pixelTexture, font);
            new IconButton("R", layout.RefreshButton).Draw(spriteBatch, pixelTexture, font);

            spriteBatch.DrawString(font, "Tilemap Menu", new Vector2(layout.HeaderPanel.X + 420f, layout.HeaderPanel.Y + 16f), Color.White);
            spriteBatch.DrawString(font, $"Current: {editor.CurrentMapName}", new Vector2(layout.HeaderPanel.X + 420f, layout.HeaderPanel.Y + 46f), Color.White);

            string menuHelp = "Menu Keys: N New Map, Enter Continue, R Refresh, Click Card To Load";
            spriteBatch.DrawString(font, menuHelp, new Vector2(layout.HeaderPanel.X + 16f, layout.HeaderPanel.Bottom + 12f), Color.White);

            if (menu.Items.Count == 0)
            {
                spriteBatch.DrawString(font, "No saved maps yet. Create one with New Tilemap or save from editor.", new Vector2(layout.ListX, layout.ListStartY), Color.White);
            }

            for (int i = 0; i < menu.Items.Count; i++)
            {
                TileMapMenuItem item = menu.Items[i];
                Rectangle card = new Rectangle(layout.ListX, layout.ListStartY + i * (layout.CardHeight + layout.CardSpacing), layout.ListWidth, layout.CardHeight);
                DrawCard(spriteBatch, item, card);
            }

            spriteBatch.End();
        }

        private void DrawCard(SpriteBatch spriteBatch, TileMapMenuItem item, Rectangle card)
        {
            spriteBatch.Draw(pixelTexture, card, new Color(10, 10, 10, 255));
            DrawBorder(spriteBatch, card, 2, Color.White);

            Rectangle preview = new Rectangle(card.X + 10, card.Y + 10, 84, 84);
            DrawPreview(spriteBatch, preview, item);
            DrawBorder(spriteBatch, preview, 1, Color.White);

            string updated = item.LastUpdatedUtc == DateTime.MinValue
                ? "Unknown"
                : item.LastUpdatedUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm");

            spriteBatch.DrawString(font, item.Name, new Vector2(card.X + 106, card.Y + 14), Color.White);
            spriteBatch.DrawString(font, $"Updated: {updated}", new Vector2(card.X + 106, card.Y + 40), Color.White);
            spriteBatch.DrawString(font, "Click to load and edit", new Vector2(card.X + 106, card.Y + 68), Color.White);
        }

        private void DrawPreview(SpriteBatch spriteBatch, Rectangle area, TileMapMenuItem item)
        {
            int width = Math.Max(1, item.Width);
            int height = Math.Max(1, item.Height);

            int sampleCols = 8;
            int sampleRows = 8;
            int cellW = Math.Max(1, area.Width / sampleCols);
            int cellH = Math.Max(1, area.Height / sampleRows);

            for (int y = 0; y < sampleRows; y++)
            {
                for (int x = 0; x < sampleCols; x++)
                {
                    int sampleX = x * width / sampleCols;
                    int sampleY = y * height / sampleRows;
                    int index = sampleY * width + sampleX;
                    int tileId = index >= 0 && index < item.PreviewTileIds.Length ? item.PreviewTileIds[index] : 0;

                    Color fill = tileId == 0 ? new Color(18, 18, 18) : Color.White;
                    spriteBatch.Draw(pixelTexture, new Rectangle(area.X + x * cellW, area.Y + y * cellH, cellW, cellH), fill);
                }
            }
        }

        private void CreateNewMap(World world, TileMapEditorComponent editor, TileMapMenuComponent menu)
        {
            ClearAllLayers(world);
            editor.CurrentMapName = $"Untitled-{DateTime.Now:HHmmss}";
            editor.HasUnsavedChanges = false;
            editor.ShowLeaveToMenuPopup = false;
            editor.SelectedLayerIndex = 0;
            menu.IsVisible = false;
            menu.NeedsRefresh = true;
            editor.IsEnabled = true;
        }

        private static void ClearAllLayers(World world)
        {
            List<TileMapLayerComponent> layers = new List<TileMapLayerComponent>();
            for (int i = 0; i < world.Entities.Count; i++)
            {
                if (world.Entities[i].TryGetComponent<TileMapLayerComponent>(out TileMapLayerComponent layer) && layer != null)
                {
                    layers.Add(layer);
                }
            }

            for (int layerIndex = 0; layerIndex < layers.Count; layerIndex++)
            {
                TileMapLayerComponent layer = layers[layerIndex];
                for (int y = 0; y < layer.Height; y++)
                {
                    for (int x = 0; x < layer.Width; x++)
                    {
                        layer.SetTile(x, y, 0);
                    }
                }
            }
        }

        private bool WasJustPressed(KeyboardState keyboard, Keys key)
        {
            return keyboard.IsKeyDown(key) && !previousKeyboard.IsKeyDown(key);
        }

        private bool WasVirtualClicked(Rectangle bounds, MouseState currentMouse, MouseState previousMouseState, Point virtualMouse)
        {
            bool justPressed = currentMouse.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released;
            return justPressed && bounds.Contains(virtualMouse);
        }

        private float GetUiScale()
        {
            float scaleX = graphicsDevice.Viewport.Width / VirtualWidth;
            float scaleY = graphicsDevice.Viewport.Height / VirtualHeight;
            return MathF.Max(0.5f, MathF.Min(scaleX, scaleY));
        }

        private Point ToVirtualPoint(Point screenPoint)
        {
            float scale = GetUiScale();
            return new Point((int)(screenPoint.X / scale), (int)(screenPoint.Y / scale));
        }

        private static bool TryGetMenuAndEditor(World world, out TileMapMenuComponent menu, out TileMapEditorComponent editor)
        {
            for (int i = 0; i < world.Entities.Count; i++)
            {
                Entity entity = world.Entities[i];
                if (entity.TryGetComponent<TileMapMenuComponent>(out menu)
                    && menu != null
                    && entity.TryGetComponent<TileMapEditorComponent>(out editor)
                    && editor != null)
                {
                    return true;
                }
            }

            menu = null;
            editor = null;
            return false;
        }

        private void DrawBorder(SpriteBatch spriteBatch, Rectangle rect, int thickness, Color color)
        {
            spriteBatch.Draw(pixelTexture, new Rectangle(rect.X, rect.Y, rect.Width, thickness), color);
            spriteBatch.Draw(pixelTexture, new Rectangle(rect.X, rect.Bottom - thickness, rect.Width, thickness), color);
            spriteBatch.Draw(pixelTexture, new Rectangle(rect.X, rect.Y, thickness, rect.Height), color);
            spriteBatch.Draw(pixelTexture, new Rectangle(rect.Right - thickness, rect.Y, thickness, rect.Height), color);
        }

        private MenuLayout BuildLayout()
        {
            return new MenuLayout
            {
                HeaderPanel = new Rectangle(16, 16, 1248, 104),
                NewButton = new Rectangle(30, 32, 170, 40),
                ContinueButton = new Rectangle(206, 32, 150, 40),
                RefreshButton = new Rectangle(362, 32, 40, 40),
                ListX = 16,
                ListStartY = 150,
                ListWidth = 1248,
                CardHeight = 104,
                CardSpacing = 12,
            };
        }

        private struct MenuLayout
        {
            public Rectangle HeaderPanel;
            public Rectangle NewButton;
            public Rectangle ContinueButton;
            public Rectangle RefreshButton;
            public int ListX;
            public int ListStartY;
            public int ListWidth;
            public int CardHeight;
            public int CardSpacing;
        }
    }
}
