namespace PhysicsElaSim.physics
{
        public class RigidBody
    {
        public int Id;
        private static int nextId = 0;
        public Vector2 Pos;
        public Vector2 Velocity;
        public Vector2 Acceleration;
        public float InvMass;
        public Shape Shape;
        public float Restitution;
        public bool IsStatic;

        public RigidBody(Shape shape, Vector2 pos = default, bool isStatic = false,float invMass = 1.0f, float restitution = 0.5f)
        {
            Shape = shape;
            Pos = pos;
            Velocity = Vector2.Zero;
            Acceleration = Vector2.Zero;
            InvMass = invMass;
            Restitution = restitution;
            IsStatic = isStatic;

            Id = nextId;
            nextId++;
        }

        public void AddImpulse(Vector2 impulse) {
            if(IsStatic) return;
            Velocity += impulse * InvMass;
            Console.WriteLine("Adding Impulse " + impulse.X.ToString() + ", " + impulse.Y.ToString() + " to " + Id.ToString());
        }
    }
}