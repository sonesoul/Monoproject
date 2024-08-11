global using GlobalTypes.Extensions;

using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using Monoproject.GameUI;
using GlobalTypes;
using GlobalTypes.Events;
using GlobalTypes.Interfaces;
using Engine;
using Engine.Drawing;
using Engine.Modules;
using Engine.Types;

namespace Monoproject
{
    public class Main : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private InterfaceDrawer interfaceDrawer;
        private IngameDrawer ingameDrawer;
        
        private TextObject[] objects = new TextObject[1];
        public TextObject player;
        private TextObject cursorObj;
        private static bool isConsoleTogglePressed = true;
        private bool canJump = false;
        
        public SpriteBatch SpriteBatch => spriteBatch;
        public int WindowWidth => graphics.PreferredBackBufferWidth;
        public int WindowHeight => graphics.PreferredBackBufferHeight;
        public static Main Instance { get; private set; }
        public static Color BackgroundColor { get; set; } = Color.Black;

        public Main()
        {
            long memBefore = GC.GetTotalMemory(true);

            Instance = this;
            
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            IsMouseVisible = false;
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;

            graphics.SynchronizeWithVerticalRetrace = true;
            IsFixedTimeStep = false;
            Window.AllowUserResizing = true;

            GameConsole.New();
            Console.WriteLine($"ctor: {memBefore.ToSizeString()}");
        }

        protected override void LoadContent()
        {
            spriteBatch = new(GraphicsDevice);
            ingameDrawer = IngameDrawer.CreateInstance(spriteBatch, GraphicsDevice);
            interfaceDrawer = InterfaceDrawer.CreateInstance(spriteBatch, GraphicsDevice);

            SetLoadables();
        }
        protected override void Initialize()
        {
            base.Initialize();

            SetInitables();
            CreateObjects();

            Console.WriteLine("initend: " + GC.GetTotalMemory(false).ToSizeString());
            GameConsole.Execute("mem");
        }
        
       
        protected override void Update(GameTime gameTime)
        {
            GameEvents.Update.Trigger(gameTime);

            if (Keyboard.GetState().IsKeyDown(Keys.F1))
                Exit();

            if(Mouse.GetState().LeftButton == ButtonState.Pressed)
            {
                var obj = new TextObject(IngameDrawer.Instance, "0", UI.Font);

                obj.AddModule<Collider>();
                obj.position = Mouse.GetState().Position.ToVector2();
            }

            UpdateControls();

            GameEvents.PostUpdate.Trigger(gameTime);
            base.Update(gameTime);
        }
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(BackgroundColor);
            
            GameEvents.PreDraw.Trigger(gameTime);
            DrawFrame(gameTime);
            GameEvents.PostDraw.Trigger(gameTime);

            base.Draw(gameTime);
        }
        
        private void DrawFrame(GameTime gameTime)
        {
            spriteBatch.Begin(blendState: BlendState.NonPremultiplied, samplerState: SamplerState.PointClamp, transformMatrix: Camera.GetViewMatrix());
            ingameDrawer.DrawAll(gameTime);
            spriteBatch.End();

            spriteBatch.Begin(blendState: BlendState.NonPremultiplied, samplerState: SamplerState.PointClamp);
            interfaceDrawer.DrawAll(gameTime);
            spriteBatch.End();
        }

        private void UpdateControls()
        {
            KeyboardState state = Keyboard.GetState();

            if (state.IsKeyDown(GameConsole.ToggleKey) && isConsoleTogglePressed)
            {
                GameConsole.ToggleState();
                isConsoleTogglePressed = false;
            }
            if (state.IsKeyUp(GameConsole.ToggleKey) && !isConsoleTogglePressed)
                isConsoleTogglePressed = true;

            player.position += new Vector2
            {
                X = (state.IsKeyDown(Keys.D) ? 400 : 0) - (state.IsKeyDown(Keys.A) ? 400 : 0),
                Y = 0
            } * HTime.DeltaTime;

            if (state.IsKeyDown(Keys.Space) && canJump)
            {
                player.GetModule<Rigidbody>().velocity = Vector2.Zero;
                player.GetModule<Rigidbody>().AddForce(new Vector2(0, -600));
                canJump = false;
            }

            if (state.IsKeyUp(Keys.Space))
                canJump = true;

            cursorObj.position = Mouse.GetState().Position.ToVector2();
        }
        private void CreateObjects()
        {
            for (int i = 0; i < objects.Length; i++)
            {
                static Vector2 RandomPos(Vector2 center, float radius)
                {
                    Random random = new();
                    double angle = random.NextDouble() * Math.PI * 2;
                    float distance = (float)(random.NextDouble() * radius);

                    float x = center.X + distance * (float)Math.Cos(angle);
                    float y = center.Y + distance * (float)Math.Sin(angle);

                    return new Vector2(x, y);
                }

                objects[i] = new(ingameDrawer, $"{i}", UI.Font)
                {
                    position = RandomPos(new(
                        WindowWidth / 2,
                        WindowHeight / 2), 300)
                };
                var objColl = objects[i].AddModule<Collider>();
                objColl.polygon = Polygon.Rectangle(50, 50);
                
                objects[i].AddModule<Rigidbody>();
            }

            player = new(ingameDrawer, "#", UI.Font)
            {
                position = new(400, 400),
                color = Color.Green,
                size = new(2, 2),
            };
            player.center += new Vector2(0, -2.5f);
            player.AddModule<Collider>().polygon = Polygon.Rectangle(30, 30);
            player.AddModule<Rigidbody>();

            cursorObj = new(ingameDrawer, "", UI.Font) { position = new(0, 0) };
            cursorObj.AddModule<Collider>().polygon = Polygon.Rectangle(50, 50);
            cursorObj.GetModule<Collider>().Mode = ColliderMode.Static;

            var wallDown = new TextObject(ingameDrawer, "", UI.Font)
            {
                position = new(WindowWidth / 2, WindowHeight),
            }.AddModule<Collider>();
            wallDown.polygon = Polygon.Rectangle(WindowWidth, 20);
            wallDown.Mode = ColliderMode.Static;

            var wallLeft = new TextObject(ingameDrawer, "", UI.Font)
            {
                position = new(WindowWidth, WindowHeight / 2 - 11),
            }.AddModule<Collider>();
            wallLeft.polygon = Polygon.Rectangle(20, WindowHeight);
            wallLeft.Mode = ColliderMode.Static;

            InputZone iz = new(ingameDrawer, "SOMETHING", UI.Font);
            iz.GetModule<Collider>().OnTouchStay += (gt) => GameConsole.WriteLine(".");
            iz.position = new(WindowWidth / 2, WindowHeight - 30);
        }

        private static void SetLoadables() => CreateInstances<ILoadable>().ForEach(l => CallPrivateMethod(l, ILoadable.MethodName));
        private static void SetInitables() => CreateInstances<IInitable>().ForEach(i => CallPrivateMethod(i, IInitable.MethodName));
        public static List<T> CreateInstances<T>() where T : class
        {
            var instances = new List<T>();

            var types = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => typeof(T).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            foreach (var type in types)
            {
                if (Activator.CreateInstance(type) is T instance)
                    instances.Add(instance);
            }

            return instances;
        }
        public static void CallPrivateMethod<T>(T instance, string methodName)
        {
            var method = 
                typeof(T).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance) 
                ?? throw new InvalidOperationException($"Method [{methodName}] not found.");
            method.Invoke(instance, null);
        }
    }
    public static class Camera
    {
        private static Vector2 _position = Vector2.Zero;
        private static float _zoom = 1f;

        public static Matrix GetViewMatrix() => Matrix.CreateTranslation(new(-_position, 0)) * Matrix.CreateScale(_zoom, _zoom, 1);
        public static void Move(Vector2 direction, float speed) => _position += direction * speed * HTime.DeltaTime;
        public static void ZoomIn(float amount, float speed) => _zoom += amount * speed * HTime.DeltaTime;

        public static float Zoom 
        {
            get => _zoom; 
            set => _zoom = Math.Max(0, value); 
        }
        public static Vector2 Position
        {
            get => _position;
            set => _position = value;
        }
    }
}