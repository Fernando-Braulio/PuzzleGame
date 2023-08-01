using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Maui.Controls;

namespace PuzzleGame.Views
{
    public partial class PuzzleGamePage : ContentPage
    {
        private int gridSize = 3; // Tamanho do tabuleiro de quebra-cabeça (3x3)
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
        }

        private void InitializeGame()
        {
            // Inicialize o tabuleiro do quebra-cabeça
            InitializePuzzleBoard();

            // Crie a grade de botões do quebra-cabeça
            CreatePuzzleGrid();

            // Inicialize o contador de movimentos e o cronômetro
            movesCount = 0;
            UpdateLabelMovimentos();
            StartTimer();

            // Atualize a exibição do tabuleiro
            UpdatePuzzleGrid();
        }

        private void InitializePuzzleBoard()
        {
            // Crie o tabuleiro com os números embaralhados
            List<int> numbers = new List<int>();
            for (int i = 0; i < gridSize * gridSize; i++)
            {
                numbers.Add(i);
            }
            ShuffleList(numbers); // Embaralhar a lista usando o método personalizado

            // Preencha a matriz do tabuleiro com os números embaralhados
            puzzleBoard = new int[gridSize, gridSize];
            int index = 0;
            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    puzzleBoard[i, j] = numbers[index];
                    index++;
                }
            }
        }

        private void CreatePuzzleGrid()
        {
            // Crie a matriz de botões para representar as peças do quebra-cabeça
            puzzleButtons = new Button[gridSize, gridSize];

            // Limpe as RowDefinitions e ColumnDefinitions existentes
            puzzleGrid.RowDefinitions.Clear();
            puzzleGrid.ColumnDefinitions.Clear();

            // Defina as RowDefinitions
            for (int i = 0; i < gridSize; i++)
            {
                puzzleGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Star });
            }

            // Defina as ColumnDefinitions
            for (int j = 0; j < gridSize; j++)
            {
                puzzleGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
            }

            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    int pieceValue = puzzleBoard[i, j];
                    Button button = new Button
                    {
                        Text = (pieceValue == 0) ? string.Empty : pieceValue.ToString(),
                        FontSize = 24,
                        HeightRequest = 80,
                        WidthRequest = 80,
                        //Command = new Command(() => MovePiece(i, j)),
                    };

                    button.Clicked += (sender, e) =>
                    {
                        int row = Grid.GetRow(button);
                        int col = Grid.GetColumn(button);
                        MovePiece(row, col);
                    };

                    puzzleButtons[i, j] = button;
                    puzzleGrid.Children.Add(button);
                    Grid.SetRow(button, i); // Define a linha do botão na Grid
                    Grid.SetColumn(button, j); // Define a coluna do botão na Grid
                }
            }
        }

        private void MovePiece(int row, int col)
        {
            // Encontre a posição da peça vazia (valor 0) no tabuleiro
            int emptyRow = -1;
            int emptyCol = -1;
            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
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
                for (int i = 0; i < gridSize; i++)
                {
                    for (int j = 0; j < gridSize; j++)
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
            }
        }

        private void UpdateLabelMovimentos()
        {
            movesLabel.Text = $"Movimentos: {movesCount}";
        }

        private bool IsValidMove(int row, int col)
        {
            int emptyRow = FindEmptyPieceRow();
            int emptyCol = FindEmptyPieceColumn();

            return (row == emptyRow && Math.Abs(col - emptyCol) == 1) || (col == emptyCol && Math.Abs(row - emptyRow) == 1);
        }

        private int FindEmptyPieceRow()
        {
            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
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
            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
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
            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    int pieceValue = puzzleBoard[i, j];
                    puzzleButtons[i, j].Text = (pieceValue == 0) ? string.Empty : pieceValue.ToString();
                }
            }

            // Atualize o contador de movimentos e verifique se o jogo foi concluído
            UpdateLabelMovimentos();
            CheckGameCompleted();
        }

        private void CheckGameCompleted()
        {
            bool isGameCompleted = true;

            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    int pieceValue = puzzleBoard[i, j];
                    // Verifique se todas as peças estão nas posições corretas, exceto a última peça (peça vazia)
                    if (i == gridSize - 1 && j == gridSize - 1)
                    {
                        if (pieceValue != 0)
                        {
                            isGameCompleted = false;
                            break;
                        }
                    }
                    else if (pieceValue != i * gridSize + j + 1)
                    {
                        isGameCompleted = false;
                        break;
                    }
                }
            }

            if (isGameCompleted)
            {
                StopTimer();
                // Exiba uma mensagem para o jogador quando o quebra-cabeça for concluído
                DisplayAlert("Parabéns!", "Você completou o quebra-cabeça!", "OK");
                // Implemente a progressão de níveis ou qualquer ação desejada após a conclusão do jogo.
            }
        }

        private void ShufflePuzzle()
        {
            // Implemente a lógica para embaralhar as peças do quebra-cabeça.
            // ...
        }

        private void StartTimer()
        {
            stopwatch.Start();
            timer = new Timer(UpdateTimeLabel, null, 0, 1000); // Atualize o tempo a cada 1 segundo
        }

        private void StopTimer()
        {
            stopwatch.Stop();
            timer?.Dispose();
        }

        private void UpdateTimeLabel(object? state)
        {
            timeInSeconds = (int)stopwatch.Elapsed.TotalSeconds;
#pragma warning disable CS0612 // Type or member is obsolete
            Device.InvokeOnMainThreadAsync(() => timeLabel.Text = $"Tempo: {timeInSeconds} segundos");
#pragma warning restore CS0612 // Type or member is obsolete
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
            InitializeGame();
            //ResetTimer();
        }

    }
}