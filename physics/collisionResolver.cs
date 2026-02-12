using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;

namespace PhysicsElaSim.physics
{
    static class CollisionResolver
    {
        private const float positionCorrectionPercent = 0.2f;
        private const float positionCorrectionMin = 0.01f;
        public static Collision? CheckCollision(RigidBody a, RigidBody b)
        {
            if(a.IsStatic && b.IsStatic) return null;

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
            return new(A, B, normal, depth);
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
            if (!isInside) normal = -normal;

            return new(A, B, normal, circleA.Radius - dist);
        }

        private static Collision? RectVsCircle(RigidBody A, RigidBody B, Rectangle rectA, Circle circleB)
            => CircleVsRect(B, A, circleB, rectA);

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
            float dy1 = downA - upB;
            float dy2 = downB - upA;

            if(dx1 < 0f || dx2 < 0f || dy1 < 0f || dy2 < 0f) return null;
            
            float depth = MathF.Min(MathF.Min(dx1,dx2),Math.Min(dy1,dy2));
            Vector2 normal = Vector2.Zero;
            if(depth == dx1)
            {
                normal = Vector2.Right;
            }
            else if(depth == dx2)
            {
                normal = Vector2.Left;
            }
            else if(depth == dy1)
            {
                normal = Vector2.Up;
            }
            else if(depth == dy2)
            {
                normal = Vector2.Down;
            }

            return new(A,B,normal,depth);
        }

        public static void ResolveVelocity(Collision collision)
        {   
            RigidBody bodyA = collision.BodyA;
            RigidBody bodyB = collision.BodyB;
            Vector2 n = collision.Normal;

            float sumInvMass =  bodyA.InvMass + bodyB.InvMass;
            if (sumInvMass == 0) return;
            
            float restitition = MathF.Max(bodyA.Restitution, bodyB.Restitution);

            Vector2 vRel = bodyA.Velocity - bodyB.Velocity;
            float velAlongNormal = Vector2.Dot(vRel, n);

            if(velAlongNormal >=0f) return;
            float normalImpulse = velAlongNormal * ( -(1f + restitition)) / (Vector2.Dot(n, n) * sumInvMass);

            bodyA.AddImpulse(n * normalImpulse);
            bodyB.AddImpulse(-n * normalImpulse);

            // friction
            Vector2 tangentVel = (vRel - n * Vector2.Dot(vRel,n)).Normalized();
            float friction = MathF.Sqrt(bodyA.Friction*bodyB.Friction);
            float frictionLimit = normalImpulse * friction;

            float tangentImpulse = Math.Clamp(-Vector2.Dot(vRel, tangentVel) / sumInvMass, -frictionLimit, frictionLimit);

            bodyA.AddImpulse(tangentVel * tangentImpulse);
            bodyB.AddImpulse(-tangentVel * tangentImpulse);

        }   
        public static void ResolvePosition(Collision collision)
        {
            RigidBody bodyA = collision.BodyA;
            RigidBody bodyB = collision.BodyB;

            float sumInvMass =  bodyA.InvMass + bodyB.InvMass;
            if (sumInvMass == 0) return;

            float correctionMagnitude = Math.Max(collision.Depth - positionCorrectionMin, 0.0f) / (bodyA.InvMass + bodyB.InvMass) * positionCorrectionPercent;
            Vector2 correction = collision.Normal * correctionMagnitude;

            bodyA.Pos += correction * bodyA.InvMass;
            bodyB.Pos -= correction * bodyB.InvMass;
        }
    }   
}