namespace GolpaMotorFinal.Models.ViewModels
{
    public class PaginationViewModel
    {
        public const int DefaultPageSize = 10;

        public int PageIndex { get; set; }
        public int PageSize { get; set; } = DefaultPageSize;
        public int RecordCount { get; set; }
        public int PageCount { get; set; }
        public string GridId { get; set; } = "crudGrid";
        public string? ComponentName { get; set; }
        public string ListUrl { get; set; } = string.Empty;
        public string PageParameter { get; set; } = "pageIndex";
        public bool ShowPageSizeSelector { get; set; }
        public List<int> PageSizeOptions { get; set; } = new() { 10, 25, 50 };

        public static PaginationViewModel For(
            string gridId,
            int pageIndex,
            int pageCount,
            int recordCount,
            string listUrl,
            int pageSize = DefaultPageSize,
            string pageParameter = "pageIndex")
        {
            return new PaginationViewModel
            {
                GridId = gridId,
                PageIndex = pageIndex < 0 ? 0 : pageIndex,
                PageSize = pageSize <= 0 ? DefaultPageSize : pageSize,
                PageCount = pageCount,
                RecordCount = recordCount,
                ListUrl = listUrl,
                PageParameter = string.IsNullOrWhiteSpace(pageParameter) ? "pageIndex" : pageParameter,
                ShowPageSizeSelector = false
            };
        }
    }
}
