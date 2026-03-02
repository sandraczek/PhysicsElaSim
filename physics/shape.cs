
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
            return (1f / 12f) * mass * (Width * Width + Height * Height);
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
}