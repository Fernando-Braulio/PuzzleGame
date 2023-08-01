using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Maui.Controls;

namespace PuzzleGame.Views
{
    public partial class PuzzleGamePage : ContentPage
    {
        public int GridSize
        {
            get { return piecesCount; }
        }

        public int MovesLimit
        {
            get { return currentLevel * 100; }
        }

        private int currentLevel = 1;
        private int initialPiecesCount = 2;
        private int piecesCount; // Variável que armazenará a quantidade atual de peças no tabuleiro
        private CancellationTokenSource cancellationTokenSource;
        //private int gridSize { get; set; } // Tamanho do tabuleiro de quebra-cabeça
        private Button[,] puzzleButtons;
        private int movesCount = 0;
        private int timeInSeconds = 0;
        private bool isGameCompleted = false;
        private Stopwatch stopwatch = new Stopwatch();
        private Timer timer;
        private int[,] puzzleBoard = new int[,]
            {
            { 1, 2, 3 },
            { 4, 5, 6 },
            { 7, 8, 0 }, // O valor 0 representa o espaço vazio onde as peças podem ser movidas
            };

        public PuzzleGamePage()
        {
            InitializeComponent();
            //CreatePuzzleGrid();
            //StartTimer();

            InitializeGame();
            StartTimer();
            UpdateLabelMovesLimit();
        }

        private void InitializeGame()
        {
            piecesCount = initialPiecesCount + currentLevel - 1; // Defina a quantidade de peças para o nível atual
            //gridSize = (int)Math.Sqrt(piecesCount); // Tamanho do grid com base na quantidade de peças do nível atual
            Console.WriteLine($"46 ---- GridSize{GridSize} ---- currentLevel{currentLevel} ---- initialPiecesCount{initialPiecesCount} ---- piecesCount{piecesCount}");

            // Inicialize o tabuleiro do quebra-cabeça
            InitializePuzzleBoard();

            // Crie a grade de botões do quebra-cabeça
            CreatePuzzleGrid();

            // Inicialize o contador de movimentos e o cronômetro
            movesCount = 0;
            UpdateLabelMovimentos();
            //StartTimer();

            // Atualize a exibição do tabuleiro
            //UpdatePuzzleGrid();

            UpdateLabelNivel(); // Adicione este método para exibir o nível atual na tela
        }

        private void InitializePuzzleBoard()
        {
            // Crie o tabuleiro com os números embaralhados
            List<int> numbers = new List<int>();
            for (int i = 0; i < GridSize * GridSize; i++)
            {
                numbers.Add(i);
            }
            ShuffleList(numbers); // Embaralhar a lista usando o método personalizado

            // Preencha a matriz do tabuleiro com os números embaralhados
            puzzleBoard = new int[GridSize, GridSize];
            int index = 0;
            for (int i = 0; i < GridSize; i++)
            {
                for (int j = 0; j < GridSize; j++)
                {
                    puzzleBoard[i, j] = numbers[index];
                    index++;
                }
            }
        }

        private void CreatePuzzleGrid()
        {
            // Crie a matriz de botões para representar as peças do quebra-cabeça
            puzzleButtons = new Button[GridSize, GridSize];
            Console.WriteLine($"97 ---- GridSize{GridSize} ---- currentLevel{currentLevel} ---- initialPiecesCount{initialPiecesCount} ---- piecesCount{piecesCount}");

            puzzleGrid.Children.Clear();

            // Limpe as RowDefinitions e ColumnDefinitions existentes
            puzzleGrid.RowDefinitions.Clear();
            puzzleGrid.ColumnDefinitions.Clear();

            for (int i = 0; i < GridSize; i++)
            {
                puzzleGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
                puzzleGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            }

            // Calcule o tamanho desejado dos botões com base na quantidade de peças no tabuleiro e no tamanho da tela
            double buttonSize = CalculateButtonSize();

            // Atualize os botões existentes no grid e crie novos botões se necessário
            for (int i = 0; i < GridSize; i++)
            {
                for (int j = 0; j < GridSize; j++)
                {
                    int pieceValue = puzzleBoard[i, j];
                    Button button = puzzleButtons[i, j]; // Obtenha o botão existente do grid

                    if (button == null)
                    {
                        button = new Button
                        {
                            FontSize = 24,
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
            for (int i = 0; i < GridSize; i++)
            {
                for (int j = 0; j < GridSize; j++)
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
                for (int i = 0; i < GridSize; i++)
                {
                    for (int j = 0; j < GridSize; j++)
                    {
                        int pieceValue = puzzleBoard[i, j];
                        puzzleButtons[i, j].Text = (pieceValue == 0) ? string.Empty : pieceValue.ToString();
                    }
                }

                // Incremente o contador de movimentos
                movesCount++;

                // Atualize a label de contagem de movimentos na tela
                UpdateLabelMovimentos();

                // Verifique se o jogo foi concluído após o movimento
                CheckGameCompleted();

                if (!isGameCompleted && movesCount >= MovesLimit)
                {
                    await DisplayAlert("Gamer Over", $"Você nao conseguiu concluir no limite de movimentos.", "OK");
                    ResetTimer();
                    currentLevel = 1;
                    initialPiecesCount = 2;
                    piecesCount = initialPiecesCount + currentLevel - 1;
                    InitializeGame();
                    StartTimer();
                    UpdateLabelMovesLimit();
                }
            }
        }

        private void UpdateLabelMovimentos()
        {
            movesLabel.Text = $"Movimentos: {movesCount}";
        }

        private int FindEmptyPieceRow()
        {
            for (int i = 0; i < GridSize; i++)
            {
                for (int j = 0; j < GridSize; j++)
                {
                    if (puzzleBoard[i, j] == 0)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        private int FindEmptyPieceColumn()
        {
            for (int i = 0; i < GridSize; i++)
            {
                for (int j = 0; j < GridSize; j++)
                {
                    if (puzzleBoard[i, j] == 0)
                    {
                        return j;
                    }
                }
            }
            return -1;
        }

        private void UpdatePuzzleGrid()
        {
            // Atualize a exibição do tabuleiro após cada movimento.
            for (int i = 0; i < GridSize; i++)
            {
                for (int j = 0; j < GridSize; j++)
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

            for (int i = 0; i < GridSize; i++)
            {
                for (int j = 0; j < GridSize; j++)
                {
                    int pieceValue = puzzleBoard[i, j];
                    // Verifique se todas as peças estão nas posições corretas, exceto a última peça (peça vazia)
                    if (i == GridSize - 1 && j == GridSize - 1)
                    {
                        if (pieceValue != 0)
                        {
                            isGameCompleted = false;
                            break;
                        }
                    }
                    else if (pieceValue != i * GridSize + j + 1)
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
                await DisplayAlert("Parabéns!", $"Você concluiu o nível {currentLevel} em {movesCount} movimentos.", "OK");

                // Aumente o nível e a quantidade de peças para o próximo jogo
                currentLevel++;
                piecesCount = initialPiecesCount + currentLevel - 1; // Aumente a quantidade de peças para o próximo nível
                //gridSize = (int)Math.Sqrt(piecesCount); // Defina o tamanho do grid com base na quantidade de peças
                Console.WriteLine($"281 ---- GridSize{GridSize} ---- currentLevel{currentLevel} ---- initialPiecesCount{initialPiecesCount} ---- piecesCount{piecesCount}");
                ResetTimer();
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
            levelLabel.Text = $"Nível: {currentLevel}";
        }

        private void UpdateLabelMovesLimit()
        {
            movesLimit.Text = $"Limite de Movimentos: {MovesLimit}";
        }

        void TesteClick(Object sender, EventArgs e)
        {
            // Aumente o nível e a quantidade de peças para o próximo jogo
            currentLevel++;
            piecesCount = initialPiecesCount + currentLevel - 1; // Aumente a quantidade de peças para o próximo nível

            ResetTimer();
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
            int totalCells = GridSize * GridSize;
            double maxButtonSize = Math.Min(screenWidth, screenHeight) / (GridSize + 1); // Divida por (GridSize + 1) para evitar que os botões se sobreponham

            // Defina o tamanho do botão proporcionalmente ao nível atual
            double buttonSize = maxButtonSize * (1.0 - currentLevel * 0.1); // Diminua 10% do tamanho do botão para cada nível

            // Defina um limite superior para o tamanho do botão (por exemplo, 25% do tamanho da célula)
            double maxButtonSizeLimit = maxButtonSize * 0.25;
            buttonSize = Math.Min(buttonSize, maxButtonSizeLimit);

            // Certifique-se de que o tamanho do botão não ultrapasse um tamanho mínimo desejado
            double minButtonSize = 50;
            return Math.Max(buttonSize, minButtonSize);
        }

    }
}