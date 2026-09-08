using GolpaMotorFinal.Models.ViewModels;

namespace GolpaMotorFinal.Models.ViewModels.CRUD
{
    public class CrudGridViewModel
    {
        public string GridId { get; set; } = "crudGrid";
        public List<string> Headers { get; set; } = new();
        public List<GridRow> Rows { get; set; } = new();
        public bool ShowRowNumber { get; set; } = true;
        public int StartRowNumber { get; set; } = 1;
        public PaginationViewModel? Pager { get; set; }
        public string EmptyMessage { get; set; } = "هیچ داده‌ای وجود ندارد.";
    }
}
