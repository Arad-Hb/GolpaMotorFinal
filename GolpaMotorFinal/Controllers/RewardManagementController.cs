using DataAccess.Services;
using DomainModel.ViewModels.Reward;
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
        public IActionResult CatalogList(RewardCatalogSearchModel sm)
        {
            return ViewComponent("RewardCatalogList", new { sm });
        }

        [HttpGet]
        public IActionResult RequestList(RewardRequestSearchModel sm)
        {
            return ViewComponent("RewardRequestList", new { sm });
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
