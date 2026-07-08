namespace GolpaMotorFinal.Models.ViewModels
{
    public class PaginationViewModel
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int RecordCount { get; set; }
        public int PageCount { get; set; }
        public string GridId { get; set; } = "crudGrid";
        public string? ComponentName { get; set; }
        public List<int> PageSizeOptions { get; set; } = new() { 10, 25, 50 };
    }
}
