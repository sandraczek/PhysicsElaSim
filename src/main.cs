using PhysicsElaSim.physics;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

class App
{
    static void Main(string[] args)
    {
        RenderWindow window = new(new VideoMode(new Vector2u(800, 600)), "Physics Simulation");

        World world = new(new Vector2(0, 9.81f));
        Renderer renderer = new();

        RigidBody circle = new(
            new Circle(10f),
            new Vector2(100, 100),
            invMass: 0.1f,
            restitution: 0.7f
        );
        world.Bodies.Add(circle);

        RigidBody rect = new(
            new Rectangle(20f, 10f),
            new Vector2(200, 100),
            invMass: 0,
            restitution: 0.2f
        );
        world.Bodies.Add(rect);

        Clock clock = new();
        float accumulator = 0f;
        const float timeStep = 1f / 60f;

        bool isPaused = false;

        window.Closed += (sender, e) => window.Close();
        window.KeyPressed += (sender, e) =>
        {
            if (e.Code == Keyboard.Key.Space)
                isPaused = !isPaused;
        };

        while (window.IsOpen)
        {
            window.DispatchEvents();

            if (!isPaused)
            {
                float dt = clock.Restart().AsSeconds();
                accumulator += dt;
                while (accumulator >= timeStep)
                {
                    world.Update(timeStep);
                    accumulator -= timeStep;
                }
            }
            else
                clock.Restart();

            window.Clear(new Color(30, 30, 30));

            renderer.Render(window, world);

            window.Display();
        }
    }
}
