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
	private static G.Color bgColor = new(100,100,100);
	private static bool showCenters = false;
	private static bool showCorners = true;
	static void Main(string[] args)
	{
		G.RenderWindow window  = new(new W.VideoMode(new S.Vector2u(735, 478)), "Physics Simulation", W.Styles.Default, W.State.Windowed, new()
		{
			AntialiasingLevel = 8
		});
		
		World world = new();
		Dictionary<int, G.Shape> Shapes = []; // has to be before OnKeyPressed. can change structure later

		window.Closed += (sender, e) => window.Close();
		window.KeyPressed += OnKeyPressed;

		Vector2 floorSize = new(1000f,30f);
		SpawnRect(floorSize, new Vector2(200, 200), G.Color.White ,true, invMass: 0f, restitution: 0.15f, 0.5f);

		SpawnBall(50f, new Vector2(100, 100),G.Color.Red, invMass: 0.1f, restitution: 0.9f, friction: 0.2f);
		SpawnRect(new(60f,6f),new(600f,180f),new(209, 175, 54),true, 0, 1.2f,0f);

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
				shape.Rotation = body.Rotation * 360 / (2 * MathF.PI);

			}
			

			// --------- View -------------
			window.Clear(bgColor);

			foreach (G.Shape shape in Shapes.Values)
			{
				window.Draw(shape);
			}
			if(showCenters) ShowCenters();
			if(showCorners) ShowRectCorners();
			
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
			if (e.Code == W.Keyboard.Key.R)
			{
				S.Vector2f worldMousePos = window.MapPixelToCoords(W.Mouse.GetPosition(window));
				SpawnRect(new(10f,16f),MathP.ToP(worldMousePos), new(113, 173, 201));
			}

			if (e.Code == W.Keyboard.Key.D)
			{
				S.Vector2f worldMousePos = window.MapPixelToCoords(W.Mouse.GetPosition(window));
				var ball = SpawnBall(10f,MathP.ToP(worldMousePos), new(113, 201, 186));
				ball.AddCenterImpulse(Vector2.Right * spawnImpulse);
			}
			if (e.Code == W.Keyboard.Key.A)
			{
				S.Vector2f worldMousePos = window.MapPixelToCoords(W.Mouse.GetPosition(window));
				var ball = SpawnBall(10f,MathP.ToP(worldMousePos), new(113, 201, 186));
				ball.AddCenterImpulse(Vector2.Left * spawnImpulse);
			}
			if (e.Code == W.Keyboard.Key.W)
			{
				S.Vector2f worldMousePos = window.MapPixelToCoords(W.Mouse.GetPosition(window));
				var ball = SpawnBall(10f,MathP.ToP(worldMousePos), new(113, 201, 186));
				ball.AddCenterImpulse(Vector2.Up * spawnImpulse);
			}
			if (e.Code == W.Keyboard.Key.S)
			{
				S.Vector2f worldMousePos = window.MapPixelToCoords(W.Mouse.GetPosition(window));
				var ball = SpawnBall(10f,MathP.ToP(worldMousePos), new(113, 201, 186));
				ball.AddCenterImpulse(Vector2.Down * spawnImpulse);
			}
			if (e.Code == W.Keyboard.Key.X)
			{
				Vector2 size = new(30f,40f);
				S.Vector2f SFworldMousePos = window.MapPixelToCoords(W.Mouse.GetPosition(window));
				Vector2 worldMousePos = MathP.ToP(SFworldMousePos);
				var rect = SpawnRect(size,worldMousePos, new(113, 173, 201));
				rect.AddImpulse(Vector2.Right * spawnImpulse, worldMousePos);
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
			float restitution = 0.2f, 
			float friction = 0.2f
			)
		{
			RigidBody rectBody = new(new Rectangle(size), position, isStatic, invMass, restitution, friction);
			world.Bodies.Add(rectBody.Id, rectBody);
			G.RectangleShape rectShape = new(MathP.ToSF(size))
			{
				FillColor = color,
				OutlineColor = G.Color.Black,
				OutlineThickness = 0f,
				Origin = MathP.ToSF(size) * 0.5f
			};
			Shapes.Add(rectBody.Id, rectShape);

			return rectBody;
		}

		void ShowRectCorners()
		{
			foreach ((int index, G.Shape shape) in Shapes)
			{
				RigidBody body = world.Bodies[index];
				if(body.Shape is Rectangle rect){
					G.Vertex[] corners = new G.Vertex[5];
					var vertices = rect.GetVertices(body.Pos, body.Rotation);
					for (int i = 0; i<4;i++)
					{
						corners[i] = new(MathP.ToSF(vertices[i]),G.Color.Black);
					}
					
					corners [4] = corners[0];
					window.Draw(corners, G.PrimitiveType.LineStrip);
				}
			}
		}
		void ShowCenters()
		{
			G.Vertex[] centers = new G.Vertex[Shapes.Count];
			var keys = Shapes.Keys.ToArray();
			for (int i = 0; i< Shapes.Count; i++) 								// TODO: set a drawing priority
			{
				centers[i] = new(MathP.ToSF(world.Bodies[keys[i]].Pos), G.Color.Black);
			}
			window.Draw(centers, G.PrimitiveType.Points);
		}
	}

	
}}