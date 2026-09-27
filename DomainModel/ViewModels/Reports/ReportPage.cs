namespace DomainModel.ViewModels.Reports
{
    public class ReportPage<T>
    {
        public List<T> Items { get; set; } = new();
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int RecordCount { get; set; }
        public int PageCount => PageSize <= 0
            ? 0
            : (int)Math.Ceiling(RecordCount / (double)PageSize);
    }
}
