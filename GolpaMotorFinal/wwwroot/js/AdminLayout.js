//-----General CRUD Events-----\\


// Generic helper to refresh a grid/container from server HTML
function refreshGrid(targetId, targetUrl, closeOnSuccess = true, afterRefresh) {
    if (!targetUrl) return;

    $.get(targetUrl)
        .done(function (html) {
            if (targetId) {
                $(targetId).html(html);
            }

            if (closeOnSuccess) {
                try { generalModal.hide(); } catch (e) { }
            }

            if (typeof afterRefresh === 'function') afterRefresh();
        })
        .fail(function () {
            alert('Failed to refresh grid.');
        });

// Merge accounts - submit merge form via AJAX and refresh grid
$(document).on("submit", ".merge-form", function (e) {

    e.preventDefault();

    const form = $(this);
    const url = form.attr('action');
    const data = form.serialize();
    const targetUrl = form.data('refresh-grid-url');
    const targetId = form.data('grid-id') ? ('#' + form.data('grid-id')) : null;

    $.ajax({
        url: url,
        type: 'POST',
        data: data,
        headers: {
            'RequestVerificationToken': form.find('input[name="__RequestVerificationToken"]').val(),
            'X-Requested-With': 'XMLHttpRequest'
        }
    }).done(function (op) {
        if (!op.success) {
            if (op.errors && Array.isArray(op.errors) && op.errors.length) {
                alert(op.errors.join('\n'));
                return;
            }

            alert(op.message || 'عملیات ناموفق بود');
            return;
        }

        // success - refresh grid and close modal
        refreshGrid(targetId, targetUrl, true, function () {
            alert(op.message || 'عملیات با موفقیت انجام شد');
        });

    }).fail(function () {
        alert('خطا در انجام عملیات ادغام');
    });

});
}

//SaveCreate / SaveEdit
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
            "RequestVerificationToken":
                form.find('input[name="__RequestVerificationToken"]').val()
        },

        success: function (op) {

            if (!op.success) {
                // If server returned validation errors, show them
                if (op.errors && Array.isArray(op.errors) && op.errors.length) {
                    console.warn('Validation errors:', op.errors);
                    alert(op.errors.join('\n'));
                    return;
                }

                alert(op.message || 'Operation failed');
                return;
            }

            if (form.data("refresh-grid")) {
                refreshGrid(targetId, targetUrl, form.data("close-on-success"), function () {
                    alert(op.message);
                });
            }
            else {
                if (form.data("close-on-success")) {
                    generalModal.hide();
                }

                alert(op.message);
            }
        },

        error: function (xhr) {

            if (xhr.responseJSON?.message) {
                alert(xhr.responseJSON.message);
            }
            else {
                alert("An unexpected error occurred.");
            }
        }
    });

});

//Delete
$(document).on("click", ".btnDelete", function () {

    if (!confirm("آیا از حذف مطمئن هستید؟"))
        return;

    const sendingUrl = $(this).data("url");
    const refreshUrl = $(this).data("refresh-target-url");
    const targetID = $(this).data("refresh-target-id") ? ("#" + $(this).data("refresh-target-id")) : null;

    $.post(sendingUrl, {
        userID: $(this).data("id")
    })
        .done(function (op) {

            if (op.success) {

                refreshGrid(targetID, refreshUrl, true, function () {
                    alert(op.message);
                });

            } else {
                alert(op.message);
            }

        })
        .fail(function () {
            alert("An error occurred while deleting the user.");
        });

});
