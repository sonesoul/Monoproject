global using GlobalTypes.Extensions;
global using MonoconsoleLib;

using System;
using System.Globalization;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GlobalTypes;
using GlobalTypes.Events;
using GlobalTypes.Interfaces;
using GlobalTypes.InputManagement;
using Engine.Drawing;
using InGame;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Monoproject
{
    public class Main : Game
    {
        public static Color BackgroundColor { get; set; } = Color.Black;
        
        public Thread WindowThread { get; init; }
        public SynchronizationContext SyncContext { get; private set; }
        public static Main Instance { get; private set; }

        public GraphicsDeviceManager GraphicsManager => _graphics;
        public SpriteBatch SpriteBatch => _spriteBatch;
        public static Key ConsoleToggleKey => Key.OemTilde;

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private InterfaceDrawer _interfaceDrawer;
        private IngameDrawer _ingameDrawer;
        private GameMain _gameInstance;

        public Main()
        {
            long memCtor = GC.GetTotalMemory(true);

            Instance = this;

            _graphics = new(this)
            {
                PreferredBackBufferWidth = 720,
                PreferredBackBufferHeight = 720,
                SynchronizeWithVerticalRetrace = true
            };

            Window.AllowUserResizing = false;
            IsFixedTimeStep = false;
            Content.RootDirectory = "Content";

            SyncContext = SynchronizationContext.Current;
            WindowThread = Thread.CurrentThread;
            WindowThread.CurrentUICulture = new CultureInfo("en-US");
            WindowThread.CurrentCulture = new CultureInfo("en-US");

            InstanceInfo.UpdateVariables();

            Monoconsole.Handler = input => Executor.FromString(input);
            Monoconsole.Opened += () =>
            {
                string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
                StringBuilder sb = new();
                Random rnd = new();

                for (int i = 0; i < 5; i++)
                    sb.Append(chars[rnd.Next(chars.Length)]);

                List<ConsoleColor> colors = Enum.GetValues<ConsoleColor>().Where(c => c != ConsoleColor.Black).ToList();
                Monoconsole.WriteLine($"| monoconsole [{sb}]\n", colors[new Random().Next(colors.Count)]);
            };
            
            Monoconsole.New();
            Monoconsole.WriteInfo($"ctor: {memCtor.ToSizeString()}"); 
        }

        protected override void LoadContent()
        {
            LoadAttribute.Invoke();
            ILoadable.LoadAll();
        }
        protected override void Initialize()
        {
            //calls LoadContent
            base.Initialize();

            _spriteBatch = new(GraphicsDevice);
            _ingameDrawer = IngameDrawer.CreateInstance(_spriteBatch, GraphicsDevice);
            _interfaceDrawer = InterfaceDrawer.CreateInstance(_spriteBatch, GraphicsDevice);
            InstanceInfo.UpdateVariables();
            
            InitAttribute.Invoke();
            IInitable.InitAll();

            _gameInstance = new();
           
            Monoconsole.WriteInfo("init: " + GC.GetTotalMemory(false).ToSizeString());
            Monoconsole.Execute("ram");
            
            Input.Bind(Key.F1, KeyPhase.Press, () => Monoconsole.Execute("f1"));
            Input.Bind(ConsoleToggleKey, KeyPhase.Press, () => Monoconsole.Toggle());
        }

        protected override void Update(GameTime gameTime)
        {
            FrameEvents.Update.Trigger(gameTime);
            FrameEvents.EndUpdate.Trigger(gameTime);

            FrameEvents.EndSingle.Trigger(gameTime);
        }
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(BackgroundColor);
            
            FrameEvents.PreDraw.Trigger(gameTime);
            DrawFrame(gameTime);
            FrameEvents.PostDraw.Trigger(gameTime);

            base.Draw(gameTime);
        }
        
        private void DrawFrame(GameTime gameTime)
        {
            _spriteBatch.Begin(blendState: BlendState.NonPremultiplied, samplerState: SamplerState.PointClamp, transformMatrix: Camera.GetViewMatrix());
            _ingameDrawer.DrawAll(gameTime);
            _spriteBatch.End();

            _spriteBatch.Begin(blendState: BlendState.NonPremultiplied, samplerState: SamplerState.PointClamp);
            _interfaceDrawer.DrawAll(gameTime);
            _spriteBatch.End();
        }
                 
        public void PostToMainThread(Action action)
        {
            SyncContext.Post(_ =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    DialogBox.ShowException(ex);
                }
            }, null);
        }
    }

    public static class Camera
    {
        private static Vector2 _position = Vector2.Zero;
        private static float _zoom = 1f;

        public static Matrix GetViewMatrix() => Matrix.CreateTranslation(new(-_position, 0)) * Matrix.CreateScale(_zoom, _zoom, 1);
        public static void Move(Vector2 direction, float speed) => _position += direction * speed * FrameInfo.DeltaTime;
        public static void ZoomIn(float amount, float speed) => _zoom += amount * speed * FrameInfo.DeltaTime;

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