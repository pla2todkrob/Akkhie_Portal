$(function () {
    'use strict';

    const form = $('#ticketActionForm');
    const csrfToken = form.find('input[name="__RequestVerificationToken"]').val();

    const acceptBtn = $('#acceptTicketBtn');
    const resolveBtn = $('#resolveTicketBtn');

    function handleAction(button, url, successMessage) {
        const spinner = button.find('.spinner-border');
        button.prop('disabled', true);
        spinner.removeClass('d-none');

        const formData = form.serialize();

        $.ajax({
            url: url,
            type: 'POST',
            data: formData,
            headers: { 'RequestVerificationToken': csrfToken },
            success: function (response) {
                if (response.success) {
                    app.showSuccessAlert(successMessage, {
                        onAction: () => location.reload()
                    });
                } else {
                    app.showErrorAlert(response.message || 'เกิดข้อผิดพลาดที่ไม่รู้จัก');
                }
            },
            error: function () {
                app.showErrorAlert('ไม่สามารถเชื่อมต่อกับเซิร์ฟเวอร์ได้');
            },
            complete: function () {
                button.prop('disabled', false);
                spinner.addClass('d-none');
            }
        });
    }

    acceptBtn.on('click', function () {
        app.showConfirmDialog({
            title: 'ยืนยันการรับงาน',
            text: 'คุณต้องการรับ Ticket นี้เพื่อดำเนินการต่อใช่หรือไม่?',
            confirmText: 'ใช่, รับงาน'
        }).then((result) => {
            if (result.isConfirmed) {
                handleAction($(this), '/Support/Accept', 'รับงานสำเร็จ!');
            }
        });
    });

    resolveBtn.on('click', function () {
        const categorySelect = $('#CategoryId');
        if (!categorySelect.val()) {
            app.showErrorToast('กรุณาเลือกหมวดหมู่ก่อนปิดงาน');
            categorySelect.focus();
            return;
        }

        app.showConfirmDialog({
            title: 'ยืนยันการแก้ไขเสร็จสิ้น',
            text: 'คุณได้ดำเนินการแก้ไขปัญหานี้เสร็จสิ้นแล้วใช่หรือไม่?',
            confirmText: 'ใช่, เสร็จสิ้น'
        }).then((result) => {
            if (result.isConfirmed) {
                handleAction($(this), '/Support/Resolve', 'บันทึกการแก้ไขสำเร็จ!');
            }
        });
    });
});
