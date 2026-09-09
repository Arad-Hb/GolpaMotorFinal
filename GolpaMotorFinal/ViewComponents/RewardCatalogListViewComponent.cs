using DataAccess.Services;
using DomainModel.ViewModels.Reward;
using GolpaMotorFinal.Models.ViewModels;
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

        public async Task<IViewComponentResult> InvokeAsync(RewardCatalogSearchModel? sm = null)
        {
            sm ??= new RewardCatalogSearchModel();
            sm.PageSize = PaginationViewModel.DefaultPageSize;
            var result = await repo.Search(sm);
            var vm = new RewardCatalogListPageViewModel
            {
                Items = result.CatalogList,
                PageIndex = result.sm.PageIndex,
                PageCount = result.sm.PageCount,
                RecordCount = result.sm.RecordCount,
                Filter = result.sm ?? sm
            };
            return View(vm);
        }
    }
}
