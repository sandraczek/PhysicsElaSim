using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace PhysicsElaSim.physics
{
    static class CollisionResolver
    {
        private const float positionCorrectionPercent = 0.2f;
        private const float positionCorrectionMin = 0.001f;
        private const float contactPointTolerance = 0.001f;
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
            Vector2 contactPoint = A.Pos + normal * circleA.Radius;
            return new(A, B, normal, [contactPoint], [depth]);
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
            return new(A, B, normal, [contactPoint], [depth]);
        }

        private static Collision? RectVsCircle(RigidBody A, RigidBody B, Rectangle rectA, Circle circleB)
            => CircleVsRect(B, A, circleB, rectA);


        private static Collision? RectVsRect(RigidBody A, RigidBody B, Rectangle rectA, Rectangle rectB)
        {
            List<Vector2> VerticesA = rectA.GetVertices(A.Pos,A.Rotation);
            List<Vector2> VerticesB = rectB.GetVertices(B.Pos,B.Rotation);
            
            List<Vector2> axes = [];
            axes.Add(new (MathF.Cos(A.Rotation), MathF.Sin(A.Rotation)));
            axes.Add(new (-MathF.Sin(A.Rotation), MathF.Cos(A.Rotation)));
            axes.Add(new (MathF.Cos(B.Rotation), MathF.Sin(B.Rotation)));
            axes.Add(new (-MathF.Sin(B.Rotation), MathF.Cos(B.Rotation)));

            Vector2 normal = Vector2.Zero;
            float minOverlap = float.MaxValue;

            foreach (Vector2 axis in axes)
            {
                ProjectVertices(VerticesA, axis, out float minA, out float maxA);
                ProjectVertices(VerticesB, axis, out float minB, out float maxB);

                float overlap = MathF.Min(maxB,maxA) - MathF.Max(minA,minB);
                if(overlap < 0f) return null;
                if(overlap < minOverlap)
                {
                    minOverlap = overlap;
                    normal = axis;
                }
            }

            if(Vector2.Dot(B.Pos - A.Pos, normal) > 0f ) normal = -normal ; //setting sense
            

            int refIndex = FindMostParallelFaceIndex(normal,VerticesB);
            int incIndex = FindMostParallelFaceIndex(-normal, VerticesA);

            Vector2 refV1 = VerticesB[refIndex];
            Vector2 refV2 = VerticesB[(refIndex + 1) % VerticesB.Count];
            Vector2 refTangent = (refV2 - refV1).Normalized();

            Vector2 incV1 = VerticesA[incIndex];
            Vector2 incV2 = VerticesA[(incIndex + 1) % VerticesA.Count];

            Clip(refV1, refTangent, ref incV1, ref incV2);
            Clip(refV2, -refTangent, ref incV1, ref incV2);

            float d1 = Vector2.Dot(refV1 - incV1, normal);
            float d2 = Vector2.Dot(refV1 - incV2, normal);
            
            if(d1 < contactPointTolerance) return new(A,B,normal, [incV2], [d2]);
            else if(d2 < contactPointTolerance) return new(A,B,normal, [incV1], [d1]);
            else return new(A,B,normal, [incV1, incV2], [d1, d2]);
        }

        private static int FindMostParallelFaceIndex(Vector2 normal, List<Vector2> vertices)
        { // return index of first vertex of the face. For second vertex use +1 and modulo
            float maxDot = 0f;
            int index = 0;
            for (int i = 0;i< vertices.Count; i++)
            {
                int j = (i + 1) % vertices.Count;
                Vector2 faceNormal = (vertices[i] - vertices[j]).Rotated90(); //TODO: check if normal is pointing outwards
                float dot = Vector2.Dot(faceNormal, normal);
                if(dot < maxDot)
                {
                    maxDot = dot;
                    index = i;
                }
            }
            return index;
        }

        private static void Clip(Vector2 wallPoint, Vector2 wallNormal, ref Vector2 v1, ref Vector2 v2)
        {
            float d1 = Vector2.Dot(v1 - wallPoint, wallNormal);
            float d2 = Vector2.Dot(v2 - wallPoint, wallNormal);
            if (d1 >= 0 && d2 < 0) //v1 inside v2 outside 
            {
                float t = d1 / (d1 - d2);
                v2 = v1 + (v2 - v1) * t;
            }
            else if (d1 < 0 && d2 >= 0) //v1 outside v2 inside
            {
                float t = d2 / (d2 - d1);
                v1 = v2 + (v1 - v2) * t;
            }
        }

        public static void ResolveVelocity(Collision collision, int poi)
        {   
            
            RigidBody bodyA = collision.BodyA;
            RigidBody bodyB = collision.BodyB;
            Vector2 n = collision.Normal;
            Vector2 p = collision.ContactPoints[poi];

            //if (MathF.Abs(n.LengthSquared() - 1f) < 0.0001f) throw new("Normal Should Be Normalized");

            float sumInvMass =  bodyA.InvMass + bodyB.InvMass;
            if (sumInvMass == 0) return;
            
            float restitution = MathF.Max(bodyA.Restitution, bodyB.Restitution);

            Vector2 vRel = bodyA.GetPointVelocity(p) - bodyB.GetPointVelocity(p);
            float velAlongNormal = Vector2.Dot(vRel, n);
            
            if(velAlongNormal >=0f) return;
            if(velAlongNormal > -0f) restitution = 0f;

            float rani = Vector2.Cross(n,p - bodyA.Pos);
            rani *= rani * bodyA.InvInertia;
            float rbni = Vector2.Cross(n,p - bodyB.Pos);
            rbni *= rbni * bodyB.InvInertia;

            float normalImpulse = velAlongNormal * ( -(1f + restitution)) / (sumInvMass + rani + rbni);

            bodyA.AddImpulse(n * normalImpulse, p);
            bodyB.AddImpulse(-n * normalImpulse, p);

            //friction
            Vector2 tangentVel = (vRel - n * Vector2.Dot(vRel,n)).Normalized();
            float friction = MathF.Sqrt(bodyA.Friction*bodyB.Friction);
            float frictionLimit = normalImpulse * friction;

            float rati = Vector2.Cross(tangentVel,p - bodyA.Pos);
            rati *= rati * bodyA.InvInertia;
            float rbti = Vector2.Cross(tangentVel,p - bodyB.Pos);
            rbti *= rbti * bodyB.InvInertia;

            float tangentImpulse = Math.Clamp(-Vector2.Dot(vRel, tangentVel) / (sumInvMass + rati + rbti), -frictionLimit, frictionLimit);

            bodyA.AddImpulse(tangentVel * tangentImpulse, p);
            bodyB.AddImpulse(-tangentVel * tangentImpulse, p);

        }   
        public static void ResolvePosition(Collision collision, int poi)
        {
            RigidBody bodyA = collision.BodyA;
            RigidBody bodyB = collision.BodyB;

            float sumInvMass =  bodyA.InvMass + bodyB.InvMass;
            if (sumInvMass == 0) return;

            float correctionMagnitude = Math.Max(collision.Depths[poi] - positionCorrectionMin, 0.0f) / sumInvMass * positionCorrectionPercent;
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