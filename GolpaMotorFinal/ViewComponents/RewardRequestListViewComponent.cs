using DataAccess.Services;
using DomainModel.ViewModels.Reward;
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

        public async Task<IViewComponentResult> InvokeAsync(string? searchTerm = null, int? statusId = null, int pageIndex = 0)
        {
            var search = new RewardRequestSearchModel
            {
                SearchTerm = searchTerm,
                RewardDeliveryStatusID = statusId,
                PageIndex = pageIndex,
                PageSize = 10
            };
            var result = await repo.Search(search);
            var vm = new RewardRequestListPageViewModel
            {
                Items = result.RequestList,
                PageIndex = result.sm.PageIndex,
                PageCount = result.sm.PageCount,
                RecordCount = result.sm.RecordCount,
                SearchTerm = searchTerm,
                StatusId = statusId
            };
            return View(vm);
        }
    }
}
