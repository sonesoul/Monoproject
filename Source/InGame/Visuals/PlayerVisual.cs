using Engine.Drawing;
using GlobalTypes.Events;
using GlobalTypes;
using InGame.GameObjects;
using InGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace InGame.Visuals
{
    [Init(nameof(Init))]
    public static class PlayerVisual
    {
        private class ComboVisual
        {
            public Combo Combo { get; set; }
            public Vector2 Position { get; set; }
            public Vector2 TargetPosition { get; set; }

            public float Alpha { get; set; } = 1f;
            public bool IsDisappearing { get; set; } = false;

            public ComboVisual(Combo combo, Vector2 position)
            {
                Combo = combo;
                Position = position;
                TargetPosition = position;
            }
        }
        public static bool CanDraw { get; set; } = true;
        
        private static readonly List<ComboVisual> comboDisplays = new();
        private static Vector2 comboStartPosition = new(10, 10);
        private static float comboSpacing = 30f;

        private static void Init()
        {
            InterfaceDrawer.Instance.AddDrawAction(DrawCombos);
            FrameEvents.Update.Append(Update);

            Player.OnComboAdd += AddCombo;
            Player.OnComboRemove += RemoveCombo;
        }

        private static void Update()
        {
            for (int i = comboDisplays.Count - 1; i >= 0; i--)
            {
                var display = comboDisplays[i];

                if (display.IsDisappearing)
                {
                    display.Alpha -= 0.10f;
                    if (display.Alpha <= 0)
                    {
                        comboDisplays.RemoveAt(i);
                        UpdateTargetPositions();
                        continue;
                    }
                }

                display.Position = Vector2.Lerp(display.Position, display.TargetPosition, 0.2f);
            }
        }
        private static void DrawCombos()
        {
            if (!CanDraw)
                return;

            foreach (var display in comboDisplays)
            {
                Color color = Color.White * display.Alpha;
                InstanceInfo.SpriteBatch.DrawString(UI.Silk, display.Combo.ToString(), display.Position, color);
            }
        }

        public static void AddCombo(Combo combo)
        {
            Vector2 position = comboStartPosition + new Vector2(0, comboDisplays.Count * comboSpacing);
            comboDisplays.Add(new ComboVisual(combo, position));
        }
        public static void RemoveCombo(Combo combo)
        {
            var display = comboDisplays.FirstOrDefault(c => c.Combo == combo);
            if (display != null)
            {
                display.IsDisappearing = true;
            }

            UpdateTargetPositions();
        }

        private static void UpdateTargetPositions()
        {
            for (int i = 0; i < comboDisplays.Count; i++)
            {
                comboDisplays[i].TargetPosition = comboStartPosition + new Vector2(0, i * comboSpacing);
            }
        }
    }
}