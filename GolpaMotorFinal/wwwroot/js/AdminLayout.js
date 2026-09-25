$(document).on("keydown", ".search__input, #productSearch", function (e) {
    if (e.key !== "Enter") return;
    e.preventDefault();
    const btn = $(this).closest(".search").find(".search__button");
    if (btn.length) btn.trigger("click");
    else $("#btnSearchProduct").trigger("click");
});

$(document).on("click", "#sidebarCollapse, .sidebar_toggle", function (e) {
    e.preventDefault();
    e.stopPropagation();
    const body = document.body;
    if (window.matchMedia("(max-width: 991.98px)").matches) {
        body.classList.toggle("sidebar-open");
        body.classList.remove("sidebar-collapsed");
    } else {
        body.classList.toggle("sidebar-collapsed");
        body.classList.remove("sidebar-open");
    }
});

$(document).on("click", ".search__button", function (e) {
    e.preventDefault();
    var searchBox = $(this).closest(".search");
    var componentId = searchBox.data("component-id");
    var url = searchBox.data("action");
    var target = searchBox.data("target");
    var input = $("#searchInput-" + componentId);
    var payload = {
        __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').first().val(),
        [input.attr("name")]: input.val()
    };
    var mergeForm = $(this).closest("#mergeForm");
    if (mergeForm.length) {
        payload["CurrentUser.UserID"] = mergeForm.find("input[name='CurrentUser.UserID']").val();
    }
    $.ajax({
        url: url,
        type: "POST",
        data: payload,
        success: function (html) {
            $("#" + target).html(html);
        }
    });
});

$(document).on("input", ".table-filter-input", function () {
    const query = ($(this).val() || "").toString().trim().toLowerCase();
    const target = $(this).data("table-target");
    const root = target ? $(target) : $(this).closest(".white_shd, .card, .table_section, .tab-pane").find("table").first();
    const table = root.is("table") ? root : root.find("table").first();
    table.find("tbody tr").each(function () {
        const rowText = ($(this).text() || "").toLowerCase();
        const emptyRow = $(this).find("td").length <= 1 && rowText.indexOf("یافت") !== -1;
        $(this).toggle(!query || emptyRow || rowText.indexOf(query) !== -1);
    });
});

function filterBarForGrid(targetId) {
    if (!targetId) return $();
    var selector = String(targetId).charAt(0) === "#" ? targetId : ("#" + targetId);
    return $('[data-filter-bar][data-target="' + selector + '"]');
}

function refreshGrid(targetId, targetUrl, closeOnSuccess = true, afterRefresh) {
    function finish() {
        if (closeOnSuccess && typeof closeModal === "function") closeModal();
        if (typeof afterRefresh === "function") afterRefresh();
    }

    var bar = filterBarForGrid(targetId);
    if (bar.length && typeof window.applyTableFilter === "function") {
        applyTableFilter(bar);
        finish();
        return;
    }

    if (!targetUrl) {
        finish();
        return;
    }

    $.get(targetUrl)
        .done(function (html) {
            if (targetId) {
                $(targetId).html(html);
            }
            finish();
        })
        .fail(function () {
            toastError("بارگذاری مجدد لیست ناموفق بود.");
        });
}

$(document).on("submit", ".merge-form", function (e) {
    e.preventDefault();

    const form = $(this);
    $.ajax({
        url: form.attr("action"),
        type: "POST",
        data: form.serialize(),
        headers: {
            RequestVerificationToken: form.find('input[name="__RequestVerificationToken"]').val(),
            "X-Requested-With": "XMLHttpRequest"
        }
    }).done(function (op) {
        if (!op.success) {
            toastError(op.message || "عملیات ناموفق بود");
            return;
        }
        const targetUrl = form.data("refresh-grid-url");
        const targetId = form.data("grid-id") ? ("#" + form.data("grid-id")) : null;
        refreshGrid(targetId, targetUrl, true, function () {
            toastSuccess(op.message);
        });
    }).fail(function () {
        toastError("خطا در انجام عملیات ادغام");
    });
});

$(document).on("submit", ".crud-form", function (e) {
    e.preventDefault();
    const form = $(this);
    const formData = new FormData(this);
    const targetId = form.data("grid-id") ? ("#" + form.data("grid-id")) : null;
    const targetUrl = form.data("refresh-grid-url");

    $.ajax({
        url: form.attr("action"),
        type: form.attr("method"),
        data: formData,
        processData: false,
        contentType: false,
        cache: false,
        headers: {
            RequestVerificationToken: form.find('input[name="__RequestVerificationToken"]').val()
        },
        success: function (op) {
            if (!op.success) {
                toastError(op.message || "عملیات ناموفق بود");
                return;
            }
            if (form.data("refresh-grid")) {
                refreshGrid(targetId, targetUrl, form.data("close-on-success"), function () {
                    toastSuccess(op.message);
                });
            } else {
                if (form.data("close-on-success")) {
                    if (typeof closeModal === "function") closeModal();
                }
                toastSuccess(op.message);
            }
        },
        error: function (xhr) {
            toastError(xhr.responseJSON?.message || "خطای غیرمنتظره رخ داد.");
        }
    });
});

$(document).on("change", ".image-preview-input", function () {
    const file = this.files && this.files[0];
    const row = $(this).closest(".modal-photo-row");
    const img = row.length ? row.find(".image-preview") : $(this).closest("form").find(".image-preview");
    if (!file || !img.length) return;
    const reader = new FileReader();
    reader.onload = function (e) {
        img.attr("src", e.target.result).removeClass("d-none");
    };
    reader.readAsDataURL(file);
    row.find(".js-remove-photo-flag").val("false");
});

$(document).on("click", ".js-pick-photo", function () {
    $(this).closest(".modal-photo-row").find(".image-preview-input").trigger("click");
});

$(document).on("click", ".js-remove-photo", function () {
    const row = $(this).closest(".modal-photo-row");
    const input = row.find(".image-preview-input");
    input.val("");
    const fallback = row.data("fallback") || "/images/imageUsers/noimage.jpg";
    row.find(".image-preview").attr("src", fallback).removeClass("d-none");
    row.find(".js-remove-photo-flag").val("true");
});

$(document).on("click", ".btnDelete", async function () {
    const ok = await confirmDelete("این مورد حذف خواهد شد.");
    if (!ok) return;

    const btn = $(this);
    const idName = btn.data("id-name") || "userID";
    const payload = {};
    payload[idName] = btn.data("id");

    $.ajax({
        url: btn.data("url"),
        type: "POST",
        data: payload,
        headers: {
            RequestVerificationToken: token()
        }
    }).done(function (op) {
        if (op.success) {
            const gridId = btn.data("refresh-target-id") || btn.closest("[data-grid]").attr("data-grid");
            const targetID = gridId ? ("#" + gridId) : null;
            refreshGrid(targetID, btn.data("refresh-target-url"), true, function () {
                toastSuccess(op.message);
            });
        } else {
            toastError(op.message);
        }
    }).fail(function () {
        toastError("خطا در حذف");
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

 // $(document).on("click", ".btnRemovePicture", async function () {
        //     const ok = await confirmDelete("عکس محصول حذف شود؟");
        //     if (!ok) return;
        //     const id = $(this).data("id");
        //     const token = $('#antiforgery-form input[name="__RequestVerificationToken"]').val();
        //     $.ajax({
        //         url: '/ProductManagement/RemovePicture',
        //         type: 'POST',
        //         data: { productID: id, __RequestVerificationToken: token },
        //         success: function (res) {
        //             if (res.success) {
        //                 toastSuccess(res.message);
        //                 applyTableFilter($("#ProductGrid").closest(".padding_infor_info").find("[data-filter-bar]"));
        //             } else {
        //                 toastError(res.message);
        //             }
        //         }
        //     });
        // });
