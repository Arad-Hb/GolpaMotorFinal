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

        public async Task<IViewComponentResult> InvokeAsync(List<UserListItemViewModel> model)
        {
            return View(model);
        }
    }
}
