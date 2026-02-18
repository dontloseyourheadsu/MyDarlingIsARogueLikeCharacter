using Microsoft.Xna.Framework;
using RoguelikeDarling.Core.Ecs;
using RoguelikeDarling.Core.Physics2D;

namespace RoguelikeDarling.Core.Camera
{
    public sealed class CameraControllerComponent
    {
        public float MoveSpeedPixelsPerSecond { get; set; } = 250f;

        public int? AttachedRigidBodyEntityId { get; private set; }

        public Vector2 FollowOffset { get; private set; }

        public bool IsAttachedToRigidBody => AttachedRigidBodyEntityId.HasValue;

        public bool AttachToRigidBody(Entity entity, Vector2? followOffset = null)
        {
            if (entity == null || !entity.TryGetComponent<RigidBody2DComponent>(out RigidBody2DComponent rigidBody) || rigidBody == null)
            {
                return false;
            }

            AttachedRigidBodyEntityId = entity.Id;
            FollowOffset = followOffset ?? Vector2.Zero;
            return true;
        }

        public void DetachFromRigidBody()
        {
            AttachedRigidBodyEntityId = null;
            FollowOffset = Vector2.Zero;
        }
    }
}