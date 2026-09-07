using DataAccess.Services;
using DomainModel.ViewModels.Reward;
using GolpaMotorFinal.Models.ViewModels.RewardManagement;
using Microsoft.AspNetCore.Mvc;

namespace GolpaMotorFinal.ViewComponents
{
    [ViewComponent(Name = "RewardCatalogList")]
    public class RewardCatalogListViewComponent : ViewComponent
    {
        private readonly IRewardCatalogRepository repo;

        public RewardCatalogListViewComponent(IRewardCatalogRepository repo)
        {
            this.repo = repo;
        }

        public async Task<IViewComponentResult> InvokeAsync(string? title = null, int pageIndex = 0)
        {
            var search = new RewardCatalogSearchModel
            {
                Title = title,
                PageIndex = pageIndex,
                PageSize = 10
            };
            var result = await repo.Search(search);
            var vm = new RewardCatalogListPageViewModel
            {
                Items = result.CatalogList,
                PageIndex = result.sm.PageIndex,
                PageCount = result.sm.PageCount,
                RecordCount = result.sm.RecordCount,
                Title = title
            };
            return View(vm);
        }
    }
}
