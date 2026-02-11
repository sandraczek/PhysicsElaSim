using PhysicsElaSim.physics;

namespace PhysicsElaSim.src
{
using G = SFML.Graphics;
using W = SFML.Window;
using S = SFML.System;
class App
{
	static void Main(string[] args)
	{
		G.RenderWindow window  = new(new W.VideoMode(new S.Vector2u(735, 478)), "Physics Simulation", W.Styles.Default, W.State.Windowed);
		
		World world = new();
		Dictionary<int, G.Shape> Shapes = []; // has to be before OnKeyPressed. can change structure later

		window.Closed += (sender, e) => window.Close();
		window.KeyPressed += OnKeyPressed;

		

		float circleRadius1 = 10f;
		RigidBody circleBody1 = new(new Circle(circleRadius1), new Vector2(100, 100), invMass: 0.1f, restitution: 0.9f);
		world.Bodies.Add(circleBody1.Id, circleBody1);

		float circleRadius2 = 10f;
		RigidBody circleBody2 = new(new Circle(circleRadius2), new Vector2(120, 80), invMass: 1f, restitution: 0.8f);
		world.Bodies.Add(circleBody2.Id, circleBody2);

		float circleRadius3 = 20f;
		RigidBody circleBody3 = new(new Circle(circleRadius3), new Vector2(90, 150), isStatic: true, invMass: 0f, restitution: 0.2f);
		world.Bodies.Add(circleBody3.Id, circleBody3);

		Vector2 rectSize1 = new(40f,8f);
		RigidBody rectBody1 = new(new Rectangle(rectSize1), new Vector2(120, 190), isStatic: true, invMass: 0f, restitution: 0.3f);
		world.Bodies.Add(rectBody1.Id, rectBody1);
		Vector2 floorSize = new(1000f,30f);
		RigidBody floorBody = new(new Rectangle(floorSize), new Vector2(200, 200), isStatic: true, invMass: 0f, restitution: 0.4f);
		world.Bodies.Add(floorBody.Id, floorBody);

		// TODO: rewrite this to the SpawnBall and SpawnRect methods (already implemented)

		G.CircleShape SFMLcircleShape1 = new(circleRadius1) { FillColor = G.Color.Red};
		Shapes.Add(circleBody1.Id, SFMLcircleShape1);
		G.CircleShape SFMLcircleShape2 = new(circleRadius2) { FillColor = G.Color.Blue };
		Shapes.Add(circleBody2.Id, SFMLcircleShape2);
		G.CircleShape SFMLcircleShape3 = new(circleRadius3) { FillColor = G.Color.Cyan };
		Shapes.Add(circleBody3.Id, SFMLcircleShape3);
		G.RectangleShape SFMLrectShape1 = new(MathP.ToSF(rectSize1)) { FillColor = G.Color.Green};
		Shapes.Add(rectBody1.Id, SFMLrectShape1);
		G.RectangleShape SFMLfloorShape = new(MathP.ToSF(floorSize)) { FillColor = G.Color.White};
		Shapes.Add(floorBody.Id, SFMLfloorShape);

		foreach (G.Shape shape in Shapes.Values){ // setting origin to center
			if(shape is G.CircleShape circle)
				shape.Origin = new S.Vector2f(circle.Radius, circle.Radius);
			else if(shape is G.RectangleShape rect)
				shape.Origin = new S.Vector2f(rect.Size.X, rect.Size.Y) * 0.5f;
		}


		S.Clock clock = new();

		while (window.IsOpen)
		{
			float deltaTime = clock.Restart().AsSeconds();
			window.DispatchEvents();

			world.Update(deltaTime);

			foreach ((int index, G.Shape shape) in Shapes) 				// is it better foreach shape or foreach body in Bodies? 
			{															// one way to think is that this is a file for resolving shapes
				shape.Position = MathP.ToSF(world.Bodies[index].Pos); 	// the other is that each body has a sprite not the other way around
			}
			

			// --------- View -------------
			G.Color bgColor = new(100,100,100);
			window.Clear(bgColor);


			//List<G.Vertex> points = [];
			foreach (G.Shape shape in Shapes.Values)
			{
				window.Draw(shape); 												// TODO: set a drawing priority
				//points.Add(new(shape.Position, G.Color.Magenta));

                    G.CircleShape center = new(0.5f) // Centers of Shapes. Unoptimal. Later - just uncomment the vertex list
                    {
                        Position = shape.Position,
						Origin = new(0.5f,0.5f)
                    };
                    window.Draw(center);

			}
			//window.Draw(points.ToArray(), 1,1, PrimitiveType.Points);

			window.Display();
		}

		 void OnKeyPressed(object? sender, W.KeyEventArgs e)
		{
			if (e.Code == W.Keyboard.Key.S)
			{
				S.Vector2f worldMousePos = window.MapPixelToCoords(W.Mouse.GetPosition(window));
				SpawnBall(10f,MathP.ToP(worldMousePos), G.Color.Yellow);
			}
			if (e.Code == W.Keyboard.Key.D)
			{
				S.Vector2f worldMousePos = window.MapPixelToCoords(W.Mouse.GetPosition(window));
				SpawnRect(new(12f,8f),MathP.ToP(worldMousePos), G.Color.Yellow);
			}
		}
		void SpawnBall(float radius, Vector2 position, G.Color color, bool isStatic = false, float invMass = 1f, float restitution = 0.5f)
		{
			RigidBody circleBody = new(new Circle(radius), position, isStatic, invMass, restitution);
			world.Bodies.Add(circleBody.Id, circleBody);

			G.CircleShape circleShape = new(radius)
			{
				FillColor = color,
				Origin = new S.Vector2f(radius, radius)
			};
			Shapes.Add(circleBody.Id, circleShape);
		}
		void SpawnRect(Vector2 size, Vector2 position, G.Color color, bool isStatic = false, float invMass = 1f, float restitution = 0.5f)
		{
			RigidBody rectBody = new(new Rectangle(size), position, isStatic, invMass, restitution);
			world.Bodies.Add(rectBody.Id, rectBody);
			G.RectangleShape rectShape = new(MathP.ToSF(size))
			{
				FillColor = color,
				Origin = MathP.ToSF(size) * 0.5f
			};
			Shapes.Add(rectBody.Id, rectShape);
		}
	}

	
}}