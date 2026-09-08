const modalElement = document.getElementById("generalModal");
const generalModal = modalElement ? new bootstrap.Modal(modalElement) : null;

function token() {
    return $('input[name="__RequestVerificationToken"]').first().val();
}

function toastSuccess(message) {
    if (window.Swal) {
        Swal.fire({ icon: "success", title: message || "عملیات موفق", confirmButtonText: "باشه" });
    } else {
        alert(message);
    }
}

function toastError(message) {
    if (window.Swal) {
        Swal.fire({ icon: "error", title: message || "خطا", confirmButtonText: "باشه" });
    } else {
        alert(message);
    }
}

async function confirmDelete(text) {
    if (window.Swal) {
        const result = await Swal.fire({
            title: "آیا مطمئن هستید؟",
            text: text || "این عملیات قابل بازگشت نیست.",
            icon: "warning",
            showCancelButton: true,
            confirmButtonText: "بله، حذف شود",
            cancelButtonText: "انصراف"
        });
        return result.isConfirmed;
    }
    return confirm(text || "آیا از حذف مطمئن هستید؟");
}

function openModal(url, title, id, idName, extra) {
    if (!generalModal) return;

    document.getElementById("generalModalTitle").innerHTML = title || "";
    document.getElementById("generalModalBody").innerHTML =
        '<div class="text-center p-5"><div class="spinner-border"></div></div>';
    generalModal.show();

    const data = extra || {};
    const key = idName || "userID";
    if (id !== null && id !== undefined && id !== "null") {
        data[key] = id;
    }

    $.get(url, data, function (result) {
        document.getElementById("generalModalBody").innerHTML = result;
    }).fail(function () {
        document.getElementById("generalModalBody").innerHTML =
            '<div class="alert alert-danger">خطا در بارگذاری فرم</div>';
    });
}

function closeModal() {
    if (generalModal) generalModal.hide();
    const body = document.getElementById("generalModalBody");
    if (body) body.innerHTML = "";
}

$(document).on("click", ".open-modal", function () {
    openModal(
        $(this).data("url"),
        $(this).data("title"),
        $(this).data("id"),
        $(this).data("id-name"),
        {
            gridId: $(this).data("grid-id"),
            refreshUrl: $(this).data("refresh-url")
        }
    );
});

$(document).on("click", ".cancel-modal", function () {
    closeModal();
});

$(document).on("change", ".js-user-province", function () {
    const provinceId = $(this).val();
    const $city = $(this).closest("form").find(".js-user-city");
    $city.empty().append($("<option>").val("").text("انتخاب شهر"));
    if (!provinceId) return;

    $.get("/UserManagement/GetCitiesByProvince", { provinceId: provinceId }, function (res) {
        if (!res || !res.success || !res.data) return;
        res.data.forEach(function (city) {
            const id = city.cityID ?? city.cityId ?? city.CityID;
            const name = city.name ?? city.Name;
            $city.append($("<option>").val(id).text(name));
        });
    });
});

$(document).on("click", ".btnSubmitRewardRequest", function () {
    const button = $(this);
    const catalogId = button.data("id");
    const userId = button.data("user-id");
    if (!catalogId || !userId) return;

    button.prop("disabled", true);
    $.ajax({
        url: "/UserManagement/RequestReward",
        type: "POST",
        data: { userID: userId, rewardCatalogID: catalogId },
        headers: { RequestVerificationToken: token() },
        success: function (res) {
            $.get("/UserManagement/EligibleRewards", { userID: userId }, function (html) {
                const body = document.getElementById("generalModalBody");
                if (body) body.innerHTML = html;
                const box = $("#rewardRequestAlert");
                if (res && res.message && box.length) {
                    box.removeClass("d-none alert-success alert-danger");
                    box.addClass(res.success ? "alert-success" : "alert-danger");
                    box.text(res.message);
                }
            });
        },
        error: function () {
            button.prop("disabled", false);
            toastError("خطا در ثبت درخواست پاداش");
        }
    });
});
