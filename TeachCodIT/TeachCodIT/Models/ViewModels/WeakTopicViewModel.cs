namespace TeachCodIT.Models.ViewModels
{
    public class WeakTopicViewModel
    {
        public string TopicName { get; set; }
        public int CorrectAnswers { get; set; }
        public int TotalAttempts { get; set; }
        public int ErrorRate { get; set; }
        public string Recommendation { get; set; }
        public string PracticeUrl { get; set; }
        public string? Icon { get; set; }
    }
}