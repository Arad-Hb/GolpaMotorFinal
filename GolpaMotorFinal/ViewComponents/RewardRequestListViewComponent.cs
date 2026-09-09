using DataAccess.Services;
using DomainModel.ViewModels.Reward;
using Framework.Common;
using GolpaMotorFinal.Models.ViewModels;
using GolpaMotorFinal.Models.ViewModels.RewardManagement;
using Microsoft.AspNetCore.Mvc;

namespace GolpaMotorFinal.ViewComponents
{
    [ViewComponent(Name = "RewardRequestList")]
    public class RewardRequestListViewComponent : ViewComponent
    {
        private readonly IRewardRequestRepository repo;

        public RewardRequestListViewComponent(IRewardRequestRepository repo)
        {
            this.repo = repo;
        }

        public async Task<IViewComponentResult> InvokeAsync(RewardRequestSearchModel? sm = null)
        {
            sm ??= new RewardRequestSearchModel();
            sm.RequestFrom = PersianDate.ParseOrNull(sm.RequestFromJalali);
            sm.RequestTo = PersianDate.ParseOrNull(sm.RequestToJalali);
            sm.PageSize = PaginationViewModel.DefaultPageSize;
            var result = await repo.Search(sm);
            var vm = new RewardRequestListPageViewModel
            {
                Items = result.RequestList,
                PageIndex = result.sm.PageIndex,
                PageCount = result.sm.PageCount,
                RecordCount = result.sm.RecordCount,
                Filter = result.sm ?? sm
            };
            return View(vm);
        }
    }
}
