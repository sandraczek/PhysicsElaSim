using System.Reflection.Metadata;
using PhysicsElaSim.physics;
using SFML.Graphics;

namespace PhysicsElaSim.src
{
using G = SFML.Graphics;
using W = SFML.Window;
using S = SFML.System;
class App
{
	private const float FixedDt = 1.0f / 60.0f;
	private static G.Color bgColor = new(100,100,100);
	private static readonly bool showCenters = false;
	private static readonly bool showCorners = true;
	static void Main(string[] args)
	{
		G.RenderWindow window  = new(new W.VideoMode(new S.Vector2u(735, 478)), "Physics Simulation", W.Styles.Default, W.State.Windowed, new()
		{
			AntialiasingLevel = 8
		});
		G.Font font = new("Assets/Fonts/Work_Sans/static/WorkSans-Light.ttf");
		
		World world = new();
		Dictionary<int, G.Shape> Shapes = []; // has to be before OnKeyPressed. can change structure later

		window.Closed += (sender, e) => window.Close();
		window.KeyPressed += OnKeyPressed;

            // initial Objects
        Vector2 floorSize = new(1000f,30f);
		SpawnRect(floorSize, new Vector2(200, 400), G.Color.White ,true, invMass: 0f, restitution: 0.15f, 0.5f);
		SpawnBall(50f, new Vector2(100, 100),G.Color.Red, invMass: 0.1f, restitution: 0.9f, friction: 0.2f);
		SpawnRect(new(60f,6f),new(600f,380f),new(209, 175, 54),true, 0, 1.2f,0f);

		float fixedDtAccumulator = 0f;
		int fpsCounter = 0;
		float fpsTimer = 0f;
		G.Text fpsCountText = new(font)
		{
			Position = window.MapPixelToCoords(new(2,2)),
			FillColor = G.Color.White,
			CharacterSize = 15,
			DisplayedString = "0"
		};

		int objectCounter = 0;
		G.Text objectCountText = new(font)
		{
			Position = window.MapPixelToCoords(new(100, 2)),
			FillColor = G.Color.White,
			CharacterSize = 15,
			DisplayedString = "0"
		};

		S.Clock clock = new();
		while (window.IsOpen)
		{
			window.DispatchEvents();

			float deltaTime = clock.Restart().AsSeconds();
			if(deltaTime > 0.25f) deltaTime = 0.25f;
			fixedDtAccumulator += deltaTime;
			fpsCounter += 1;
			fpsTimer +=deltaTime;
			if(fpsTimer >= 0.25f)
			{
				int fps = (int)(fpsCounter / fpsTimer);
				fpsCounter = 0;
				fpsCountText.DisplayedString = $"fps: {fps}";
				fpsTimer= 0f;

			}
			if(objectCounter != world.Bodies.Count)
			{
				objectCounter = world.Bodies.Count;
				objectCountText.DisplayedString = $"objects: {objectCounter}";
			}

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
			//debug
			foreach (var pair in Shapes)
			{
				if(pair.Value is G.ConvexShape polygon)
				{
					var body = world.Bodies[pair.Key];
					Console.WriteLine($"{body.AngularVelocity}");
				}
			}

			// --------- View -------------
			window.Clear(bgColor);

			foreach (G.Shape shape in Shapes.Values)
			{
				window.Draw(shape);
			}
			if(showCenters) ShowCenters();
			if(showCorners) ShowRectCorners();

			window.Draw(fpsCountText);
			window.Draw(objectCountText);
			
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
				Vector2 size = new(30f,40f);
				S.Vector2f SFworldMousePos = window.MapPixelToCoords(W.Mouse.GetPosition(window));
				Vector2 worldMousePos = MathP.ToP(SFworldMousePos);
				var rect = SpawnRect(size,worldMousePos, new(113, 173, 201));
				rect.AddImpulseLocal(Vector2.Right * spawnImpulse + Vector2.Up * spawnImpulse, new(-15f,-20f));
			}
			if (e.Code == W.Keyboard.Key.P)
			{
				Vector2[] vertices =
				[
					new(50f, 0f),     // Right
					new(35f, 35f),    // Top-Right
					new(0f, 50f),     // Top
					new(-35f, 35f),   // Top-Left
					new(-50f, 0f),    // Left
					new(-30f, -40f),  // Bottom-Left
					new(20f, -45f)    // Bottom-Right
				];
				S.Vector2f SFworldMousePos = window.MapPixelToCoords(W.Mouse.GetPosition(window));
				Vector2 worldMousePos = MathP.ToP(SFworldMousePos);
				var heptagon = SpawnPolygon(vertices, worldMousePos, new(110, 158, 47));
				heptagon.AddImpulseLocal(Vector2.Right * spawnImpulse + Vector2.Up * spawnImpulse, new(-35,-35));
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
			float friction = 0.4f
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

		RigidBody SpawnPolygon(
			Vector2[] vertices, 
			Vector2 position, 
			G.Color color, 
			bool isStatic = false,
			float invMass = 1f, 
			float restitution = 0.2f, 
			float friction = 0.4f
			)
		{
			RigidBody polygonBody = new(new Polygon(vertices), position, isStatic, invMass, restitution, friction);
			world.Bodies.Add(polygonBody.Id, polygonBody);
			
			G.ConvexShape convexShape = new((uint)vertices.Length)
			{
				FillColor = color,
				OutlineColor = G.Color.Black,
				OutlineThickness = 0f,
				Origin = new(0f, 0f)
			};
			for (uint i = 0; i < (uint)vertices.Length; i++)
			{
				convexShape.SetPoint(i, new(vertices[i].X, vertices[i].Y));
			}

			Shapes.Add(polygonBody.Id, convexShape);

			return polygonBody;
		}

		void ShowRectCorners()
		{
			foreach (int index in Shapes.Keys)
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