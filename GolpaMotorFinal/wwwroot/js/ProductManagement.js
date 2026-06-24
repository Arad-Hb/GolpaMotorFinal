$(document).ready(function () {
    refreshList();
});

/* ============ LOAD LIST ============ */
function refreshList() {
    $("#dvContent").load("/ProductManagement/ProductListAction");
}

/* ============ SUCCESS / ERROR ============ */
function success(msg) {
    Swal.fire("موفق", msg, "success");
}

function error(msg) {
    Swal.fire("خطا", msg, "error");
}

/* ============ ADD MODAL ============ */
$(document).on("click", ".btn-add", function () {

    $.get("/ProductManagement/Add", function (html) {
        $("#dvModalContent").html(html);
        $("#mainModal").modal("show");
    });

});

/* ============ SAVE ADD ============ */
$(document).on("click", ".save-add", function () {

    let form = new FormData($("#frmAddProduct")[0]);

    $.ajax({
        url: "/ProductManagement/Add",
        type: "POST",
        data: form,
        processData: false,
        contentType: false,
        success: function (res) {

            if (res.success) {
                $("#mainModal").modal("hide");
                refreshList();
                success(res.message);
            } else {
                error(res.message);
            }

        }
    });
});

/* ============ DELETE ============ */
$(document).on("click", ".btn-delete", function () {

    let id = $(this).data("id");

    Swal.fire({
        title: "حذف شود؟",
        icon: "warning",
        showCancelButton: true,
        confirmButtonText: "بله",
        cancelButtonText: "خیر"
    }).then((result) => {

        if (result.isConfirmed) {

            $.post("/ProductManagement/Delete", { productID: id }, function (res) {

                if (res.success) {
                    $("#tr_" + id).remove();
                    success(res.message);
                } else {
                    error(res.message);
                }

            });

        }

    });

});

/* ============ UPDATE ============ */
$(document).on("click", ".btn-update", function () {

    let id = $(this).data("id");

    $.get("/ProductManagement/Update", { productID: id }, function (html) {
        $("#dvModalContent").html(html);
        $("#mainModal").modal("show");
    });

});

$(document).on("click", ".save-update", function () {

    let form = new FormData($("#frmUpdateProduct")[0]);

    $.ajax({
        url: "/ProductManagement/Update",
        type: "POST",
        data: form,
        processData: false,
        contentType: false,
        success: function (res) {

            if (res.success) {
                $("#mainModal").modal("hide");
                refreshList();
                success(res.message);
            } else {
                error(res.message);
            }

        }
    });

});

/* ============ DETAILS ============ */
$(document).on("click", ".btn-details", function () {

    let id = $(this).data("id");

    $.get("/ProductManagement/Details", { productID: id }, function (html) {
        $("#dvModalContent").html(html);
        $("#mainModal").modal("show");
    });

});

/* ============ CLOSE MODAL ============ */
$(document).on("click", ".btn-close-modal", function () {
    $("#mainModal").modal("hide");
});