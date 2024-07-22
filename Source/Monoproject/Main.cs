global using GlobalTypes.Extensions;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

using Monoproject.GameUI;
using GlobalTypes;
using GlobalTypes.Events;
using Engine;
using Engine.FrameDrawing;
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
        

        public TextObject player;
        private TextObject cursorObj;

        private List<IInitable> initables = new();
        private List<ILoadable> loadables = new();
        private static bool isGameCycled = true;

        bool canJump = false;

        public SpriteBatch SpriteBatch => spriteBatch;
        public int WindowWidth => graphics.PreferredBackBufferWidth;
        public int WindowHeight => graphics.PreferredBackBufferHeight;
        public static Main Instance { get; private set; }

        public Main()
        {
            Instance = this;
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = false;

            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;

            graphics.SynchronizeWithVerticalRetrace = false;
            IsFixedTimeStep = false;
            Window.AllowUserResizing = true;
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
        }
        protected override void Update(GameTime gameTime)
        {
            if (!isGameCycled)
            {
                base.Update(gameTime);
                return;
            }


            GameEvents.OnUpdate.Trigger(gameTime);
            if (Keyboard.GetState().IsKeyDown(Keys.F1))
                Exit();

            UpdateControls();

            base.Update(gameTime);
        }
        protected override void Draw(GameTime gameTime)
        {
            if (!isGameCycled) 
            {
                base.Draw(gameTime);
                return;
            }


            GraphicsDevice.Clear(Color.Black);
            
            GameEvents.OnBeforeDraw.Trigger(gameTime);
            DrawFrame(gameTime);
            GameEvents.OnAfterDraw.Trigger(gameTime);

            base.Draw(gameTime);
        }
        
        public void DrawFrame(GameTime gameTime)
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
            player.position += new Vector2
            {
                X = (Keyboard.GetState().IsKeyDown(Keys.D) ? 1 : 0) - (Keyboard.GetState().IsKeyDown(Keys.A) ? 1 : 0),
                Y = (Keyboard.GetState().IsKeyDown(Keys.S) ? 1 : 0) - (Keyboard.GetState().IsKeyDown(Keys.W) ? 1 : 0)
            } * HTime.UnitsPerSec(1f);


            if (Keyboard.GetState().IsKeyDown(Keys.Space) && canJump)
            {
                player.GetModule<Rigidbody>().velocity = Vector2.Zero;
                player.GetModule<Rigidbody>().AddForce(new(0, HCoords.ToPixels(-3)));
                canJump = false;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.F))
            {
                player.GetModule<Rigidbody>().AddForce(new(1, 0));
            }
            if (Keyboard.GetState().IsKeyUp(Keys.Space))
            {
                canJump = true;
            }

            cursorObj.position = Mouse.GetState().Position.ToVector2();
        }
        private void CreateObjects()
        {
            TextObject[] objects = new TextObject[5];

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
                objects[i].AddModule<Rigidbody>();
            }


            player = new(ingameDrawer, "#", UI.Font)
            {
                position = new(400, 400),
                color = Color.Green
            };

            player.AddModule<Collider>().polygon = Polygon.Rectangle(50, 50);
            player.AddModule<Rigidbody>();

            cursorObj = new(ingameDrawer, "", UI.Font)
            {
                position = new(0, 0),
                color = Color.White
            };
            cursorObj.AddModule<Collider>().polygon = Polygon.Rectangle(100, 30);

            var wallDown = new TextObject(ingameDrawer, "", UI.Font)
            {
                position = new(WindowWidth / 2, WindowHeight),
                color = Color.Gray,
            }.AddModule<Collider>();
            
            var wallLeft = new TextObject(ingameDrawer, "", UI.Font)
            {
                position = new(WindowWidth, WindowHeight / 2 - 11),
                color = Color.Gray,
            }.AddModule<Collider>();
            wallDown.polygon = Polygon.Rectangle(WindowWidth, 20);
            wallDown.Mode = ColliderMode.Static;
            wallLeft.polygon = Polygon.Rectangle(20, WindowHeight);
            wallLeft.Mode = ColliderMode.Static;
        }

        private void SetLoadables()
        {
            loadables = CreateInstances<ILoadable>();

            foreach (var item in loadables)
                item.Load();
        }
        private void SetInitables()
        {
            initables = CreateInstances<IInitable>();

            foreach (var item in initables)
                item.Init();
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

        public static void BreakCycle() => isGameCycled = false;
    }
    public static class Camera
    {
        private static Vector2 _position = Vector2.Zero;
        private static float _zoom = 1f;

        public static Matrix GetViewMatrix()
        {
            return 
                Matrix.CreateTranslation(new(-_position, 0)) *
                Matrix.CreateScale(_zoom, _zoom, 1);
        }
        public static void Move(Vector2 direction, float speed)
        {
            _position += HCoords.ToPixels(direction) * speed * HTime.DeltaTime;
        }
        public static void ZoomIn(float amount, float speed)
        {
            _zoom += HCoords.ToPixels(amount) * speed * HTime.DeltaTime;
        }
        
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

    public interface ILoadable
    {
        void Load();
    }
    public interface IInitable
    {
        void Init();
    }
}