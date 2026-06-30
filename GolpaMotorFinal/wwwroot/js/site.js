function showOperationResult(success, message) {

    Swal.fire({
        icon: success ? 'success' : 'error',
        title: success ? 'Success' : 'Error',
        text: message,
        confirmButtonColor: success ? '#28a745' : '#dc3545'
    });

}