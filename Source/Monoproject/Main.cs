global using GlobalTypes.Extensions;
global using MonoconsoleLib;
global using Microsoft.Xna.Framework;
global using GlobalTypes.Assets;
global using static GlobalTypes.Extensions.ActionExtensions;

using System;
using System.Globalization;

using System.Threading;
using Microsoft.Xna.Framework.Graphics;
using GlobalTypes;
using GlobalTypes.Events;
using GlobalTypes.InputManagement;
using Engine.Drawing;
using InGame;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monoproject
{
    public class Main : Game
    {
        public Thread WindowThread { get; init; }
        public SynchronizationContext SyncContext { get; private set; }
        public static Main Instance { get; private set; }

        public GraphicsDeviceManager GraphicsManager => _graphics;
        public SpriteBatch SpriteBatch => _spriteBatch;
        public static Key ConsoleToggleKey => Key.OemTilde;

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private double updateBuffer = 0;

        public Main()
        {
            Instance = this;

            _graphics = new(this)
            {
                PreferredBackBufferWidth = 720,
                PreferredBackBufferHeight = 720,
                SynchronizeWithVerticalRetrace = true
            };
            
            Window.AllowUserResizing = false;

            Content.RootDirectory = "Content";
            
            IsFixedTimeStep = true;
            IsMouseVisible = true;
            
            SyncContext = SynchronizationContext.Current;
            WindowThread = Thread.CurrentThread;
            WindowThread.CurrentUICulture = new CultureInfo("en-US");
            WindowThread.CurrentCulture = new CultureInfo("en-US");

            GlobalTypes.Window.UpdateInfo();
            Asset.Content = Content;

            Monoconsole.Handler = Executor.FromString;
            Monoconsole.Opened += async () =>
            {
                string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
                StringBuilder sb = new();
                Random rnd = new();

                for (int i = 0; i < 5; i++)
                {
                    int index = rnd.Next(chars.Length);
                    sb.Append(chars[index]);
                }

                List<ConsoleColor> colors = 
                    Enum.GetValues<ConsoleColor>()
                    .Where(c => c != ConsoleColor.Black)
                    .ToList();
                
                int colorIndex = rnd.Next(colors.Count);

                await Monoconsole.WriteLine($"| monoconsole [{sb}]\n", colors[colorIndex]);
            };

            Monoconsole.HideFromTaskbar = false;
            Monoconsole.HideButtons = false;

            Monoconsole.New();
            Monoconsole.Execute("size 40 15");
        }

        protected override void LoadContent() => LoadAttribute.Invoke();
        protected override void Initialize()
        {
            //calls LoadContent
            base.Initialize();

            _spriteBatch = new(GraphicsDevice);

            GlobalTypes.Window.UpdateInfo();
            
            InitAttribute.Invoke();

            Session.GoToMainMenu();

            Input.Bind(Key.F1, KeyPhase.Press, () => Monoconsole.Execute("f1"));
            Input.Bind(ConsoleToggleKey, KeyPhase.Press, () => Monoconsole.Toggle());
        }

        protected override void Update(GameTime gameTime)
        {
            FrameState.UpdateGameTime(gameTime);
            FrameState.Update();

            FrameEvents.UpdateUnscaled.Trigger();

            updateBuffer += FrameState.DeltaTime;
            double interval = FrameState.FixedDeltaTime;

            while (updateBuffer > 0)
            {
                updateBuffer -= interval;

                FrameEvents.Update.Trigger();
                FrameEvents.EndUpdate.Trigger();
            }

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


        public void ForceGC() => PostToMainThread(GC.Collect);
        public void PostToMainThread(Action action) => SyncContext.Post(_ => action(), null);
    }
}