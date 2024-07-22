Monoproject.Main game = null;
try
{
    game = new Monoproject.Main();
    game.Run();
}
catch (System.Exception e)
{
    if(game != null)
        game.IsMouseVisible = true;
    
    System.Windows.Forms.MessageBox.Show($"{e.Message}\n\nStack trace:\n{e.StackTrace}", "Critical error!");
}
finally
{
    game?.Dispose();
}