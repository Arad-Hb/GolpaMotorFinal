using DataAccess.Services;
using GolpaMotorFinal.FrameworkUI.Services;
using GolpaMotorFinal.Models.ViewModels.UserManagement;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GolpaMotorFinal.ViewComponents
{
    [ViewComponent(Name = "UserList")]
    public class UserListViewComponent : ViewComponent
    {

        private readonly IUserService servive;

        public UserListViewComponent(IUserService _servive)
        {
            servive = _servive;
        }

        public async Task<IViewComponentResult> InvokeAsync(IEnumerable<UserListItemViewModel>? users = null)
        {
            var vm = users?.ToList() ?? await servive.GetUsers();
            return View(vm);
        }
    }
}
