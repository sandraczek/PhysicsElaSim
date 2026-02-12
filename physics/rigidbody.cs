using System.Diagnostics;
using System.Diagnostics.Contracts;

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
        public float Friction;
        public bool IsStatic;
        public bool IsAwake = true;
        private float sleepTimer = 0f;
        private const float timeToSleep = 1f;

        public RigidBody(Shape shape, Vector2 pos, bool isStatic = false,float invMass = 1.0f, float restitution = 0.25f, float friction = 0.25f)
        {
            Shape = shape;
            Pos = pos;
            Velocity = Vector2.Zero;
            Acceleration = Vector2.Zero;
            InvMass = invMass;
            Restitution = restitution;
            Friction = friction;
            IsStatic = isStatic;

            Id = nextId;
            if(nextId == int.MaxValue) throw new("Id Limit Reached.");
            nextId++;
        }

        public void AddImpulse(Vector2 impulse) {
            if(IsStatic) return;
            if(impulse == Vector2.Zero) return;
            Velocity += impulse * InvMass;
            Console.WriteLine("Adding Impulse " + impulse.ToString() + " to Entity: " + Id.ToString());
        }
        public void UpdateSleep(float dt, float sleepVelThreshold) // object go to sleep when inactive
        {
            if(IsStatic) return;
            if (Velocity.LengthSquared() < sleepVelThreshold * sleepVelThreshold)
            {
                sleepTimer+=dt;
                if(sleepTimer >= timeToSleep)
                {
                    IsAwake = false;
                    Velocity = Vector2.Zero;
                    Console.WriteLine("Going to sleep: " + Id);
                }
            }
            else
            {
                WakeUp();
            }
        }
        public void WakeUp()
        {
            if(IsStatic) return;
            sleepTimer = 0f;
            if(!IsAwake) Console.WriteLine("Waking up: " + Id);
            IsAwake = true;
        }
    }
}