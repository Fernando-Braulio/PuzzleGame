using System.Text.Json;
using PuzzleGame.Models;
using PuzzleGame.Views;

namespace PuzzleGame;

public partial class App : Application
{
    public readonly string NameKeyGameState = "GameState";
    public readonly string NameKeyIsFirstRun = "IsFirstRun";

    public static PuzzleGameState PuzzleState { get; set; }

    public App()
	{
		InitializeComponent();
        LoadGameState();

        MainPage = new NavigationPage(new PuzzleGamePage());
    }

    protected override void OnStart()
    {
        base.OnStart();

        if (!Preferences.ContainsKey(NameKeyIsFirstRun))
        {
            Preferences.Set(NameKeyIsFirstRun, false);
            MainPage = new NavigationPage(new OnboardingPage());
        }
    }

    protected override void OnSleep()
    {
        // Salvar o estado do jogo quando o aplicativo for suspenso (pressionar botão Início ou alternar para outro aplicativo)
        SaveGameState();
    }

    protected override void OnResume()
    {
        // Carregar o estado do jogo quando o aplicativo for retomado após ser suspenso
        LoadGameState();
    }

    #region global methods
    public void SaveGameState()
    {
        var jsonString = JsonSerializer.Serialize(PuzzleState);
        Preferences.Set(NameKeyGameState, jsonString);
    }

    public void LoadGameState()
    {
        if (!Preferences.ContainsKey(NameKeyGameState))
            PuzzleState = new PuzzleGameState();
        else
            GetGameState();
    }

    private void GetGameState()
    {
        var jsonString = Preferences.Get(NameKeyGameState, null);
        if (!string.IsNullOrEmpty(jsonString))
        {
            var gameState = JsonSerializer.Deserialize<PuzzleGameState>(jsonString);
            if (gameState != null)
            {
                PuzzleState = new PuzzleGameState()
                {
                    PiecesCount = gameState.PiecesCount,
                    InitialPiecesCount = gameState.InitialPiecesCount,
                    CurrentLevel = gameState.CurrentLevel,
                    GridSize = gameState.GridSize,
                    PuzzleBoard = gameState.PuzzleBoard,
                    MovesCount = gameState.MovesCount,
                    MovesLimit = gameState.MovesLimit,
                };
            }
            else
            {
                PuzzleState = new PuzzleGameState();
            }
        }
    }
    #endregion
}
