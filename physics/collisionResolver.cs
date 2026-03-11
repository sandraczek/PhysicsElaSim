using System.Diagnostics;
using System.Globalization;
using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;

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
                (Circle cA, Polygon pB) => CircleVsPolygon(a, b, cA, pB),
                (Polygon pA, Circle cB) => PolygonVsCircle(a, b, pA, cB),
                (Polygon pA, Polygon pB) => PolygonVsPolygon(a, b, pA, pB),
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

        private static Collision? PolygonVsCircle(RigidBody bodyA, RigidBody bodyB, Polygon polyA, Circle circleB)
        {
            Vector2[] verticesA = polyA.GetVertices(bodyA.Pos,bodyA.Rotation);

            Vector2 normal = Vector2.Zero;
            float minOverlap = float.MaxValue;

            bool CheckAxis(Vector2 axis, ref Vector2 currentNormal, ref float currentMinOverlap)
            {
                ProjectVertices(verticesA, axis, out float minA, out float maxA);
                float center = Vector2.Dot(axis, bodyB.Pos);
                float minB = center - circleB.Radius;
                float maxB = center + circleB.Radius;

                float overlap = MathF.Min(maxB,maxA) - MathF.Max(minA,minB);
                if(overlap < 0f) return false;
                if(overlap < currentMinOverlap)
                {
                    currentMinOverlap = overlap;
                    currentNormal = axis;
                }
                return true;
            }
            int lenA = verticesA.Length;
            for (int i = 0; i < lenA; i++)
            {
                int j = (i + 1) % lenA;
                Vector2 axis = (verticesA[j] - verticesA[i]).Rotated90().Normalized();
                if(!CheckAxis(axis, ref normal, ref minOverlap)) return null;
            }

            Vector2 closestVertex = Vector2.Zero;
            float closestVertexLenghtSq = float.MaxValue;
            foreach (var vertex in verticesA)
            {
                var currentLengthSq = (vertex - bodyB.Pos).LengthSquared();
                if(currentLengthSq < closestVertexLenghtSq){
                    closestVertex = vertex;
                    closestVertexLenghtSq = currentLengthSq;
                }
            }
            Vector2 voronoiaAxis = (closestVertex - bodyB.Pos).Normalized();
            if(!CheckAxis(voronoiaAxis, ref normal, ref minOverlap)) return null;

            if(Vector2.Dot(normal, bodyA.Pos - bodyB.Pos) < 0f)
                normal = -normal;
            
            Vector2 contactPoint = bodyB.Pos + normal * circleB.Radius;

            return new(bodyA,bodyB,normal,[contactPoint], [minOverlap]);
        }

        private static Collision? CircleVsPolygon(RigidBody A, RigidBody B, Circle circleA, Polygon polyB)
            => PolygonVsCircle(B, A, polyB, circleA);

        private static Collision? PolygonVsPolygon(RigidBody bodyA, RigidBody bodyB, Polygon polyA, Polygon polyB)
        {
            Vector2[] verticesA = polyA.GetVertices(bodyA.Pos,bodyA.Rotation);
            Vector2[] verticesB = polyB.GetVertices(bodyB.Pos,bodyB.Rotation);

            Vector2 normal = Vector2.Zero;
            float minOverlap = float.MaxValue;
            bool isA = true;
            
            bool CheckAxes(Vector2[] vertices, bool isA, ref Vector2 currentNormal, ref float currentMinOverlap, ref bool currentIsA)
            {
                int len = vertices.Length;
                for (int i = 0; i < len; i++)
                {
                    int j = (i + 1) % len;
                    Vector2 axis = (vertices[j] - vertices[i]).Rotated90().Normalized();

                    ProjectVertices(verticesA, axis, out float minA, out float maxA);
                    ProjectVertices(verticesB, axis, out float minB, out float maxB);

                    float overlap = MathF.Min(maxB,maxA) - MathF.Max(minA,minB);
                    if(overlap < 0f) return false;
                    if(overlap < minOverlap)
                    {
                        currentMinOverlap = overlap;
                        currentNormal = axis;
                        currentIsA = isA;
                    }
                }
                return true;
            }

            if (!CheckAxes(verticesA, true, ref normal, ref minOverlap, ref isA)) return null;
            if (!CheckAxes(verticesB, false, ref normal, ref minOverlap, ref isA)) return null;

            if (isA) //setting sense
            {
                if (Vector2.Dot(bodyB.Pos - bodyA.Pos, normal) < 0f) normal = -normal;
            }
            else 
            {
                if (Vector2.Dot(bodyA.Pos - bodyB.Pos, normal) < 0f) normal = -normal;
            }
            
            Vector2[] refVertices = isA ? verticesA : verticesB;
            Vector2[] incVertices = isA ? verticesB : verticesA;

            int refIndex = FindMostParallelFaceIndex(normal,refVertices);
            int incIndex = FindMostParallelFaceIndex(-normal, incVertices);

            Vector2 refV1 = refVertices[refIndex];
            Vector2 refV2 = refVertices[(refIndex + 1) % refVertices.Length];
            Vector2 refTangent = (refV2 - refV1).Normalized();

            Vector2 incV1 = incVertices[incIndex];
            Vector2 incV2 = incVertices[(incIndex + 1) % incVertices.Length];

            Clip(refV1, refTangent, ref incV1, ref incV2);
            Clip(refV2, -refTangent, ref incV1, ref incV2);

            float d1 = Vector2.Dot(refV1 - incV1, normal);
            float d2 = Vector2.Dot(refV1 - incV2, normal);
            
            if(d1 < contactPointTolerance) return new(bodyA,bodyB,isA? -normal:normal, [incV2], [d2]);
            else if(d2 < contactPointTolerance) return new(bodyA,bodyB,isA? -normal:normal, [incV1], [d1]);
            else return new(bodyA,bodyB,isA? -normal:normal, [incV1, incV2], [d1, d2]);
        }
        private static int FindMostParallelFaceIndex(Vector2 normal, Vector2[] vertices)
        { // return index of first vertex of the face. For second vertex use +1 and modulo
            float maxDot = float.MinValue;
            int index = 0;
            for (int i = 0; i < vertices.Length; i++)
            {
                int j = (i + 1) % vertices.Length;
                Vector2 faceNormal = (vertices[j] - vertices[i]).Rotated90(); //TODO: check if normal is pointing outwards
                float dot = Vector2.Dot(faceNormal, normal);
                if(dot > maxDot)
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
        private static void ProjectVertices(Vector2[] vertices, Vector2 normal, out float min, out float max)
        {
            min = float.MaxValue;
            max = float.MinValue;
            for (int i = 0; i < vertices.Length; i++)
            {
                float p = Vector2.Dot(normal, vertices[i]);
                if(p > max) max = p;
                if(p < min) min = p;
            }
        }
    }   
}