using GolpaMotorFinal.Models.ViewModels;
using GolpaMotorFinal.Models.ViewModels.CRUD;

namespace GolpaMotorFinal.Helpers
{
    public static class CrudGridPager
    {
        public static (List<T> Items, int PageIndex, int PageCount, int RecordCount) Slice<T>(
            IList<T>? source,
            int pageIndex,
            int pageSize = PaginationViewModel.DefaultPageSize)
        {
            var count = source?.Count ?? 0;
            if (pageSize <= 0)
                pageSize = PaginationViewModel.DefaultPageSize;
            var pageCount = count == 0 ? 1 : (int)Math.Ceiling(count / (double)pageSize);
            if (pageIndex < 0)
                pageIndex = 0;
            if (pageIndex >= pageCount)
                pageIndex = pageCount - 1;
            var items = count == 0
                ? new List<T>()
                : source!.Skip(pageIndex * pageSize).Take(pageSize).ToList();
            return (items, pageIndex, pageCount, count);
        }

        public static CrudGridViewModel Attach(
            CrudGridViewModel grid,
            string gridId,
            int pageIndex,
            int pageCount,
            int recordCount,
            string listUrl,
            int pageSize = PaginationViewModel.DefaultPageSize)
        {
            grid.GridId = gridId;
            grid.StartRowNumber = pageIndex * (pageSize <= 0 ? PaginationViewModel.DefaultPageSize : pageSize) + 1;
            grid.Pager = PaginationViewModel.For(gridId, pageIndex, pageCount, recordCount, listUrl, pageSize);
            return grid;
        }
    }
}
