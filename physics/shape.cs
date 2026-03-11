
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace PhysicsElaSim.physics
{
    abstract public class Shape
    {
        protected bool _isDirty;
        abstract public float GetInertia(float mass);
        public void MarkDirty()
        {
            _isDirty = true;
        }
    }

    public class Circle : Shape 
    {
        public float Radius;

        public Circle(float radius)
        {
            Radius = radius;
        }

        public override float GetInertia(float mass)
        {
            return 0.5f * mass * Radius * Radius;
        }
    }

    public class Polygon : Shape
    {
        private readonly int _verticeCount;
        public int VerticeCount => _verticeCount;
        private readonly Vector2[] _localVertices;
        private readonly Vector2[] _globalVertices;

        public Polygon(Vector2[] vertices)
        {
            _verticeCount = vertices.Length;
            _localVertices = new Vector2[_verticeCount];
            Array.Copy(vertices, _localVertices, _verticeCount);

            EnsureCounterClockwise();
            SetCentroid();

            _globalVertices = new Vector2[_verticeCount];
        }
        public void EnsureCounterClockwise()
        {
            if(_verticeCount < 3) return;

            float area = 0;

            //sum of all of the triangles
            for (int i = 0; i < _verticeCount; i++)
            {
                Vector2 p1 = _localVertices[i];
                Vector2 p2 = _localVertices[(i + 1) % _verticeCount];

                float cross = Vector2.Cross(p2, p1);

                area += cross;
            }

            if(area < 0f)
            {
                Array.Reverse(_localVertices,0,_verticeCount);
            }
        }
        public void SetCentroid()
        {
            if(_verticeCount < 3) return;

            float area = 0;
            Vector2 centroid = Vector2.Zero; //center of mass of the polygon

            //sum of all of the triangles
            for (int i = 0; i < _verticeCount; i++)
            {
                Vector2 p1 = _localVertices[i];
                Vector2 p2 = _localVertices[(i + 1) % _verticeCount];

                float cross = Vector2.Cross(p1, p2);

                area += cross;
                centroid += (p1 + p2) * cross;
            }

            //check for degenerate polygons
            if (Math.Abs(area) < float.Epsilon) throw new("Area Of The Polygon Should Not Be Zero");

            area /= 2f;
            centroid *= 1f / (6f * area);

            for (int i = 0; i < _verticeCount; i++)
            {
                _localVertices[i] -= centroid;
            }
        }

        public override float GetInertia(float mass)
        {
            if (_verticeCount < 3) return 0;

            float area = 0;
            float inertia = 0;

            //sum of all of the triangles
            for (int i = 0; i < _verticeCount; i++)
            {
                Vector2 p1 = _localVertices[i];
                Vector2 p2 = _localVertices[(i + 1) % _verticeCount];

                float cross = Vector2.Cross(p1, p2);

                area += cross;
                inertia += (p1.LengthSquared() + Vector2.Dot(p1, p2) + p2.LengthSquared()) * cross;
            }

            //check for degenerate polygons
            if (Math.Abs(area) < float.Epsilon) throw new("Area Of The Polygon Should Not Be Zero");

            area /= 2f;
            inertia /= 12f;

            //convert area moment to mass moment assuming uniform density
            inertia *= mass / area; 

            //returning abosute value because cross product could return negative area if the vertices are clockwise
            return Math.Abs(inertia);
        }
        private void ResolveDirty(Vector2 pos, float rotation)
        {
            for(int i = 0; i<_verticeCount; i++)
            {
                var local = _localVertices[i];
                _globalVertices[i] = pos + new Vector2(
                    local.X * MathF.Cos(rotation) - local.Y * MathF.Sin(rotation),
                    local.X * MathF.Sin(rotation) + local.Y * MathF.Cos(rotation)
                );
            }
            _isDirty = false;
        }
        public Vector2[] GetVertices(Vector2 pos, float rotation)
        {
            if(_isDirty) ResolveDirty(pos,rotation);
            return _globalVertices;
        }
        public Vector2[] GetLocalVertices() => _localVertices;
    }
}