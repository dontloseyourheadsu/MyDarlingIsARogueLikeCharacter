using System;
using RoguelikeDarling.Core.Localization;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RoguelikeDarling.Core.Camera;
using RoguelikeDarling.Core.Collision2D;
using RoguelikeDarling.Core.Ecs;
using RoguelikeDarling.Core.Input;
using RoguelikeDarling.Core.Physics2D;
using RoguelikeDarling.Core.Rendering2D;
using RoguelikeDarling.Core.Spatial;
using RoguelikeDarling.Core.Tilemap;

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

            world = new World();
            world.AddSystem(new CameraInputSystem());
            world.AddSystem(new TileMapDebugInputSystem());
            world.AddSystem(new PlayerMovementInputSystem());
            world.AddSystem(new RigidBody2DMovementSystem());
            world.AddSystem(new Collision2DSolverSystem());
            world.AddSystem(new IsometricTileMapRenderSystem(GraphicsDevice));
            world.AddSystem(new SpriteRender2DSystem());
            world.AddSystem(new Collision2DDebugRenderSystem(GraphicsDevice, isCollisionDebugEnabled));

            BuildIsometricDemoScene();

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
            // Clears the screen with the MonoGame orange color before drawing.
            GraphicsDevice.Clear(new Color(32, 34, 46));

            if (spriteBatch != null && world != null)
            {
                world.Draw(gameTime, spriteBatch);

                if (hudFont != null)
                {
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
                    string debugText =
                        "Isometric TileMap Demo\n" +
                        "Arrow Keys: Move Camera\n" +
                        "W/A/S/D: Move Playable RigidBody\n" +
                        "Shift + Arrows: Faster Camera\n" +
                        "1..9: Select Layer by Z order\n" +
                        "I/J/K/L: Nudge Selected Layer Offset\n" +
                        "Y: Toggle Y Sort (Selected Layer)\n" +
                        "+/-: Change Selected Layer Draw Tile Size\n" +
                        "Shift + +/-: Change Selected Layer Tile Stride\n" +
                        "PageUp/PageDown: Change Selected Layer ZIndex\n" +
                        "Player collides with map + static rigidbody\n" +
                        $"Collision debug: {(isCollisionDebugEnabled ? "ON" : "OFF")}\n" +
                        "Esc: Exit";
                    spriteBatch.DrawString(hudFont, debugText, new Vector2(16f, 16f), Color.White);
                    spriteBatch.End();
                }
            }

            base.Draw(gameTime);
        }

        private void BuildIsometricDemoScene()
        {
            if (world == null)
            {
                return;
            }

            const int worldCollisionLayer = 0;
            const int actorCollisionLayer = 1;
            uint collideWithWorld = 1u << worldCollisionLayer;
            uint collideWithActors = 1u << actorCollisionLayer;

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

            paletteEntity.AddComponent(palette);

            var cameraEntity = world.CreateEntity();
            cameraEntity
                .AddComponent(new Camera2DComponent { Position = new Vector2(0f, 0f), Zoom = 1f })
                .AddComponent(new CameraControllerComponent { MoveSpeedPixelsPerSecond = 420f });

            var atlasLayer = new TileMapLayerComponent("AtlasGround", 16, 16, new Point(100, 60), zIndex: 0)
            {
                Offset = new Vector2(420f, 100f),
                TileStride = new Point(75, 40),
                RenderOrder = TileRenderOrder.IsometricDiamondDownHorizontalOffset,
                YSortEnabled = true,
            };

            var atlasCollisionLayer = new TileMapCollisionLayerComponent(atlasLayer.Width, atlasLayer.Height, isCollisionDebugEnabled, collisionDebugColor)
            {
                ShapeType = CollisionShapeType.Polygon,
                PolygonPoints = new[]
                {
                    new Vector2(0.5f, 0f),
                    new Vector2(1f, 0.5f),
                    new Vector2(0.5f, 1f),
                    new Vector2(0f, 0.5f),
                },
            };

            for (int y = 0; y < atlasLayer.Height; y++)
            {
                for (int x = 0; x < atlasLayer.Width; x++)
                {
                    int tile = ((x + y) % 3) + 1;
                    atlasLayer.SetTile(x, y, tile);

                    bool border = x == 0 || y == 0 || x == atlasLayer.Width - 1 || y == atlasLayer.Height - 1;
                    bool centerBarrier = y == (atlasLayer.Height / 2) && x >= 4 && x <= 11;
                    bool diagonalBarrier = x == y && x >= 3 && x <= 8;
                    atlasCollisionLayer.SetSolid(x, y, border || centerBarrier || diagonalBarrier);
                }
            }

            var mixedLayer = new TileMapLayerComponent("MixedDecor", 10, 10, new Point(36, 18), zIndex: 1)
            {
                Offset = new Vector2(430f, 130f),
                RenderOrder = TileRenderOrder.IsometricDiamondDownHorizontalOffset,
                YSortEnabled = true,
            };

            for (int y = 0; y < mixedLayer.Height; y++)
            {
                for (int x = 0; x < mixedLayer.Width; x++)
                {
                    if ((x + y) % 2 == 0)
                    {
                        mixedLayer.SetTile(x, y, 2);
                    }
                }
            }

            var proceduralLayer = new TileMapLayerComponent("ProceduralTop", 8, 8, new Point(48, 24), zIndex: 2)
            {
                Offset = new Vector2(470f, 80f),
                RenderOrder = TileRenderOrder.IsometricDiamondDownHorizontalOffset,
                YSortEnabled = false,
            };

            for (int y = 0; y < proceduralLayer.Height; y++)
            {
                for (int x = 0; x < proceduralLayer.Width; x++)
                {
                    if (x == y || x + y == proceduralLayer.Width - 1)
                    {
                        proceduralLayer.SetTile(x, y, 4);
                    }
                    else if (x == proceduralLayer.Width / 2 || y == proceduralLayer.Height / 2)
                    {
                        proceduralLayer.SetTile(x, y, 5);
                    }
                }
            }

            world.CreateEntity()
                .AddComponent(atlasLayer)
                .AddComponent(atlasCollisionLayer)
                .AddComponent(new CollisionLayerComponent(worldCollisionLayer, collideWithActors));
            world.CreateEntity().AddComponent(mixedLayer);
            world.CreateEntity().AddComponent(proceduralLayer);

            world.CreateEntity()
                .AddComponent(new Transform2DComponent { Position = new Vector2(430f, 260f), Scale = Vector2.One })
                .AddComponent(new PlayerControllerComponent { MoveSpeedPixelsPerSecond = 175f })
                .AddComponent(new RigidBody2DComponent { IsStatic = false })
                .AddComponent(Collider2DComponent.CreateRectangle(new Vector2(44f, 28f), isCollisionDebugEnabled, collisionDebugColor))
                .AddComponent(new CollisionLayerComponent(actorCollisionLayer, collideWithWorld | collideWithActors))
                .AddComponent(new SpriteRender2DComponent
                {
                    Texture = tinyBlocksTexture,
                    SourceRect = new Rectangle(72, 0, 18, 18),
                    LocalScale = new Vector2(2.4f, 2.1f),
                    LocalPosition = Vector2.Zero,
                    ZIndex = 50,
                });

            world.CreateEntity()
                .AddComponent(new Transform2DComponent { Position = new Vector2(550f, 320f), Scale = Vector2.One })
                .AddComponent(new RigidBody2DComponent { IsStatic = true })
                .AddComponent(Collider2DComponent.CreateOval(new Vector2(58f, 44f), isCollisionDebugEnabled, collisionDebugColor))
                .AddComponent(new CollisionLayerComponent(actorCollisionLayer, collideWithActors))
                .AddComponent(new SpriteRender2DComponent
                {
                    Texture = tinyBlocksTexture,
                    SourceRect = new Rectangle(90, 0, 18, 18),
                    LocalScale = new Vector2(2.8f, 2.4f),
                    LocalPosition = new Vector2(0f, -4f),
                    ZIndex = 51,
                    Tint = new Color(230, 210, 255),
                });
        }
    }
}