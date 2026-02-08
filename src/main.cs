using SFML.Graphics;
using SFML.Window;
using SFML.System;
using PhysicsElaSim.physics;

class App
{
	static void Main(string[] args)
	{
		RenderWindow window  = new(new VideoMode(new Vector2u(800, 600)), "Physics Simulation");

		window.Closed += (sender, e) => window.Close();

		World world = new();

		//TODO: position to center
		RigidBody circleBody = new(new Circle(10f), new Vector2(100, 100), invMass: 0.1f, restitution: 0.5f);
        world.Bodies.Add(circleBody);

        CircleShape shape = new(10.0f) { FillColor = Color.Red };

        Clock clock = new();

		while (window.IsOpen)
		{
			float delatTime = clock.Restart().AsSeconds();
			window.DispatchEvents();

			world.Update(delatTime);

			shape.Position = new Vector2f(world.Bodies[0].Pos.X, world.Bodies[0].Pos.Y);

			window.Clear();
			window.Draw(shape);
			window.Display();
		}
	}
}