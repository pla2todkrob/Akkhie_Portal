$(function () {
    'use strict';

    // =========================================================================
    // SELECTOR CACHE
    // =========================================================================
    const selectors = {
        form: $('#ticketActionForm'),
        acceptBtn: $('#acceptTicketBtn'),
        resolveBtn: $('#resolveTicketBtn'),
        closeBtn: $('#closeTicketBtn'),
        cancelBtn: $('#cancelTicketBtn'),
        rejectBtn: $('#rejectTicketBtn'),
        ticketIdInput: $('input[name="TicketId"]'),
        assigneeDropdown: $('#AssignedToEmployeeId'),
        priorityDropdown: $('#Priority'),
        categoryDropdown: $('#CategoryId'),
        commentTextarea: $('#Comment'),
        csrfToken: $('input[name="__RequestVerificationToken"]')
    };

    // =========================================================================
    // UTILITY FUNCTIONS
    // =========================================================================

    /**
     * A generic function to handle AJAX form submissions for ticket actions.
     * @param {object} options - Configuration for the AJAX call.
     * @param {string} options.url - The target URL for the request.
     * @param {object} options.data - The data payload to send.
     * @param {jQuery} options.button - The jQuery object for the button that was clicked.
     */
    const submitAction = ({ url, data, button }) => {
        app.showLoading(button);

        $.ajax({
            url: url,
            type: 'POST',
            data: data,
            headers: {
                'RequestVerificationToken': selectors.csrfToken.val()
            },
            success: function (response) {
                if (response.success) {
                    app.showSuccessToast(response.message || 'ดำเนินการสำเร็จ');
                    setTimeout(() => location.reload(), 1500);
                } else {
                    app.showErrorAlert(response.message || 'เกิดข้อผิดพลาด');
                }
            },
            error: function () {
                app.showErrorAlert('ไม่สามารถเชื่อมต่อกับเซิร์ฟเวอร์ได้');
            },
            complete: function () {
                app.hideLoading(button);
            }
        });
    };

    // =========================================================================
    // EVENT LISTENERS
    // =========================================================================

    /**
     * Handles the click event for the "Accept Ticket" button.
     */
    selectors.acceptBtn.on('click', function () {
        const ticketId = selectors.ticketIdInput.val();
        const assignedTo = selectors.assigneeDropdown.val();
        const priority = selectors.priorityDropdown.val();

        if (!priority) {
            app.showErrorToast('กรุณากำหนดความสำคัญของงาน');
            return;
        }

        app.showConfirmDialog({
            title: 'ยืนยันการรับงาน?',
            text: 'สถานะของ Ticket จะถูกเปลี่ยนเป็น "In Progress"',
            icon: 'question',
            confirmButtonText: 'ยืนยัน'
        }).then((result) => {
            if (result.isConfirmed) {
                submitAction({
                    url: '/Support/Accept',
                    data: {
                        TicketId: ticketId,
                        AssignedToEmployeeId: assignedTo,
                        Priority: priority
                    },
                    button: $(this)
                });
            }
        });
    });

    /**
     * Handles the click event for the "Resolve Ticket" button.
     */
    selectors.resolveBtn.on('click', function () {
        const ticketId = selectors.ticketIdInput.val();
        const categoryId = selectors.categoryDropdown.val();
        const comment = selectors.commentTextarea.val();

        if (!categoryId) {
            app.showErrorToast('กรุณาเลือกหมวดหมู่ของการแก้ไข');
            return;
        }

        app.showConfirmDialog({
            title: 'ยืนยันการปิดงานแก้ไข?',
            text: 'สถานะของ Ticket จะถูกเปลี่ยนเป็น "Resolved" และจะส่งอีเมลแจ้งผู้ใช้',
            icon: 'question',
            confirmButtonText: 'ยืนยัน'
        }).then((result) => {
            if (result.isConfirmed) {
                submitAction({
                    url: '/Support/Resolve',
                    data: {
                        TicketId: ticketId,
                        CategoryId: categoryId,
                        Comment: comment
                    },
                    button: $(this)
                });
            }
        });
    });

    /**
     * Handles the click event for the "Close Ticket" button.
     */
    selectors.closeBtn.on('click', function () {
        const ticketId = selectors.ticketIdInput.val();

        app.showConfirmDialog({
            title: 'ยืนยันการปิดงาน?',
            text: 'คุณพอใจกับการแก้ไขและต้องการปิด Ticket นี้ใช่หรือไม่?',
            icon: 'success',
            confirmButtonText: 'ใช่, ปิดงานเลย'
        }).then((result) => {
            if (result.isConfirmed) {
                submitAction({
                    url: '/Support/Close',
                    data: { TicketId: ticketId },
                    button: $(this)
                });
            }
        });
    });

    /**
     * Handles the click event for the "Cancel Ticket" button.
     */
    selectors.cancelBtn.on('click', function () {
        const ticketId = selectors.ticketIdInput.val();

        app.showConfirmDialog({
            title: 'ยืนยันการยกเลิก Ticket?',
            text: 'กรุณาระบุเหตุผลในการยกเลิก Ticket นี้',
            icon: 'warning',
            input: 'textarea',
            inputPlaceholder: 'ระบุเหตุผลที่นี่...',
            confirmButtonText: 'ยืนยันการยกเลิก',
            confirmButtonColor: '#d33',
            showCancelButton: true,
            cancelButtonText: 'กลับ',
            inputValidator: (value) => {
                if (!value) {
                    return 'กรุณาระบุเหตุผลในการยกเลิก!'
                }
            }
        }).then((result) => {
            if (result.isConfirmed && result.value) {
                submitAction({
                    url: '/Support/Cancel',
                    data: {
                        TicketId: ticketId,
                        Comment: result.value
                    },
                    button: $(this)
                });
            }
        });
    });

    selectors.rejectBtn.on('click', function () {
        const ticketId = selectors.ticketIdInput.val();

        app.showConfirmDialog({
            title: 'ยืนยันการปฏิเสธ Ticket?',
            text: 'กรุณาระบุเหตุผลในการปฏิเสธ Ticket นี้',
            icon: 'warning',
            input: 'textarea',
            inputPlaceholder: 'ระบุเหตุผลที่นี่...',
            confirmButtonText: 'ยืนยันการปฏิเสธ',
            confirmButtonColor: '#d33',
            showCancelButton: true,
            cancelButtonText: 'กลับ',
            inputValidator: (value) => {
                if (!value) {
                    return 'กรุณาระบุเหตุผลในการปฏิเสธ!'
                }
            }
        }).then((result) => {
            if (result.isConfirmed && result.value) {
                submitAction({
                    url: '/Support/Reject',
                    data: {
                        TicketId: ticketId,
                        Comment: result.value
                    },
                    button: $(this)
                });
            }
        });
    });
});
