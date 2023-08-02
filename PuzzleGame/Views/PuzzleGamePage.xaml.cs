using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Maui.Controls;

namespace PuzzleGame.Views
{
    public partial class PuzzleGamePage : ContentPage
    {
        private readonly double fontScaleFactor = 0.3;

        private CancellationTokenSource cancellationTokenSource;
        private Button[,] puzzleButtons;
        private bool isGameCompleted = false;
        private int[,] puzzleBoard;

        public PuzzleGamePage()
        {
            InitializeComponent();

            //App.PuzzleState.MovesCount = 0;
            InitializeGame();
            StartTimer();
            UpdateLabelMovesLimit();
        }

        protected override void OnDisappearing()
        {
            int gridSize = puzzleBoard.GetLength(0);
            App.PuzzleState.PuzzleBoard = new int[gridSize * gridSize];

            int index = 0;
            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    App.PuzzleState.PuzzleBoard[index] = puzzleBoard[i, j];
                    index++;
                }
            }

            base.OnDisappearing();
        }

        private void InitializeGame()
        {
            // Defina a quantidade de peças para o nível atual
            App.PuzzleState.PiecesCount = App.PuzzleState.InitialPiecesCount + App.PuzzleState.CurrentLevel - 1;

            puzzleBoard = new int[App.PuzzleState.GridSize, App.PuzzleState.GridSize];

            // Inicialize o tabuleiro do quebra-cabeça
            InitializePuzzleBoard();

            // Crie a grade de botões do quebra-cabeça
            CreatePuzzleGrid();

            // Inicialize o contador de movimentos e o cronômetro
            //movesCount = 0;
            UpdateLabelMovimentos();
            //StartTimer();

            // Atualize a exibição do tabuleiro
            //UpdatePuzzleGrid();

            UpdateLabelNivel(); // Adicione este método para exibir o nível atual na tela
        }

        private void InitializePuzzleBoard()
        {
            // Determine o número total de botões disponíveis no tabuleiro
            int totalButtons = App.PuzzleState.GridSize * App.PuzzleState.GridSize;

            // Crie uma lista de números de 1 até o total de botões disponíveis
            List<int> numbers = Enumerable.Range(1, totalButtons - 1).ToList();
            numbers.Add(0); // Adicione o valor 0 (espaço vazio) à lista

            // Embaralhe a lista usando o algoritmo de Fisher-Yates
            Random random = new Random();
            int n = numbers.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                int value = numbers[k];
                numbers[k] = numbers[n];
                numbers[n] = value;
            }

            // Preencha a matriz do tabuleiro com os números embaralhados
            int index = 0;
            for (int i = 0; i < App.PuzzleState.GridSize; i++)
            {
                for (int j = 0; j < App.PuzzleState.GridSize; j++)
                {
                    puzzleBoard[i, j] = numbers[index];
                    index++;
                }
            }
        }

        private async void CreatePuzzleGrid()
        {
            // Calcule o tamanho desejado dos botões com base na quantidade de peças no tabuleiro e no tamanho da tela
            double buttonSize = CalculateButtonSize();

            // Obtenha a largura e altura da tela em pixels
            //var displayInfo = DeviceDisplay.MainDisplayInfo;
            double screenWidth = puzzleGrid.Width;
            double screenHeight = puzzleGrid.Height;

            // Calcule a quantidade máxima de botões que podem caber horizontalmente e verticalmente na tela
            int maxButtonsHorizontal = (int)(screenWidth / buttonSize);
            int maxButtonsVertical = (int)(screenHeight / buttonSize);

            // Determine o limite máximo de botões com base na quantidade máxima horizontal e verticalmente
            int maxTotalButtons = maxButtonsHorizontal * maxButtonsVertical;

            // Verifique se o número total de botões no tabuleiro excede o limite máximo
            int currentTotalButtons = App.PuzzleState.GridSize * App.PuzzleState.GridSize;

            if (maxTotalButtons != 0 && currentTotalButtons >= (maxTotalButtons - 5))
            {
                // Já excedeu o limite máximo de botões, não adicione mais botões
                var ret = await DisplayAlert("FIM", $"Já exedeu o limite máximo de botoes para o tamanho da tela", "Reiniciar", "Calcelar");

                if (ret)
                {
                    ResetGame();
                }

                return;
            }

            // Crie a matriz de botões para representar as peças do quebra-cabeça
            puzzleButtons = new Button[App.PuzzleState.GridSize, App.PuzzleState.GridSize];
            
            puzzleGrid.Children.Clear();

            // Limpe as RowDefinitions e ColumnDefinitions existentes
            puzzleGrid.RowDefinitions.Clear();
            puzzleGrid.ColumnDefinitions.Clear();

            for (int i = 0; i < App.PuzzleState.GridSize; i++)
            {
                puzzleGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
                puzzleGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            }

            // Atualize os botões existentes no grid e crie novos botões se necessário
            for (int i = 0; i < App.PuzzleState.GridSize; i++)
            {
                for (int j = 0; j < App.PuzzleState.GridSize; j++)
                {
                    int pieceValue = puzzleBoard[i, j];
                    Button button = puzzleButtons[i, j]; // Obtenha o botão existente do grid

                    if (button == null)
                    {
                        button = new Button
                        {
                            FontSize = buttonSize * fontScaleFactor,
                            HeightRequest = buttonSize,
                            WidthRequest = buttonSize,
                        };

                        button.Clicked += (sender, e) =>
                        {
                            int row = Grid.GetRow(button);
                            int col = Grid.GetColumn(button);
                            MovePiece(row, col);
                        };

                        puzzleButtons[i, j] = button;
                    }
                    else
                    {
                        // Atualize o tamanho do botão existente
                        button.HeightRequest = buttonSize;
                        button.WidthRequest = buttonSize;
                        button.FontSize = buttonSize * fontScaleFactor;
                    }

                    button.Text = (pieceValue == 0) ? string.Empty : pieceValue.ToString();
                    puzzleButtons[i, j] = button;
                    puzzleGrid.Children.Add(button);
                    Grid.SetRow(button, i); // Define a linha do botão na Grid
                    Grid.SetColumn(button, j); // Define a coluna do botão na Grid
                }
            }
        }

        private async void MovePiece(int row, int col)
        {
            // Encontre a posição da peça vazia (valor 0) no tabuleiro
            int emptyRow = -1;
            int emptyCol = -1;
            for (int i = 0; i < App.PuzzleState.GridSize; i++)
            {
                for (int j = 0; j < App.PuzzleState.GridSize; j++)
                {
                    if (puzzleBoard[i, j] == 0)
                    {
                        emptyRow = i;
                        emptyCol = j;
                        break;
                    }
                }
            }

            // Verifique se a peça selecionada está adjacente à peça vazia (ou seja, se está imediatamente acima, abaixo, à esquerda ou à direita da peça vazia)
            if ((row == emptyRow && Math.Abs(col - emptyCol) == 1) || (col == emptyCol && Math.Abs(row - emptyRow) == 1))
            {
                // Se a peça selecionada for adjacente à peça vazia, troque as posições das peças selecionada e vazia no tabuleiro
                puzzleBoard[emptyRow, emptyCol] = puzzleBoard[row, col];
                puzzleBoard[row, col] = 0;

                // Atualize a exibição dos botões na Grid com base no estado atual do tabuleiro
                for (int i = 0; i < App.PuzzleState.GridSize; i++)
                {
                    for (int j = 0; j < App.PuzzleState.GridSize; j++)
                    {
                        int pieceValue = puzzleBoard[i, j];
                        puzzleButtons[i, j].Text = (pieceValue == 0) ? string.Empty : pieceValue.ToString();
                    }
                }

                // Incremente o contador de movimentos
                App.PuzzleState.MovesCount++;

                // Atualize a label de contagem de movimentos na tela
                UpdateLabelMovimentos();

                // Verifique se o jogo foi concluído após o movimento
                CheckGameCompleted();

                if (!isGameCompleted && App.PuzzleState.MovesCount >= App.PuzzleState.MovesLimit)
                {
                    await DisplayAlert("Gamer Over", $"Você nao conseguiu concluir no limite de movimentos.", "OK");
                    ResetGame();
                }
            }
        }

        private void ResetGame()
        {
            ResetTimer();
            App.PuzzleState.CurrentLevel = 1;
            App.PuzzleState.InitialPiecesCount = 2;
            App.PuzzleState.PiecesCount = App.PuzzleState.InitialPiecesCount + App.PuzzleState.CurrentLevel - 1;
            App.PuzzleState.MovesCount = 0;
            InitializeGame();
            StartTimer();
            UpdateLabelMovesLimit();
        }

        private void UpdateLabelMovimentos()
        {
            movesLabel.Text = $"Movimentos: {App.PuzzleState.MovesCount}";
        }

        private void UpdatePuzzleGrid()
        {
            // Atualize a exibição do tabuleiro após cada movimento.
            for (int i = 0; i < App.PuzzleState.GridSize; i++)
            {
                for (int j = 0; j < App.PuzzleState.GridSize; j++)
                {
                    int pieceValue = puzzleBoard[i, j];
                    puzzleButtons[i, j].Text = (pieceValue == 0) ? string.Empty : pieceValue.ToString();
                }
            }

            // Atualize o contador de movimentos e verifique se o jogo foi concluído
            UpdateLabelMovimentos();
            CheckGameCompleted();
        }

        private async void CheckGameCompleted()
        {
            bool isGameCompleted = true;

            for (int i = 0; i < App.PuzzleState.GridSize; i++)
            {
                for (int j = 0; j < App.PuzzleState.GridSize; j++)
                {
                    int pieceValue = puzzleBoard[i, j];
                    // Verifique se todas as peças estão nas posições corretas, exceto a última peça (peça vazia)
                    if (i == App.PuzzleState.GridSize - 1 && j == App.PuzzleState.GridSize - 1)
                    {
                        if (pieceValue != 0)
                        {
                            isGameCompleted = false;
                            break;
                        }
                    }
                    else if (pieceValue != i * App.PuzzleState.GridSize + j + 1)
                    {
                        isGameCompleted = false;
                        break;
                    }
                }
            }

            if (isGameCompleted)
            {
                StopTimer();

                // O jogo foi concluído, exiba uma mensagem ao usuário
                await DisplayAlert("Parabéns!", $"Você concluiu o nível {App.PuzzleState.CurrentLevel} em {App.PuzzleState.MovesCount} movimentos.", "OK");

                // Aumente o nível e a quantidade de peças para o próximo jogo
                App.PuzzleState.CurrentLevel++;
                App.PuzzleState.PiecesCount = App.PuzzleState.InitialPiecesCount + App.PuzzleState.CurrentLevel - 1; // Aumente a quantidade de peças para o próximo nível

                ResetTimer();
                App.PuzzleState.MovesCount = 0;
                InitializeGame();
                StartTimer();
                UpdateLabelMovesLimit();
            }
        }

        private void StartTimer()
        {
            cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            Task.Run(async () =>
            {
                int seconds = 0;
                while (true)
                {
                    await Task.Delay(1000, cancellationToken);
                    seconds++;
                    //Device.BeginInvokeOnMainThread(() =>
                    //{
                        //timeLabel.Text = $"Tempo: {TimeSpan.FromSeconds(seconds).ToString("g")}";
                    //});
                }
            }, cancellationToken);
        }

        private void ResetTimer()
        {
            cancellationTokenSource?.Cancel();
            //timeLabel.Text = "Tempo: 00:00:00";
        }

        private void StopTimer()
        {
            cancellationTokenSource?.Cancel();
            //stopwatch.Stop();
            //timer?.Dispose();
        }

        private void ShuffleList<T>(List<T> list)
        {
            Random rng = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        private void RestartButton_Clicked(object sender, EventArgs e)
        {
            //ResetTimer();
            InitializeGame();
        }

        private void UpdateLabelNivel()
        {
            levelLabel.Text = $"Level: {App.PuzzleState.CurrentLevel}";
        }

        private void UpdateLabelMovesLimit()
        {
            movesLimit.Text = $"Movimentos máx: {App.PuzzleState.MovesLimit}";
        }

        void TesteClick(Object sender, EventArgs e)
        {
            // Aumente o nível e a quantidade de peças para o próximo jogo
            App.PuzzleState.CurrentLevel++;
            App.PuzzleState.PiecesCount = App.PuzzleState.InitialPiecesCount + App.PuzzleState.CurrentLevel - 1; // Aumente a quantidade de peças para o próximo nível

            ResetTimer();
            App.PuzzleState.MovesCount = 0;
            InitializeGame();
            StartTimer();
            UpdateLabelMovesLimit();
        }

        private double CalculateButtonSize()
        {
            // Obtenha o tamanho da tela em pixels
            double screenWidth = DeviceDisplay.MainDisplayInfo.Width;
            double screenHeight = DeviceDisplay.MainDisplayInfo.Height;

            // Calcule o tamanho máximo desejado do botão com base na quantidade de células no tabuleiro
            int totalCells = App.PuzzleState.GridSize * App.PuzzleState.GridSize;
            double maxButtonSize = Math.Min(screenWidth, screenHeight) / (App.PuzzleState.GridSize + 1); // Divida por (GridSize + 1) para evitar que os botões se sobreponham

            // Defina o tamanho do botão proporcionalmente ao nível atual
            double buttonSize = maxButtonSize * (1.0 - App.PuzzleState.CurrentLevel * 0.1); // Diminua 10% do tamanho do botão para cada nível

            // Defina um limite superior para o tamanho do botão (por exemplo, 25% do tamanho da célula)
            double maxButtonSizeLimit = maxButtonSize * 0.25;
            buttonSize = Math.Min(buttonSize, maxButtonSizeLimit);

            // Certifique-se de que o tamanho do botão não ultrapasse um tamanho mínimo desejado
            double minButtonSize = 50;
            return Math.Max(buttonSize, minButtonSize);
        }

        // Evento de clique do botão "Embaralhar"
        private void ShuffleButton_Clicked(object sender, EventArgs e)
        {
            ShufflePuzzle(); // Chame o método para embaralhar os números do quebra-cabeça
            UpdatePuzzleGrid(); // Atualize a exibição do quebra-cabeça na tela
        }

        // Método para embaralhar os números do quebra-cabeça
        private void ShufflePuzzle()
        {
            List<int> numbers = new List<int>();
            for (int i = 0; i < App.PuzzleState.GridSize * App.PuzzleState.GridSize; i++)
            {
                numbers.Add(i);
            }
            ShuffleList(numbers);

            puzzleBoard = new int[App.PuzzleState.GridSize, App.PuzzleState.GridSize];
            int index = 0;
            for (int i = 0; i < App.PuzzleState.GridSize; i++)
            {
                for (int j = 0; j < App.PuzzleState.GridSize; j++)
                {
                    puzzleBoard[i, j] = numbers[index];
                    index++;
                }
            }
        }

        //private async void OnMenuClicked(Object sender, EventArgs e)
        //{
        //    await Navigation.PushModalAsync(new MenuPage());
        //}
    }
}