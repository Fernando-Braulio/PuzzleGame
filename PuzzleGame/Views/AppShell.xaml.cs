namespace PuzzleGame.Views;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
    }

    private async void OnClickCreateBy(Object sender, TappedEventArgs e)
    {
        await Launcher.TryOpenAsync(e.Parameter.ToString());
    }
}