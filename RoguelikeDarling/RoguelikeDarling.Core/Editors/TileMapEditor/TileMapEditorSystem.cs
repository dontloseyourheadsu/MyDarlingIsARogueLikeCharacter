using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RoguelikeDarling.Core.Camera;
using RoguelikeDarling.Core.Ecs;
using RoguelikeDarling.Core.GameSpecific.Menus.TileMap;
using RoguelikeDarling.Core.Storage;
using RoguelikeDarling.Core.Tilemap;
using RoguelikeDarling.Core.UI.Buttons;

namespace RoguelikeDarling.Core.Editors.TileMapEditor
{
    public sealed class TileMapEditorSystem : IGameSystem
    {
        private const float VirtualWidth = 1280f;
        private const float VirtualHeight = 720f;

        private static readonly ButtonStyle ActiveButtonStyle = new ButtonStyle
        {
            FillColor = Color.White,
            BorderColor = Color.Black,
            ContentColor = Color.Black,
            BorderThickness = 2,
        };

        private readonly TileMapEditorStorage storage;
        private readonly SpriteFont font;
        private readonly Texture2D pixelTexture;
        private readonly GraphicsDevice graphicsDevice;

        private readonly Color[] customColorPresets =
        {
            new Color(80, 180, 255),
            new Color(120, 210, 180),
            new Color(180, 140, 220),
            new Color(255, 120, 160),
            new Color(255, 200, 80),
            new Color(250, 250, 250),
            new Color(170, 170, 170),
            new Color(60, 60, 60),
        };

        private KeyboardState previousKeyboard;
        private MouseState previousMouse;
        private bool showColorSelector;
        private Point hoveredCell = new Point(-1, -1);

        private enum EditTool
        {
            Paint,
            Erase,
        }

        private EditTool activeTool = EditTool.Paint;

        public TileMapEditorSystem(TileMapEditorStorage storage, SpriteFont font, GraphicsDevice graphicsDevice)
        {
            this.storage = storage;
            this.font = font;
            this.graphicsDevice = graphicsDevice;
            pixelTexture = new Texture2D(graphicsDevice, 1, 1);
            pixelTexture.SetData(new[] { Color.White });
        }

        public void Update(World world, GameTime gameTime)
        {
            if (!TryGetEditorAndMenu(world, out TileMapEditorComponent editor, out TileMapMenuComponent menu))
            {
                previousKeyboard = Keyboard.GetState();
                previousMouse = Mouse.GetState();
                return;
            }

            KeyboardState keyboard = Keyboard.GetState();
            MouseState mouse = Mouse.GetState();
            Point virtualMouse = ToVirtualPoint(mouse.Position);
            bool ctrlHeld = keyboard.IsKeyDown(Keys.LeftControl) || keyboard.IsKeyDown(Keys.RightControl);

            if (!editor.IsEnabled || menu.IsVisible)
            {
                previousKeyboard = keyboard;
                previousMouse = mouse;
                return;
            }

            var layers = CollectLayers(world);
            if (layers.Count == 0)
            {
                previousKeyboard = keyboard;
                previousMouse = mouse;
                return;
            }

            editor.SelectedLayerIndex = Math.Clamp(editor.SelectedLayerIndex, 0, layers.Count - 1);
            var layout = BuildLayout();

            if (editor.ShowLeaveToMenuPopup)
            {
                HandleLeavePopup(world, editor, menu, mouse, virtualMouse, layout);
                previousKeyboard = keyboard;
                previousMouse = mouse;
                return;
            }

            if (WasVirtualClicked(layout.BackButton, mouse, previousMouse, virtualMouse))
            {
                RequestLeaveToMenu(editor, menu);
            }

            bool savePressed = WasVirtualClicked(layout.SaveButton, mouse, previousMouse, virtualMouse)
                || (ctrlHeld && WasJustPressed(keyboard, Keys.S));
            if (savePressed && storage.SaveFromWorld(world, editor.CurrentMapName))
            {
                editor.HasUnsavedChanges = false;
            }

            if (WasVirtualClicked(layout.PaintButton, mouse, previousMouse, virtualMouse) || WasJustPressed(keyboard, Keys.P))
            {
                activeTool = EditTool.Paint;
            }

            if (WasVirtualClicked(layout.EraseButton, mouse, previousMouse, virtualMouse) || WasJustPressed(keyboard, Keys.E))
            {
                activeTool = EditTool.Erase;
            }

            if (WasVirtualClicked(layout.LayerPrevButton, mouse, previousMouse, virtualMouse) || WasJustPressed(keyboard, Keys.OemOpenBrackets))
            {
                editor.SelectedLayerIndex = Math.Max(0, editor.SelectedLayerIndex - 1);
            }

            if (WasVirtualClicked(layout.LayerNextButton, mouse, previousMouse, virtualMouse) || WasJustPressed(keyboard, Keys.OemCloseBrackets))
            {
                editor.SelectedLayerIndex = Math.Min(layers.Count - 1, editor.SelectedLayerIndex + 1);
            }

            if (WasVirtualClicked(layout.ColorPickerButton, mouse, previousMouse, virtualMouse) || WasJustPressed(keyboard, Keys.C))
            {
                showColorSelector = !showColorSelector;
                editor.SelectedTileId = 9;
            }

            ApplyTileSelectionButtons(editor, mouse, virtualMouse, layout);
            ApplyQuickTileKeys(editor, keyboard);
            ApplyCustomColorPicker(world, editor, mouse, virtualMouse, layout);

            TileMapLayerComponent selectedLayer = layers[editor.SelectedLayerIndex];
            bool hasHovered = TryFindTileFromMouse(world, selectedLayer, mouse.Position.ToVector2(), out Point hovered)
                && hovered.X >= 0
                && hovered.Y >= 0
                && hovered.X < selectedLayer.Width
                && hovered.Y < selectedLayer.Height;

            hoveredCell = hasHovered ? hovered : new Point(-1, -1);

            bool pointerInUi = layout.Panel.Contains(virtualMouse)
                || (showColorSelector && layout.ColorPopup.Contains(virtualMouse));

            if (!pointerInUi && hasHovered)
            {
                if (mouse.LeftButton == ButtonState.Pressed)
                {
                    if (activeTool == EditTool.Paint)
                    {
                        selectedLayer.SetTile(hovered.X, hovered.Y, editor.SelectedTileId);
                    }
                    else
                    {
                        selectedLayer.SetTile(hovered.X, hovered.Y, 0);
                    }

                    editor.HasUnsavedChanges = true;
                }
                else if (mouse.RightButton == ButtonState.Pressed)
                {
                    selectedLayer.SetTile(hovered.X, hovered.Y, 0);
                    editor.HasUnsavedChanges = true;
                }
            }

            if (WasJustPressed(keyboard, Keys.M))
            {
                RequestLeaveToMenu(editor, menu);
            }

            previousKeyboard = keyboard;
            previousMouse = mouse;
        }

        public void Draw(World world, GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (!TryGetEditorAndMenu(world, out TileMapEditorComponent editor, out TileMapMenuComponent menu)
                || !editor.IsEnabled
                || menu.IsVisible)
            {
                return;
            }

            var layers = CollectLayers(world);
            if (layers.Count == 0)
            {
                return;
            }

            TileMapLayerComponent layer = layers[Math.Clamp(editor.SelectedLayerIndex, 0, layers.Count - 1)];
            Camera2DComponent camera = FindCamera(world);
            if (camera != null)
            {
                DrawLayerGrid(spriteBatch, layer, camera);
            }

            var layout = BuildLayout();
            float uiScale = GetUiScale();
            spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.PointClamp,
                DepthStencilState.None,
                RasterizerState.CullNone,
                effect: null,
                transformMatrix: Matrix.CreateScale(uiScale));

            spriteBatch.Draw(pixelTexture, layout.Panel, new Color(12, 12, 12, 235));
            DrawBorder(spriteBatch, layout.Panel, 2, Color.White);

            var backButton = new TextButton("Back", layout.BackButton);
            var saveButton = new IconButton("S", layout.SaveButton);
            var paintButton = new TextButton("Paint", layout.PaintButton, activeTool == EditTool.Paint ? ActiveButtonStyle : null);
            var eraseButton = new TextButton("Erase", layout.EraseButton, activeTool == EditTool.Erase ? ActiveButtonStyle : null);
            var layerPrev = new TextButton("<", layout.LayerPrevButton);
            var layerNext = new TextButton(">", layout.LayerNextButton);
            var colorButton = new TextButton("Color", layout.ColorPickerButton, editor.SelectedTileId == 9 ? ActiveButtonStyle : null);

            backButton.Draw(spriteBatch, pixelTexture, font);
            saveButton.Draw(spriteBatch, pixelTexture, font);
            paintButton.Draw(spriteBatch, pixelTexture, font);
            eraseButton.Draw(spriteBatch, pixelTexture, font);
            layerPrev.Draw(spriteBatch, pixelTexture, font);
            layerNext.Draw(spriteBatch, pixelTexture, font);
            colorButton.Draw(spriteBatch, pixelTexture, font);

            DrawTileButtons(spriteBatch, editor, layout);

            string keys =
                "Editor Keys\n" +
                "P Paint, E Erase\n" +
                "1..5 Tile Presets\n" +
                "9/C Custom Color Tile\n" +
                "[ / ] Select Layer\n" +
                "Ctrl+S Save\n" +
                "M Back To Menu";

            spriteBatch.DrawString(font, "Tilemap Editor", new Vector2(layout.Panel.X + 16f, layout.Panel.Y + 12f), Color.White);
            spriteBatch.DrawString(font, keys, new Vector2(layout.Panel.X + 16f, layout.Panel.Y + 220f), Color.White);

            string hoverText = hoveredCell.X >= 0
                ? $"Hover Cell: ({hoveredCell.X}, {hoveredCell.Y})"
                : "Hover Cell: none";

            string layerInfo =
                $"Layer: {layer.Name} ({editor.SelectedLayerIndex + 1}/{layers.Count})\n" +
                $"Relative Position: ({layer.Offset.X:0}, {layer.Offset.Y:0})\n" +
                "Rotation: 0 deg\n" +
                $"Orientation: {layer.RenderOrder}\n" +
                $"Stride: ({layer.TileStride.X}, {layer.TileStride.Y})\n" +
                hoverText;

            spriteBatch.DrawString(font, layerInfo, new Vector2(layout.Panel.X + 16f, layout.Panel.Y + 410f), Color.White);

            if (showColorSelector)
            {
                DrawColorPopup(spriteBatch, layout);
            }

            if (editor.ShowLeaveToMenuPopup)
            {
                DrawLeavePopup(spriteBatch, layout);
            }

            spriteBatch.End();
        }

        private void ApplyTileSelectionButtons(TileMapEditorComponent editor, MouseState mouse, Point virtualMouse, EditorLayout layout)
        {
            if (WasVirtualClicked(layout.Tile1Button, mouse, previousMouse, virtualMouse))
            {
                editor.SelectedTileId = 1;
            }

            if (WasVirtualClicked(layout.Tile2Button, mouse, previousMouse, virtualMouse))
            {
                editor.SelectedTileId = 2;
            }

            if (WasVirtualClicked(layout.Tile3Button, mouse, previousMouse, virtualMouse))
            {
                editor.SelectedTileId = 3;
            }

            if (WasVirtualClicked(layout.Tile4Button, mouse, previousMouse, virtualMouse))
            {
                editor.SelectedTileId = 4;
            }

            if (WasVirtualClicked(layout.Tile5Button, mouse, previousMouse, virtualMouse))
            {
                editor.SelectedTileId = 5;
            }

            if (WasVirtualClicked(layout.CustomTileButton, mouse, previousMouse, virtualMouse))
            {
                editor.SelectedTileId = 9;
            }
        }

        private void ApplyQuickTileKeys(TileMapEditorComponent editor, KeyboardState keyboard)
        {
            for (int tile = 1; tile <= 5; tile++)
            {
                if (WasJustPressed(keyboard, Keys.D0 + tile))
                {
                    editor.SelectedTileId = tile;
                }
            }

            if (WasJustPressed(keyboard, Keys.D9))
            {
                editor.SelectedTileId = 9;
            }
        }

        private void ApplyCustomColorPicker(World world, TileMapEditorComponent editor, MouseState mouse, Point virtualMouse, EditorLayout layout)
        {
            if (!showColorSelector)
            {
                return;
            }

            MutableDiamondTileVisual customVisual = FindCustomTileVisual(world);
            if (customVisual == null)
            {
                return;
            }

            for (int i = 0; i < customColorPresets.Length; i++)
            {
                Rectangle swatch = GetColorSwatchRect(layout, i);
                if (WasVirtualClicked(swatch, mouse, previousMouse, virtualMouse))
                {
                    customVisual.FillColor = customColorPresets[i];
                    editor.SelectedTileId = 9;
                    showColorSelector = false;
                    return;
                }
            }
        }

        private void DrawTileButtons(SpriteBatch spriteBatch, TileMapEditorComponent editor, EditorLayout layout)
        {
            DrawTileButton(spriteBatch, "T1", layout.Tile1Button, editor.SelectedTileId == 1, new Color(88, 148, 92));
            DrawTileButton(spriteBatch, "T2", layout.Tile2Button, editor.SelectedTileId == 2, new Color(120, 120, 120));
            DrawTileButton(spriteBatch, "T3", layout.Tile3Button, editor.SelectedTileId == 3, new Color(190, 170, 112));
            DrawTileButton(spriteBatch, "T4", layout.Tile4Button, editor.SelectedTileId == 4, new Color(120, 210, 180));
            DrawTileButton(spriteBatch, "T5", layout.Tile5Button, editor.SelectedTileId == 5, new Color(180, 140, 220));
            DrawTileButton(spriteBatch, "TC", layout.CustomTileButton, editor.SelectedTileId == 9, new Color(80, 180, 255));
        }

        private void DrawTileButton(SpriteBatch spriteBatch, string label, Rectangle rect, bool selected, Color accent)
        {
            spriteBatch.Draw(pixelTexture, rect, selected ? Color.White : Color.Black);
            DrawBorder(spriteBatch, rect, 2, selected ? Color.Black : Color.White);

            Rectangle accentRect = new Rectangle(rect.X + 4, rect.Y + 4, 10, rect.Height - 8);
            spriteBatch.Draw(pixelTexture, accentRect, accent);

            Color textColor = selected ? Color.Black : Color.White;
            spriteBatch.DrawString(font, label, new Vector2(rect.X + 18f, rect.Y + 8f), textColor);
        }

        private void DrawColorPopup(SpriteBatch spriteBatch, EditorLayout layout)
        {
            spriteBatch.Draw(pixelTexture, layout.ColorPopup, Color.Black);
            DrawBorder(spriteBatch, layout.ColorPopup, 2, Color.White);
            spriteBatch.DrawString(font, "Pick Custom Color", new Vector2(layout.ColorPopup.X + 12f, layout.ColorPopup.Y + 10f), Color.White);

            for (int i = 0; i < customColorPresets.Length; i++)
            {
                Rectangle swatch = GetColorSwatchRect(layout, i);
                spriteBatch.Draw(pixelTexture, swatch, customColorPresets[i]);
                DrawBorder(spriteBatch, swatch, 2, Color.White);
            }
        }

        private void DrawLayerGrid(SpriteBatch spriteBatch, TileMapLayerComponent layer, Camera2DComponent camera)
        {
            spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.PointClamp,
                DepthStencilState.None,
                RasterizerState.CullNone,
                effect: null,
                transformMatrix: camera.GetViewMatrix());

            float halfW = layer.TileStride.X * 0.5f;
            float halfH = layer.TileStride.Y * 0.5f;

            for (int y = 0; y < layer.Height; y++)
            {
                for (int x = 0; x < layer.Width; x++)
                {
                    float baseX = (x - y) * halfW + layer.Offset.X;
                    float baseY = (x + y) * halfH + layer.Offset.Y;

                    Vector2 top = new Vector2(baseX + halfW, baseY);
                    Vector2 right = new Vector2(baseX + layer.TileStride.X, baseY + halfH);
                    Vector2 bottom = new Vector2(baseX + halfW, baseY + layer.TileStride.Y);
                    Vector2 left = new Vector2(baseX, baseY + halfH);

                    bool isHovered = hoveredCell.X == x && hoveredCell.Y == y;
                    if (isHovered)
                    {
                        DrawDiamondFill(spriteBatch, top, right, bottom, left, new Color(120, 210, 180, 70));
                    }

                    Color lineColor = isHovered ? Color.White : new Color(100, 180, 210, 160);
                    DrawLine(spriteBatch, top, right, lineColor, 1f);
                    DrawLine(spriteBatch, right, bottom, lineColor, 1f);
                    DrawLine(spriteBatch, bottom, left, lineColor, 1f);
                    DrawLine(spriteBatch, left, top, lineColor, 1f);
                }
            }

            spriteBatch.End();
        }

        private void DrawDiamondFill(SpriteBatch spriteBatch, Vector2 top, Vector2 right, Vector2 bottom, Vector2 left, Color color)
        {
            float minY = top.Y;
            float maxY = bottom.Y;
            for (float y = minY; y <= maxY; y += 1f)
            {
                float tTop = (y - top.Y) / Math.Max(1f, right.Y - top.Y);
                float tBottom = (y - left.Y) / Math.Max(1f, bottom.Y - left.Y);
                float xStart;
                float xEnd;

                if (y <= right.Y)
                {
                    xStart = MathHelper.Lerp(top.X, left.X, tTop);
                    xEnd = MathHelper.Lerp(top.X, right.X, tTop);
                }
                else
                {
                    xStart = MathHelper.Lerp(left.X, bottom.X, tBottom);
                    xEnd = MathHelper.Lerp(right.X, bottom.X, tBottom);
                }

                if (xEnd < xStart)
                {
                    (xStart, xEnd) = (xEnd, xStart);
                }

                spriteBatch.Draw(pixelTexture, new Rectangle((int)xStart, (int)y, (int)(xEnd - xStart) + 1, 1), color);
            }
        }

        private void HandleLeavePopup(World world, TileMapEditorComponent editor, TileMapMenuComponent menu, MouseState mouse, Point virtualMouse, EditorLayout layout)
        {
            if (WasVirtualClicked(layout.PopupSaveButton, mouse, previousMouse, virtualMouse))
            {
                if (storage.SaveFromWorld(world, editor.CurrentMapName))
                {
                    editor.HasUnsavedChanges = false;
                }

                editor.ShowLeaveToMenuPopup = false;
                menu.NeedsRefresh = true;
                menu.IsVisible = true;
                editor.IsEnabled = false;
                return;
            }

            if (WasVirtualClicked(layout.PopupDiscardButton, mouse, previousMouse, virtualMouse))
            {
                editor.ShowLeaveToMenuPopup = false;
                editor.HasUnsavedChanges = false;
                menu.NeedsRefresh = true;
                menu.IsVisible = true;
                editor.IsEnabled = false;
                return;
            }

            if (WasVirtualClicked(layout.PopupCancelButton, mouse, previousMouse, virtualMouse))
            {
                editor.ShowLeaveToMenuPopup = false;
            }
        }

        private void DrawLeavePopup(SpriteBatch spriteBatch, EditorLayout layout)
        {
            spriteBatch.Draw(pixelTexture, layout.PopupRect, Color.Black);
            DrawBorder(spriteBatch, layout.PopupRect, 2, Color.White);
            spriteBatch.DrawString(font, "Save before leaving editor?", new Vector2(layout.PopupRect.X + 20f, layout.PopupRect.Y + 16f), Color.White);

            new TextButton("Save", layout.PopupSaveButton).Draw(spriteBatch, pixelTexture, font);
            new TextButton("Discard", layout.PopupDiscardButton).Draw(spriteBatch, pixelTexture, font);
            new TextButton("Cancel", layout.PopupCancelButton).Draw(spriteBatch, pixelTexture, font);
        }

        private void RequestLeaveToMenu(TileMapEditorComponent editor, TileMapMenuComponent menu)
        {
            if (editor.HasUnsavedChanges)
            {
                editor.ShowLeaveToMenuPopup = true;
                return;
            }

            menu.NeedsRefresh = true;
            menu.IsVisible = true;
            editor.IsEnabled = false;
        }

        private static List<TileMapLayerComponent> CollectLayers(World world)
        {
            var layers = new List<TileMapLayerComponent>();
            for (int i = 0; i < world.Entities.Count; i++)
            {
                if (world.Entities[i].TryGetComponent<TileMapLayerComponent>(out TileMapLayerComponent layer) && layer != null)
                {
                    layers.Add(layer);
                }
            }

            layers.Sort((a, b) => a.ZIndex.CompareTo(b.ZIndex));
            return layers;
        }

        private static bool TryFindTileFromMouse(World world, TileMapLayerComponent layer, Vector2 mouseScreenPosition, out Point tileCell)
        {
            tileCell = Point.Zero;
            Camera2DComponent camera = FindCamera(world);
            if (camera == null)
            {
                return false;
            }

            Matrix inverseView = Matrix.Invert(camera.GetViewMatrix());
            Vector2 worldPosition = Vector2.Transform(mouseScreenPosition, inverseView);

            float halfWidth = layer.TileStride.X * 0.5f;
            float halfHeight = layer.TileStride.Y * 0.5f;
            if (halfWidth <= 0f || halfHeight <= 0f)
            {
                return false;
            }

            float localX = worldPosition.X - layer.Offset.X;
            float localY = worldPosition.Y - layer.Offset.Y;

            float x = 0.5f * ((localX / halfWidth) + (localY / halfHeight));
            float y = 0.5f * ((localY / halfHeight) - (localX / halfWidth));

            tileCell = new Point((int)Math.Floor(x), (int)Math.Floor(y));
            return true;
        }

        private static Camera2DComponent FindCamera(World world)
        {
            for (int i = 0; i < world.Entities.Count; i++)
            {
                if (world.Entities[i].TryGetComponent<Camera2DComponent>(out Camera2DComponent camera) && camera != null)
                {
                    return camera;
                }
            }

            return null;
        }

        private MutableDiamondTileVisual FindCustomTileVisual(World world)
        {
            for (int i = 0; i < world.Entities.Count; i++)
            {
                if (world.Entities[i].TryGetComponent<TilePaletteComponent>(out TilePaletteComponent palette)
                    && palette != null
                    && palette.TryGet(9, out TileDefinition definition)
                    && definition?.Visual is MutableDiamondTileVisual customVisual)
                {
                    return customVisual;
                }
            }

            return null;
        }

        private bool WasJustPressed(KeyboardState keyboard, Keys key)
        {
            return keyboard.IsKeyDown(key) && !previousKeyboard.IsKeyDown(key);
        }

        private bool WasVirtualClicked(Rectangle bounds, MouseState currentMouse, MouseState previousMouseState, Point virtualPoint)
        {
            bool justPressed = currentMouse.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released;
            return justPressed && bounds.Contains(virtualPoint);
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

        private Rectangle GetColorSwatchRect(EditorLayout layout, int index)
        {
            int cols = 4;
            int row = index / cols;
            int col = index % cols;
            return new Rectangle(layout.ColorPopup.X + 14 + col * 42, layout.ColorPopup.Y + 42 + row * 42, 34, 34);
        }

        private static bool TryGetEditorAndMenu(World world, out TileMapEditorComponent editor, out TileMapMenuComponent menu)
        {
            for (int i = 0; i < world.Entities.Count; i++)
            {
                Entity entity = world.Entities[i];
                if (entity.TryGetComponent<TileMapEditorComponent>(out editor)
                    && editor != null
                    && entity.TryGetComponent<TileMapMenuComponent>(out menu)
                    && menu != null)
                {
                    return true;
                }
            }

            editor = null;
            menu = null;
            return false;
        }

        private void DrawLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, float thickness)
        {
            Vector2 delta = end - start;
            float length = delta.Length();
            if (length <= 0f)
            {
                return;
            }

            float rotation = (float)Math.Atan2(delta.Y, delta.X);
            spriteBatch.Draw(
                pixelTexture,
                start,
                null,
                color,
                rotation,
                Vector2.Zero,
                new Vector2(length, thickness),
                SpriteEffects.None,
                0f);
        }

        private void DrawBorder(SpriteBatch spriteBatch, Rectangle rect, int thickness, Color color)
        {
            spriteBatch.Draw(pixelTexture, new Rectangle(rect.X, rect.Y, rect.Width, thickness), color);
            spriteBatch.Draw(pixelTexture, new Rectangle(rect.X, rect.Bottom - thickness, rect.Width, thickness), color);
            spriteBatch.Draw(pixelTexture, new Rectangle(rect.X, rect.Y, thickness, rect.Height), color);
            spriteBatch.Draw(pixelTexture, new Rectangle(rect.Right - thickness, rect.Y, thickness, rect.Height), color);
        }

        private EditorLayout BuildLayout()
        {
            return new EditorLayout
            {
                Panel = new Rectangle(14, 14, 350, 690),
                BackButton = new Rectangle(28, 58, 112, 36),
                SaveButton = new Rectangle(146, 58, 36, 36),
                PaintButton = new Rectangle(28, 104, 92, 36),
                EraseButton = new Rectangle(126, 104, 92, 36),
                LayerPrevButton = new Rectangle(224, 104, 40, 36),
                LayerNextButton = new Rectangle(270, 104, 40, 36),
                Tile1Button = new Rectangle(28, 152, 48, 34),
                Tile2Button = new Rectangle(82, 152, 48, 34),
                Tile3Button = new Rectangle(136, 152, 48, 34),
                Tile4Button = new Rectangle(190, 152, 48, 34),
                Tile5Button = new Rectangle(244, 152, 48, 34),
                CustomTileButton = new Rectangle(298, 152, 48, 34),
                ColorPickerButton = new Rectangle(28, 192, 120, 34),
                ColorPopup = new Rectangle(160, 192, 190, 130),
                PopupRect = new Rectangle(420, 250, 450, 160),
                PopupSaveButton = new Rectangle(450, 330, 110, 40),
                PopupDiscardButton = new Rectangle(570, 330, 120, 40),
                PopupCancelButton = new Rectangle(700, 330, 120, 40),
            };
        }

        private struct EditorLayout
        {
            public Rectangle Panel;
            public Rectangle BackButton;
            public Rectangle SaveButton;
            public Rectangle PaintButton;
            public Rectangle EraseButton;
            public Rectangle LayerPrevButton;
            public Rectangle LayerNextButton;
            public Rectangle Tile1Button;
            public Rectangle Tile2Button;
            public Rectangle Tile3Button;
            public Rectangle Tile4Button;
            public Rectangle Tile5Button;
            public Rectangle CustomTileButton;
            public Rectangle ColorPickerButton;
            public Rectangle ColorPopup;
            public Rectangle PopupRect;
            public Rectangle PopupSaveButton;
            public Rectangle PopupDiscardButton;
            public Rectangle PopupCancelButton;
        }
    }
}
