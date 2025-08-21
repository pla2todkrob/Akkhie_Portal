$(function () {
    'use strict';
    const table = app.setupDataTable('#permissionTable', { columnDefs: [{ orderable: false, targets: 3 }] });

    $('#createPermissionBtn').on('click', () => {
        app.showGlobalModal({ url: '/Permission/_CreateOrEdit', title: 'สร้างสิทธิ์ใหม่' });
    });

    $('#permissionTable').on('click', '.edit-btn', function () {
        const id = $(this).closest('tr').data('id');
        app.showGlobalModal({ url: `/Permission/_CreateOrEdit/${id}`, title: 'แก้ไขสิทธิ์' });
    });

    $('#globalModal').on('submit', '#permissionForm', function (e) {
        e.preventDefault();
        const form = $(this);
        if (!form.valid()) return;

        const button = form.find('button[type="submit"]');
        app.showLoading(button);

        $.ajax({
            url: form.attr('action'),
            type: 'POST',
            data: form.serialize(),
            success: function (response) {
                if (response.success) {
                    $('#globalModal').modal('hide');
                    app.showSuccessToast(response.message || 'บันทึกข้อมูลสำเร็จ');
                    setTimeout(() => location.reload(), 1500);
                } else {
                    app.showErrorToast(response.message || 'เกิดข้อผิดพลาด');
                }
            },
            error: () => app.showErrorToast('ไม่สามารถเชื่อมต่อเซิร์ฟเวอร์ได้'),
            complete: () => app.hideLoading(button)
        });
    });

    $('#permissionTable').on('click', '.delete-btn', function () {
        const id = $(this).closest('tr').data('id');
        const token = $('input[name="__RequestVerificationToken"]').val();

        app.showConfirmDialog({
            title: 'ยืนยันการลบ',
            text: 'คุณแน่ใจหรือไม่ว่าต้องการลบสิทธิ์นี้?'
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    url: '/Permission/Delete',
                    type: 'POST',
                    data: { id: id },
                    headers: { 'RequestVerificationToken': token },
                    success: function (response) {
                        if (response.success) {
                            app.showSuccessToast(response.message || 'ลบข้อมูลสำเร็จ');
                            setTimeout(() => location.reload(), 1500);
                        } else {
                            app.showErrorToast(response.message);
                        }
                    },
                    error: () => app.showErrorToast('เกิดข้อผิดพลาดในการลบข้อมูล')
                });
            }
        });
    });
});