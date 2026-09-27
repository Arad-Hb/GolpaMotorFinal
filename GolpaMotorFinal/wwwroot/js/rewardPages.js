(function () {
    function antiForgeryToken() {
        if (typeof token === "function") return token();
        return $('#antiforgery-form input[name="__RequestVerificationToken"]').val();
    }

    function loadRequests() {
        const bar = $('[data-filter-bar][data-target="#RequestGrid"]');
        if (bar.length) applyTableFilter(bar);
    }

    function refreshRewardRequestSurfaces() {
        loadRequests();
        const dashboard = $("#dashboardPendingRequests");
        if (dashboard.length) {
            dashboard.load("/Admin/PendingRewardRequests");
        }
    }

    window.syncRewardCatalogCashFields = function (root) {
        const scope = $(root || document);
        const selected = scope.find('input[name="IsCashReward"]:checked').val();
        if (selected === undefined) return;

        const isCash = String(selected).toLowerCase() === "true";
        const group = scope.find(".js-reward-cash-value-group");
        const input = group.find('input[name="CashValue"]');

        group.toggleClass("d-none", !isCash);
        input.prop("disabled", !isCash);
        if (!isCash) {
            input.val("");
        } else if (window.initNumericInputs) {
            window.initNumericInputs(group[0]);
        }
    };

    $(document).on("change", ".js-reward-cash-type", function () {
        window.syncRewardCatalogCashFields($(this).closest("form"));
    });

    $(document).on("click", ".btnApproveRequest", async function () {
        const ok = await confirmDelete("این درخواست تأیید و امتیاز کاربر کسر شود؟");
        if (!ok) return;
        const id = $(this).data("id");
        $.ajax({
            url: "/RewardManagement/Approve",
            type: "POST",
            data: { rewardRequestID: id },
            headers: { RequestVerificationToken: antiForgeryToken() },
            success: function (res) {
                if (res.success) {
                    closeModal();
                    refreshRewardRequestSurfaces();
                    toastSuccess(res.message);
                } else {
                    toastError(res.message);
                }
            }
        });
    });

    $(document).on("click", ".btnRejectRequest", async function () {
        const ok = await confirmDelete("این درخواست رد شود؟");
        if (!ok) return;
        const id = $(this).data("id");
        $.ajax({
            url: "/RewardManagement/Reject",
            type: "POST",
            data: { rewardRequestID: id },
            headers: { RequestVerificationToken: antiForgeryToken() },
            success: function (res) {
                if (res.success) {
                    closeModal();
                    refreshRewardRequestSurfaces();
                    toastSuccess(res.message);
                } else {
                    toastError(res.message);
                }
            }
        });
    });
})();
