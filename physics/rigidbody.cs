namespace PhysicsElaSim.physics
{
        public class RigidBody
    {
        public Vector2 Pos;
        public Vector2 Velocity;
        public Vector2 Acceleration;
        public float InvMass;
        public Shape Shape;
        public float Restitution;

        public RigidBody(Shape shape, Vector2 pos = default, float invMass = 1.0f, float restitution = 0.5f)
        {
            Shape = shape;
            Pos = pos;
            Velocity = Vector2.Zero;
            Acceleration = Vector2.Zero;
            InvMass = invMass;
            Restitution = restitution;
        }

        public void AddImpulse(Vector2 impulse) {
            Velocity += impulse * InvMass;
        }
    }
}