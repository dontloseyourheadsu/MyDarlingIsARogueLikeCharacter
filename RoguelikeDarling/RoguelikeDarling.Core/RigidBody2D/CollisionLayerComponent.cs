namespace RoguelikeDarling.Core.RigidBody2D
{
    public sealed class CollisionLayerComponent
    {
        public CollisionLayerComponent(int layer, uint collidesWithMask)
        {
            Layer = layer;
            CollidesWithMask = collidesWithMask;
        }

        public int Layer { get; }

        public uint CollidesWithMask { get; set; }

        public bool CanCollideWith(CollisionLayerComponent other)
        {
            if (other == null || Layer < 0 || Layer > 31 || other.Layer < 0 || other.Layer > 31)
            {
                return false;
            }

            uint thisMask = 1u << other.Layer;
            uint otherMask = 1u << Layer;
            return (CollidesWithMask & thisMask) != 0 && (other.CollidesWithMask & otherMask) != 0;
        }
    }
}