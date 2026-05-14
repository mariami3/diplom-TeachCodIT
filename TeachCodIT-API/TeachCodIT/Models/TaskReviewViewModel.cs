using TeachCodIT.Models;

namespace TeachCodIT.Models.ViewModels
{
    public class TaskReviewViewModel
    {
        public UserTaskAttempt Attempt { get; set; } = new();
        public LessonTask Task { get; set; } = new();
    }
}