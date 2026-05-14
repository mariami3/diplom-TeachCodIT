namespace TeachCodIT.Models.ViewModels
{
    public class CourseDetailsViewModel
    {
        public int IdCourse { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";

        public List<ModuleViewModel> Modules { get; set; } = new();

        public bool HasWeakTasks { get; set; }          
        public int WeakTasksCount { get; set; }         
        public double WeakErrorRate { get; set; }
        public int TotalLessons { get; set; }
        public int CompletedLessons { get; set; }
        public int TotalTasks { get; set; }
        public int ProgressPercent { get; set; }
    }
}