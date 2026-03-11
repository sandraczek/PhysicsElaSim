using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Runtime.ExceptionServices;
using PhysicsElaSim.physics;
using SFML.Graphics;

namespace PhysicsElaSim.src
{
using G = SFML.Graphics;
using W = SFML.Window;
using S = SFML.System;
class App
{	// todo: cleanup textures and resolving textures in functions etc. 
	// add real materials with restitution, friction and textures 

	// update view logic with vertex batching (requires circle handling)
	private const float FixedDt = 1.0f / 60.0f;
	private static G.Color bgColor = new(100,100,100);
	private static readonly bool showCenters = false;
	private static readonly bool showEdges = false;
	private static readonly float spawnImpulse = 50f;
	// view
	private static readonly float zoomMultiplier = 0.004f;
	private static bool isPanning = false;
	private static S.Vector2i previousMousePos;
	private static G.Text? mousePosText;
	private static G.Text? rulerText;
	private static readonly float rulerLength = 64f;

	static void Main(string[] args)
	{
		//loading assets
		G.Font font = new("Assets/Fonts/Work_Sans/static/WorkSans-Light.ttf");
		G.Texture oakWoodTexture = new("Assets/Textures/oak.jpg")
		{
			Smooth = true,
			Repeated = true
		};
		G.Texture metalTexture = new("Assets/Textures/metal.jpeg")
		{
			Smooth = true,
			Repeated = true
		};
		G.Texture woodGrainTexture = new("Assets/Textures/woodGrain.jpg")
		{
			Smooth = true,
			Repeated = true
		};

		G.RenderWindow window  = new(new W.VideoMode(new S.Vector2u(735, 478)), "Physics Simulation", W.Styles.Default, W.State.Windowed, new()
		{
			AntialiasingLevel = 8
		});
		G.View freeCamera = new(new FloatRect(new(0f,0f),new(735f, 478f)));
		G.View UIview = new(new FloatRect(new(0f,0f),new(735f, 478f)));
		freeCamera.Zoom(0.01f);
		freeCamera.Center = new S.Vector2f(110f, 364f);

		World world = new();
		Dictionary<int, G.Shape> Shapes = []; // has to be before OnKeyPressed. can change structure later

		window.Closed += (sender, e) => window.Close();
		window.KeyPressed += OnKeyPressed;
		window.MouseWheelScrolled += OnMouseWheelScrolled;
		window.MouseButtonPressed += OnMouseButtonPressed;
		window.MouseButtonReleased += OnMouseButtonReleased;
		window.MouseMoved += OnMouseMoved;

        // initial Objects
        Vector2 floorSize = new(1000f,30f);
		SpawnRect(floorSize, new Vector2(200f, 400f), G.Color.White ,null, null, true, invMass: 0f, restitution: 0.15f, 0.5f);
		SpawnBall(50f, new Vector2(240f, 100f),null, metalTexture, new(new(200,200),new(128,128)), invMass: 1f/32656000000f, restitution: 0.9f, friction: 0.2f);

		//trampolines
		SpawnRect(new(60f,6f),new(600f,380f),new(179, 175, 54),null, null, true, 0, 1.2f,0.4f);
		var jump2 = SpawnRect(new(60f,6f),new(650f,366f),new(179, 175, 54),null, null, true, 0, 1.2f,0.4f);
		jump2.Rotation = -0.55f;
		var jump3 = SpawnRect(new(60f,6f),new(550f,366f),new(179, 175, 54),null, null, true, 0, 1.2f,0.4f);
		jump3.Rotation = 0.55f;
		SpawnRect(new(6f,120f),new(675f,290f),new(179, 175, 54),null, null, true, 0, 1.2f,0.4f);
		SpawnRect(new(6f,120f),new(525f,290f),new(179, 175, 54),null, null, true, 0, 1.2f,0.4f);
		
		//chest
		SpawnRect(new(20f,100f), new Vector2(10f, 315f), new(101,67,33) ,woodGrainTexture, new(new(40,30),new(40,200)),true, invMass: 0f, restitution: 0.15f, 0.5f);
		SpawnRect(new(200f,20f), new Vector2(100f, 375f), new(101,67,33) ,woodGrainTexture,new(new(40,30),new(400,40)),true, invMass: 0f, restitution: 0.15f, 0.5f);
		SpawnRect(new(20f,100f), new Vector2(190f, 315f), new(101,67,33) ,woodGrainTexture,new(new(40,30),new(40,200)),true, invMass: 0f, restitution: 0.15f, 0.5f);

		//table
		SpawnRect(new(30f,0.4f), new Vector2(110f, 365.2f), G.Color.White ,metalTexture, new(new(40,30),new(3000,40)),true, invMass: 0f, restitution: 0.15f, 0.5f);

		SpawnRect(new(0.2f,1.2f), new Vector2(109.1f, 364.4f), new(101,67,33) ,woodGrainTexture,new(new(40,30),new(40,200)),true, invMass: 0f, restitution: 0.15f, 0.5f);
		SpawnRect(new(0.2f,1.2f), new Vector2(110.9f, 364.4f), new(101,67,33) ,woodGrainTexture,new(new(40,30),new(40,200)),true, invMass: 0f, restitution: 0.15f, 0.5f);
		SpawnRect(new(2f,0.4f), new Vector2(110f, 364f), new(101,67,33) ,woodGrainTexture, new(new(40,30),new(400,40)),true, invMass: 0f, restitution: 0.15f, 0.5f);

		float fixedDtAccumulator = 0f;
		int fpsCounter = 0;
		float fpsTimer = 0f;
		G.Text fpsCountText = new(font)
		{
			Position = window.MapPixelToCoords(new(2, 2), UIview),
			FillColor = G.Color.White,
			CharacterSize = 15,
			DisplayedString = "0"
		};

		int objectCounter = 0;
		G.Text objectCountText = new(font)
		{
			Position = window.MapPixelToCoords(new(100, 2), UIview),
			FillColor = G.Color.White,
			CharacterSize = 15,
			DisplayedString = "0"
		};

		mousePosText = new(font)
		{
			Position = window.MapPixelToCoords(new(200, 2), UIview),
			FillColor = G.Color.White,
			CharacterSize = 15,
			DisplayedString = "0"
		};
		rulerText = new(font)
		{
			Position = new S.Vector2f(UIview.Size.X, 0f) + window.MapPixelToCoords(new(-40, 84), UIview),
			Rotation = MathF.PI / 2f,
			FillColor = G.Color.White,
			CharacterSize = 10,
			DisplayedString = "0"
		};
		G.RectangleShape rulerRect1 = new(new S.Vector2f(2f, rulerLength))
		{
			FillColor = new(220,220,220,80),
			Position = new S.Vector2f(UIview.Size.X, 0f) + window.MapPixelToCoords(new(-10, 100), UIview)
		};
		G.RectangleShape rulerRect2 = new(new S.Vector2f(8f, 2f))
		{
			FillColor = new(220,220,220,80),
			Position = new S.Vector2f(UIview.Size.X - 3f, -2f) + window.MapPixelToCoords(new(-10, 100), UIview)
		};
		G.RectangleShape rulerRect3 = new(new S.Vector2f(8f, 2f))
		{
			FillColor = new(220,220,220,80),
			Position = new S.Vector2f(UIview.Size.X - 3f, rulerLength) + window.MapPixelToCoords(new(-10, 100), UIview)
		};
		UpdateRuler();

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

			// --------- View -------------
			window.Clear(bgColor);

			window.SetView(freeCamera);

			foreach (G.Shape shape in Shapes.Values)
			{
				window.Draw(shape);
			}
			if(showCenters) ShowCenters();
			if(showEdges) ShowPolyCorners();

			window.SetView(UIview);
			window.Draw(fpsCountText);
			window.Draw(objectCountText);
			window.Draw(mousePosText);
			window.Draw(rulerRect1);
			window.Draw(rulerRect2);
			window.Draw(rulerRect3);
			window.Draw(rulerText);
			
			window.Display();
		}
		void OnMouseButtonPressed(object? sender, W.MouseButtonEventArgs e)
		{
			if(e.Button == W.Mouse.Button.Right)
			{
				isPanning = true;
				previousMousePos = new S.Vector2i(e.Position.X, e.Position.Y);
			}
		}
		void OnMouseButtonReleased(object? sender, W.MouseButtonEventArgs e)
		{
			if(e.Button == W.Mouse.Button.Right)
			{
				isPanning = false;
			}
		}
		void OnMouseMoved(object? sender, W.MouseMoveEventArgs e)
		{
			if(mousePosText != null){
				var mousePos = window.MapPixelToCoords(e.Position,freeCamera);
				mousePosText.DisplayedString = $"mouse pos: [{mousePos.X:0.00}, {mousePos.Y:0.00}]";
			}

			if (isPanning)
			{
				S.Vector2f delta = window.MapPixelToCoords(previousMousePos,freeCamera) - window.MapPixelToCoords(e.Position,freeCamera);

				freeCamera.Center += delta;
				window.SetView(freeCamera);

				previousMousePos = e.Position;
			}

		}
		void OnMouseWheelScrolled(object? sender, W.MouseWheelScrollEventArgs e)
		{
			S.Vector2f mouseWorldPosBeforeZoom = window.MapPixelToCoords(e.Position, freeCamera);

			float zoomFactor = 1f - zoomMultiplier * e.Delta;
			if (zoomFactor <= 0.01f) zoomFactor = 0.01f;
			freeCamera.Zoom(zoomFactor);

			S.Vector2f mouseWorldPosAfterZoom = window.MapPixelToCoords(e.Position, freeCamera);

			freeCamera.Center += mouseWorldPosBeforeZoom - mouseWorldPosAfterZoom;

			window.SetView(freeCamera);

			UpdateRuler();
		}
		void OnKeyPressed(object? sender, W.KeyEventArgs e)
		{
			float relation = 0.01f;
			float relativeSize = freeCamera.Size.Y * relation;
			float woodDensity = 700f;
			float relativeInvMass = 1f/(woodDensity * 4f/3f * MathF.PI * relativeSize * relativeSize * relativeSize);
			Console.WriteLine(relativeInvMass);
			if (e.Code == W.Keyboard.Key.F)
			{
				S.Vector2f worldMousePos = window.MapPixelToCoords(W.Mouse.GetPosition(window), freeCamera);
				SpawnBall(relativeSize,MathP.ToP(worldMousePos), null, oakWoodTexture, null, false, relativeInvMass);
			}
			if (e.Code == W.Keyboard.Key.R)
			{
				S.Vector2f worldMousePos = window.MapPixelToCoords(W.Mouse.GetPosition(window), freeCamera);
				SpawnRect(new(10f,16f),MathP.ToP(worldMousePos), new(113, 153, 241), null, null, false, 0.00001f);
			}

			if (e.Code == W.Keyboard.Key.D)
			{
				S.Vector2f worldMousePos = window.MapPixelToCoords(W.Mouse.GetPosition(window), freeCamera);
				var ball = SpawnBall(relativeSize,MathP.ToP(worldMousePos), null, oakWoodTexture, null, false, relativeInvMass);
				ball.AddImpulse(Vector2.Right * (spawnImpulse * relativeSize / relativeInvMass));
			}
			if (e.Code == W.Keyboard.Key.A)
			{
				S.Vector2f worldMousePos = window.MapPixelToCoords(W.Mouse.GetPosition(window), freeCamera);
				var ball = SpawnBall(relativeSize,MathP.ToP(worldMousePos), null, oakWoodTexture, null, false, relativeInvMass);
				ball.AddImpulse(Vector2.Left * (spawnImpulse * relativeSize / relativeInvMass));
			}
			if (e.Code == W.Keyboard.Key.W)
			{
				S.Vector2f worldMousePos = window.MapPixelToCoords(W.Mouse.GetPosition(window), freeCamera);
				var ball = SpawnBall(relativeSize,MathP.ToP(worldMousePos), null, oakWoodTexture, null, false, relativeInvMass);
				ball.AddImpulse(Vector2.Up * (spawnImpulse * relativeSize / relativeInvMass));
			}
			if (e.Code == W.Keyboard.Key.S)
			{
				S.Vector2f worldMousePos = window.MapPixelToCoords(W.Mouse.GetPosition(window), freeCamera);
				var ball = SpawnBall(relativeSize,MathP.ToP(worldMousePos), null, oakWoodTexture, null, false, relativeInvMass);
				ball.AddImpulse(Vector2.Down * (spawnImpulse * relativeSize / relativeInvMass));
			}
			if (e.Code == W.Keyboard.Key.X)
			{
				Vector2 size = new(30f,40f);
				S.Vector2f SFworldMousePos = window.MapPixelToCoords(W.Mouse.GetPosition(window), freeCamera);
				Vector2 worldMousePos = MathP.ToP(SFworldMousePos);
				var rect = SpawnRect(size,worldMousePos, new(113, 173, 201));
				rect.AddImpulseLocal(Vector2.Right * spawnImpulse + Vector2.Up * spawnImpulse, new(-15f,-20f));
			}
			
			if (e.Code == W.Keyboard.Key.P)
			{
				Vector2[] vertices =
				[
					new(-7f, -8f), // 1. Lewy dół
					new(8f, -5f),  // 2. Prawy dół (pod lekkim skosem)
					new(10f, 8f),   // 3. Skrajny prawy punkt
					new(3f, 10f),    // 4. Górny wierzchołek
					new(-8f, 5f)
				];
				S.Vector2f SFworldMousePos = window.MapPixelToCoords(W.Mouse.GetPosition(window), freeCamera);
				Vector2 worldMousePos = MathP.ToP(SFworldMousePos);
				var ngon = SpawnPolygon(vertices, worldMousePos, new(110, 158, 47), null, null, false, 0.001f, 0.65f, 0.4f);
				//ngon.AddImpulseLocal(Vector2.Right * spawnImpulse + Vector2.Up * spawnImpulse, new(-35,-35));
			}
			if (e.Code == W.Keyboard.Key.O)
			{
				Vector2[] vertices =
				[
					new(-4f, 8f),  // 1. Lewy górny płaski róg
					new(-8f, 2f),  // 2. Lewy skrajny (najszerszy punkt)
					new(0f, -8f),  // 3. Dolny ostry szpic
					new(8f, 2f),   // 4. Prawy skrajny (najszerszy punkt)
					new(4f, 8f)
				];
				S.Vector2f SFworldMousePos = window.MapPixelToCoords(W.Mouse.GetPosition(window), freeCamera);
				Vector2 worldMousePos = MathP.ToP(SFworldMousePos);
				var diamond = SpawnPolygon(vertices, worldMousePos, new(154,197,219), null, null, false, 1f/3000000f);
				//diamond.AddImpulseLocal(Vector2.Right * spawnImpulse + Vector2.Up * spawnImpulse, new(-35,-35));
			}
			if (e.Code == W.Keyboard.Key.I)
			{
				Vector2[] vertices =
				[
					new(-4f, -3f),   // 1. Lewy górny płaski rdzeń (początek górnej krawędzi)
					new(-8f, -2f),   // 2. Lewe górne ścięcie (fazowanie)
					new(-9f, 6f), // 3. Lewy dolny róg (najszerszy punkt na dole)
					new(9f, 6f),  // 4. Prawy dolny róg (najszerszy punkt na dole)
					new(8f, -2f),    // 5. Prawe górne ścięcie (fazowanie)
					new(4f, -3f)
				];
				S.Vector2f SFworldMousePos = window.MapPixelToCoords(W.Mouse.GetPosition(window), freeCamera);
				Vector2 worldMousePos = MathP.ToP(SFworldMousePos);
				var gold = SpawnPolygon(vertices, worldMousePos, new(212, 175, 55), null, null, false, 1f/20000000f);
				//gold.AddImpulseLocal(Vector2.Right * spawnImpulse + Vector2.Up * spawnImpulse, new(-35,-35));
			}
		}
		RigidBody SpawnBall(
			float radius, 
			Vector2 position, 
			G.Color? color = null,
			G.Texture? texture = null, 
			G.IntRect? textureRect = null,
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
				FillColor = color ?? G.Color.White,
				OutlineColor = G.Color.Black,
				OutlineThickness = showEdges? 1f:0f,
				Origin = new S.Vector2f(radius, radius),
				Texture = texture
			};
			if(textureRect.HasValue) circleShape.TextureRect = textureRect.Value;
			Shapes.Add(circleBody.Id, circleShape);

			return circleBody;
		}
		RigidBody SpawnRect(
			Vector2 size, 
			Vector2 position, 
			G.Color? color = null,
			G.Texture? texture = null, 
			G.IntRect? textureRect = null,
			bool isStatic = false,
			float invMass = 1f, 
			float restitution = 0.2f, 
			float friction = 0.4f
			)
		{
			float halfWidth = size.X * 0.5f;
			float halfHeight = size.Y * 0.5f;
			Vector2[] vertices = 
			[
				new Vector2(halfWidth, -halfHeight),
				new Vector2(-halfWidth, -halfHeight),
				new Vector2(-halfWidth, halfHeight),
				new Vector2(halfWidth, halfHeight),

			];

			return SpawnPolygon(vertices, position, color, texture, textureRect, isStatic, invMass, restitution, friction);
		}

		RigidBody SpawnPolygon(
			Vector2[] vertices, 
			Vector2 position, 
			G.Color? color = null,
			G.Texture? texture = null, 
			G.IntRect? textureRect = null,
			bool isStatic = false,
			float invMass = 1f, 
			float restitution = 0.3f, 
			float friction = 0.4f
			)
		{
			var poly = new Polygon(vertices);
			RigidBody polygonBody = new(poly, position, isStatic, invMass, restitution, friction);
			world.Bodies.Add(polygonBody.Id, polygonBody);
			
			G.ConvexShape convexShape = new((uint)vertices.Length)
			{
				FillColor = color ?? G.Color.White,
				OutlineColor = G.Color.Black,
				OutlineThickness = 0f,
				Origin = new(0f, 0f),
				Texture = texture
			};
			if(textureRect.HasValue) convexShape.TextureRect = textureRect.Value;

			var centeredVertices = poly.GetLocalVertices();

			for (uint i = 0; i < (uint)vertices.Length; i++)
			{
				convexShape.SetPoint(i, MathP.ToSF(centeredVertices[i]));
			}

			Shapes.Add(polygonBody.Id, convexShape);

			return polygonBody;
		}

		void ShowPolyCorners()
		{
			foreach (int index in Shapes.Keys)
			{
				RigidBody body = world.Bodies[index];
				if(body.Shape is Polygon poly){
					G.Vertex[] corners = new G.Vertex[poly.VerticeCount + 1];
					var vertices = poly.GetVertices(body.Pos, body.Rotation);
					for (int i = 0; i<poly.VerticeCount;i++)
					{
						corners[i] = new(MathP.ToSF(vertices[i]),G.Color.Black);
					}
					
					corners[poly.VerticeCount] = corners[0];
					window.Draw(corners, G.PrimitiveType.LineStrip);
				}
			}
		}
		void ShowCenters()
		{
			G.Vertex[] centers = new G.Vertex[Shapes.Count];
			var keys = Shapes.Keys.ToArray();
			for (int i = 0; i< Shapes.Count; i++) 								// TODO? : set a drawing priority
			{
				centers[i] = new(MathP.ToSF(world.Bodies[keys[i]].Pos), G.Color.Black);
			}
			window.Draw(centers, G.PrimitiveType.Points);
		}
		void UpdateRuler()
		{
		if(rulerText != null){
			float length = freeCamera.Size.Y * rulerLength / UIview.Size.Y;
			string scale;
			float outLength = length;
			if(length > 1000f)
			{
				scale = "km";
				outLength /= 1000f;
			}
			else if(length > 1f)
			{
				scale = "m";
			}
			else if(length > 0.01f)
			{
				scale = "cm";
				outLength *= 100f;
			}
			else
			{
				scale = "mm";
				outLength *= 1000f;
			}
			rulerText.DisplayedString = $"{outLength:0.0}{scale}";
		}
		}
	}

	
}}