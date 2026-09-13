using DataAccess.Services;
using DomainModel.ViewModels.Reward;
using Framework.Common;
using GolpaMotorFinal.Helpers;
using GolpaMotorFinal.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GolpaMotorFinal.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RewardManagementController : Controller
    {
        private readonly IRewardCatalogRepository catalogRepo;
        private readonly IRewardRequestRepository requestRepo;

        public RewardManagementController(
            IRewardCatalogRepository catalogRepo,
            IRewardRequestRepository requestRepo)
        {
            this.catalogRepo = catalogRepo;
            this.requestRepo = requestRepo;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Catalogs = await catalogRepo.GetAll();
            ViewBag.Statuses = await requestRepo.GetStatuses();
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> CatalogList(RewardCatalogSearchModel sm)
        {
            sm ??= new RewardCatalogSearchModel();
            sm.PageSize = PaginationViewModel.DefaultPageSize;
            var result = await catalogRepo.Search(sm);
            var filter = result.sm ?? sm;
            var grid = AdminListGrids.BuildRewardCatalogGrid(result.CatalogList ?? new List<RewardCatalogListItem>());
            CrudGridPager.Attach(
                grid,
                "CatalogGrid",
                filter.PageIndex,
                filter.PageCount,
                filter.RecordCount,
                FilterUrl.Combine("/RewardManagement/CatalogList", new
                {
                    filter.Title,
                    filter.IsCashReward,
                    filter.IsActive,
                    filter.RequiredFrom,
                    filter.RequiredTo
                }));
            return ViewComponent("CrudGrid", new { model = grid });
        }

        [HttpGet]
        public async Task<IActionResult> RequestList(RewardRequestSearchModel sm)
        {
            sm ??= new RewardRequestSearchModel();
            sm.RequestFrom = PersianDate.ParseOrNull(sm.RequestFromJalali);
            sm.RequestTo = PersianDate.ParseOrNull(sm.RequestToJalali);
            sm.PageSize = PaginationViewModel.DefaultPageSize;
            var result = await requestRepo.Search(sm);
            var filter = result.sm ?? sm;
            var grid = AdminListGrids.BuildRewardRequestGrid(result.RequestList ?? new List<RewardRequestListItem>());
            CrudGridPager.Attach(
                grid,
                "RequestGrid",
                filter.PageIndex,
                filter.PageCount,
                filter.RecordCount,
                FilterUrl.Combine("/RewardManagement/RequestList", new
                {
                    filter.SearchTerm,
                    filter.RewardDeliveryStatusID,
                    filter.RewardCatalogID,
                    filter.RequestFromJalali,
                    filter.RequestToJalali
                }));
            return ViewComponent("CrudGrid", new { model = grid });
        }

        [HttpGet]
        public IActionResult Create()
        {
            return PartialView("_Create", new RewardCatalogAddEditModel { IsActive = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Create(RewardCatalogAddEditModel model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "اطلاعات معتبر نیست" });

            var result = await catalogRepo.Add(model);
            return Json(new { success = result.Success, message = result.Message });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int rewardCatalogID)
        {
            var catalog = await catalogRepo.Get(rewardCatalogID);
            if (catalog == null)
                return NotFound();

            return PartialView("_Edit", catalog);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(RewardCatalogAddEditModel model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "اطلاعات معتبر نیست" });

            var result = await catalogRepo.Update(model);
            return Json(new { success = result.Success, message = result.Message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Delete(int rewardCatalogID)
        {
            var result = await catalogRepo.Delete(rewardCatalogID);
            return Json(new { success = result.Success, message = result.Message });
        }

        [HttpGet]
        public async Task<IActionResult> Details(int rewardCatalogID)
        {
            var catalog = await catalogRepo.GetDetails(rewardCatalogID);
            if (catalog == null)
                return NotFound();

            return PartialView("_Details", catalog);
        }

        [HttpGet]
        public async Task<IActionResult> RequestDetails(int rewardRequestID)
        {
            var request = await requestRepo.GetDetails(rewardRequestID);
            if (request == null)
                return NotFound();

            return PartialView("_RequestDetails", request);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Approve(int rewardRequestID)
        {
            var result = await requestRepo.Approve(rewardRequestID);
            return Json(new { success = result.Success, message = result.Message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Reject(int rewardRequestID)
        {
            var result = await requestRepo.Reject(rewardRequestID);
            return Json(new { success = result.Success, message = result.Message });
        }
    }
}
