$(document).ready(function () {
    // Initialize DataTables
    const permissionTable = $('#permissionTable').DataTable({
        responsive: true,
        "language": {
            "url": "//cdn.datatables.net/plug-ins/1.11.5/i18n/th.json"
        },
        "columnDefs": [{
            "targets": 'no-sort',
            "orderable": false,
        }]
    });

    // Handle Delete Button Click
    $('#permissionTable').on('click', '.delete-btn', function () {
        const permissionId = $(this).data('id');
        const permissionName = $(this).data('name');
        const token = $('input[name="__RequestVerificationToken"]').val();

        app.showDeleteConfirmDialog({
            title: 'ยืนยันการลบสิทธิ์',
            html: `คุณต้องการลบสิทธิ์ "<b>${permissionName}</b>" ใช่หรือไม่?`,
            confirmButtonText: 'ยืนยัน, ลบเลย!'
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    url: `/Permission/Delete/${permissionId}`,
                    type: 'DELETE',
                    headers: {
                        'RequestVerificationToken': token
                    },
                    success: function (response) {
                        if (response.success) {
                            app.showSuccessToast(response.message);
                            // Use a timeout to allow the toast to be seen before reloading
                            setTimeout(() => location.reload(), 1500);
                        } else {
                            app.showErrorAlert(response.message || 'เกิดข้อผิดพลาดในการลบข้อมูล');
                        }
                    },
                    error: function () {
                        app.showErrorAlert('ไม่สามารถเชื่อมต่อกับเซิร์ฟเวอร์ได้');
                    }
                });
            }
        });
    });
});
