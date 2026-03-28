using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RoguelikeDarling.Core.Camera;
using RoguelikeDarling.Core.Ecs;
using RoguelikeDarling.Core.Spatial;

namespace RoguelikeDarling.Core.Skeleton2D.Humanoid
{
    public sealed class HumanoidSkeletonRenderSystem : IGameSystem
    {
        private readonly Texture2D pixelTexture;

        public HumanoidSkeletonRenderSystem(GraphicsDevice graphicsDevice)
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
            var drawCommands = new List<DrawCommand>();

            for (int i = 0; i < world.Entities.Count; i++)
            {
                Entity entity = world.Entities[i];

                if (camera == null && entity.TryGetComponent<Camera2DComponent>(out Camera2DComponent cameraComponent))
                {
                    camera = cameraComponent;
                }

                if (!entity.TryGetComponent<Transform2DComponent>(out Transform2DComponent transform)
                    || !entity.TryGetComponent<SkeletonPartRender2DComponent>(out SkeletonPartRender2DComponent render)
                    || transform == null
                    || render == null
                    || !render.Visible)
                {
                    continue;
                }

                Texture2D texture = render.Texture ?? pixelTexture;
                Rectangle? sourceRect = render.Texture != null
                    ? render.SourceRect ?? new Rectangle(0, 0, texture.Width, texture.Height)
                    : new Rectangle(0, 0, 1, 1);

                Vector2 sourceSize = sourceRect.HasValue
                    ? new Vector2(sourceRect.Value.Width, sourceRect.Value.Height)
                    : new Vector2(texture.Width, texture.Height);

                if (sourceSize.X <= 0f || sourceSize.Y <= 0f)
                {
                    continue;
                }

                Vector2 size = new Vector2(
                    render.Size.X * transform.Scale.X,
                    render.Size.Y * transform.Scale.Y);

                if (size.X <= 0f || size.Y <= 0f)
                {
                    continue;
                }

                Vector2 scale = new Vector2(size.X / sourceSize.X, size.Y / sourceSize.Y);

                Vector2 pivot = render.Pivot;
                if (render.Texture == null)
                {
                    pivot = new Vector2(
                        render.Size.X > 0f ? render.Pivot.X / render.Size.X : 0f,
                        render.Size.Y > 0f ? render.Pivot.Y / render.Size.Y : 0f);
                }

                drawCommands.Add(new DrawCommand(
                    texture,
                    sourceRect,
                    transform.Position,
                    render.Color,
                    transform.Rotation,
                    pivot,
                    scale,
                    render.ZIndex));
            }

            if (drawCommands.Count == 0)
            {
                return;
            }

            drawCommands.Sort((a, b) => a.ZIndex.CompareTo(b.ZIndex));

            spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.PointClamp,
                DepthStencilState.None,
                RasterizerState.CullNone,
                effect: null,
                transformMatrix: camera?.GetViewMatrix() ?? Matrix.Identity);

            for (int i = 0; i < drawCommands.Count; i++)
            {
                DrawCommand cmd = drawCommands[i];
                spriteBatch.Draw(
                    cmd.Texture,
                    cmd.Position,
                    cmd.SourceRect,
                    cmd.Color,
                    cmd.Rotation,
                    cmd.Origin,
                    cmd.Scale,
                    SpriteEffects.None,
                    0f);
            }

            spriteBatch.End();
        }

        private readonly struct DrawCommand
        {
            public DrawCommand(
                Texture2D texture,
                Rectangle? sourceRect,
                Vector2 position,
                Color color,
                float rotation,
                Vector2 origin,
                Vector2 scale,
                int zIndex)
            {
                Texture = texture;
                SourceRect = sourceRect;
                Position = position;
                Color = color;
                Rotation = rotation;
                Origin = origin;
                Scale = scale;
                ZIndex = zIndex;
            }

            public Texture2D Texture { get; }

            public Rectangle? SourceRect { get; }

            public Vector2 Position { get; }

            public Color Color { get; }

            public float Rotation { get; }

            public Vector2 Origin { get; }

            public Vector2 Scale { get; }

            public int ZIndex { get; }
        }
    }
}
