using Engine.Drawing;
using GlobalTypes.Events;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using static MonoconsoleLib.Monoconsole;
using Monoproject;
using System.Reflection;
using InGame;
using System.Threading.Tasks;
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
                { "drawdebug", static arg => SetProp(arg, nameof(UI.DrawDebug), typeof(UI))},
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
                { "new", static _ => New() },
                { "clear", static _ => Console.Clear() },
                { "color", ColorCommand },
                { "size", SizeCommand },
                { "position", PositionCommand },

                { "f1", static _ => Main.Instance.Exit() },
                { "ram", static _ => Ram() },
                { "rem", static _ => Rem()},
                { "gccollect", static _ => GC.Collect()},
                { "throw", static _ => throw new()},
                { "fps", FpsCommand},

                { "stopwatch", StopwatchCommand },
                { "ruler", RulerCommand },
                { "captwin", static arg => CaptureWindow(string.IsNullOrEmpty(arg) ? 1 : int.Parse(arg)) },
                { "writeinput", static _ => ToggleWriteInput() },
                { "level", LevelCommand },
                { "combo", ComboCommand },
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

            private static void ComboCommand(string arg)
            {
                SubInput(arg, out var subCommand, out var subArg);
                var player = Level.GetObject<Player>();

                switch (subCommand)
                {
                    case "add":

                        player.AddCombo(Combo.NewRandom());

                        break;
                    case "remove":
                        
                        Combo combo = player.Combos[int.Parse(subArg)];
                        player.RemoveCombo(combo);

                        break;
                    default:
                        LogInvalidArg(subCommand, nameof(subCommand));
                        break;
                }
            }
            private static void FpsCommand(string arg)
            {
                float fps = float.Parse(arg);
                Main.Instance.TargetElapsedTime = TimeSpan.FromSeconds(1.0 / fps);
                WriteInfo($"Set to: {fps}");
            }
            private static void StopwatchCommand(string arg)
            {
                SubInput(arg, out var subCommand, out var subarg);

                switch (subCommand)
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
            private static void ToggleWriteInput()
            {
                static void WriteKey(Key key)
                {
                    UI.CustomInfo = key.ToString();
                }

                if (!writeInputEnabled)
                {
                    Input.KeyPressed += WriteKey;
                    writeInputEnabled = true;
                    WriteInfo("Enabled");
                }
                else
                {
                    Input.KeyPressed -= WriteKey;
                    writeInputEnabled = false;
                    WriteInfo("Disabled");
                }
            }

            private static void CaptureWindow(int scaleFactor = 1)
            {
                if (scaleFactor > 5)
                    scaleFactor = 5;
                else if (scaleFactor < 1)
                    scaleFactor = 1;

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

            public static void ColorCommand(string arg)
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
            public static void SizeCommand(string arg)
            {
                SubInput(arg, out var width, out var height);

                if(int.TryParse(width, out int newWidth))
                    Console.WindowWidth = newWidth;
                
                if(int.TryParse(height, out int newHeight))
                    Console.WindowHeight = newHeight;
            }
            public static void PositionCommand(string arg)
            {
                string[] sizes = SplitInput(arg);

                if (sizes.Length > 1)
                {
                    Console.SetWindowPosition(int.Parse(sizes[0]), int.Parse(sizes[1]));
                }
            }

            public static void RulerCommand(string arg)
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
            public static void LevelCommand(string arg)
            {
                SubInput(arg, out var subCommand, out var subArg);

                switch (subCommand)
                {
                    case "new":
                        
                        Level.New();
                        
                        break;
                    case "clear":
                        
                        Level.Clear();
                        
                        break;
                    default:
                        LogInvalidArg(subCommand, nameof(subCommand));
                        break;
                }
            }
        }
        private static class ExecutionPipeline
        {
            public static Dictionary<string, Action<string>> Commands => _commands;

            private readonly static Dictionary<string, Action<string>> _commands = new()
            {
                { "run", Execute },
                { "async", static arg => System.Threading.Tasks.Task.Run(() => ExecuteAsync(arg)) },
                { "unsafe", static arg => ExecuteOnThread(Main.Instance.SyncContext, arg, (ex) => DialogBox.ShowException(ex)) },
                { "on", RunOn },
                { "if", If },
                { "for", For },
                { "while", While },
                { "wait", Wait },
                { "batch", Batch },
                { "batchbegin", static _ => BatchControl() },
                { "batchfile", BatchFileControl },

                { "varset", VarSet },
                { "vardel", VarDel },
                { "varall", static _ => VarAll() },
                { "varinc", static arg => VarCollection[arg]++ },
                { "vardec", static arg => VarCollection[arg]-- },
                { "varrandom", VarRandom },

                { "writel", CustomWriteLine },
                { "write", CustomWrite },
            };

            private static Dictionary<string, int> VarCollection { get; set; } = new();
            private static Dictionary<string, Func<string, bool>> VarConditions { get; set; } = new();

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
                }
            }

            private static void CustomWriteLine(string arg)
            {
                if (TryVarParseArg(arg, out int varValue))
                    WriteLine(varValue, ConsoleColor.Cyan).Wait();
                else
                    WriteLine(arg[(arg.IndexOf('"') + 1)..arg.LastIndexOf('"')], ForeColor).Wait();
            }
            private static void CustomWrite(string arg)
            {
                if (TryVarParseArg(arg, out int varValue))
                    Write(varValue, ConsoleColor.Cyan).Wait();
                else
                    Write(arg[(arg.IndexOf('"') + 1)..arg.LastIndexOf('"')], ForeColor).Wait();
            }

            private static void For(string arg)
            {
                SubInput(arg, out var countArg, out var command);

                int count = VarParseArg(countArg);
                bool isVar = VarCollection.ContainsKey(countArg);

                for (int i = 0; i < count; i++)
                {
                    if (isVar)
                        count = VarCollection[countArg];

                    Execute(command);
                }
            }
            private static void While(string arg)
            {
                (string condition, string action) = arg.Partition(':');

                action = action[1..].Trim();
                while (IsConditionTrue(condition))
                    Execute(action);
            }
            private static void Wait(string arg)
            {
                float time = 0f;
                if (VarCollection.TryGetValue(arg, out int intTime))
                {
                    time = intTime;
                }
                else
                {
                    time = float.Parse(arg);
                }

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
            
            private static void If(string arg)
            {
                int colonIndex = arg.IndexOf(':');
                string condition = arg[..colonIndex].Trim();

                char openBracket = '(';
                char closeBracket = ')';

                bool isconditionTrue = IsConditionTrue(condition);

                int startIfAction = arg.IndexOf(openBracket, colonIndex) + 1;
                int endIfAction = FindClosingBracket(arg, startIfAction - 1, openBracket, closeBracket);
                string ifAction = arg.Substring(startIfAction, endIfAction - startIfAction).Trim();

                if (isconditionTrue)
                    Execute(ifAction);
                else
                {
                    int elseIndex = arg.IndexOf("else", endIfAction);
                    if (elseIndex != -1)
                    {
                        int startElseAction = arg.IndexOf(openBracket, elseIndex) + 1;
                        int endElseAction = FindClosingBracket(arg, startElseAction - 1, openBracket, closeBracket);
                        string elseAction = arg.Substring(startElseAction, endElseAction - startElseAction).Trim();
                        Execute(elseAction);
                    }
                    
                }
            }
            private static bool EvaluateCondition(string condition)
            {
                string[] parts = condition.Split(' ');
                if (parts.Length < 2)
                    throw new ArgumentException("Invalid condition.");

                string varCommand = parts[0];
                string comparison = parts[1];
                
                if (VarConditions.TryGetValue(varCommand, out var func))
                    return func.Invoke(comparison);
                
                throw new ArgumentException($"Can't find variable condition: \"{varCommand}\"");
            }
            private static bool IsConditionTrue(string condition)
            {
                string[] conditions = condition.Split(new[] { "&&", "||" }, StringSplitOptions.None);
                string[] operators = condition.Where(c => c == '&' || c == '|').Select(c => c.ToString()).Distinct().ToArray();

                bool result = EvaluateCondition(conditions[0].Trim());

                for (int i = 1; i < conditions.Length; i++)
                {
                    bool nextCondition = EvaluateCondition(conditions[i].Trim());

                    if (operators[i - 1] == "&")
                    {
                        result &= nextCondition;
                    }
                    else if (operators[i - 1] == "|")
                    {
                        result |= nextCondition;
                    }
                }
                return result;
            }
            private static int FindClosingBracket(string input, int openBracketIndex, char openBracket = '(', char closeBracket = ')')
            {
                int level = 1;

                for (int i = openBracketIndex + 1; i < input.Length; i++)
                {
                    if (input[i] == openBracket) level++;
                    else if (input[i] == closeBracket)
                    {
                        level--;
                        if (level == 0)
                            return i;
                    }
                }

                throw new ArgumentException("No matching closing bracket found.");
            }

            private static void VarSet(string arg)
            {
                SubInput(arg, out string name, out string toSet);
                int intValue;
                try
                {
                    intValue = VarParseArg(toSet);
                }
                catch
                {
                    toSet.Partition(' ', out string first, out string second);
                    if (first == "random")
                    {
                        intValue = new Random().Next(VarParseArg(second));
                    }
                    else
                        throw;
                }
                
                if (VarCollection.ContainsKey(name))
                {
                    VarCollection[name] = intValue;
                }
                else
                {
                    VarCollection.Add(name, intValue);

                    VarConditions.Add($"{name}==", arg => VarCollection[name] == VarParseArg(arg));
                    VarConditions.Add($"{name}!=", arg => VarCollection[name] != VarParseArg(arg));
                    VarConditions.Add($"{name}>=", arg => VarCollection[name] >= VarParseArg(arg));
                    VarConditions.Add($"{name}<=", arg => VarCollection[name] <= VarParseArg(arg));
                    VarConditions.Add($"{name}>", arg => VarCollection[name] > VarParseArg(arg));
                    VarConditions.Add($"{name}<", arg => VarCollection[name] < VarParseArg(arg));
                }
            }
            private static void VarDel(string varname)
            {
                VarConditions.Remove($"{varname}==");
                VarConditions.Remove($"{varname}!=");
                VarConditions.Remove($"{varname}>=");
                VarConditions.Remove($"{varname}<=");
                VarConditions.Remove($"{varname}>");
                VarConditions.Remove($"{varname}<");

                VarCollection.Remove(varname);
            }
            private static void VarWritel(string varname) => WriteLine(VarCollection[varname], ConsoleColor.Cyan);
            private static void VarAll()
            {
                foreach (var item in VarCollection)
                {
                    VarWritel(item.Key);
                }
            }
            private static void VarRandom(string arg)
            {
                SubInput(arg, out string name, out string max);

                VarCollection[name] = new Random().Next(VarParseArg(max));
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
                        BatchReceive -= Append;
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
                        BatchReceive += Append;

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
                        BatchReceive -= Append;
                    }
                }

                IsBatchBegun = true;
                BatchReceive += Append;
            }

            public static int VarParseArg(string arg)
            {
                arg = arg.Trim();
                int num;

                if (VarCollection.TryGetValue(arg, out num))
                    return num;
                else if (int.TryParse(arg, out num))
                    return num;


                throw new ArgumentException($"Variable \"{arg}\" not found and \"{arg}\" can't be cast to int.");
            }
            public static bool TryVarParseArg(string arg, out int num)
            {
                try
                {
                    num = VarParseArg(arg);
                    return true;
                }
                catch
                {
                    num = -1;
                    return false;
                }
            }
        }

        public static bool IsBatchBegun { get; private set; } = false;
        private static Dictionary<string, Action<string>> Commands
            => GeneralExecution.Commands
            .Concat(ExecutionPipeline.Commands)
            .Concat(SettingsCommands.Commands)
            .ToDictionary(pair => pair.Key, pair => pair.Value);

        public static event Action<string> BatchReceive;
        private readonly static Queue<string> batchQueue = new();


        public static void FromString(string input)
        {
            input = input.Trim();

            if (string.IsNullOrEmpty(input))
                return;

            if (IsBatchBegun)
            {
                BatchReceive?.Invoke(input);
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