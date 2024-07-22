using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using Source.UtilityTypes;
using Source.FrameDrawing;
using Source.Engine;
using Source.GameUI;
using static Source.UtilityTypes.HCoords;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

namespace Source
{
    public class GameMain : Game
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
        public static GameMain Instance { get; private set; }

        public GameMain()
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
            GameEvents.OnLoad();
        }
        protected override void Initialize()
        {
            base.Initialize();
            GameEvents.OnInit();

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

            GameEvents.OnUpdate(gameTime);

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
            
            GameEvents.OnBeforeDraw(gameTime);
            DrawFrame(gameTime);
            GameEvents.OnAfterDraw(gameTime);

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
                player.GetModule<Rigidbody>().AddForce(new(0, ToPixels(-3)));
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

            player.AddModule<Collider>().polygon = PolygonFactory.Rectangle(50, 50);
            player.AddModule<Rigidbody>();

            cursorObj = new(ingameDrawer, "", UI.Font)
            {
                position = new(0, 0),
                color = Color.White
            };
            cursorObj.AddModule<Collider>().polygon = PolygonFactory.Rectangle(100, 30);

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
            wallDown.polygon = PolygonFactory.Rectangle(WindowWidth, 20);
            wallDown.Mode = ColliderMode.Static;
            wallLeft.polygon = PolygonFactory.Rectangle(20, WindowHeight);
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
    public static class GameEvents
    {
        public static event Action Loaded;
        public static event Action Inited;
        public static event Action<GameTime> Update;
        public static event Action<GameTime> BeforeDraw;
        public static event Action<GameTime> AfterDraw;

        public static void OnLoad() => Loaded?.Invoke();
        public static void OnInit() => Inited?.Invoke();
        public static void OnUpdate(GameTime gameTime) => Update?.Invoke(gameTime);
        public static void OnBeforeDraw(GameTime gameTime) => BeforeDraw?.Invoke(gameTime);
        public static void OnAfterDraw(GameTime gameTime) => AfterDraw?.Invoke(gameTime);
    }
    public static class NewGameEvents
    {
        public struct EventSub<T>
        {
            public Action<T> action;
            public int order;
            public EventSub(Action<T> action, int order)
            {
                this.action = action;
                this.order = order;
            }
        }
        public struct EventSub
        {
            public Action action;
            public int order;

            public EventSub(Action action, int order)
            {
                this.action = action;
                this.order = order;
                
            }
        }

        private readonly static List<EventSub<GameTime>> Update = new();
        private readonly static List<EventSub<GameTime>> BeforeDraw = new();
        private readonly static List<EventSub<GameTime>> AfterDraw = new();
        private readonly static List<EventSub> Loaded = new();
        private readonly static List<EventSub> Inited = new();
        
        public static void AddToUpdate(Action<GameTime> action, int order = 0) => AddTo(Update, action, order);
        public static void AddToBeforeDraw(Action<GameTime> action, int order = 0) => AddTo(BeforeDraw, action, order);
        public static void AddToAfterDraw(Action<GameTime> action, int order = 0) => AddTo(AfterDraw, action, order);
        public static void AddToInited(Action action, int order = 0) => AddTo(Inited, action, order);
        public static void AddToLoaded(Action action, int order = 0) => AddTo(Loaded, action, order);

        public static void OnUpdate(GameTime gt)
        {
            foreach (var item in Update)
                item.action?.Invoke(gt);
        }
        public static void OnBeforeDraw(GameTime gameTime)
        {
            foreach (var item in BeforeDraw)
                item.action?.Invoke(gameTime);
        }
        public static void OnAfterDraw(GameTime gameTime)
        {
            foreach (var item in AfterDraw)
                item.action?.Invoke(gameTime);
        }
        public static void OnLoaded()
        {
            foreach (var item in Loaded)
                item.action?.Invoke();
        }
        public static void OnInited()
        {
            foreach (var item in Inited)
                item.action?.Invoke();
        }


        private static void AddTo(List<EventSub> list, Action action, int order) 
        {
            list.Add(new(action, order));
            list.Sort((first, second) => first.order.CompareTo(second));
        }
        private static void AddTo(List<EventSub<GameTime>> list, Action<GameTime> action, int order)
        {
            list.Add(new(action, order));
            list.Sort((first, second) => first.order.CompareTo(second));
        }
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
            _position += ToPixels(direction) * speed * HTime.DeltaTime;
        }
        public static void ZoomIn(float amount, float speed)
        {
            _zoom += ToPixels(amount) * speed * HTime.DeltaTime;
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