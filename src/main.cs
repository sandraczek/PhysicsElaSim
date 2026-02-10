using PhysicsElaSim.physics;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

class App
{
    static void Main(string[] args)
    {
        RenderWindow window = new(new VideoMode(new Vector2u(800, 600)), "Physics Simulation");
        window.Closed += (sender, e) => window.Close();
        Renderer renderer = new();

        World world = new(new Vector2(0, 9.81f));

        RigidBody circleBody = new(
            new Circle(10f),
            new Vector2(100, 100),
            invMass: 0.1f,
            restitution: 0.7f
        );
        world.Bodies.Add(circleBody);

        Clock clock = new();

        while (window.IsOpen)
        {
            float delatTime = clock.Restart().AsSeconds();
            world.Update(delatTime);

            window.DispatchEvents();
            window.Clear(new Color(30, 30, 30));

            renderer.Render(window, world);

            window.Display();
        }
    }
}

