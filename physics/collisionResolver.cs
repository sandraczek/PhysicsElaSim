using System.Numerics;

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

        private static Collision? CircleVsCircle(RigidBody A, RigidBody B, Circle circleA, Circle circleB) {
            
            float depth = circleA.Radius + circleB.Radius - Vector2.Distance(A.Pos, B.Pos);
            Vector2 normal = (A.Pos - B.Pos).Normalized();
            
            if (depth < 0) return null;
            return new Collision(A, B, normal, depth);
        }

        private static Collision? CircleVsRect(RigidBody A, RigidBody B, Circle circleA, Rectangle rectB) {
            Vector2 d = B.Pos - A.Pos;
            float halfW = rectB.Width * 0.5f;
            float halfH = rectB.Height * 0.5f;

            Vector2 closestPoint = new(Math.Clamp(d.X, -halfW, halfW), Math.Clamp(d.Y, -halfH, halfH));
            Vector2 normalVec = d - closestPoint;

            float distSq = normalVec.LengthSquared();

            bool isInside = false;
            Vector2 closestSide;
            if (distSq == 0) // circle center is inside the rectangle
            {
                isInside = true;
                if (Math.Abs(d.X) / halfW > Math.Abs(d.Y) / halfH) 
                {
                    closestSide = new(d.X > 0 ? halfW : -halfW, closestPoint.Y);
                }
                else
                {
                    closestSide = new(closestPoint.X, d.Y > 0 ? halfH : -halfH);
                }
                normalVec = d - closestSide;
            }

            float dist = normalVec.Length();

            if (dist > circleA.Radius && !isInside) return null;

            Vector2 normal = normalVec.Normalized();
            if (isInside) normal = -normal;

            return new(A, B, normal, circleA.Radius - dist);
        }

        private static Collision? RectVsCircle(RigidBody A, RigidBody B, Rectangle rectA, Circle circleB) {
            Collision? collision = CircleVsRect(B, A, circleB, rectA);
            if (collision.HasValue)
            {
                Collision c = collision.Value;
                return new Collision(c.BodyB, c.BodyA, c.Normal, c.Depth);
            }
            return null;
        }


        private static Collision? RectVsRect(RigidBody A, RigidBody B, Rectangle rectA, Rectangle rectB)
        {
            float leftA = A.Pos.X - rectA.Width * 0.5f;
            float rightA = A.Pos.X + rectA.Width * 0.5f;
            float leftB = B.Pos.X - rectB.Width * 0.5f;
            float rightB = B.Pos.X + rectB.Width * 0.5f;
            float upA = A.Pos.Y - rectA.Height * 0.5f;
            float downA = A.Pos.Y + rectA.Height * 0.5f;
            float upB = B.Pos.Y - rectB.Height * 0.5f;
            float downB = B.Pos.Y + rectB.Height * 0.5f;

            float dx1 = rightB - leftA;
            float dx2 = rightA - leftB;
            float dy1 = upB - downA;
            float dy2 = upA - downB;

            if(dx1 < 0f || dx2 < 0f || dy1 < 0f || dy2 < 0f) return null;
            
            return null;
            // float depth = 
        }

        public static void ResolveCollision(Collision collision)
        {   
            RigidBody bodyA = collision.BodyA;
            RigidBody bodyB = collision.BodyB;
            Vector2 n = collision.Normal;
            
            float e = Math.Min(bodyA.Restitution, bodyB.Restitution);

            Vector2 vAB = bodyA.Velocity - bodyB.Velocity;
            float velAlongNormal = Vector2.Dot(vAB, collision.Normal);
            if(velAlongNormal >0f) return;

            float j1 = Vector2.Dot(n, n) * (bodyA.InvMass + bodyB.InvMass);
            if (j1 == 0) return;
            float j = Vector2.Dot(n, vAB) * ( -(1f + e)) / j1;

            bodyA.AddImpulse(n * j);
            bodyB.AddImpulse(-n * j);
        }   

        //check collision -> body a, body b, vec normal, depth
        //resolve collision -> pos, impulse 
        //http://www.chrishecker.com/images/e/e7/Gdmphys3.pdf
    }
}