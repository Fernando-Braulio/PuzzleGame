using PuzzleGame.Views;

namespace PuzzleGame;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();

        MainPage = new NavigationPage(new PuzzleGamePage());
    }

    protected override void OnStart()
    {
        base.OnStart();

        if (!Preferences.ContainsKey("IsFirstRun"))
        {
            // O usuário está acessando o aplicativo pela primeira vez
            // Defina a MainPage para a página de tutorial ou informações
            Preferences.Set("IsFirstRun", false);
            MainPage = new NavigationPage(new TutorialPage());
        }
    }
}
