using GlobalTypes;
using GlobalTypes.InputManagement;
using InGame.GameObjects;
using InGame.Interfaces;
using InGame.Managers;
using Monoproject;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InGame
{
    public static class Tutorial
    {
        private static StepTask tutorialTask = null;

        private static bool enabled = false;

        public static void Enable()
        {
            StepTask.Replace(ref tutorialTask, HelpTask, false);
        }
        public static void Disable() 
        {
            tutorialTask?.Break();
            Session.Restart();
        }
        public static void Toggle()
        {
            if (!enabled) 
            {
                Enable();
                enabled = true;
            }
            else
            {
                Disable();
                enabled = false;
            }
        }

        private static IEnumerator HelpTask()
        {
            Console.Clear();
            if (SessionManager.IsStarted)
            {
                Monoconsole.WriteLine("First, let's start with the main menu...");
                
                yield return StepTask.DelayUnscaled(2);
                SessionManager.End();
                Session.GoToMainMenu();

                yield return StepTask.DelayUnscaled(2);
                Monoconsole.WriteLine("Here we are.");
                yield return StepTask.DelayUnscaled(3);

            }
            
            OverlayManager.DisableScreen();
            Monoconsole.WriteLine("Focus the game window, then read anything that will be written there.");

            bool focusedWindow = false;

            EventHandler<EventArgs> focusAction = null;

            Main.Instance.Activated += focusAction = (obj, e) =>
            {
                focusedWindow = true;
                Main.Instance.Activated -= focusAction;
            };
            
            yield return StepTask.WaitUntil(() => focusedWindow);
            yield return StepTask.DelayUnscaled(1);
            
            Monoconsole.WriteLine("That's it. Do you see the (S)tart button?");
            yield return StepTask.DelayUnscaled(1);

            Monoconsole.WriteLine("Press the S key on your keyboard to interact\n", ConsoleColor.Yellow);
            OverlayManager.EnableScreen();

            bool sPressed = false;
            Input.BindSingle(Key.S, KeyPhase.Press, () => sPressed = true);
            
            yield return StepTask.WaitUntil(() => sPressed);

            CodeStorage storage = null;

            bool created = false;
            void DisableTimerAndTask()
            {
                storage = Level.GetObject<CodeStorage>();
                storage.Timer.Stop();
                Level.CurrentTask.Finish();
                SessionManager.Difficulty.StopScaling();

                Level.Created -= DisableTimerAndTask;
                created = true;
            }
            Level.Created += DisableTimerAndTask;


            yield return StepTask.WaitUntil(() => created);
            Console.Clear();
            
            Monoconsole.WriteLine("You can move and jump using the arrows on your keyboard!");

            yield return StepTask.DelayUnscaled(2);
            Monoconsole.WriteLine("Move left, right, and jump\n", ConsoleColor.Yellow);

            bool movedLeft = false;
            bool movedRight = false;
            bool jumped = false;

            Input.BindSingle(Key.Left, KeyPhase.Press, () => movedLeft = true);
            Input.BindSingle(Key.Right, KeyPhase.Press, () => movedRight = true);
            Input.BindSingle(Key.Up, KeyPhase.Press, () => jumped = true);

            yield return StepTask.WaitUntil(() => jumped && movedRight && movedLeft);

            Console.Clear();
            Monoconsole.WriteLine(@"Now, find something that looks like /\ and approach it");

            bool touchedFiller = false;

            var filler = Level.GetObject<StorageFiller>();
            Action fillerTouched = () =>
            {
                touchedFiller = true;
            };
            fillerTouched.Invoke(w => filler.Activated += w, w => filler.Activated -= w);

            yield return StepTask.WaitUntil(() => touchedFiller);

            Monoconsole.WriteLine("Great! You've discovered a Filler");
            yield return StepTask.DelayUnscaled(3);

            Monoconsole.WriteLine("\nOh.. you have no codes. Okay, here it is!");
            yield return StepTask.DelayUnscaled(2);

            var player = Level.GetObject<Player>();
            player.Codes.CanManuallyPop = false;
            player.Codes.CanCombinate = false;

            player.Codes.Push(new("QWER"));
            yield return StepTask.Delay(0.5f);
            player.Codes.Push(new("WQER"));
            yield return StepTask.Delay(0.5f);
            player.Codes.Push(new("WEQR"));
            
            yield return StepTask.DelayUnscaled(2);
            Monoconsole.WriteLine("You can pop a last got code by pressing the space bar. Give it a try!");

            yield return StepTask.DelayUnscaled(1);
            Monoconsole.WriteLine("Press the Spacebar to pop a code\n", ConsoleColor.Yellow);
            player.Codes.CanManuallyPop = true;
            
            bool popped = false;
            Input.BindSingle(Key.Space, KeyPhase.Press, () => popped = true);

            yield return StepTask.WaitUntil(() => popped);
            player.Codes.CanManuallyPop = false;

            Monoconsole.WriteLine("Great! It's your Code Stack. You can hold only 3 codes at the same time and you can use only the one that you've got last!");
            yield return StepTask.DelayUnscaled(3);
            
            Monoconsole.WriteLine(
                $"\nDo you see something that looks like [{storage.Requirement}]? It's the Storage\n" +
                $"The {storage.Requirement} is the required letter you need in a code. It rolls every time when you enter anything in the Storage.\n" +
                $"Enter right codes to raise progress!");

            yield return StepTask.DelayUnscaled(2);
            Monoconsole.WriteLine("Approach the Filler and press buttons that shown in the code", ConsoleColor.Yellow);

            bool pushed = false;
            player.Codes.CanCombinate = true;

            Action<Code> codePushed = (c) => pushed = true;
            codePushed.Invoke(w => storage.Pushed += w, w => storage.Pushed -= w);

            yield return StepTask.WaitUntil(() => pushed);
            Monoconsole.WriteLine("\nWell done!");

            yield return StepTask.DelayUnscaled(2);
            Monoconsole.WriteLine($"\nDid you see the progress percent? Even if you not, you always can display it");

            yield return StepTask.DelayUnscaled(2);
            Monoconsole.WriteLine("Hold Tab to display additional interface elements", ConsoleColor.Yellow);

            bool uiShown = false;
            bool uiHidden = false;

            Input.BindSingle(Key.Tab, KeyPhase.Press, () => uiShown = true);
            Input.BindSingle(Key.Tab, KeyPhase.Release, () => uiHidden = true);

            yield return StepTask.WaitUntil(() => uiShown && uiHidden);

            Monoconsole.WriteLine($"\nThere is a {(int)(100 * (storage.Progress / storage.Capacity))}% progress. To complete the level you need to make 100% progress.");
            
            yield return StepTask.Delay(3);
            Monoconsole.WriteLine("To get codes, you need to complete tasks. It's infinite, but the time is not - you have only one minute to finish levels.");
            yield return StepTask.DelayUnscaled(3);

            Monoconsole.WriteLine(
                $"\nLevels also have interactables, to interact you need to approach them and press Arrow Down.\n" +
                $"It consumes your bits, which you can see flying to you when you hold Tab\n" +
                $"You can get bits by completing levels, it gets more when you faster!");
            
            yield return StepTask.DelayUnscaled(3);
            Monoconsole.WriteLine("\nOh, and I've disabled the timer and tasks on this level to make it easier to teach you. So, type \"help\" in the console to complete the tutorial.");
        }
    }
}