// wwwroot/js/Division/formEvent.js
(function ($) {
    'use strict';

    $(document).ready(function () {
        app.showInfoAlert(
            'กรุณาทราบว่าการเปลี่ยนแปลงจะไม่มีผลหากไม่กด "บันทึก"',
            {
                title: 'คำแนะนำก่อนเริ่ม',
                showConfirmButton: true,
                confirmButtonText: 'เข้าใจแล้ว',
                onAction: () => {
                    app.showSuccessToast('ผู้ใช้ยืนยันว่าเข้าใจแล้ว');
                }
            }
        );

        app.handleFormSubmission({
            formSelector: '#SaveForm',
            successUrl: '/Division/Index',
            beforeSubmit: function () {
                // Reset validation
                const $form = $('#SaveForm');
                $form.removeData('validator');
                $form.removeData('unobtrusiveValidation');
                $.validator.unobtrusive.parse($form);
                return true;
            }
        });

        // Delete button handler (only needed in Edit.cshtml)
        $('#DeleteButton').click(function () {
            const targetId = $(this).data('id');
            const token = $('input[name="__RequestVerificationToken"]').val();

            app.showConfirmDialog({
                title: 'ยืนยันการลบข้อมูล',
                html: '<span class="text-center">คุณแน่ใจหรือไม่ว่าต้องการลบรายการนี้?<br />การกระทำนี้จะมีผลถึงแม้ไม่กด "บันทึก"</span>',
                confirmText: 'ยืนยันลบ'
            }).then((result) => {
                if (result.isConfirmed) {
                    fetch(`/Division/Delete/${targetId}`, {
                        method: 'GET',
                        headers: {
                            'RequestVerificationToken': token
                        }
                    })
                        .then(response => {
                            if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);
                            return response.json();
                        })
                        .then(data => {
                            if (!data.success) throw new Error(data.message || 'การลบข้อมูลไม่สำเร็จ');
                            return data;
                        })
                        .then(data => {
                            app.showSuccessAlert(data.message || 'ลบข้อมูลเรียบร้อยแล้ว')
                                .then(() => window.location.href = '/Division/Index');
                        })
                        .catch(error => {
                            app.showErrorAlert(error.message || 'เกิดข้อผิดพลาดในการลบข้อมูล');
                        });
                }
            });
        });
    });
})(jQuery);
