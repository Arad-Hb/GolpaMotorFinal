namespace GolpaMotorFinal.Models.ViewModels
{
    
    public class SearchBoxViewModel
    {
        public string Controller { get; set; } = "";

        public string Action { get; set; } = "";

        public string Placeholder { get; set; } = "Search";

        public string SearchTerm { get; set; } = "";

        public string SearchParameterName { get; set; } = "searchTerm";

        public string? ComponentId { get; set; } 

        public bool UseAjax { get; set; }

        public string? UpdateTargetId { get; set; }
    }
}
