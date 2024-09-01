using Engine;
using Engine.Drawing;
using GlobalTypes;
using GlobalTypes.Events;
using GlobalTypes.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Linq;
using System.Text;

namespace InGame.GameObjects
{
    class StorageFiller : ModularObject
    {
        private readonly WordStorage storage;
        private readonly EventListener<GameTime> updateListener;

        private string brackets;
        private string bracketSpace = "       ";
        private string input = "";
        private Vector2 bracketsOrigin;
        private Vector2 inputOrigin;
        private Color inputColor = Color.White;
        private Color bracketsColor = Color.White;
        private int maxLength;


        private bool canWrite = true;

        private static IngameDrawer Drawer => IngameDrawer.Instance;

        public StorageFiller(WordStorage storage, int maxLength)
        {
            this.maxLength = maxLength;
            this.storage = storage;
            updateListener = FrameEvents.Update.Append(Update);
            Drawer.AddDrawAction(Draw);

            StringBuilder sb = new();

            sb.Append("[" + bracketSpace);

            for (int i = 0; i < maxLength; i++)
            {
                sb.Append(bracketSpace);
            }

            sb.Append(bracketSpace + "]");

            brackets = sb.ToString();

            bracketsOrigin = UI.Font.MeasureString(brackets) / 2;
            inputOrigin = UI.Font.MeasureString(input) / 2;
        }

        private void Draw(GameTime gt)
        {
            Drawer.SpriteBatch.DrawString(
                UI.Font, 
                input, 
                position, 
                inputColor,
                0,
                inputOrigin,
                new Vector2(0.8f, 0.8f),
                SpriteEffects.None, 
                0
                );

            Drawer.SpriteBatch.DrawString(
                UI.Font,
                brackets,
                position,
                bracketsColor,
                0,
                bracketsOrigin,
                Vector2.One,
                SpriteEffects.None,
                0
                );
        }

        private void Update(GameTime gt) 
        {
            Keys[] keys = FrameState.KeyState.GetPressedKeys();

            if (keys.Length > 0 && canWrite)
            {
                Keys key = keys[0];
                string stringKey = key.ToString();
                if (stringKey.Length != 1)
                    return;

                char keyChar = stringKey[0];

                if (key == Keys.Back && input.Length > 0)
                {
                    input = input.SkipLast(1).ToString();
                    inputOrigin = UI.Font.MeasureString(input) / 2;
                }
                else if (WordStorage.AlphabetUpper.Contains(keyChar) && input.Length < maxLength)
                {
                    input += keyChar;
                    inputOrigin = UI.Font.MeasureString(input) / 2;

                    brackets = brackets.Remove(1, bracketSpace.Length);
                    bracketsOrigin = UI.Font.MeasureString(brackets) / 2;
                }

                InputManager.AddSingleTrigger(key, KeyEvent.Release, () => canWrite = true);
                canWrite = false;
            }
        }

        public override void Destroy()
        {
            base.Destroy();
            FrameEvents.Update.Remove(updateListener);
        }
    }
}