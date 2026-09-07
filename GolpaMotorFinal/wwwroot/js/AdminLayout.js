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

$(document).on("input", ".table-filter-input", function () {
    const query = ($(this).val() || "").toString().trim().toLowerCase();
    const target = $(this).data("table-target");
    const $root = target ? $(target) : $(this).closest(".white_shd, .card, .table_section, .tab-pane").find("table").first();
    const $table = $root.is("table") ? $root : $root.find("table").first();
    $table.find("tbody tr").each(function () {
        const rowText = ($(this).text() || "").toLowerCase();
        const emptyRow = $(this).find("td").length <= 1 && rowText.indexOf("یافت") !== -1;
        $(this).toggle(!query || emptyRow || rowText.indexOf(query) !== -1);
    });
});

function refreshGrid(targetId, targetUrl, closeOnSuccess = true, afterRefresh) {
    if (!targetUrl) {
        if (closeOnSuccess) {
            try { generalModal.hide(); } catch (e) { }
        }
        if (typeof afterRefresh === "function") afterRefresh();
        return;
    }

    $.get(targetUrl)
        .done(function (html) {
            if (targetId) {
                $(targetId).html(html);
            }
            if (closeOnSuccess) {
                try { generalModal.hide(); } catch (e) { }
            }
            if (typeof afterRefresh === "function") afterRefresh();
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
                    try { generalModal.hide(); } catch (err) { }
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
    const img = $(this).closest("form").find(".image-preview");
    if (!file || !img.length) return;
    const reader = new FileReader();
    reader.onload = function (e) {
        img.attr("src", e.target.result).removeClass("d-none");
    };
    reader.readAsDataURL(file);
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
            const targetID = btn.data("refresh-target-id") ? ("#" + btn.data("refresh-target-id")) : null;
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
