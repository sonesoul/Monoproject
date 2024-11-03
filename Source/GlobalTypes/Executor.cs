using Engine.Drawing;
using GlobalTypes.Events;
using Microsoft.Xna.Framework;
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
using InGame.GameObjects;

namespace GlobalTypes
{
    public static class Executor
    {
        private static class SettingsCommands
        {
            public static Dictionary<string, Action<string>> Commands => _commands;
            private readonly static Dictionary<string, Action<string>> _commands = new()
            {
                { "writeexec", static arg => SetProp(arg, nameof(WriteExecuted), typeof(Monoconsole)) },
                { "isfpsfixed", static arg => SetProp(arg, nameof(Main.Instance.IsFixedTimeStep), Main.Instance) },
                { "drawdebug", static arg =>
                {
                    SetProp(arg, nameof(UI.DrawDebug), typeof(UI));
                }},
                { "customcur", CustomCurCommand },
            };
            private static void SetProp(string arg, string propname, Type t, object obj = null)
            {
                var property = t.GetProperty(propname);
                
                bool before = (bool)property.GetValue(obj);
                bool after = string.IsNullOrEmpty(arg) ? !before : bool.Parse(arg);

                property.SetValue(obj, after);
                WriteSet(after);
            }

            private static void SetProp(string arg, string propname, object obj) => SetProp(arg, propname, obj.GetType(), obj);
            private static void WriteSet(bool value) => WriteInfo(value ? "Enabled" : "Disabled");

            private static void CustomCurCommand(string arg)
            {
                Main main = Main.Instance;
                bool value = string.IsNullOrEmpty(arg) ? !UI.UseCustomCursor : bool.Parse(arg);

                WriteSet(value);

                void Toggle(object sender, EventArgs args)
                {
                    UI.UseCustomCursor = value;
                    main.Activated -= Toggle;
                };
                main.Activated -= Toggle;
                main.Activated += Toggle;
            }

        }
        private static class GeneralExecution
        {
            public static Dictionary<string, Action<string>> Commands => _commands;

            private readonly static Dictionary<string, Action<string>> _commands = new()
            {
                //console control
                { "new", static _ => New() },
                { "clear", static _ => Console.Clear() },
                { "color", Color },
                { "size", Size },
                { "position", Position },

                //game control
                { "f1", static _ => Main.Instance.Exit() },
                { "ram", static _ => Ram() },
                { "rem", static _ => Rem()},
                { "gccollect", static _ => GC.Collect()},
                { "throw", static _ => throw new()},
                { "fps", Fps},

                //utilities
                { "stopwatch", Stopwatch },
                { "ruler", Ruler },
                { "captwin", CaptureWindow },
                { "level", Level },
            };

            private readonly static Ruler ruler = new();
            private readonly static KeyBinding[] rulerBindings =
            {
                new(Key.MouseLeft, KeyPhase.Hold, () => ruler.End = FrameInfo.MousePosition),
                new(Key.MouseRight, KeyPhase.Hold, () => ruler.Start = FrameInfo.MousePosition),
                new(Key.MouseMiddle, KeyPhase.Hold, () => ruler.InfoPosition = FrameInfo.MousePosition)
            };
            private static bool writeInputEnabled = false;
            private readonly static Stopwatch stopwatch = new();

            private static void Fps(string arg)
            {
                if (string.IsNullOrEmpty(arg))
                {
                    WriteInfo(FrameInfo.FPS.ToString());
                    return;
                }

                float fps = float.Parse(arg);
                Main.Instance.TargetElapsedTime = TimeSpan.FromSeconds(1.0 / fps);
                WriteInfo($"Target FPS set to: {fps}");
            }
            private static void Stopwatch(string arg)
            {
                switch (arg)
                {
                    case "start":
                        stopwatch.Start();
                        break;
                    case "new":
                        stopwatch.Restart();
                        break;
                    case "stop":
                        stopwatch.Stop();
                        break;
                    case "info":
                        TimeSpan elapsed = stopwatch.Elapsed;
                        WriteInfo($"{elapsed.Minutes:00}:{elapsed.Seconds:00}.{elapsed.Milliseconds:000}");
                        break;
                }
            }
           
            private static void CaptureWindow(string arg)
            {
                int scaleFactor = 1;

                if (!string.IsNullOrEmpty(arg))
                {
                    scaleFactor = int.Parse(arg);
                }

                GraphicsDevice graphics = InstanceInfo.GraphicsDevice;
                InstanceInfo.WindowSize.ToPoint().Deconstruct(out int width, out int height);

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
                Main.Instance.PostToMainThread(() => GC.Collect());

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
                    case string s when string.IsNullOrEmpty(s):

                        var values = Enum.GetValues<ConsoleColor>()
                             .Cast<ConsoleColor>()
                             .Where(v => v != ConsoleColor.Black)
                             .OrderBy(color => color.ToString(), StringComparer.OrdinalIgnoreCase).ToList();

                        foreach (var value in values)
                            WriteLine(value, value).Wait();

                        break;
                    case "reset":
                        Console.ResetColor();
                        WriteInfo($"Color set to {ForeColor}");
                        break;
                    default:
                        if (!Enum.TryParse<ConsoleColor>(arg, true, out var color))
                            WriteError($"Color \"{arg}\" not found");
                        else
                        {
                            if (color == ConsoleColor.Black)
                            {
                                WriteInfo("I can't see black text on my black console background. Suffer with me :3");
                                return;
                            }

                            ForeColor = color;
                            WriteLine($"Color set to {color}", ForeColor);
                        }
                        break;
                }
            }
            public static void Size(string arg)
            {
                SubInput(arg, out var width, out var height);

                if(int.TryParse(width, out int newWidth))
                    Console.WindowWidth = newWidth;
                
                if(int.TryParse(height, out int newHeight))
                    Console.WindowHeight = newHeight;
            }
            public static void Position(string arg)
            {
                string[] sizes = SplitInput(arg);

                if (sizes.Length > 1)
                {
                    Console.SetWindowPosition(int.Parse(sizes[0]), int.Parse(sizes[1]));
                }
            }

            public static void Ruler(string arg)
            {
                if(!string.IsNullOrEmpty(arg))
                {
                    SubInput(arg, out string subCommand, out string subArg);

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

                            ruler.Thickness = int.Parse(subArg);

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

                            ruler.InfoSize = float.Parse(subArg);

                            break;
                        default:
                            LogInvalidArg(subCommand, nameof(subCommand));
                            break;
                    }
                }
                else
                {
                    if (!ruler.IsActive)
                    {
                        ruler.Show();

                        Input.Bind(rulerBindings);

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
            public static void Level(string arg)
            {
                SubInput(arg, out var subCommand, out var subArg);

                switch (arg)
                {
                    case "new":

                        InGame.Level.Load();
                        
                        break;
                    case "clear":

                        InGame.Level.Clear();
                        
                        break;
                    case "load":

                        InGame.Level.Load();

                        break;
                    default:
                        LogInvalidArg(arg, nameof(arg));
                        break;
                }
            }
        }
        private static class ExecutionPipeline
        {
            public static Dictionary<string, Action<string>> Commands => _commands;

            private readonly static Dictionary<string, Action<string>> _commands = new()
            {
                { "async", static arg => System.Threading.Tasks.Task.Run(() => ExecuteAsync(arg)) },
                { "on", RunOn },
                { "for", For },
                { "wait", Wait },
                { "batch", Batch },
                { "batchbegin", static _ => BatchControl() },
                { "batchfile", BatchFileControl },

                { "writel", CustomWriteLine },
                { "write", CustomWrite },
            };

            private static void RunOn(string arg)
            {
                SubInput(arg, out var eventType, out var command);

                switch (eventType)
                {
                    case "update":
                        FrameEvents.Update.AppendSingle(() => Execute(command));
                        break;
                    case "endupdate":
                        FrameEvents.EndUpdate.AppendSingle(() => Execute(command));
                        break;
                    case "predraw":
                        FrameEvents.PreDraw.AppendSingle(() => Execute(command));
                        break;
                    case "postdraw":
                        FrameEvents.PostDraw.AppendSingle(() => Execute(command));
                        break;
                    case "main":
                        ExecuteOnThread(Main.Instance.SyncContext, command, (ex) => DialogBox.ShowException(ex));
                        break;
                    case "this":
                        Execute(command);
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
                SubInput(arg, out var countArg, out var command);

                for (int i = 0; i < float.Parse(countArg); i++)
                {
                    Execute(command);
                }
            }
            private static void Wait(string arg)
            {
                float time = float.Parse(arg);

                System.Threading.Tasks.Task.Delay((int)(1000f * time)).Wait();
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
            
            private static void BatchFileControl(string arg)
            {
                static void Read(string path)
                {
                    string[] lines = File.ReadAllLines(path)
                        .Where(l => !l.StartsWith('#') && !string.IsNullOrWhiteSpace(l))
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
                    if (command == "batchfile end")
                    {
                        OnBatchReceive -= Append;
                        IsBatchBegun = false;

                        return;
                    }

                    batchQueue.Enqueue(command);
                }

                SubInput(arg, out var subCommand, out var subArg);

                switch (subCommand)
                {
                    case "read":

                        Read(subArg);

                        break;
                    case "begin":

                        IsBatchBegun = true;
                        OnBatchReceive += Append;

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
                        }
                        catch (Exception ex)
                        {
                            WriteError($"An error occurred while writing to file: {ex.Message}");
                            lines.ForEach(l => batchQueue.Enqueue(l));
                        }

                        break;

                    case "clear":

                        batchQueue.Clear();

                        break;
                        
                }
            }

            private static void BatchControl()
            {
                static void Append(string command)
                {
                    if (command != "batchend")
                    {
                        batchQueue.Enqueue(command);
                    }
                    else
                    {
                        IsBatchBegun = false;

                        while (batchQueue.Count > 0)
                        {
                            FromString(batchQueue.Dequeue());
                        }
                        OnBatchReceive -= Append;
                    }
                }

                IsBatchBegun = true;
                OnBatchReceive += Append;
            }
        }

        public static bool IsBatchBegun { get; private set; } = false;
        private static Dictionary<string, Action<string>> Commands
            => GeneralExecution.Commands
            .Concat(ExecutionPipeline.Commands)
            .Concat(SettingsCommands.Commands)
            .ToDictionary(pair => pair.Key, pair => pair.Value);

        public static event Action<string> OnBatchReceive;
        private readonly static Queue<string> batchQueue = new();


        public static void FromString(string input)
        {
            input = input.Trim();

            if (string.IsNullOrEmpty(input))
                return;

            if (IsBatchBegun)
            {
                OnBatchReceive?.Invoke(input);
                return;
            }

            SubInput(input, out string command, out string arg);

            if (!Commands.TryGetValue(command.ToLower(), out var action))
            {
                WriteLine($"Unexpected command. Did you mean \"{FindClosestWord(input, Commands.Keys.ToArray())}\"?", ConsoleColor.Red).Wait();
                return;
            }

            action?.Invoke(arg);
        }

        private static void LogInvalidArg(string value, string name)
        {
            WriteLine($"Invalid argument -> \"{value}\" ({name})", ConsoleColor.Red).Wait();
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

        private static void SubInput(string input, out string value1, out string value2)
        {
            int index = input.IndexOf(' ');
            if (index != -1)
            {
                value1 = input[..index].Trim();
                value2 = input[index..].Trim();
            }
            else
            {
                value1 = input.Trim();
                value2 = "";
            }

        }
        private static string[] SplitInput(string input, char splitter = ' ') => input.Split(splitter).Select(c => c.Trim()).ToArray();
    }
}