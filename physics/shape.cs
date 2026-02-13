
namespace PhysicsElaSim.physics
{
    abstract public class Shape
    {
        abstract public float GetInertia(float mass);
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

        public Rectangle(float width, float height)
        {
            Width = width;
            Height = height;
        }
        public Rectangle(Vector2 size)
        {
            Width = size.X;
            Height = size.Y;
        }

        public override float GetInertia(float mass)
        {
            return (1f / 12f) * mass * (Width * Width + Height * Height);
        }
        public List<Vector2> GetVertices(Vector2 pos, float rotation)
        {
            List<Vector2> vertices = [];
            vertices.Add(pos + new Vector2(
                0.5f * Width * MathF.Cos(rotation) + 0.5f * Height * MathF.Sin(rotation),
                0.5f * Width * MathF.Sin(rotation) - 0.5f * Height * MathF.Cos(rotation)
                ));
            vertices.Add(pos + new Vector2(
                -0.5f * Width * MathF.Cos(rotation) + 0.5f * Height * MathF.Sin(rotation),
                -0.5f * Width * MathF.Sin(rotation) - 0.5f * Height * MathF.Cos(rotation)
                ));
            vertices.Add(pos + new Vector2(
                -0.5f * Width * MathF.Cos(rotation) - 0.5f * Height * MathF.Sin(rotation),
                -0.5f * Width * MathF.Sin(rotation) + 0.5f * Height * MathF.Cos(rotation)
                ));
            vertices.Add(pos + new Vector2(
                0.5f * Width * MathF.Cos(rotation) - 0.5f * Height * MathF.Sin(rotation),
                0.5f * Width * MathF.Sin(rotation) + 0.5f * Height * MathF.Cos(rotation)
                ));

            return vertices;
        }
    }
}