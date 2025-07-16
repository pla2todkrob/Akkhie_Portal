$(document).ready(function () {
    const ticketActionForm = $('#ticketActionForm');
    const resolveTicketBtn = $('#resolveTicketBtn');
    const acceptTicketBtn = $('#acceptTicketBtn');
    const fileUploader = $('#file-uploader');
    const fileListContainer = $('#file-list-container');
    const ticketId = $('input[name="TicketActionRequest.TicketId"]').val();

    // ฟังก์ชันสำหรับจัดการการอัพโหลดไฟล์
    const handleFileUpload = async () => {
        const files = fileUploader[0].files;
        if (files.length === 0) {
            return;
        }

        const formData = new FormData();
        for (let i = 0; i < files.length; i++) {
            formData.append('files', files[i]);
        }

        // แสดงสถานะกำลังอัพโหลด (ตัวอย่าง)
        fileListContainer.append('<div class="text-muted small my-2" id="upload-indicator"><span class="spinner-border spinner-border-sm me-2"></span>กำลังอัพโหลด...</div>');
        fileUploader.prop('disabled', true);

        try {
            const response = await fetch('/api/FileUpload/upload', {
                method: 'POST',
                body: formData,
                headers: {
                    // Token จำเป็นสำหรับการยืนยันตัวตนใน API
                    'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                }
            });

            const result = await response.json();

            if (result.success) {
                // วนลูปแสดงไฟล์ที่อัพโหลดสำเร็จ
                result.data.forEach(file => {
                    const fileHtml = `
                        <div class="alert alert-light alert-dismissible fade show p-2 small" role="alert">
                            <i class="bi bi-file-earmark-check-fill text-success"></i>
                            <span class="ms-1">${file.originalFileName}</span>
                            <button type="button" class="btn-close p-2" data-bs-dismiss="alert" aria-label="Close"></button>
                            <input type="hidden" name="UploadedFileIds" value="${file.id}" />
                        </div>`;
                    fileListContainer.append(fileHtml);
                });
                toastr.success('อัพโหลดไฟล์สำเร็จ');
            } else {
                toastr.error(result.message || 'เกิดข้อผิดพลาดในการอัพโหลดไฟล์');
            }

        } catch (error) {
            console.error('File upload error:', error);
            toastr.error('ไม่สามารถเชื่อมต่อกับเซิร์ฟเวอร์เพื่ออัพโหลดไฟล์ได้');
        } finally {
            // ล้างค่าใน input และคืนค่าสถานะ
            fileUploader.val('');
            $('#upload-indicator').remove();
            fileUploader.prop('disabled', false);
        }
    };

    // Event listener เมื่อมีการเลือกไฟล์
    fileUploader.on('change', handleFileUpload);


    // ฟังก์ชันสำหรับส่งข้อมูล Action (เช่น รับงาน, ปิดงาน)
    const submitAction = (url, data) => {
        // ... (โค้ดเดิมสำหรับ submit ajax)
        // เพิ่มการรวบรวม ID ของไฟล์เข้าไปใน data object
        data.UploadedFileIds = [];
        $('input[name="UploadedFileIds"]').each(function () {
            data.UploadedFileIds.push($(this).val());
        });

        // ... (ส่วนที่เหลือของโค้ด ajax เหมือนเดิม)
    };

    // ... (โค้ดเดิมสำหรับ event click ของปุ่ม acceptTicketBtn และ resolveTicketBtn)
    // ควรแน่ใจว่าตอนเรียก submitAction ได้ส่ง data ที่ถูกต้องไป

    // ตัวอย่างการเรียกใช้ใน resolveTicketBtn
    resolveTicketBtn.on('click', function () {
        const data = {
            TicketId: ticketId,
            CategoryId: $('#actionModel_CategoryId').val(),
            Comment: $('#actionModel_Comment').val(),
            UploadedFileIds: [] // เตรียม array ไว้
        };

        // รวบรวม ID จาก hidden input ที่สร้างขึ้นหลังอัพโหลดไฟล์สำเร็จ
        $('input[name="UploadedFileIds"]').each(function () {
            data.UploadedFileIds.push(parseInt($(this).val()));
        });

        // ส่งข้อมูลไปที่ Controller
        // $.ajax({...})
        console.log('Data to be sent:', data);
        // ในที่นี้คุณจะต้องมีฟังก์ชัน AJAX เพื่อส่ง data ไปยัง Controller Action ที่เหมาะสม
        // เช่น /Support/ResolveTicket
    });


});
