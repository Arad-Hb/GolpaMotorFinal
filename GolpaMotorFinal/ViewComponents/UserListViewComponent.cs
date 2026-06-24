using DataAccess.Services;
using GolpaMotorFinal.Models.ViewModels.UserManagement;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var users = await repo.GetAll();
            return View(users);
        }
    }
}
