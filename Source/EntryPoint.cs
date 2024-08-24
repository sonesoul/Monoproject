using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

Monoproject.Main game = null;
try
{
    game = new Monoproject.Main();
    game.Run();
}
catch (Exception e)
{
    if (game != null)
        game.IsMouseVisible = true;

    string msg = $"{e.Message}\n\nStack trace:\n{e.StackTrace}";

    if(MessageBox.Show($"{msg}\n\nClick OK if you want to copy this info", "Critical error", MessageBoxButtons.OKCancel) == DialogResult.OK)
    {
        Thread thread = new(() => Clipboard.SetText("[Monoproject exception info (Main handle)]\n\n" + msg));
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
    }
}
finally
{
    game?.Dispose();
}