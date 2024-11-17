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

            MainContext.UpdateInfo();

            Monoconsole.Handler = input => Executor.FromString(input);
            Monoconsole.Opened += async () =>
            {
                string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
                StringBuilder sb = new();
                Random rnd = new();

                for (int i = 0; i < 5; i++)
                    sb.Append(chars[rnd.Next(chars.Length)]);

                List<ConsoleColor> colors = Enum.GetValues<ConsoleColor>().Where(c => c != ConsoleColor.Black).ToList();
                await Monoconsole.WriteLine($"| monoconsole [{sb}]\n", colors[new Random().Next(colors.Count)]);
            };

            Monoconsole.HideButtons = false;
            Monoconsole.HideFromTaskbar = false;
            Monoconsole.New();
            Monoconsole.WriteInfo($"ctor: {memCtor.ToSizeString()}");
        }

        protected override void LoadContent() => LoadAttribute.Invoke();
        protected override void Initialize()
        {
            //calls LoadContent
            base.Initialize();

            _spriteBatch = new(GraphicsDevice);
           
            MainContext.UpdateInfo();
            
            InitAttribute.Invoke();
            
            _gameInstance = new();
           
            Monoconsole.WriteInfo("init: " + GC.GetTotalMemory(false).ToSizeString());
            Monoconsole.Execute("ram");
            
            Input.Bind(Key.F1, KeyPhase.Press, () => Monoconsole.Execute("f1"));
            Input.Bind(ConsoleToggleKey, KeyPhase.Press, () => Monoconsole.Toggle());
        }

        protected override void Update(GameTime gameTime)
        {
            FrameState.UpdateGameTime(gameTime);
            FrameState.Update();

            FrameEvents.Update.Trigger();
            FrameEvents.EndUpdate.Trigger();

            FrameEvents.EndSingle.Trigger();
        }
        protected override void Draw(GameTime gameTime)
        {
            FrameState.UpdateGameTime(gameTime);

            Drawer.Erase();

            FrameEvents.PreDraw.Trigger();
            Drawer.DrawAll();
            FrameEvents.PostDraw.Trigger();

            base.Draw(gameTime);
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
}