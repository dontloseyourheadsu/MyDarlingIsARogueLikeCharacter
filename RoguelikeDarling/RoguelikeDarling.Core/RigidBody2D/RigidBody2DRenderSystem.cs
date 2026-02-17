using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RoguelikeDarling.Core.Camera;
using RoguelikeDarling.Core.Ecs;

namespace RoguelikeDarling.Core.RigidBody2D
{
    public sealed class RigidBody2DRenderSystem : IGameSystem
    {
        public void Update(World world, GameTime gameTime)
        {
        }

        public void Draw(World world, GameTime gameTime, SpriteBatch spriteBatch)
        {
            Camera2DComponent camera = null;
            var drawCommands = new List<RenderCommand>();

            for (int i = 0; i < world.Entities.Count; i++)
            {
                Entity entity = world.Entities[i];
                if (camera == null && entity.TryGetComponent<Camera2DComponent>(out Camera2DComponent cameraComponent))
                {
                    camera = cameraComponent;
                }

                if (!entity.TryGetComponent<Transform2DComponent>(out Transform2DComponent transform)
                    || !entity.TryGetComponent<RigidBody2DRenderComponent>(out RigidBody2DRenderComponent render)
                    || transform == null
                    || render == null
                    || render.Texture == null)
                {
                    continue;
                }

                Vector2 worldPosition = transform.Position + new Vector2(
                    render.LocalPosition.X * transform.Scale.X,
                    render.LocalPosition.Y * transform.Scale.Y);

                Vector2 worldScale = new Vector2(
                    transform.Scale.X * render.LocalScale.X,
                    transform.Scale.Y * render.LocalScale.Y);

                int width = (int)(render.SourceRect.Width * worldScale.X);
                int height = (int)(render.SourceRect.Height * worldScale.Y);
                if (width <= 0 || height <= 0)
                {
                    continue;
                }

                var destination = new Rectangle(
                    (int)(worldPosition.X - (width * 0.5f)),
                    (int)(worldPosition.Y - (height * 0.5f)),
                    width,
                    height);

                drawCommands.Add(new RenderCommand(render.Texture, render.SourceRect, destination, render.Tint, render.ZIndex));
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
                RenderCommand cmd = drawCommands[i];
                spriteBatch.Draw(cmd.Texture, cmd.Destination, cmd.SourceRect, cmd.Tint);
            }

            spriteBatch.End();
        }

        private readonly struct RenderCommand
        {
            public RenderCommand(Texture2D texture, Rectangle sourceRect, Rectangle destination, Color tint, int zIndex)
            {
                Texture = texture;
                SourceRect = sourceRect;
                Destination = destination;
                Tint = tint;
                ZIndex = zIndex;
            }

            public Texture2D Texture { get; }

            public Rectangle SourceRect { get; }

            public Rectangle Destination { get; }

            public Color Tint { get; }

            public int ZIndex { get; }
        }
    }
}