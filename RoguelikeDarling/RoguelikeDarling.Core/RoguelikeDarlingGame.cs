using System;
using RoguelikeDarling.Core.Localization;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RoguelikeDarling.Core.Camera;
using RoguelikeDarling.Core.Ecs;
using RoguelikeDarling.Core.Storage;
using RoguelikeDarling.Core.Tilemap;
using RoguelikeDarling.Core.Editors.TileMapEditor;
using RoguelikeDarling.Core.GameSpecific.Menus.TileMap;

namespace RoguelikeDarling.Core
{
    /// <summary>
    /// The main class for the game, responsible for managing game components, settings, 
    /// and platform-specific configurations.
    /// </summary>
    public class RoguelikeDarlingGame : Game
    {
        // Resources for drawing.
        private GraphicsDeviceManager graphicsDeviceManager;
        private SpriteBatch spriteBatch;
        private World world;
        private SpriteFont hudFont;
        private TileMapEditorStorage tileMapEditorStorage;
        private readonly bool isCollisionDebugEnabled;
        private readonly Color collisionDebugColor;

        /// <summary>
        /// Indicates if the game is running on a mobile platform.
        /// </summary>
        public readonly static bool IsMobile = OperatingSystem.IsAndroid() || OperatingSystem.IsIOS();

        /// <summary>
        /// Indicates if the game is running on a desktop platform.
        /// </summary>
        public readonly static bool IsDesktop = OperatingSystem.IsMacOS() || OperatingSystem.IsLinux() || OperatingSystem.IsWindows();

        /// <summary>
        /// Initializes a new instance of the game. Configures platform-specific settings, 
        /// initializes services like settings and leaderboard managers, and sets up the 
        /// screen manager for screen transitions.
        /// </summary>
        public RoguelikeDarlingGame(bool isCollisionDebugEnabled = false, Color? collisionDebugColor = null)
        {
            graphicsDeviceManager = new GraphicsDeviceManager(this);
            this.isCollisionDebugEnabled = isCollisionDebugEnabled;
            this.collisionDebugColor = collisionDebugColor ?? new Color(220, 30, 30);

            IsMouseVisible = true;
            Window.AllowUserResizing = true;

            // Share GraphicsDeviceManager as a service.
            Services.AddService(typeof(GraphicsDeviceManager), graphicsDeviceManager);

            Content.RootDirectory = "Content";

            // Configure screen orientations.
            graphicsDeviceManager.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;
        }

        /// <summary>
        /// Initializes the game, including setting up localization and adding the 
        /// initial screens to the ScreenManager.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            // Load supported languages and set the default language.
            List<CultureInfo> cultures = LocalizationManager.GetSupportedCultures();
            var languages = new List<CultureInfo>();
            for (int i = 0; i < cultures.Count; i++)
            {
                languages.Add(cultures[i]);
            }

            // TODO You should load this from a settings file or similar,
            // based on what the user or operating system selected.
            var selectedLanguage = LocalizationManager.DEFAULT_CULTURE_CODE;
            LocalizationManager.SetCulture(selectedLanguage);
        }

        /// <summary>
        /// Loads game content, such as textures and particle systems.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            hudFont = Content.Load<SpriteFont>("Fonts/Hud");
            tileMapEditorStorage = new TileMapEditorStorage();

            world = new World();
            world.AddSystem(new IsometricTileMapRenderSystem(GraphicsDevice));
            world.AddSystem(new TileMapEditorSystem(tileMapEditorStorage, hudFont, GraphicsDevice));
            world.AddSystem(new TileMapMenuSystem(tileMapEditorStorage, hudFont, GraphicsDevice));

            BuildTileMapEditorScene();

            base.LoadContent();
        }

        /// <summary>
        /// Updates the game's logic, called once per frame.
        /// </summary>
        /// <param name="gameTime">
        /// Provides a snapshot of timing values used for game updates.
        /// </param>
        protected override void Update(GameTime gameTime)
        {
            // Exit the game if the Back button (GamePad) or Escape key (Keyboard) is pressed.
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed
                || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            SyncCameraViewportCenters();
            world.Update(gameTime);

            base.Update(gameTime);
        }

        /// <summary>
        /// Draws the game's graphics, called once per frame.
        /// </summary>
        /// <param name="gameTime">
        /// Provides a snapshot of timing values used for rendering.
        /// </param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(10, 10, 10));

            if (spriteBatch != null && world != null)
            {
                world.Draw(gameTime, spriteBatch);
            }

            base.Draw(gameTime);
        }

        private void BuildTileMapEditorScene()
        {
            if (world == null)
            {
                return;
            }

            Texture2D tinyBlocksTexture = Content.Load<Texture2D>("tinyBlocks");

            var paletteEntity = world.CreateEntity();
            var palette = new TilePaletteComponent();

            TileAtlasRegion regionGrass = new TileAtlasRegion(tinyBlocksTexture, "tinyBlocks", new Rectangle(0, 0, 18, 18));
            TileAtlasRegion regionStone = new TileAtlasRegion(tinyBlocksTexture, "tinyBlocks", new Rectangle(18, 0, 18, 18));
            TileAtlasRegion regionSand = new TileAtlasRegion(tinyBlocksTexture, "tinyBlocks", new Rectangle(36, 0, 18, 18));

            palette.Register(new TileDefinition(1, "AtlasGrass", new AtlasTileVisual(regionGrass)));
            palette.Register(new TileDefinition(2, "AtlasStone", new AtlasTileVisual(regionStone)));
            palette.Register(new TileDefinition(3, "AtlasSand", new AtlasTileVisual(regionSand)));
            palette.Register(new TileDefinition(4, "CodeMint", new ProceduralDiamondTileVisual(new Color(120, 210, 180))));
            palette.Register(new TileDefinition(5, "CodePurple", new ProceduralDiamondTileVisual(new Color(180, 140, 220))));
            palette.Register(new TileDefinition(9, "CustomColor", new MutableDiamondTileVisual(new Color(80, 180, 255), Color.Black)));

            paletteEntity.AddComponent(palette);

            world.CreateEntity()
                .AddComponent(new Camera2DComponent { Position = new Vector2(0f, 0f), Zoom = 1f });

            var baseLayer = new TileMapLayerComponent("Base", 24, 24, new Point(84, 44), zIndex: 0)
            {
                Offset = new Vector2(540f, 140f),
                TileStride = new Point(64, 34),
                RenderOrder = TileRenderOrder.IsometricDiamondDownHorizontalOffset,
                YSortEnabled = true,
            };

            var decorLayer = new TileMapLayerComponent("Decor", 24, 24, new Point(84, 44), zIndex: 1)
            {
                Offset = new Vector2(540f, 140f),
                TileStride = new Point(64, 34),
                RenderOrder = TileRenderOrder.IsometricDiamondDownHorizontalOffset,
                YSortEnabled = true,
            };

            for (int y = 0; y < baseLayer.Height; y++)
            {
                for (int x = 0; x < baseLayer.Width; x++)
                {
                    if ((x + y) % 2 == 0)
                    {
                        baseLayer.SetTile(x, y, 4);
                    }
                }
            }

            world.CreateEntity().AddComponent(baseLayer);
            world.CreateEntity().AddComponent(decorLayer);

            world.CreateEntity()
                .AddComponent(new TileMapEditorComponent
                {
                    IsEnabled = false,
                    CurrentMapName = "Untitled",
                })
                .AddComponent(new TileMapMenuComponent
                {
                    IsVisible = true,
                    NeedsRefresh = true,
                });
        }

        private void SyncCameraViewportCenters()
        {
            if (world == null)
            {
                return;
            }

            Vector2 viewportCenter = new Vector2(
                GraphicsDevice.Viewport.Width * 0.5f,
                GraphicsDevice.Viewport.Height * 0.5f);

            for (int i = 0; i < world.Entities.Count; i++)
            {
                Entity entity = world.Entities[i];
                if (entity.TryGetComponent<Camera2DComponent>(out Camera2DComponent camera) && camera != null)
                {
                    camera.ViewportCenter = viewportCenter;
                }
            }
        }
    }
}