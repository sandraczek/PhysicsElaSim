using PhysicsElaSim.physics;
using SFML.Graphics;
using SFML.System;

public class Renderer
{
    private readonly CircleShape _circleShape = new();
    private readonly RectangleShape _rectShape = new();

    public void Render(RenderWindow window, World world)
    {
        foreach (RigidBody body in world.Bodies)
            DrawBody(window, body);
    }

    private void DrawBody(RenderWindow window, RigidBody body)
    {
        switch (body.Shape)
        {
            case Circle circle:
                _circleShape.Radius = circle.Radius;
                _circleShape.Origin = new(circle.Radius, circle.Radius);
                _circleShape.Position = asSFMLVec(body.Pos);
                _circleShape.FillColor = Color.White;
                window.Draw(_circleShape);
                break;
            case Rectangle rect:
                _rectShape.Size = new(rect.Width, rect.Height);
                _rectShape.Origin = new(rect.Width / 2, rect.Height / 2);
                _rectShape.Position = asSFMLVec(body.Pos);
                _rectShape.FillColor = Color.White;
                window.Draw(_rectShape);
                break;
        }
    }

    private Vector2f asSFMLVec(in Vector2 vec)
    {
        return new Vector2f(vec.X, vec.Y);
    }
}
