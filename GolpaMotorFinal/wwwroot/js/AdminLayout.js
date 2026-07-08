//-----General CRUD Events-----\\


//SaveCreate

$(document).on("submit", ".crud-form", function (e) {

    e.preventDefault();

    const form = $(this);
    const formData = new FormData(this);

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
                alert(op.message);
                return;
            }
            
            if (form.data("refresh-grid")) {

                const targetID = "#" + form.data("grid-id");
                const url = form.data("refresh-grid-url");

                if (url) {
                    $.get(url, function (html) {
                        $(targetID).html(html);
                        alert(op.message);
                    });

                    // grid.load(url);
                }
            }

            if (form.data("close-on-success")) {
                generalModal.hide();
            }

            alert(op.message);
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




//SaveEdit
$(document).on("submit", "#UserFormEdit", function (e) {

    e.preventDefault();

    var formData = new FormData(this);

    $.ajax({

        url: "/UserManagement/Edit",

        type: "POST",

        data: formData,

        processData: false,

        contentType: false,

        headers: {
            RequestVerificationToken:
                $('input[name="__RequestVerificationToken"]', this).val()
        },

        success: function (res) {

            if (res.success) {

                $("#UserModal").modal("hide");

                LoadUsers();

                alert(res.message);
            }
            else {

                alert(res.message);
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
    const targetID = "#" + $(this).data("refresh-target-id");

    $.post(sendingUrl, {
        userID: $(this).data("id")
    })
        .done(function (op) {

            if (op.success) {

                $.get(refreshUrl, function (html) {
                    $(targetID).html(html);
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
