(function () {
})();
// $(document).on("click", ".btnSubmitRewardRequest", function () {
//     const button = $(this);
//     const catalogId = button.data("id");
//     const userId = button.data("user-id");
//     if (!catalogId || !userId) return;

//     button.prop("disabled", true);
//     $.ajax({
//         url: "/UserManagement/RequestReward",
//         type: "POST",
//         data: { userID: userId, rewardCatalogID: catalogId },
//         headers: { RequestVerificationToken: token() },
//         success: function (res) {
//             $.get("/UserManagement/EligibleRewards", { userID: userId }, function (html) {
//                 const body = document.getElementById("generalModalBody");
//                 if (body) body.innerHTML = html;
//                 const box = $("#rewardRequestAlert");
//                 if (res && res.message && box.length) {
//                     box.removeClass("d-none alert-success alert-danger");
//                     box.addClass(res.success ? "alert-success" : "alert-danger");
//                     box.text(res.message);
//                 }
//             });
//         },
//         error: function () {
//             button.prop("disabled", false);
//             toastError("خطا در ثبت درخواست پاداش");
//         }
//     });
//});