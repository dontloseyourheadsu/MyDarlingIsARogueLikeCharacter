using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RoguelikeDarling.Core.Camera;
using RoguelikeDarling.Core.Ecs;
using RoguelikeDarling.Core.Spatial;
using RoguelikeDarling.Core.Tilemap;

namespace RoguelikeDarling.Core.Collision2D
{
    public sealed class Collision2DDebugRenderSystem : IGameSystem
    {
        private readonly Texture2D pixelTexture;
        private readonly bool isDebugEnabled;

        public Collision2DDebugRenderSystem(GraphicsDevice graphicsDevice, bool isDebugEnabled)
        {
            pixelTexture = new Texture2D(graphicsDevice, 1, 1);
            pixelTexture.SetData(new[] { Color.White });
            this.isDebugEnabled = isDebugEnabled;
        }

        public void Update(World world, GameTime gameTime)
        {
        }

        public void Draw(World world, GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (!isDebugEnabled)
            {
                return;
            }

            Camera2DComponent camera = null;
            for (int i = 0; i < world.Entities.Count; i++)
            {
                if (world.Entities[i].TryGetComponent<Camera2DComponent>(out Camera2DComponent cameraComponent) && cameraComponent != null)
                {
                    camera = cameraComponent;
                    break;
                }
            }

            spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.PointClamp,
                DepthStencilState.None,
                RasterizerState.CullNone,
                effect: null,
                transformMatrix: camera?.GetViewMatrix() ?? Matrix.Identity);

            for (int i = 0; i < world.Entities.Count; i++)
            {
                Entity entity = world.Entities[i];

                if (entity.TryGetComponent<Transform2DComponent>(out Transform2DComponent transform)
                    && entity.TryGetComponent<Collider2DComponent>(out Collider2DComponent collider)
                    && transform != null
                    && collider != null)
                {
                    collider.RenderDebugCollision(spriteBatch, pixelTexture, transform);
                }

                if (entity.TryGetComponent<TileMapLayerComponent>(out TileMapLayerComponent tileLayer)
                    && entity.TryGetComponent<TileMapCollisionLayerComponent>(out TileMapCollisionLayerComponent tileCollisionLayer)
                    && tileLayer != null
                    && tileCollisionLayer != null)
                {
                    tileCollisionLayer.RenderDebugCollision(spriteBatch, pixelTexture, tileLayer);
                }
            }

            spriteBatch.End();
        }
    }
}
