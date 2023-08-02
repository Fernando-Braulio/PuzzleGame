namespace PuzzleGame.Models
{
    public class Onboarding
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string ImageUrl { get; set; }
        public bool NotTitle { get; set; } = true;
        public bool NotImage { get; set; } = true;
    }
}