using DomainModel.ViewModels.Reports;
using Framework.Common;
using GolpaMotorFinal.Models.ViewModels.CRUD;
using Microsoft.AspNetCore.Mvc;

namespace GolpaMotorFinal.Helpers
{
    public static class ReportGridResult
    {
        public static IActionResult FromSearch<T>(
            Controller controller,
            ReportSearchResult<ReportPage<T>> result,
            CrudGridViewModel grid,
            string gridId,
            string pagerBaseUrl)
        {
            if (!result.Operation.Success)
            {
                return SearchFailure(controller, result.Operation);
            }

            var page = result.Data!;
            CrudGridPager.Attach(
                grid,
                gridId,
                page.PageIndex,
                page.PageCount,
                page.RecordCount,
                pagerBaseUrl,
                page.PageSize);

            return controller.ViewComponent("CrudGrid", new { model = grid });
        }

        public static IActionResult InvalidFilter(Controller controller)
        {
            var operation = new OperationResult("ReportSearch");
            operation.ToFailed("مقادیر فیلتر نامعتبر است.", "InvalidFilter");
            return SearchFailure(controller, operation);
        }

        public static IActionResult SearchFailure(Controller controller, OperationResult operation)
        {
            controller.Response.StatusCode =
                operation.ErrorCode == "InvalidFilter" ? 400 : 503;

            return controller.PartialView("_SearchFailure", operation);
        }
    }
}
