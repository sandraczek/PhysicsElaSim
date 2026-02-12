using System.Data;
using System.Globalization;

namespace PhysicsElaSim.physics
{
    class World 
    {
        private readonly Vector2 GravityAcceleration;
        private readonly float _gravityMultiplier = 5f;
        private readonly int _velCorrectionNum = 20;
        private readonly int _posCorrectionNum = 3;
        public Dictionary<int,RigidBody> Bodies;
        private List<Collision> collisions = [];
        private float _sleepVelThresholdBias = 2f;
        private float _sleepVelThreshold;
        private float _wakeUpMult = 0.1f;

        public World(Vector2 gravity)
        {
            GravityAcceleration = gravity;
            Bodies = [];
        }
        public World()
        {
            //GravityAcceleration = Vector2.Zero;
            GravityAcceleration = new Vector2(0, 9.81f);
            Bodies = [];
        }

        public void FixedUpdate(float FixedDt) {
            PhysicsUpdate(FixedDt);
            ResolveCollisions();
        }    //😎
        private void PhysicsUpdate(float dt)
        {
            // sleep threshold is how small velocities must be for an object to sleep; max with 0.05 so that when gravity is 0 they still sleep
            _sleepVelThreshold = Math.Max(GravityAcceleration.Length() * _gravityMultiplier * _sleepVelThresholdBias * dt, 0.05f);

            foreach (RigidBody body in Bodies.Values) {

                if(body.IsStatic) continue;
                if(!body.IsAwake) continue;

                body.Acceleration = Vector2.Zero;
                body.Acceleration += GravityAcceleration * _gravityMultiplier;
                body.Velocity += body.Acceleration * dt;
                body.Pos += body.Velocity * dt;

                body.UpdateSleep(dt, _sleepVelThreshold);
            }
        }
        private void ResolveCollisions()
        {
            collisions.Clear();
            foreach(RigidBody body1 in Bodies.Values)
            {
                foreach (RigidBody body2 in Bodies.Values)
                {
                    if(body2 == body1) break;

                    Collision? coll = CollisionResolver.CheckCollision(body1, body2);
                    if(!coll.HasValue) continue;

                    collisions.Add(coll.Value);
                    
                    // handle waking up: this is a tradeof - saves computing power but sometimes velocity is not enough to wake up a ball
                    // so its like concrete (usually only with heavy balls and lower velocities)
                    float velAlongNormal = Vector2.Dot(body2.Velocity - body1.Velocity, coll.Value.Normal);
                    float wakeThreshold = _sleepVelThreshold * _wakeUpMult;
                    
                    if(velAlongNormal < -wakeThreshold)
                    {
                        if(!body1.IsAwake) body1.WakeUp();
                        if(!body2.IsAwake) body2.WakeUp();
                    }

                    if(body1.IsAwake && body2.IsAwake) 
                        Console.WriteLine("Collision: " + body1.Id.ToString() + " and " + body2.Id.ToString());
                }
            }

            for (int i = 0;i<_velCorrectionNum;i++){
                if(i%2 == 0){
                    for (int j = 0;j<collisions.Count;j++)
                    {
                        CollisionResolver.ResolveVelocity(collisions[j]);
                    }
                }
                else            // were doing forwards -> backwards -> forwards ->.. , it fixes chain ball collisions
                {
                    for (int j = collisions.Count -1 ;j>=0;j--)
                    {
                        CollisionResolver.ResolveVelocity(collisions[j]);
                    }
                }
            }
            for (int i = 0; i < _posCorrectionNum; i++)
            {
                foreach (Collision coll in collisions)
                {
                    Collision? trueColl = CollisionResolver.CheckCollision(coll.BodyA,coll.BodyB); // checking again
                    if(!trueColl.HasValue) continue;

                    CollisionResolver.ResolvePosition(trueColl.Value);
                }
            }
        }
    }
}