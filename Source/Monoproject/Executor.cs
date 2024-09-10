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
        private static class GeneralExecution
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
                { "ram", static _ => Ram() },
                { "rem", static _ => Rem()},
                { "throw", static _ => throw new()},
                { "ruler", ToggleRuler },
                { "level", LevelControl },
            };
            
            private readonly static Ruler ruler = new();
           
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
                        
                        bool pushed = Level.WordStorage.Push(subArg);
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
        private static class ExecutionPipeline
        {
            public static Dictionary<string, Action<string>> Commands => _commands;

            private readonly static Dictionary<string, Action<string>> _commands = new()
            {
                { "run", Execute },
                { "async", static arg => Task.Run(() => ExecuteAsync(arg)) },
                { "unsafe", ExecuteUnsafe },
                { "if", If },
                { "for", For },
                { "while", While },
                { "wait", Wait },
                { "batch", Batch },
                { "batchbegin", static _ => BatchBegin()},

                { "varset", VarSet },
                { "vardel", VarDel },
                { "varall", static _ => VarAll() },
                { "varinc", static arg => VarCollection[arg]++},
                { "vardec", static arg => VarCollection[arg]--},
                { "varrandom", VarRandom },

                { "writel", CustomWriteLine },
                { "write", CustomWrite },
            };

            private static Dictionary<string, int> VarCollection { get; set; } = new();
            private static Dictionary<string, Func<string, bool>> VarConditions { get; set; } = new();
            
            private static void CustomWriteLine(string arg)
            {
                if (TryParseArg(arg, out int varValue))
                    WriteLine(varValue, ConsoleColor.Cyan).Wait();
                else
                    WriteLine(arg[(arg.IndexOf('"') + 1)..arg.LastIndexOf('"')], CurrentColor).Wait();
            }
            private static void CustomWrite(string arg)
            {
                if (TryParseArg(arg, out int varValue))
                    Write(varValue, ConsoleColor.Cyan).Wait();
                else
                    Write(arg[(arg.IndexOf('"') + 1)..arg.LastIndexOf('"')], CurrentColor).Wait();
            }

            private static void For(string arg)
            {
                SubInput(arg, out var countArg, out var command);

                int count = ParseArg(countArg);
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

                Task.Delay((int)(1000f * time)).Wait();
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
                    intValue = ParseArg(toSet);
                }
                catch
                {
                    toSet.Partition(' ', out string first, out string second);
                    if (first == "random")
                    {
                        intValue = new Random().Next(ParseArg(second));
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

                    VarConditions.Add($"{name}==", arg => VarCollection[name] == ParseArg(arg));
                    VarConditions.Add($"{name}!=", arg => VarCollection[name] != ParseArg(arg));
                    VarConditions.Add($"{name}>=", arg => VarCollection[name] >= ParseArg(arg));
                    VarConditions.Add($"{name}<=", arg => VarCollection[name] <= ParseArg(arg));
                    VarConditions.Add($"{name}>", arg => VarCollection[name] > ParseArg(arg));
                    VarConditions.Add($"{name}<", arg => VarCollection[name] < ParseArg(arg));
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

                VarCollection[name] = new Random().Next(ParseArg(max));
            }

            public static int ParseArg(string arg)
            {
                arg = arg.Trim();
                int num;

                if (VarCollection.TryGetValue(arg, out num))
                    return num;
                else if (int.TryParse(arg, out num))
                    return num;


                throw new ArgumentException($"Variable \"{arg}\" not found and \"{arg}\" can't be cast to int.");
            }
            public static bool TryParseArg(string arg, out int num)
            {
                try
                {
                    num = ParseArg(arg);
                    return true;
                }
                catch
                {
                    num = -1;
                    return false;
                }
            }
        }

        private static Dictionary<string, Action<string>> Commands
            => GeneralExecution.Commands
            .Concat(ExecutionPipeline.Commands)
            .ToDictionary(pair => pair.Key, pair => pair.Value);

        private readonly static Queue<string> _batchSeq = new();
        public static bool IsBatchBegun { get; private set; } = false;

        public static void FromString(string input)
        {
            input = input.Trim();

            if (string.IsNullOrEmpty(input))
                return;

            if (IsBatchBegun)
            {
                BatchReceive(input);
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

        public static void BatchBegin() => IsBatchBegun = true;
        public static void BatchReceive(string command)
        {
            if (command == "batchend")
            {
                BatchEnd();
                return;
            }

            _batchSeq.Enqueue(command);
        }
        public static void BatchEnd()
        {
            IsBatchBegun = false;

            while (_batchSeq.Count > 0)
            {
                FromString(_batchSeq.Dequeue());
            }
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