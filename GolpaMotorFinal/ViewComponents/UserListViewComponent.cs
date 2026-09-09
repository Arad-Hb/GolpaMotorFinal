using DomainModel.ViewModels.User;
using GolpaMotorFinal.Helpers;
using GolpaMotorFinal.Models.ViewModels;
using GolpaMotorFinal.Models.ViewModels.UserManagement;
using Microsoft.AspNetCore.Mvc;

namespace GolpaMotorFinal.ViewComponents
{
    [ViewComponent(Name = "UserList")]
    public class UserListViewComponent : ViewComponent
    {
        private readonly DataAccess.Services.IUserRepository repo;

        public UserListViewComponent(DataAccess.Services.IUserRepository repo)
        {
            this.repo = repo;
        }

        public async Task<IViewComponentResult> InvokeAsync(UserSearchModel? sm = null)
        {
            sm ??= new UserSearchModel();
            sm.PageSize = PaginationViewModel.DefaultPageSize;
            var result = await repo.Search(sm);

            var items = (result.userList ?? new List<UserListItem>()).Select(u => new UserListItemViewModel
            {
                UserID = u.UserID,
                FullName = $"{u.FirstName ?? string.Empty} {u.LastName ?? string.Empty}".Trim(),
                PhoneNumber = u.PhoneNumber ?? string.Empty,
                ProfileImageUrl = u.ExistingProfileImageUrl ?? string.Empty,
                RoleName = u.RoleName ?? string.Empty,
                TotalRegisteredCards = u.TotalRegisteredCards,
                TotalEarnedPoints = u.TotalEarnedPoints,
                IsEligibleForReward = u.IsEligibleForReward,
                HasReceivedReward = u.HasReceivedReward,
                Province = u.Province ?? string.Empty,
                City = u.City ?? string.Empty
            }).ToList();

            var filter = result.sm ?? sm;
            return View(new UserListPageViewModel
            {
                Items = items,
                PageIndex = filter.PageIndex,
                PageCount = filter.PageCount,
                RecordCount = filter.RecordCount,
                Filter = filter
            });
        }
    }
}
