
namespace PhysicsElaSim.physics
{
    public class Shape
    {
        
    }

    public class Circle : Shape 
    {
        public float Radius;

        public Circle(float radius)
        {
            Radius = radius;
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
    }
}