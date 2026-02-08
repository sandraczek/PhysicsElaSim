
namespace PhysicsElaSim.physics
{
    static class CollisionResolver
    {

        public static Collision? CheckCollision(RigidBody a, RigidBody b)
        {
            return (a.Shape, b.Shape) switch
            {
                (Circle cA, Circle cB) => CircleVsCircle(a, b, cA, cB),
                (Circle cA, Rectangle rB) => CircleVsRect(a, b, cA, rB),
                (Rectangle rA, Circle cB) => RectVsCircle(a, b, rA, cB),
                (Rectangle rA, Rectangle rB) => RectVsRect(a, b, rA, rB),
                _ => null
            };
        }

        static Collision? CircleVsCircle(RigidBody A, RigidBody B, Circle circleA, Circle circleB) {
            
            //float depth = circleA.radius + circleB.radius - Vector2.distance(A.pos, B.pos);
            return null;
        }

        static Collision? CircleVsRect(RigidBody A, RigidBody B, Circle circleA, Rectangle rectB) {
            return null;
        }

        static Collision? RectVsCircle(RigidBody A, RigidBody B, Rectangle rectA, Circle circleB) {
            return null;
        }


        static Collision? RectVsRect(RigidBody A, RigidBody B, Rectangle rectA, Rectangle rectB)
        {
            return null;
        }

        static void ResolveCollision(Collision collision)
        {   
            RigidBody bodyA = collision.BodyA;
            RigidBody bodyB = collision.BodyB;
            Vector2 n = collision.Normal;
            
            float e = Math.Min(bodyA.Restitution, bodyB.Restitution);

            Vector2 vAB = bodyA.Velocity - bodyB.Velocity;

            float j1 = Vector2.Dot(n, n) * (bodyA.InvMass + bodyB.InvMass);
            if(j1 == 0) return;
            float j = Vector2.Dot(n, vAB) * ( -(1f + e)) / j1;

            bodyA.AddImpulse(n * j);
            bodyB.AddImpulse(-n * j);
        }   

        //check collision -> body a, body b, vec normal, depth
        //resolve collision -> pos, impulse 
        //http://www.chrishecker.com/images/e/e7/Gdmphys3.pdf
    }
}