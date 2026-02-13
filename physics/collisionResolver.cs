using System.Diagnostics;
using System.Numerics;
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
                (Rectangle rA, Rectangle rB) => SAT(a, b, rA, rB),
                _ => null
            };
        }

        private static Collision? CircleVsCircle(RigidBody A, RigidBody B, Circle circleA, Circle circleB) {
            
            float depth = circleA.Radius + circleB.Radius - Vector2.Distance(A.Pos, B.Pos);
            Vector2 normal = (A.Pos - B.Pos).Normalized();
            
            if (depth < 0) return null;
            Vector2 contactPoint = A.Pos + normal * circleA.Radius;
            return new(A, B, normal, contactPoint, depth);
        }

        private static Collision? CircleVsRect(RigidBody A, RigidBody B, Circle circleA, Rectangle rectB) {
            //calculating circle's position relative to rectangle's orientation
            Vector2 d = A.Pos - B.Pos;
            Vector2 posRel = Vector2.Rotate(d, -B.Rotation);

            //clamping in relative coordinates
            float halfW = rectB.Width * 0.5f;
            float halfH = rectB.Height * 0.5f;
            Vector2 contactPointRel = new(Math.Clamp(posRel.X, -halfW, halfW), Math.Clamp(posRel.Y, -halfH, halfH));
            Vector2 normalVecRel = posRel - contactPointRel;

            float dist = normalVecRel.Length();
            Vector2 normalRel;
            //if the circle center is inside the rectangle we need to find the closest side to push it out
            //bool isInside = dist == 0;
            bool isInside = posRel.X > -halfW && posRel.X < halfW && posRel.Y > -halfH && posRel.Y < halfH;
            if (isInside)
            {
                float overlapX = halfW - MathF.Abs(posRel.X);
                float overlapY = halfH - MathF.Abs(posRel.Y);

                if (overlapX < overlapY) //left or right side is the closest
                {
                    normalRel = new(posRel.X > 0 ? 1 : -1, 0); //normal points from the side to the circle centre
                    contactPointRel = new(posRel.X > 0 ? halfW : -halfW, posRel.Y);
                    dist = overlapX;
                }   
                else //top or bottom side is the closest
                {
                    normalRel = new(0, posRel.Y > 0 ? 1 : -1);
                    contactPointRel = new(posRel.X, posRel.Y > 0 ? halfH : -halfH);
                    dist = overlapY;
                }
            }
            else
            {
                if (dist > circleA.Radius) return null;
                normalRel = normalVecRel.Normalized();
            }


            //transforming back to normal world coordinates
            Vector2 normal = Vector2.Rotate(normalRel, B.Rotation);
            Vector2 contactPoint = Vector2.Rotate(contactPointRel, B.Rotation) + B.Pos;

            float depth = isInside ? circleA.Radius + dist : circleA.Radius - dist;

            Console.WriteLine("Circle Vs Rect: normal(" + normal.ToString() + ") contact point("+contactPoint.ToString()+"), depth(" + depth + ")");
            return new(A, B, normal, contactPoint, depth);
        }

        private static Collision? RectVsCircle(RigidBody A, RigidBody B, Rectangle rectA, Circle circleB)
            => CircleVsRect(B, A, circleB, rectA);

        private static Collision? AABB(RigidBody A, RigidBody B, Rectangle rectA, Rectangle rectB)
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
            return null;
            //return new(A,B,normal,depth);
        }
        private static Collision? SAT(RigidBody A, RigidBody B, Rectangle rectA, Rectangle rectB)
        {
            List<Vector2> VerticesA = rectA.GetVertices(A.Pos,A.Rotation);
            List<Vector2> VerticesB = rectB.GetVertices(B.Pos,B.Rotation);
            
            List<Vector2> axesA = [];
            axesA.Add(new (MathF.Cos(A.Rotation), MathF.Sin(A.Rotation)));
            axesA.Add(new (-MathF.Sin(A.Rotation), MathF.Cos(A.Rotation)));
            List<Vector2> axesB = [];
            axesB.Add(new (MathF.Cos(B.Rotation), MathF.Sin(B.Rotation)));
            axesB.Add(new (-MathF.Sin(B.Rotation), MathF.Cos(B.Rotation)));

            Vector2 normal = Vector2.Zero;
            float minOverlap = float.MaxValue;
            bool isA = true;

            foreach (Vector2 axis in axesA)
            {
                float minA,maxA,minB,maxB;
                ProjectVertices(VerticesA, axis, out minA, out maxA);
                ProjectVertices(VerticesB, axis, out minB, out maxB);

                float overlap = MathF.Min(maxB,maxA) - MathF.Max(minA,minB);
                if(overlap < 0f) return null;
                if(overlap < minOverlap)
                {
                    minOverlap = overlap;
                    normal = axis;
                    isA = true;
                }
            }
            foreach (Vector2 axis in axesB)
            {
                float minA,maxA,minB,maxB;
                ProjectVertices(VerticesA, axis, out minA, out maxA);
                ProjectVertices(VerticesB, axis, out minB, out maxB);
                
                float overlap = MathF.Min(maxB,maxA) - MathF.Max(minA,minB);
                if(overlap < 0f) return null;
                if(overlap < minOverlap)
                {
                    minOverlap = overlap;
                    normal = axis;
                    isA = false;
                }
            }

            if(Vector2.Dot(B.Pos - A.Pos, normal) > 0f ) normal = -normal ; //setting sense

            Vector2 cp = Vector2.Zero;
            float maxDepth = float.MinValue;
            foreach(Vector2 vertex in isA? VerticesB : VerticesA)
            {
                float d = Vector2.Dot(vertex, isA? normal:-normal);
                if(d > maxDepth)
                {
                    maxDepth = d;
                    cp = vertex;
                }
            }
            return new(A,B,normal, cp ,minOverlap);
        }

        public static void ResolveVelocity(Collision collision)
        {   
            
            RigidBody bodyA = collision.BodyA;
            RigidBody bodyB = collision.BodyB;
            Vector2 n = collision.Normal;
            Vector2 p = collision.ContactPoint;

            //if (MathF.Abs(n.LengthSquared() - 1f) < 0.0001f) throw new("Normal Should Be Normalized");

            float sumInvMass =  bodyA.InvMass + bodyB.InvMass;
            if (sumInvMass == 0) return;
            
            float restitution = MathF.Max(bodyA.Restitution, bodyB.Restitution);

            Vector2 vRel = bodyA.GetPointVelocity(p) - bodyB.GetPointVelocity(p);
            float velAlongNormal = Vector2.Dot(vRel, n);

            if(velAlongNormal >=0f) return;

            float rani = Vector2.Cross(n,p - bodyA.Pos);
            rani *= rani * bodyA.InvInertia;
            float rbni = Vector2.Cross(n,p - bodyB.Pos);
            rbni *= rbni * bodyB.InvInertia;

            float normalImpulse = velAlongNormal * ( -(1f + restitution)) / (sumInvMass + rani + rbni);

            bodyA.AddImpulse(n * normalImpulse, p);
            bodyB.AddImpulse(-n * normalImpulse, p);

            // friction
            // Vector2 tangentVel = (vRel - n * Vector2.Dot(vRel,n)).Normalized();
            // float friction = MathF.Sqrt(bodyA.Friction*bodyB.Friction);
            // float frictionLimit = normalImpulse * friction;

            // float rati = Vector2.Cross(tangentVel,p - bodyA.Pos);
            // rati *= rati * bodyA.InvInertia;
            // float rbti = Vector2.Cross(tangentVel,p - bodyB.Pos);
            // rbti *= rbti * bodyB.InvInertia;

            // float tangentImpulse = Math.Clamp(-Vector2.Dot(vRel, tangentVel) / (sumInvMass + rati + rbti), -frictionLimit, frictionLimit);

            // bodyA.AddImpulse(tangentVel * tangentImpulse, p);
            // bodyB.AddImpulse(-tangentVel * tangentImpulse, p);

        }   
        public static void ResolvePosition(Collision collision)
        {
            RigidBody bodyA = collision.BodyA;
            RigidBody bodyB = collision.BodyB;

            float sumInvMass =  bodyA.InvMass + bodyB.InvMass;
            if (sumInvMass == 0) return;

            float correctionMagnitude = Math.Max(collision.Depth - positionCorrectionMin, 0.0f) / sumInvMass * positionCorrectionPercent;
            Vector2 correction = collision.Normal * correctionMagnitude;

            bodyA.Pos += correction * bodyA.InvMass;
            bodyB.Pos -= correction * bodyB.InvMass;
        }
        private static void ProjectVertices(List<Vector2> vertices, Vector2 normal, out float min, out float max)
        {
            min = float.MaxValue;
            max = float.MinValue;
            for (int i = 0; i < vertices.Count; i++)
            {
                float p = Vector2.Dot(normal, vertices[i]);
                if(p > max) max = p;
                if(p < min) min = p;
            }
        }
    }   
}