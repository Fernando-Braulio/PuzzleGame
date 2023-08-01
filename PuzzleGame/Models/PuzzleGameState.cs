using System;
namespace PuzzleGame.Models
{
	public class PuzzleGameState
	{
        public int[] PuzzleBoard { get; set; }
        public int InitialPiecesCount { get; set; } = 2;
        public int PiecesCount { get; set; } // Variável que armazenará a quantidade atual de peças no tabuleiro
        public int CurrentLevel { get; set; } = 1;
        public int MovesCount { get; set; } = 0;

        public int GridSize
        {
            set { }
            get { return PiecesCount; }
        }

        public int MovesLimit
        {
            set { }
            get { return CurrentLevel * 70; }
        }
    }
}

