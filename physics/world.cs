namespace PhysicsElaSim.physics
{
    public class World
    {
        public Vector2 GravityAcceleration;
        public List<RigidBody> Bodies;

        public World(Vector2 gravity)
        {
            GravityAcceleration = gravity;
            Bodies = [];
        }

        public World()
        {
            GravityAcceleration = Vector2.Zero;
            Bodies = [];
        }

        public void Update(float dt)
        {
            foreach (RigidBody body in Bodies)
            {
                body.Acceleration = Vector2.Zero;
                body.Acceleration += GravityAcceleration;

                body.Velocity += body.Acceleration * dt;

                body.Pos += body.Velocity * dt;

                //resolve collisions
            }
        } //😎
    }
}
