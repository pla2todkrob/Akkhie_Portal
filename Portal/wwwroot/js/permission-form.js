$(document).ready(function () {
    const form = $('#permissionForm');
    const submitButton = $('button[type="submit"][form="permissionForm"]');
    const spinner = submitButton.find('.spinner-border');
    const successUrl = form.data('success-url');

    form.on('submit', function (e) {
        e.preventDefault();

        // Basic client-side validation check
        if (!this.checkValidity()) {
            e.stopPropagation();
            $(this).addClass('was-validated');
            return;
        }

        // Disable button and show spinner
        submitButton.prop('disabled', true);
        spinner.removeClass('d-none');

        const formData = $(this).serialize();
        const actionUrl = $(this).attr('action');
        const method = $(this).attr('method');

        const processResponse = (response) => {
            if (response.success) {
                app.showSuccessToast(response.message || 'บันทึกข้อมูลสำเร็จ');
                setTimeout(() => {
                    window.location.href = successUrl;
                }, 1500);
            } else {
                app.showErrorAlert(response.message, response.errors);
                // Re-enable button and hide spinner on failure
                submitButton.prop('disabled', false);
                spinner.addClass('d-none');
            }
        };

        const processError = (jqXHR) => {
            if (jqXHR.responseJSON && jqXHR.responseJSON.errors) {
                const errors = Object.values(jqXHR.responseJSON.errors).flat();
                app.showErrorAlert('ข้อมูลไม่ถูกต้อง', errors);
            } else {
                app.showErrorAlert('เกิดข้อผิดพลาดในการบันทึกข้อมูล');
            }
            // Re-enable button and hide spinner on error
            submitButton.prop('disabled', false);
            spinner.addClass('d-none');
        };

        // AJAX submission
        $.ajax({
            url: actionUrl,
            type: method,
            data: formData,
            success: processResponse,
            error: processError
        });
    });
});
