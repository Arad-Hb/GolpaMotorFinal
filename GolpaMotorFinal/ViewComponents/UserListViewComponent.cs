using DataAccess.Services;
using DomainModel.ViewModels.User;
using GolpaMotorFinal.Models.ViewModels;
using GolpaMotorFinal.Models.ViewModels.UserManagement;
using Microsoft.AspNetCore.Mvc;

namespace GolpaMotorFinal.ViewComponents
{
    [ViewComponent(Name = "UserList")]
    public class UserListViewComponent : ViewComponent
    {
        private readonly IUserRepository repo;

        public UserListViewComponent(IUserRepository repo)
        {
            this.repo = repo;
        }

        public async Task<IViewComponentResult> InvokeAsync(string? searchTerm = null, int pageIndex = 0)
        {
            var result = await repo.Search(new UserSearchModel
            {
                SearchTerm = searchTerm,
                PhoneNumber = string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm,
                PageIndex = pageIndex,
                PageSize = PaginationViewModel.DefaultPageSize
            });

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

            var sm = result.sm ?? new UserSearchModel();
            return View(new UserListPageViewModel
            {
                Items = items,
                PageIndex = sm.PageIndex,
                PageCount = sm.PageCount,
                RecordCount = sm.RecordCount,
                SearchTerm = searchTerm
            });
        }
    }
}
