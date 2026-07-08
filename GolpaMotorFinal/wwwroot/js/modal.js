const modalElement = document.getElementById('generalModal');

const generalModal = new bootstrap.Modal(modalElement);

async function openModal(url, title, id=null) {

    document.getElementById("generalModalTitle").innerHTML = title;

    document.getElementById("generalModalBody").innerHTML =
        '<div class="text-center p-5"><div class="spinner-border"></div></div>';

    generalModal.show();

    $.get(url, { userID: id }, function (result) {
        document.getElementById("generalModalBody").innerHTML = result;
    });
}


function closeModal() {

    generalModal.hide();

    document.getElementById("generalModalBody").innerHTML = "";

}

$(document).on("click", ".open-modal", function () {

    openModal(
        $(this).data("url"),
        $(this).data("title"),
        $(this).data("id"));
});

$(document).on("click", ".cancel-modal", function () {

    closeModal();
});
