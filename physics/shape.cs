
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

    public class Rectangle : Shape
    {
        public float Width;
        public float Height;
        private readonly Vector2[] _localVertices;
        private Vector2[] _globalVertices;

        public Rectangle(float width, float height)
        {
            Width = width;
            Height = height;
            _localVertices = new Vector2[4];
            _globalVertices = new Vector2[4];
            SetLocalVertices();
        }
        public Rectangle(Vector2 size)
        {
            Width = size.X;
            Height = size.Y;
            _localVertices = new Vector2[4];
            _globalVertices = new Vector2[4];
            SetLocalVertices();
        }
        private void SetLocalVertices()
        {
            _localVertices[0] = new Vector2(0.5f * Width, -0.5f * Height);
            _localVertices[1] = new Vector2(-0.5f * Width, -0.5f * Height);
            _localVertices[2] = new Vector2(-0.5f * Width, 0.5f * Height);
            _localVertices[3] = new Vector2(0.5f * Width, 0.5f * Height);
        }

        public override float GetInertia(float mass)
        {
            float inertia = (1f / 12f) * mass * (Width * Width + Height * Height);
            Console.WriteLine(inertia);
            return inertia;
        }
        private void ResolveDirty(Vector2 pos, float rotation)
        {
            for(int i = 0; i<4; i++)
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
    }

    public class Polygon : Shape
    {
        private readonly int _verticeCount;
        private readonly Vector2[] _localVertices;
        private Vector2[] _globalVertices;

        public Polygon(Vector2[] vertices)
        {
            _verticeCount = vertices.Length;
            _localVertices = new Vector2[_verticeCount];
            _globalVertices = new Vector2[_verticeCount];
            Array.Copy(vertices, _localVertices, _verticeCount);
        }

        public override float GetInertia(float mass)
        {
            if (_verticeCount < 3) return 0;

            float area = 0;
            Vector2 centroid = Vector2.Zero; //center of mass of the polygon
            float inertia = 0;

            //sum of all of the triangles
            for (int i = 0; i < _verticeCount; i++)
            {
                Vector2 p1 = _localVertices[i];
                Vector2 p2 = _localVertices[(i + 1) % _verticeCount];

                float cross = Vector2.Cross(p1, p2);

                area += cross;
                centroid += (p1 + p2) * cross;

                inertia += (p1.LengthSquared() + Vector2.Dot(p1, p2) + p2.LengthSquared()) * cross;
            }

            //check for degenerate polygons
            if (Math.Abs(area) < float.Epsilon) throw new("Area Of The Polygon Should Not Be Zero");

            area /= 2f;
            centroid *= 1f / (6f * area);
            inertia /= 12f;

            //convert area moment to mass moment assuming uniform density
            inertia *= mass / area; 

            //apply parallel axis theorem to shift inertia from (0,0) to the centroid
            inertia -= mass * centroid.LengthSquared();

            Console.WriteLine(inertia);
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
    }
}