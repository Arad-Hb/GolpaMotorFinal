(function () {
    function antiForgeryToken() {
        if (typeof token === "function") return token();
        return $('#antiforgery-form input[name="__RequestVerificationToken"]').val();
    }

    function loadRequests() {
        applyTableFilter($('[data-filter-bar][data-target="#RequestGrid"]'));
    }

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
                    loadRequests();
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
                    loadRequests();
                    toastSuccess(res.message);
                } else {
                    toastError(res.message);
                }
            }
        });
    });
})();
