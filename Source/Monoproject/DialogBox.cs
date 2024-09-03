using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace Monoproject
{
    public static class DialogBox
    {
        public static void ShowException(Exception e, bool exit = false)
        {
            Main main = Main.Instance;

            if (main != null)
                main.IsMouseVisible = true;

            string msg = 
                $"{GetFrom(e)}\n\n" +
                $"{GetMessage(e)}\n\n" +
                $"{e.StackTrace}";

            ShowCopyable(msg, GetCaption(e), MessageBoxIcon.Error);

            if (exit)
                main?.Exit();
            else if (main != null)
                main.IsMouseVisible = false;
        }
        public static void ShowCopyable(string msg, string caption, MessageBoxIcon icon = MessageBoxIcon.None)
        {
            DialogResult result = 
                MessageBox.Show(
                    $"{msg}\n\n(Click OK if you want to copy this info)",
                    caption, 
                    MessageBoxButtons.OKCancel,
                    icon);

            if (result == DialogResult.OK)
            {
                Thread thread = new(() => Clipboard.SetText(msg));
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
                thread.Join();
            }
        }

        private static string GetCaption(Exception ex)
        {
            if(ex is AggregateException aggrEx)
            {
                if (aggrEx.InnerExceptions.Count == 1)
                    return aggrEx.InnerExceptions[0].GetType().Name;
            }

            return ex.GetType().Name;
        }
        private static string GetMessage(Exception ex)
        {
            if (ex is AggregateException aggrEx)
            {
                if (aggrEx.InnerExceptions.Count == 1)
                    return aggrEx.InnerExceptions[0].Message;
            }

            return ex.Message;
        }
        private static string GetFrom(Exception ex)
        {
            if (ex is AggregateException aggrEx)
            {
                if (aggrEx.InnerExceptions.Count == 1)
                {
                    var first = aggrEx.InnerExceptions.First();
                    return $"{first.TargetSite.Name}";
                }
            }

            MethodBase method = ex.TargetSite;

            return $"{method.DeclaringType.Name}.{method.Name}({string.Join(',', method.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"))})";
        }
    }
}