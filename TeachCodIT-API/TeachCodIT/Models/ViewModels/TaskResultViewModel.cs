namespace TeachCodIT.Models.ViewModels
{
    public class TaskResultViewModel
    {
        public bool IsCorrect { get; set; }

        public int EarnedXP { get; set; }

        public string? Message { get; set; }
        public int AttemptsLeft { get; set; }

        public int LessonId { get; set; }

        // опционально
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    }
}