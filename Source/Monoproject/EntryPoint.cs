using Monoproject;
using System;

Main main = null;
try
{
    main = new Main();
    main.Run();
}
catch (Exception e)
{
    DialogBox.ShowException(e);
}
finally
{
    main?.Dispose();
}