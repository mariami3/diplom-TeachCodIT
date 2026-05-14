namespace TeachCodIT.Models.ViewModels
{
    public class ModuleViewModel
    {
        public int IdModule { get; set; }
        public string Title { get; set; } = "";
        public int OrderIndex { get; set; }

        public int ProgressPercent { get; set; }
        public bool IsExpanded { get; set; }

        public List<LessonItemViewModel> Lessons { get; set; } = new();
    }
}