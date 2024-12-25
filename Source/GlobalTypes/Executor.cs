using Engine.Drawing;
using GlobalTypes.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using Monoproject;
using static MonoconsoleLib.Monoconsole;
using System.Reflection;
using InGame;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using GlobalTypes.InputManagement;
using System.Diagnostics;
using InGame.Managers;
using InGame.Overlays;
using InGame.GameObjects;

namespace GlobalTypes
{
    public static class Executor
    {
        private static class SettingsCommands
        {
            public static Dictionary<string, Action<string>> Commands { get; private set; } = new()
            {
                { "writeexec", static arg => SetProp(arg, nameof(WriteExecuted), typeof(Monoconsole)) },
                { "drawperf", static arg => SetProp(arg, nameof(PerfomanceOverlay.IsPerfomanceVisible), typeof(PerfomanceOverlay))},
            };
            private static void SetProp(string arg, string propname, Type t, object obj = null)
            {
                var property = t.GetProperty(propname);
                
                bool before = (bool)property.GetValue(obj);
                bool after = arg.HasContent() ? arg.ToBool() : !before;

                property.SetValue(obj, after);
            }
        }
        private static class GeneralExecution
        {
            public static Dictionary<string, Action<string>> Commands { get; private set; } = new()
            {
                { "new", static _ => New() },
                { "clear", static _ => Console.Clear() },
                { "color", Color },
                { "size", Size },
                
                { "f1", static _ => Main.Instance.Exit() },
                { "ram", static _ => Ram() },
                { "rem", static _ => Rem() },
                { "forcegc", static _ => GC.Collect() },
                { "throw", static _ => throw new() },
                { "fps", static _ => WriteInfo(FrameState.FPS.ToString())},
                { "perf", WritePerfomance },
                { "k+", static arg => Input.SetKey(Enum.Parse<Key>(arg, true), true)},
                { "k-", static arg => Input.SetKey(Enum.Parse<Key>(arg, true), false)},
                { "bind", BindCommand },
                
                { "popup", ShowDialogBox},
                { "stopwatch", Stopwatch },
                { "ruler", Ruler },
                { "captwin", CaptureWindow },
                { "level", LevelCommand },
                { "timescale", static arg => FrameState.TimeScale = arg.ToFloat() },

                { "game", GameCommand },
                { "touch", TouchCommand },
                { "help", arg => Tutorial.Toggle() },
            };

            private static void TouchCommand(string arg)
            {
                Partition(arg, out var subCommand, out var subArg);

                switch (subCommand)
                {
                    case "player":

                        Partition(subArg, out var playerCommand, out var playerArg);

                        Player player = Level.GetObject<Player>();

                        switch (playerCommand)
                        {
                            case "wallet":

                                Partition(playerArg, out var walletCommand, out var walletArg);

                                switch (walletCommand)
                                {
                                    case "add":
                                        player.BitWallet.Deposit(walletArg.ToInt());
                                        break;
                                    case "spend":
                                        player.BitWallet.TrySpend(walletArg.ToInt());
                                        break;
                                }
                                break;

                            case "code":
                                Partition(playerArg, out var codeCommand, out var codeArg);

                                switch (codeCommand)
                                {
                                    case "push":
                                        player.Codes.Push(new(codeArg));
                                        break;
                                    case "pop":
                                        player.Codes.Pop();
                                        break;
                                    case "clear":
                                        player.Codes.Clear();
                                        break;
                                }

                                break;
                        }

                        break;
                    case "filler":

                        Partition(subArg, out var fillerCommand, out var fillerArg);
                        StorageFiller filler = Level.GetObject<StorageFiller>();

                        switch (fillerCommand)
                        {
                            case "setcode":
                                filler.SetCode(new(fillerArg));
                                break;
                            case "mistake":
                                filler.LockInput();
                                break;
                        }

                        break;
                    case "storage":
                        Partition(subArg, out var storageCommand, out var storageArg);
                        CodeStorage storage = Level.GetObject<CodeStorage>();

                        switch (storageCommand)
                        {
                            case "push":
                                storage.Push(new(storageArg));
                                break;
                            case "setreq":
                                storage.SetRequirement(storageArg);
                                break;
                        }

                        break;
                }
            }
            private static void GameCommand(string arg)
            {
                Partition(arg, out var subCommand, out var subArg);

                switch (subCommand)
                {
                    case "start":
                        Session.Start();
                        break;
                    case "end":
                        Session.End();
                        break;
                    case "freeze":
                        SessionManager.Freeze();
                        break;
                    case "unfreeze":
                        SessionManager.Unfreeze();
                        break;
                    case "main":
                        Session.GoToMainMenu();
                        break;
                    case "over":
                        SessionManager.Over();
                        break;
                    case "next":
                        Session.NextLevel();
                        break;
                }
            }

            private static void BindCommand(string arg)
            {
                Partition(arg, out var key, out var command);

                Input.Bind((Key)Enum.Parse(typeof(Key), key, true), KeyPhase.Press, () => Execute(command));
            }

            private readonly static Ruler ruler = new();
            private readonly static KeyBinding[] rulerBindings =
            {
                new(Key.MouseLeft, KeyPhase.Hold, () => ruler.End = FrameState.MousePosition),
                new(Key.MouseRight, KeyPhase.Hold, () => ruler.Start = FrameState.MousePosition),
                new(Key.MouseMiddle, KeyPhase.Hold, () => ruler.InfoPosition = FrameState.MousePosition)
            };
            private readonly static Stopwatch stopwatch = new();

            private static void ShowDialogBox(string arg)
            {
                DialogBox.ShowMessage(arg, "Message");
            } 
            private static void WritePerfomance(string _)
            {
                WriteInfo(
                    $"fps: {FrameState.FPS}\n" +
                    $"time: {FrameState.DeltaTimeUnscaled * 1000:00}ms\n" +
                    $"ram: {GC.GetTotalMemory(false).ToSizeString()}\n" +
                    $"dc: {Drawer.DrawCalls}").Wait();
            }
            private static void Stopwatch(string arg)
            {
                switch (arg)
                {
                    case "start":
                        stopwatch.Start();
                        break;
                    case "reset":
                        stopwatch.Reset();
                        break;
                    case "stop":
                        stopwatch.Stop();
                        break;
                    case "info":
                        WriteInfo(stopwatch.Elapsed.ToString());
                        break;
                }
            }
            private static void CaptureWindow(string arg)
            {
                int scaleFactor = 1;

                if (arg.HasContent())
                {
                    scaleFactor = arg.ToInt();
                }

                GraphicsDevice graphics = Window.GraphicsDevice;
                Window.Size.ToPoint().Deconstruct(out int width, out int height);

                int scaledWidth = width * scaleFactor;
                int scaledHeight = height * scaleFactor;

                Texture2D capturedTexture = new(graphics, width, height);
                Texture2D scaledTexture = new(graphics, scaledWidth, scaledHeight);

                Color[] screenData = new Color[width * height];
                graphics.GetBackBufferData(screenData);
                capturedTexture.SetData(screenData);

                Color[] scaleData = new Color[scaledWidth * scaledHeight];
                for (int y = 0; y < scaledHeight; y++)
                {
                    for (int x = 0; x < scaledWidth; x++)
                    {
                        int srcX = x / scaleFactor;
                        int srcY = y / scaleFactor;
                        scaleData[y * scaledWidth + x] = screenData[srcY * width + srcX];
                    }
                }

                scaledTexture.SetData(scaleData);

                string path = Path.Combine(Environment.CurrentDirectory, "screenshot.png");
                using (var fs = new FileStream(path, FileMode.Create))
                {
                    scaledTexture.SaveAsPng(fs, scaledWidth, scaledHeight);

                    scaledTexture.Dispose();
                    capturedTexture.Dispose();

                    scaleData = null;
                    screenData = null;

                    fs.Dispose();
                }
                Main.Instance.ForceGC();

                WriteInfo(path);
            }

            //please don't ask me what is that
            public static void Rem()
            {
                WriteLine(@"******************************************#***#**#########################################
********************************************#####******###################################
********************************************$$$$#********#############*#*#########*****###
**********************#*#**********++*******#$$$##*##*****+*#########$###*#***************
*******************+#$$$$$****##*+!!!!+*#*#$$$$$$$#*##*++!!+**####*#$$$#**#**###**********
**********************$$$$####*##***#############*#############*###$$$$***################
***************++******######**##############***###############**######**+++++****########
*************++!=!==!*****************************************************+!!!+*##**##**+*
++++**#**+**##*!!!+****************************************************+***********#$$##**
**++#$$$#*###*+************************************************+*********++****#$$$$##*++*
***+++###$$##******************************************+!********++*******+!!*****#*+*##+!
###**#*+*********+***********************+**************!++*******++****+!!++++**********=
$#*#*++*********!+**********++***********!+*************!**++++++++++++!=!++++!!*****++*+!
*+++++*********+!**********+!**#********+!**************++$#!!!!!!!!==!!!+!+!!+!!+*+!!!!!=
+*+++++**++++++!+++++++++++!+*##*#*++++*+!*+++++++++++++++###*++++!==!++!!*+++*#*!++==!!==
#*+++++**++++++!+++++++++++!+***+++++++++!+++++++++++++!+!###$#*+==!+++++!!++++*#+!!==++!=
#++++++++++++++!+++++++++++!+++++++++++*+!+++++++++++++!!=++++****++++++++=++++++++!===!!=
++++++++++++++!!+++++++++++!+***********+!++++++++++++*!++*######$#*++++++!!+++++++!!+!=;=
+++!!+++++++++!!***+++++++*!+***********+!+************!+!*###########*++++=++++++++=!;=!+
+++!!++++++!++!!***********!!************=+************!+!+#####*##$$$$##*+=++++++++;;=++=
+++!=+++++*!!*!!*********+++!************!+************!+!+!;:;:~::;=!+*#$#!++++++++=;;==;
++++=!*****+!*!!*********+!*!+***********!+************!*!+!~-=:~----~=;;+#!!+++++!!;=!=;;
++++!=+*****!+*=+*********!*+!***********+!***********+!+!#;-;=;-~;;:~*#*;+!!!++++=!:;++!;
!!!!+!=+****+!*+!*********+!*!+***********=***********++!*#+::=+++!!++###++!=!+++=!=!:!+!;
!!!!!+!=+****+!*!+*********+!+!***********!!**********!++####*******######!!=+++!=!=!;!!!;
:!!!!!!!=!****+!+!+*********!++!***********!+***********#################+!!++++=!!*;;!!!:
::=!=!!!!==+***+!+!*##*##############################################***+!!+!++=!+*=:;!!!:
:::;===!!!!=!+****+!*################################################*#*!++!+*!+!;:~:!!!;~
:::::;==;=!!!!!++!*#*####################################################*=!+!;:~~~:=!=;::
:;;;;;:;;::;=!!!+!=*####################################################*:~~~~~~~~:;=;::;:
;;;;;;;;:;;~~~:;===;=*#################################################!:~~~~~~~~:;:~:::::
;;;;;==;;;;;:::~::::~~;!*#################**********################+=:~~-~~~~~~::~::::::~
;;;;;;:;;;::;===;;;=;~~~~:;!+*#################################*+!;~~~~~~:::::::::::::::::
:;;;;::;=;;;==!==;==;;:~~--~~~:;=!+*#####################**+=;:~~~~~:::;;;;;==;;;;::;;==;:
==;;=;;=!!=;;;==;;;;:::;::;=;:::~~~=!!!++**########**++!!:~~:;::;;;;;==!!!!=;;;;;::::;;::~
*+!==!=;;;;==;::::;;;;==;;=;;!!!==;:=!!!===!!!!!!!!!!!!!;:=!++*;=;;;;;;:::;;::=!!=;::~~~~:
####*+=+#*+!=;:;::;=;;;;===;;**=+**!===!!!!=!!!!!!=!!====!*#*!+;=======;::~~::;;:;::;;=!+*
###########*=;=;=;;;;;;:::::~:;*#####++####*==!==+**#*!=###*+!;==;;=!!=;:::;;:;=+****#####
###########*+++++!===!=;::;;!*++*############!;!*#####*!####*+++!=;;;;;;;;!+!!*###########
###########+*#####*!;=!++*#####***###+*#######+######***##########*+!!=;!*###*!*##########
***########*+*#####+=+################++############*+################+!!*####!!#####*****
*********###*!*####!!+#############*+!!====!=;:;======!+*#############+!!*##*#*!+*********
++++*******++*****#!=!*#######*+=:::;=!+**!;,,--;+**+!=:~:;!*#####**#*!!+*******=*********
++++++*****+!*******+!!+******:,:=+*###*!:~,-+*:.-:!*###*!=:~:!*****+!!++++****!!*********
************+!+*******!=!+****;,:+**+=:~:=~,=**+,~=~-:!***+;-:+***++!!+*******+=*******+++");
            }
            public static void Ram()
            {
                WriteInfo(
                    $"usg: [{GC.GetTotalMemory(false).ToSizeString()}]\n" +
                    $"avg: [{GC.GetTotalMemory(true).ToSizeString()}]\n" +
                    $"---\n" +
                    $"GC0: [{GC.CollectionCount(0)}]\n" +
                    $"GC1: [{GC.CollectionCount(1)}]\n" +
                    $"GC2: [{GC.CollectionCount(2)}]").Wait();
            }

            public static void Color(string arg)
            {
                switch (arg)
                {
                    case string s when !s.HasContent():

                        var values = Enum.GetValues<ConsoleColor>()
                             .Cast<ConsoleColor>()
                             .Where(v => v != ConsoleColor.Black)
                             .OrderBy(color => color.ToString(), StringComparer.OrdinalIgnoreCase).ToList();

                        foreach (var value in values)
                        {
                            WriteLine(value, value).Wait();
                        }

                        break;
                    case "reset":
                        
                        Console.ResetColor();
                        
                        break;
                    default:

                        if (!Enum.TryParse<ConsoleColor>(arg, true, out var color))
                        {
                            WriteError($"Color \"{arg}\" not found");
                        }
                        else
                        {
                            ForeColor = color;
                        }
                        
                        break;
                }
            }
            public static void Size(string arg)
            {
                Partition(arg, out var width, out var height);

                if (int.TryParse(width, out int newWidth))
                    Console.WindowWidth = newWidth;
                
                if (int.TryParse(height, out int newHeight))
                    Console.WindowHeight = newHeight;
            }
           
            public static void Ruler(string arg)
            {
                if(arg.HasContent())
                {
                    Partition(arg, out string subCommand, out string subArg);

                    static Color? GetColor(string name)
                    {
                        Color? found = (Color?)typeof(Color).GetProperties(BindingFlags.Public | BindingFlags.Static)
                               .FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).GetValue(null);

                        if(found == null)
                            WriteError($"Color \"{name}\" not found").Wait();

                        return found;
                    }
                    
                    switch (subCommand)
                    {
                        case "thickness":

                            ruler.Thickness = subArg.ToInt();

                            break;
                        case "linecolor":

                            Color? lineColor = GetColor(subArg);

                            if (lineColor != null)
                                ruler.LineColor = lineColor.Value;

                            break;
                        case "infocolor":

                            Color? infoColor = GetColor(subArg);
                            
                            if (infoColor != null)
                                ruler.InfoColor = infoColor.Value;
                            
                            break;
                        case "infosize":

                            ruler.InfoSize = subArg.ToFloat();

                            break;
                    }
                }
                else
                {
                    if (!ruler.IsActive)
                    {
                        ruler.Show();

                        Input.AddBinds(rulerBindings);

                        WriteInfo("Enabled");
                    }
                    else
                    {
                        ruler.Hide();

                        Input.Unbind(rulerBindings);

                        WriteInfo("Disabled");
                    }
                }   
            }
            public static void LevelCommand(string arg)
            {
                Partition(arg, out var subCommand, out var subArg);

                switch (subCommand)
                {
                    case "load":
                        if (subArg.HasContent())
                            Level.Load(subArg.ToInt());
                        else
                            Level.Load();
                        break;
                    case "clear":
                        Level.Clear();
                        break;
                }
            }
        }
        private static class ExecutionPipeline
        {
            public static Dictionary<string, Action<string>> Commands { get; private set; } = new()
            {
                { "async", static arg => System.Threading.Tasks.Task.Run(() => ExecuteAsync(arg)) },
                { "on", RunOn },
                { "for", For },
                { "wait", Wait },
                { "batch", Batch },
                { "script", Script },
                { "profile", static arg => Profile($"\"{arg}\"", () => Execute(arg)) },

                { "writel", CustomWriteLine },
                { "write", CustomWrite },
            };
            private static void Profile(string taskname, Action action)
            {
                Stopwatch sw = Stopwatch.StartNew();
                action?.Invoke();
                sw.Stop();

                WriteInfo($"{taskname}: {sw.Elapsed}");
            }
            private static void RunOn(string arg)
            {
                Partition(arg, out var eventType, out var command);

                void Exec() => Execute(command);

                Action handler = null;

                switch (eventType)
                {
                    //events
                    case "update":
                        FrameEvents.Update.AppendSingle(Exec);
                        break;
                    case "endupdate":
                        FrameEvents.EndUpdate.AppendSingle(Exec);
                        break;
                    case "predraw":
                        FrameEvents.PreDraw.AppendSingle(Exec);
                        break;
                    case "postdraw":
                        FrameEvents.PostDraw.AppendSingle(Exec);
                        break;
                    case "focused":

                        handler = () =>
                        {
                            Exec();
                            Window.Focused -= handler;
                        };

                        Window.Focused += handler;
                        break;
                    case "unfocused":

                        handler = () =>
                        {
                            Exec();
                            Window.Unfocused -= handler;
                        };

                        Window.Unfocused += handler;
                        break;

                    //threads
                    case "main":
                        Main.Instance.PostToMainThread(Exec);
                        break;
                    case "this":
                        Exec();
                        break;
                }
            }

            private static void CustomWriteLine(string arg)
            {
                WriteLine(arg[(arg.IndexOf('"') + 1)..arg.LastIndexOf('"')], ForeColor).Wait();
            }
            private static void CustomWrite(string arg)
            {
                Write(arg[(arg.IndexOf('"') + 1)..arg.LastIndexOf('"')], ForeColor).Wait();
            }

            private static void For(string arg)
            {
                Partition(arg, out var countArg, out var command);

                for (int i = 0; i < countArg.ToFloat(); i++)
                {
                    Execute(command);
                }
            }
            private static void Wait(string arg)
            {
                System.Threading.Tasks.Task.Delay((int)(1000f * arg.ToFloat())).Wait();
            }
            private static void Batch(string arg)
            {
                var result = new List<string>();
                int start = 0;
                int level = 0;

                for (int i = 0; i < arg.Length; i++)
                {
                    if (arg[i] == '(')
                    {
                        if (++level == 1)
                            arg = arg.Remove(i, 1);
                    }
                    else if (arg[i] == ')')
                    {
                        if(--level == 0)
                            arg = arg.Remove(i, 1);
                    }
                    else if (arg[i] == ',' && level == 1)
                    {
                        result.Add(arg[start..i].Trim());
                        start = i + 1;
                    }
                }

                result.Add(arg.Substring(start).Trim());
                result.ForEach(i => Execute(i));
            }
            
            private static void Script(string arg)
            {
                static void Read(string path)
                {
                    string[] lines = File.ReadAllLines(path)
                        .Where(l => !l.StartsWith('#') && l.HasContent())
                        .ToArray();


                    int count = 0;

                    foreach (var item in lines)
                    {
                        try
                        {
                            count++;
                            Execute(item);
                        }
                        catch
                        {
                            WriteError($"An error occured during execute command {count}: \"{item}\"");
                        }
                    }
                }
                static void Append(string command)
                {
                    if (command == "end")
                    {
                        CommandHandler = null;
                        
                        return;
                    }

                    batchQueue.Enqueue(command);
                }

                Partition(arg, out var subCommand, out var subArg);

                switch (subCommand)
                {
                    case "read":

                        Read(subArg);

                        break;
                    case "begin":

                        CommandHandler = Append;

                        break;

                    case "build":

                        List<string> lines = new();

                        while (batchQueue.Count > 0)
                        {
                            lines.Add(batchQueue.Dequeue());
                        }
                        try
                        {
                            File.WriteAllLines(subArg, lines);
                            WriteInfo($"Success");
                        }
                        catch (Exception ex)
                        {
                            WriteError($"An error occurred: {ex.Message}");
                            lines.ForEach(l => batchQueue.Enqueue(l));
                        }

                        break;

                    case "clear":

                        batchQueue.Clear();

                        break;  
                }
            }
        }

        private static Action<string> CommandHandler { get; set; } = null;
        private static Dictionary<string, Action<string>> Commands
            => GeneralExecution.Commands
            .Concat(ExecutionPipeline.Commands)
            .Concat(SettingsCommands.Commands)
            .ToDictionary(pair => pair.Key, pair => pair.Value);

        private readonly static Queue<string> batchQueue = new();


        public static void FromString(string input)
        {
            input = input.Trim();

            if (!input.HasContent())
                return;

            if (CommandHandler == null)
            {
                HandleCommand(input);
            }
            else 
            {
                CommandHandler(input);
            }
        }
        private static void HandleCommand(string input)
        {
            Partition(input, out string command, out string arg);

            if (!Commands.TryGetValue(command.ToLower(), out var action))
            {
                WriteLine($"Unexpected command. Did you mean \"{FindClosestWord(command, Commands.Keys.ToArray())}\"?", ConsoleColor.Red).Wait();
                return;
            }

            action?.Invoke(arg);
        }


        private static string FindClosestWord(string input, string[] candidates)
        {
            static int GetDistance(string s, string t)
            {
                int n = s.Length;
                int m = t.Length;
                int[,] d = new int[n + 1, m + 1];

                if (n == 0) return m;
                if (m == 0) return n;

                for (int i = 0; i <= n; i++) d[i, 0] = i;
                for (int j = 0; j <= m; j++) d[0, j] = j;

                for (int i = 1; i <= n; i++)
                {
                    for (int j = 1; j <= m; j++)
                    {
                        int cost = (s[i - 1] == t[j - 1]) ? 0 : 1;
                        d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
                    }
                }

                return d[n, m];
            }

            string closest = null;
            int minDistance = int.MaxValue;

            foreach (var candidate in candidates)
            {
                int distance = GetDistance(input, candidate);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closest = candidate;
                }
            }

            return closest;
        }
        private static void Partition(string input, out string firstWord, out string argument)
        {
            input = input.Trim();

            int index = input.IndexOf(' ');
            
            if (index != -1)
            {
                firstWord = input[..index].Trim();
                argument = input[index..].Trim();
            }
            else
            {
                firstWord = input;
                argument = "";
            }
        }
    }
}