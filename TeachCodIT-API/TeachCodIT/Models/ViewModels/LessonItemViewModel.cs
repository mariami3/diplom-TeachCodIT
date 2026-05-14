namespace TeachCodIT.Models.ViewModels
{
    public class LessonItemViewModel
    {
        public int IdLesson { get; set; }
        public string Title { get; set; } = "";
        public int XPReward { get; set; }
        public int TasksCount { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsLocked { get; set; }
    }
}