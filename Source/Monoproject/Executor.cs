using Engine.Drawing;
using GlobalTypes.Events;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using static GlobalTypes.Monoconsole;
using Microsoft.Xna.Framework.Input;
using Monoproject;
using System.Reflection;
using InGame;
using System.Threading.Tasks;

namespace GlobalTypes
{
    public static class Executor
    {
        private static class GeneralCommands
        {
            public static Dictionary<string, Action<string>> Commands => _commands;

            private readonly static Dictionary<string, Action<string>> _commands = new()
            {
                { "new", static _ => New() },
                { "exit", static _ => Close() },
                { "clear", static _ => Console.Clear() },
                { "color", SetColor },
                { "size", SetSize },
                { "position", SetPosition },

                { "f1", static _ => Main.Instance.Exit() },
                { "mem", static _ => Mem() },
                { "throw", static _ => throw new("An exception was thrown using the throw command.")},
                { "ruler", ToggleRuler },
                { "level", LevelControl },
            };
            
            private readonly static Ruler ruler = new();
           
            public static void Mem()
            {
                WriteLine(
                    $"usg: [{GC.GetTotalMemory(false).ToSizeString()}]\n" +
                    $"avg: [{GC.GetTotalMemory(true).ToSizeString()}]\n" +
                    $"---\n" +
                    $"GC0: [{GC.CollectionCount(0)}]\n" +
                    $"GC1: [{GC.CollectionCount(1)}]\n" +
                    $"GC2: [{GC.CollectionCount(2)}]",
                    ConsoleColor.Cyan).Wait();
            }
            public static void SetColor(string arg)
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
                        WriteLine($"Color set to {CurrentColor}", CurrentColor);
                        break;
                    default:
                        if (!Enum.TryParse<ConsoleColor>(arg, true, out var color))
                            WriteLine($"Color \"{arg}\" not found", ConsoleColor.Red);
                        else
                        {
                            if (color == ConsoleColor.Black)
                            {
                                WriteLine("I can't see black text on my black console background. Suffer with me :3");
                                return;
                            }

                            ForeColor = color;
                            WriteLine($"Color set to {color}", CurrentColor);
                        }
                        break;
                }
            }
            public static void SetSize(string arg)
            {
                SubInput(arg, out var width, out var height);

                if(int.TryParse(width, out int newWidth))
                    Console.WindowWidth = newWidth;
                
                if(int.TryParse(height, out int newHeight))
                    Console.WindowHeight = newHeight;
            }
            public static void SetPosition(string arg)
            {
                string[] sizes = SplitInput(arg);

                if (sizes.Length > 1)
                {
                    Console.SetWindowPosition(int.Parse(sizes[0]), int.Parse(sizes[1]));
                }
            }
            public static void ToggleRuler(string arg)
            {
                static void UpdateInfo(GameTime gt)
                {
                    var mouse = FrameState.MouseState;
                    var mousePos = mouse.Position;

                    if (mouse.LeftButton == ButtonState.Pressed)
                        ruler.End = mousePos.ToVector2();

                    if (mouse.RightButton == ButtonState.Pressed)
                        ruler.Start = mousePos.ToVector2();

                    if (mouse.MiddleButton == ButtonState.Pressed) 
                        ruler.InfoPosition = mousePos.ToVector2();
                }

                if(!string.IsNullOrEmpty(arg))
                {
                    SubInput(arg, out string subCommand, out string subArg);

                    static Color? GetColor(string name)
                    {
                        Color? found = (Color?)typeof(Color).GetProperties(BindingFlags.Public | BindingFlags.Static)
                               .FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).GetValue(null);

                        if(found == null)
                            WriteLine($"Color \"{name}\" not found", ConsoleColor.Red).Wait();

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

                            LogMissing(subCommand, nameof(subCommand));
                            
                            break;
                    }

                    return;
                }
                
                if (!ruler.IsActive)
                {
                    ruler.Show();
                    _ = FrameEvents.Update.Append(UpdateInfo);
                    WriteLine("Enabled", ConsoleColor.Cyan);
                }
                else
                {
                    ruler.Hide();
                    FrameEvents.Update.RemoveFirst(UpdateInfo);
                    WriteLine("Disabled", ConsoleColor.Cyan);
                }
            }
            public static void LevelControl(string arg)
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
                    case "wspush":
                        
                        bool pushed = Level.WordStorage.TryPushWord(subArg);
                        WriteLine(pushed, pushed ? ConsoleColor.Green : ConsoleColor.Red).Wait();

                        break;
                    case "wscrnt":
                        
                        WriteLine(Level.WordStorage.CurrentLetter).Wait();
                        
                        break;
                    default:
                        LogMissing(subCommand, nameof(subCommand));
                        break;
                }
            }
        }
        private static class TaskCommands
        {
            public static Dictionary<string, Action<string>> Commands => _commands;
            private static TaskCompletionSource<bool> _tcs = new();

            private readonly static Dictionary<string, Action<string>> _commands = new()
            {
                { "run", static arg => Execute(arg) },
                { "async", static async arg => await ExecuteAsync(arg) },
                { "unsafe", UnsafeExec },
                { "for", For },
                { "wait", Wait },
                { "batch", Batch },
                { "if", If },

                { "varset", VarSet },
                { "var", VarWrite },
                { "vardel", VarDel },
                { "varall", static _ => VarAll() },
                { "varinc", static arg => varCollection[arg]++},
                { "vardec", static arg => varCollection[arg]--},

                { "write", static arg => Write(arg, CurrentColor).Wait() },
                { "writel", static arg => WriteLine(arg, CurrentColor).Wait()},
                { "writeasync", static arg => Write(arg, CurrentColor) },
                { "writelasync", static arg => WriteLine(arg, CurrentColor) },
            };

            private readonly static Dictionary<string, int> varCollection = new();
            private readonly static Dictionary<string, Func<string, bool>> varIfCommands = new();

            private static void For(string arg)
            {
                SubInput(arg, out var countArg, out var command);

                if (varCollection.ContainsKey(countArg))
                {
                    int count = varCollection[countArg];
                    for (int i = 0; i < count; i++)
                    {
                        Execute(command);
                        count = varCollection[countArg];
                    }
                }
                else
                {
                    for (int i = 0; i < int.Parse(countArg); i++)
                        Execute(command);
                }

                
            }
            private static void Wait(string arg)
            {
                Task.Delay(ParseOrVar(arg) * 1000).Wait();
            }
            private static void Batch(string arg)
            {
                SplitInput(arg, ';')
                    .Where(static i => !string.IsNullOrEmpty(i))
                    .ToList()
                    .ForEach(i => Execute(i));
            }
            private static void If(string arg)
            {
                arg = arg.Replace("(", "");
                arg = arg.Replace(")", "");

                string[] splitted = arg.Split(':');
                if (splitted.Length < 2)
                    throw new ArgumentException("Invalid if statement.");


                string[] conditions = splitted[0].Split(new[] { "&&", "||" }, StringSplitOptions.None);
                string[] operators = splitted[0].Where(c => c == '&' || c == '|').Select(c => c.ToString()).Distinct().ToArray();

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

                if (result)
                {
                    string ifCommand = splitted[1].Split(',')[0].Trim();
                    Execute(ifCommand);
                }
                else if (arg.Contains("else", StringComparison.OrdinalIgnoreCase))
                {
                    string elseCommand = splitted[2].Trim();
                    Execute(elseCommand);
                }
            }

            private static bool EvaluateCondition(string condition)
            {
                string[] parts = condition.Split(' ');
                if (parts.Length < 2)
                    throw new ArgumentException("Invalid condition.");

                string varCommand = parts[0];
                string comparison = parts[1];
                
                if (varIfCommands.TryGetValue(varCommand, out var func))
                {
                    return func.Invoke(comparison);
                }
                throw new ArgumentException($"Invalid command");
            }

            private static void VarSet(string arg)
            {
                SubInput(arg, out string name, out string num);

                int intValue = int.Parse(num);

                if (varCollection.ContainsKey(name))
                {
                    varCollection[name] = intValue;
                }
                else
                {
                    varCollection.Add(name, intValue);

                    varIfCommands.Add($"{name}==", arg => varCollection[name] == ParseOrVar(arg));
                    varIfCommands.Add($"{name}!=", arg => varCollection[name] != ParseOrVar(arg));
                    varIfCommands.Add($"{name}>=", arg => varCollection[name] >= ParseOrVar(arg));
                    varIfCommands.Add($"{name}<=", arg => varCollection[name] <= ParseOrVar(arg));
                    varIfCommands.Add($"{name}>", arg => varCollection[name] > ParseOrVar(arg));
                    varIfCommands.Add($"{name}<", arg => varCollection[name] < ParseOrVar(arg));
                }
                WriteLine($"{name} = {num}", ConsoleColor.Cyan).Wait();
            }
            private static void VarDel(string varname)
            {
                varIfCommands.Remove($"{varname}==");
                varIfCommands.Remove($"{varname}!=");
                varIfCommands.Remove($"{varname}>=");
                varIfCommands.Remove($"{varname}<=");
                varIfCommands.Remove($"{varname}>");
                varIfCommands.Remove($"{varname}<");

                varCollection.Remove(varname);
            }
            private static void VarWrite(string varname) => WriteLine(varCollection[varname], ConsoleColor.Cyan);
            private static void VarAll()
            {
                foreach (var item in varCollection)
                {
                    VarWrite(item.Key);
                }
            }
            private static int ParseOrVar(string arg)
            {
                if(varCollection.TryGetValue(arg, out int num))
                    return num;

                return int.Parse(arg);
            }
        }

        private static Dictionary<string, Action<string>> TotalCommands
            => GeneralCommands.Commands
            .Concat(TaskCommands.Commands)
            .ToDictionary(pair => pair.Key, pair => pair.Value);

        
        public static void FromString(string input)
        {
            input = input.ToLower().Trim();

            if (string.IsNullOrEmpty(input))
                return;

            SubInput(input, out string command, out string arg);

            if (!TotalCommands.TryGetValue(command, out var action))
            {
                WriteLine($"Unexpected command. Did you mean \"{FindClosestWord(input, TotalCommands.Keys.ToArray())}\"?", ConsoleColor.Red).Wait();
                return;
            }

            action?.Invoke(arg);
        }
        private static void LogMissing(string value, string name)
        {
            WriteLine($"Missing argument -> {value} ({name})", ConsoleColor.Red).Wait();
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
            if (input.Contains(' '))
            {
                value1 = input[..input.IndexOf(' ')].Trim();
                value2 = input[input.IndexOf(' ')..].Trim();
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