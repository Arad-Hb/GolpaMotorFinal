const modalElement = document.getElementById("generalModal");
const generalModal = modalElement ? new bootstrap.Modal(modalElement) : null;
const DEFAULT_MODAL_SIZE = "modal-lg";
const MODAL_SIZE_CLASSES = ["modal-sm", "modal-lg", "modal-xl"];

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

function getModalDialog() {
    return modalElement ? modalElement.querySelector(".modal-dialog") : null;
}

function applyModalSize(size) {
    const dialog = getModalDialog();
    if (!dialog) return;
    MODAL_SIZE_CLASSES.forEach(function (cls) {
        dialog.classList.remove(cls);
    });
    dialog.classList.add(MODAL_SIZE_CLASSES.indexOf(size) >= 0 ? size : DEFAULT_MODAL_SIZE);
}

function destroyModalWidgets(root) {
    if (!root) return;
    $(".filter-select-popup, .date-range-popup, .num-range-popup").remove();
    const el = $(root);
    el.find(".filter-select").each(function () {
        const wrap = $(this);
        const select = wrap.find("select").first();
        if (select.length) {
            wrap.replaceWith(select);
            select.removeData("selectReady");
        }
    });
}

function resetModalShell() {
    applyModalSize(DEFAULT_MODAL_SIZE);
    const title = document.getElementById("generalModalTitle");
    if (title) title.innerHTML = "";
    const body = document.getElementById("generalModalBody");
    destroyModalWidgets(body);
    if (body) body.innerHTML = "";
    if (modalElement) {
        delete modalElement.dataset.gridId;
        delete modalElement.dataset.refreshUrl;
    }
}

function openModal(url, title, id, idName, extra) {
    if (!generalModal) return;

    extra = extra || {};
    applyModalSize(extra.size);

    const body = document.getElementById("generalModalBody");
    destroyModalWidgets(body);
    document.getElementById("generalModalTitle").innerHTML = title || "";
    body.innerHTML =
        '<div class="text-center p-5"><div class="spinner-border"></div></div>';

    if (extra.gridId) modalElement.dataset.gridId = extra.gridId;
    else delete modalElement.dataset.gridId;
    if (extra.refreshUrl) modalElement.dataset.refreshUrl = extra.refreshUrl;
    else delete modalElement.dataset.refreshUrl;

    generalModal.show();

    const params = {};
    const key = idName || "userID";
    if (id !== null && id !== undefined && id !== "" && id !== "null") {
        params[key] = id;
    }

    $.get(url, params, function (result) {
        document.getElementById("generalModalBody").innerHTML = result;
        if (window.initAdminSelects) window.initAdminSelects(document.getElementById("generalModalBody"));
    }).fail(function () {
        document.getElementById("generalModalBody").innerHTML =
            '<div class="alert alert-danger">خطا در بارگذاری فرم</div>';
    });
}

function closeModal() {
    if (generalModal) generalModal.hide();
}

if (modalElement) {
    modalElement.addEventListener("hidden.bs.modal", resetModalShell);
}

$(document).on("click", ".open-modal", function () {
    openModal(
        $(this).data("url"),
        $(this).data("title"),
        $(this).data("id"),
        $(this).data("id-name"),
        {
            gridId: $(this).data("grid-id"),
            refreshUrl: $(this).data("refresh-url"),
            size: $(this).data("size")
        }
    );
});

$(document).on("click", ".cancel-modal", function () {
    closeModal();
});

$(document).on("change", ".js-user-province", function () {
    const provinceId = $(this).val();
    const city = $(this).closest("form").find(".js-user-city");
    city.empty().append($("<option>").val("").text("انتخاب شهر"));
    if (window.refreshFilterSelect) window.refreshFilterSelect(city);
    if (!provinceId) return;

    $.get("/UserManagement/GetCitiesByProvince", { provinceId: provinceId }, function (res) {
        if (!res || !res.success || !res.data) return;
        res.data.forEach(function (item) {
            const id = item.cityID ?? item.cityId ?? item.CityID;
            const name = item.name ?? item.Name;
            city.append($("<option>").val(id).text(name));
        });
        if (window.refreshFilterSelect) window.refreshFilterSelect(city);
    });
});
