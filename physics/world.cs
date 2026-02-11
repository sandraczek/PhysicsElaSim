namespace PhysicsElaSim.physics
{
    class World 
    {
        public Vector2 GravityAcceleration;
        public float gravityMultiplier = 5f;
        public Dictionary<int,RigidBody> Bodies;

        public World(Vector2 gravity)
        {
            GravityAcceleration = gravity;
            Bodies = [];
        }
        public World()
        {
            GravityAcceleration = new Vector2(0, 9.81f);
            Bodies = [];
        }

        public void Update(float dt) {
            PhysicsUpdate(dt);
            ResolveCollisions();
        }    //😎
        private void PhysicsUpdate(float dt)
        {
            foreach (RigidBody body in Bodies.Values) {

                if(body.IsStatic) continue;
                body.Acceleration = Vector2.Zero;
                body.Acceleration += GravityAcceleration * gravityMultiplier;
                
                body.Velocity += body.Acceleration * dt;
                
                body.Pos += body.Velocity * dt;
            }
        }
        private void ResolveCollisions()
        {
            foreach(RigidBody body1 in Bodies.Values)
            {
                foreach (RigidBody body2 in Bodies.Values)
                    {
                        if(body2 == body1) break;

                        Collision? coll = CollisionResolver.CheckCollision(body1, body2);
                        if(!coll.HasValue) continue;

                        Console.WriteLine("Collision: " + body1.Id.ToString() + " and " + body2.Id.ToString());
                        CollisionResolver.ResolveCollision(coll.Value);
                    }
            }
        }
    }
}