// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// SweetAlert2 Helper Functions
function showSuccessAlert(message) {
    Swal.fire({
        icon: 'success',
        title: 'Başarılı!',
        text: message,
        confirmButtonText: 'Tamam',
        confirmButtonColor: '#198754'
    });
}

function showErrorAlert(message) {
    Swal.fire({
        icon: 'error',
        title: 'Hata!',
        text: message,
        confirmButtonText: 'Tamam',
        confirmButtonColor: '#dc3545'
    });
}

function showWarningAlert(message) {
    Swal.fire({
        icon: 'warning',
        title: 'Uyarı!',
        text: message,
        confirmButtonText: 'Tamam',
        confirmButtonColor: '#ffc107'
    });
}

function showInfoAlert(message) {
    Swal.fire({
        icon: 'info',
        title: 'Bilgi',
        text: message,
        confirmButtonText: 'Tamam',
        confirmButtonColor: '#0dcaf0'
    });
}

// TempData mesajlarını SweetAlert ile göster
document.addEventListener('DOMContentLoaded', function() {
    // SuccessMessage kontrolü
    const successMessage = document.querySelector('[data-tempdata-success]');
    if (successMessage) {
        showSuccessAlert(successMessage.textContent);
    }

    // ErrorMessage kontrolü
    const errorMessage = document.querySelector('[data-tempdata-error]');
    if (errorMessage) {
        showErrorAlert(errorMessage.textContent);
    }
});
