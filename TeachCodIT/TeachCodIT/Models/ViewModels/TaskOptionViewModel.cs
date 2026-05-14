namespace TeachCodIT.Models.ViewModels
{
    public class TaskOptionViewModel
    {
        public int IdOption { get; set; }
        public string OptionText { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
    }
}
