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

        private List<IInitable> initables = new();
        private List<ILoadable> loadables = new();
        
        public SpriteBatch SpriteBatch => spriteBatch;
        public int WindowWidth => graphics.PreferredBackBufferWidth;
        public int WindowHeight => graphics.PreferredBackBufferHeight;
        public static Main Instance { get; private set; }

        
        public Main()
        {
            long memBefore = GC.GetTotalMemory(true);

            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");

            Instance = this;
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = false;

            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;

            graphics.SynchronizeWithVerticalRetrace = false;
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
        }
        protected override void Update(GameTime gameTime)
        {
            GameEvents.OnUpdate.Trigger(gameTime);
            if (Keyboard.GetState().IsKeyDown(Keys.F1))
                Exit();

            UpdateControls();
            
            base.Update(gameTime);
        }
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            
            GameEvents.OnBeforeDraw.Trigger(gameTime);
            DrawFrame(gameTime);
            GameEvents.OnAfterDraw.Trigger(gameTime);

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
                X = (Keyboard.GetState().IsKeyDown(Keys.D) ? 1 : 0) - (Keyboard.GetState().IsKeyDown(Keys.A) ? 1 : 0),
                Y = (Keyboard.GetState().IsKeyDown(Keys.S) ? 1 : 0) - (Keyboard.GetState().IsKeyDown(Keys.W) ? 1 : 0)
            } * 100 * HTime.DeltaTime;
            
            if (state.IsKeyDown(Keys.Space) && canJump)
            {
                player.GetModule<Rigidbody>().velocity = Vector2.Zero;
                player.GetModule<Rigidbody>().AddForce(new(0, -600));
                canJump = false;
            }
            if (state.IsKeyDown(Keys.F))
            {
                player.GetModule<Rigidbody>().AddForce(new(1, 0));
            }

            if (state.IsKeyUp(Keys.Space))
            {
                canJump = true;
            }
            if (state.IsKeyDown(Keys.Q))
            {
                player.GetModule<Rigidbody>().AngularVelocity -= 380f.AsRad() * HTime.DeltaTime;
            }
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
                objects[i].AddModule<Rigidbody>().Bounciness = 0.5f;
                objects[i].GetModule<Collider>().polygon = Polygon.Rectangle(50, 50);
                objects[i].GetModule<Rigidbody>().GravityScale = 0;
            }

            
            player = new(ingameDrawer, "#", UI.Font)
            {
                position = new(400, 400),
                color = Color.Green
            };


            player.AddModule<Collider>().polygon = Polygon.Rectangle(50, 50);
            player.AddModule<Rigidbody>();
            
            cursorObj = new(ingameDrawer, "", UI.Font) { position = new(0, 0) };

            cursorObj.AddModule<Collider>().polygon = Polygon.Rectangle(50, 20);
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
        }


        private void SetLoadables()
        {
            loadables = CreateInstances<ILoadable>();
            loadables.ForEach(l => l.Load());
        }
        private void SetInitables()
        {
            initables = CreateInstances<IInitable>();
            initables.ForEach(i => i.Init());
        }
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