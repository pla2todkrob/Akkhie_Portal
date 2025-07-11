$(function () {
    'use strict';

    const modalElement = $('#globalModal');
    const modalContent = $('#globalModalContent');
    const csrfToken = $('input[name="__RequestVerificationToken"]').val();

    // Initialize DataTable
    const table = app.setupDataTable('#categoryTable', {
        columnDefs: [
            { orderable: false, targets: 3 },
            { className: "text-center", targets: 3 }
        ]
    });

    // --- Event Handlers ---

    // Handle Create button click
    $('#createCategoryBtn').on('click', function () {
        loadModalContent('/SupportCategory/_CreateOrEdit');
    });

    // Handle Edit button click (delegated)
    $('#categoryTable tbody').on('click', '.edit-btn', function () {
        const id = $(this).closest('tr').data('id');
        loadModalContent(`/SupportCategory/_CreateOrEdit/${id}`);
    });

    // Handle Delete button click (delegated)
    $('#categoryTable tbody').on('click', '.delete-btn', function () {
        const id = $(this).closest('tr').data('id');
        app.showConfirmDialog({
            title: 'ยืนยันการลบ',
            text: 'คุณแน่ใจหรือไม่ว่าต้องการลบหมวดหมู่นี้?',
            confirmText: 'ใช่, ลบเลย!',
            cancelText: 'ยกเลิก'
        }).then((result) => {
            if (result.isConfirmed) {
                deleteCategory(id);
            }
        });
    });

    // Handle form submission inside the modal
    modalElement.on('submit', '#categoryForm', function (e) {
        e.preventDefault();
        const form = $(this);
        if (!form[0].checkValidity()) {
            e.stopPropagation();
            form.addClass('was-validated');
            return;
        }

        const button = form.find('button[type="submit"]');
        const spinner = button.find('.spinner-border');
        button.prop('disabled', true);
        spinner.removeClass('d-none');

        const formData = new FormData(this);

        $.ajax({
            url: '/SupportCategory/Save',
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            headers: { 'RequestVerificationToken': csrfToken },
            success: function (response) {
                if (response.success) {
                    modalElement.modal('hide');
                    app.showSuccessToast(response.message);
                    setTimeout(() => location.reload(), 1500);
                } else {
                    app.showErrorToast(response.message || 'เกิดข้อผิดพลาด');
                }
            },
            error: function () {
                app.showErrorToast('ไม่สามารถเชื่อมต่อกับเซิร์ฟเวอร์ได้');
            },
            complete: function () {
                button.prop('disabled', false);
                spinner.addClass('d-none');
            }
        });
    });


    // --- Helper Functions ---

    function loadModalContent(url) {
        modalContent.load(url, function () {
            modalElement.modal('show');
        });
    }

    function deleteCategory(id) {
        $.ajax({
            url: '/SupportCategory/Delete',
            type: 'POST',
            data: { id: id },
            headers: { 'RequestVerificationToken': csrfToken },
            success: function (response) {
                if (response.success) {
                    app.showSuccessToast(response.message);
                    setTimeout(() => location.reload(), 1500);
                } else {
                    app.showErrorToast(response.message);
                }
            },
            error: function () {
                app.showErrorToast('เกิดข้อผิดพลาดในการลบข้อมูล');
            }
        });
    }
});
