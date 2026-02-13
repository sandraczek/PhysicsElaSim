using System.Reflection.Metadata;
using PhysicsElaSim.physics;

namespace PhysicsElaSim.src
{
using G = SFML.Graphics;
using W = SFML.Window;
using S = SFML.System;
class App
{			/*--------------------------------------------
				Instructions: 
				use F for stationary ball
				use AWSD for moving balls
				G for a boring rectangle

				friction feels strong, but thats only because of lack of rotations

				you can set gravity to 0 in world.cs and play billard 😃
			*/
	private const float FixedDt = 1.0f / 60.0f;
	static void Main(string[] args)
	{
		G.RenderWindow window  = new(new W.VideoMode(new S.Vector2u(735, 478)), "Physics Simulation", W.Styles.Default, W.State.Windowed);
		
		World world = new();
		Dictionary<int, G.Shape> Shapes = []; // has to be before OnKeyPressed. can change structure later

		window.Closed += (sender, e) => window.Close();
		window.KeyPressed += OnKeyPressed;

		

		//float circleRadius1 = 10f;
		float circleRadius1 = 50f;
		RigidBody circleBody1 = new(new Circle(circleRadius1), new Vector2(100, 100), invMass: 0.1f, restitution: 0.9f, friction: 0.2f);
		world.Bodies.Add(circleBody1.Id, circleBody1);

		// float circleRadius2 = 10f;
		// RigidBody circleBody2 = new(new Circle(circleRadius2), new Vector2(120, 80), invMass: 1f, restitution: 0.8f, friction: 0.15f);
		// world.Bodies.Add(circleBody2.Id, circleBody2);

		// float circleRadius3 = 20f;
		// RigidBody circleBody3 = new(new Circle(circleRadius3), new Vector2(90, 150), isStatic: true, invMass: 0f, restitution: 0.2f);
		// world.Bodies.Add(circleBody3.Id, circleBody3);

		// Vector2 rectSize1 = new(40f,8f);
		// RigidBody rectBody1 = new(new Rectangle(rectSize1), new Vector2(120, 190), isStatic: true, invMass: 0f, restitution: 0.3f);
		// world.Bodies.Add(rectBody1.Id, rectBody1);
		Vector2 floorSize = new(1000f,30f);
		RigidBody floorBody = new(new Rectangle(floorSize), new Vector2(200, 200), isStatic: true, invMass: 0f, restitution: 0.15f, 0.5f);
		world.Bodies.Add(floorBody.Id, floorBody);

		// TODO: rewrite this to the SpawnBall and SpawnRect methods (already implemented)

		G.CircleShape SFMLcircleShape1 = new(circleRadius1) { FillColor = G.Color.Red};
		Shapes.Add(circleBody1.Id, SFMLcircleShape1);
		// G.CircleShape SFMLcircleShape2 = new(circleRadius2) { FillColor = G.Color.Blue };
		// Shapes.Add(circleBody2.Id, SFMLcircleShape2);
		// G.CircleShape SFMLcircleShape3 = new(circleRadius3) { FillColor = G.Color.Cyan };
		// Shapes.Add(circleBody3.Id, SFMLcircleShape3);
		// G.RectangleShape SFMLrectShape1 = new(MathP.ToSF(rectSize1)) { FillColor = G.Color.Green};
		// Shapes.Add(rectBody1.Id, SFMLrectShape1);
		G.RectangleShape SFMLfloorShape = new(MathP.ToSF(floorSize)) { FillColor = G.Color.White};
		Shapes.Add(floorBody.Id, SFMLfloorShape);

		foreach (G.Shape shape in Shapes.Values){ // setting origin to center
			if(shape is G.CircleShape circle)
				shape.Origin = new S.Vector2f(circle.Radius, circle.Radius);
			else if(shape is G.RectangleShape rect)
				shape.Origin = new S.Vector2f(rect.Size.X, rect.Size.Y) * 0.5f;
		}

		SpawnRect(new(40f,6f),new(600f,180f),new(209, 175, 54),true, 0, 1.2f,0f);

		S.Clock clock = new();
		float fixedDtAccumulator = 0f;

		while (window.IsOpen)
		{
			window.DispatchEvents();

			float deltaTime = clock.Restart().AsSeconds();
			if(deltaTime > 0.25f) deltaTime = 0.25f;
			fixedDtAccumulator += deltaTime;

			while(fixedDtAccumulator >= FixedDt)
			{
				world.FixedUpdate(FixedDt);
				fixedDtAccumulator -= FixedDt;
			}

			foreach ((int index, G.Shape shape) in Shapes)
			{
				RigidBody body = world.Bodies[index];
				shape.Position = MathP.ToSF(body.Pos);
				shape.Rotation = body.Rotation;
				
			}
			

			// --------- View -------------
			G.Color bgColor = new(100,100,100);
			window.Clear(bgColor);


			List<G.Vertex> centers = [];
			foreach (G.Shape shape in Shapes.Values) 								// TODO: set a drawing priority
			{
				window.Draw(shape);
				centers.Add(new(shape.Position, G.Color.Black));
				//												Centers of Shapes. For bigger - uncomment under
				// G.CircleShape center = new(0.5f) 
				// {
				// 	Position = shape.Position,
				// 	Origin = new(0.5f,0.5f)
				// };
				// window.Draw(center);

			}
			window.Draw(centers.ToArray(), G.PrimitiveType.Points);

			window.Display();
		}

		void OnKeyPressed(object? sender, W.KeyEventArgs e)
		{
			float spawnImpulse = 100f;
			if (e.Code == W.Keyboard.Key.F)
			{
				S.Vector2f worldMousePos = window.MapPixelToCoords(W.Mouse.GetPosition(window));
				SpawnBall(10f,MathP.ToP(worldMousePos), new(113, 201, 186));
			}
			if (e.Code == W.Keyboard.Key.G)
			{
				S.Vector2f worldMousePos = window.MapPixelToCoords(W.Mouse.GetPosition(window));
				SpawnRect(new(12f,8f),MathP.ToP(worldMousePos), new(113, 173, 201));
			}

			if (e.Code == W.Keyboard.Key.D)
			{
				S.Vector2f worldMousePos = window.MapPixelToCoords(W.Mouse.GetPosition(window));
				var ball = SpawnBall(10f,MathP.ToP(worldMousePos), new(113, 201, 186));
				ball.AddImpulse(Vector2.Right * spawnImpulse);
			}
			if (e.Code == W.Keyboard.Key.A)
			{
				S.Vector2f worldMousePos = window.MapPixelToCoords(W.Mouse.GetPosition(window));
				var ball = SpawnBall(10f,MathP.ToP(worldMousePos), new(113, 201, 186));
				ball.AddImpulse(Vector2.Left * spawnImpulse);
			}
			if (e.Code == W.Keyboard.Key.W)
			{
				S.Vector2f worldMousePos = window.MapPixelToCoords(W.Mouse.GetPosition(window));
				var ball = SpawnBall(10f,MathP.ToP(worldMousePos), new(113, 201, 186));
				ball.AddImpulse(Vector2.Up * spawnImpulse);
			}
			if (e.Code == W.Keyboard.Key.S)
			{
				S.Vector2f worldMousePos = window.MapPixelToCoords(W.Mouse.GetPosition(window));
				var ball = SpawnBall(10f,MathP.ToP(worldMousePos), new(113, 201, 186));
				ball.AddImpulse(Vector2.Down * spawnImpulse);
			}
			if (e.Code == W.Keyboard.Key.X)
			{
				Vector2 size = new(120f,40f);
				S.Vector2f SFworldMousePos = window.MapPixelToCoords(W.Mouse.GetPosition(window));
				Vector2 worldMousePos = MathP.ToP(SFworldMousePos);
				var rect = SpawnRect(size,worldMousePos, new(113, 173, 201));
				rect.AddImpulse(Vector2.Right * spawnImpulse + Vector2.Up * spawnImpulse, worldMousePos - size * 0.5f);
			}
		}
		RigidBody SpawnBall(
			float radius, 
			Vector2 position, 
			G.Color color, 
			bool isStatic = false,
			float invMass = 1.0f, 
			float restitution = 0.8f, 
			float friction = 0.2f
			)
		{
			RigidBody circleBody = new(new Circle(radius), position, isStatic, invMass, restitution, friction);
			world.Bodies.Add(circleBody.Id, circleBody);

			G.CircleShape circleShape = new(radius)
			{
				FillColor = color,
				OutlineColor = G.Color.Black,
				OutlineThickness = 1f,
				Origin = new S.Vector2f(radius, radius)
			};
			Shapes.Add(circleBody.Id, circleShape);

			return circleBody;
		}
		RigidBody SpawnRect(
			Vector2 size, 
			Vector2 position, 
			G.Color color, 
			bool isStatic = false,
			float invMass = 1f, 
			float restitution = 0.4f, 
			float friction = 0.2f
			)
		{
			RigidBody rectBody = new(new Rectangle(size), position, isStatic, invMass, restitution, friction);
			world.Bodies.Add(rectBody.Id, rectBody);
			G.RectangleShape rectShape = new(MathP.ToSF(size))
			{
				FillColor = color,
				OutlineColor = G.Color.Black,
				OutlineThickness = 1f,
				Origin = MathP.ToSF(size) * 0.5f
			};
			Shapes.Add(rectBody.Id, rectShape);

			return rectBody;
		}
	}

	
}}