/**
 * จัดการการทำงานทั้งหมดของ Support Modal
 * - Tab แจ้งปัญหา: รวมถึงการอัพโหลดไฟล์ และการส่งฟอร์ม
 * - Tab เบิก/ขออุปกรณ์: รวมถึงการค้นหา, จัดการตะกร้า, ส่งคำขอเบิก และส่งคำขอซื้อ
 * - Tab ประวัติ: รวมถึงการแสดงรายการและรายละเอียด Ticket ของผู้ใช้
 */
$(document).ready(function () {

    // --- Common Variables ---
    const supportModal = $('#supportModal');
    const antiForgeryToken = $('input[name="__RequestVerificationToken"]').val();

    // --- Problem Tab Variables ---
    const createTicketForm = $('#createTicketForm');
    const fileUploader = $('#ticketFiles');
    const fileListContainer = $('#file-upload-list');
    const submitProblemBtn = $('#submit-problem-btn');

    // --- Request Tab Variables ---
    const stockItemsContainer = $('#stock-items-container');
    const stockItemSearch = $('#stock-item-search');
    const cartItemsContainer = $('#cart-items-container');
    const cartItemCount = $('#cart-item-count');
    const submitRequestBtn = $('#submit-request-btn');
    const purchaseRequestForm = $('#purchaseRequestForm');
    let cart = []; // Array to store cart items { id, name, quantity, max }
    let allStockItems = []; // To cache stock items for searching

    // --- History Tab Variables ---
    const historyListContainer = $('#my-tickets-history-container');
    const historyListView = $('#history-list-view');
    const historyDetailView = $('#history-detail-view');


    // =================================================================
    // TAB 1: PROBLEM REPORTING (แจ้งปัญหา)
    // =================================================================

    const handleFileUpload = async () => {
        const files = fileUploader[0].files;
        if (!files || files.length === 0) return;

        const formData = new FormData();
        for (let i = 0; i < files.length; i++) formData.append('files', files[i]);

        submitProblemBtn.prop('disabled', true).find('.spinner-border').removeClass('d-none');
        fileUploader.prop('disabled', true);

        try {
            const response = await fetch('/UploadFile/Upload', {
                method: 'POST',
                body: formData,
                headers: { 'RequestVerificationToken': antiForgeryToken }
            });
            const result = await response.json();
            if (result.success && result.data) {
                result.data.forEach(file => {
                    const fileHtml = `
                        <div class="alert alert-light alert-dismissible fade show p-2 small d-flex align-items-center" role="alert">
                            <i class="bi bi-file-earmark-check-fill text-success me-2"></i>
                            <span class="flex-grow-1">${file.originalFileName}</span>
                            <input type="hidden" name="UploadedFileIds" value="${file.id}" />
                            <button type="button" class="btn-close p-2" data-bs-dismiss="alert" aria-label="Close"></button>
                        </div>`;
                    fileListContainer.append(fileHtml);
                });
                toastr.success('อัพโหลดไฟล์สำเร็จ!');
            } else {
                toastr.error(result.message || 'เกิดข้อผิดพลาดในการอัพโหลดไฟล์');
            }
        } catch (error) {
            console.error('File upload error:', error);
            toastr.error('ไม่สามารถเชื่อมต่อกับเซิร์ฟเวอร์เพื่ออัพโหลดไฟล์ได้');
        } finally {
            fileUploader.val('').prop('disabled', false);
            submitProblemBtn.prop('disabled', false).find('.spinner-border').addClass('d-none');
        }
    };

    fileUploader.on('change', handleFileUpload);

    createTicketForm.on('submit', function (e) {
        e.preventDefault();
        if (!this.checkValidity()) {
            e.stopPropagation();
            $(this).addClass('was-validated');
            return;
        }
        $(this).removeClass('was-validated');

        const submitButton = $(this).find('button[type="submit"]');
        submitButton.prop('disabled', true).find('.spinner-border').removeClass('d-none');

        const formData = {
            Title: $('#ticketTitle').val(),
            Description: $('#ticketDescription').val(),
            RelatedTicketId: $('#relatedTicketId').val() || null,
            UploadedFileIds: []
        };
        fileListContainer.find('input[name="UploadedFileIds"]').each(function () {
            formData.UploadedFileIds.push(parseInt($(this).val()));
        });

        $.ajax({
            url: '/Support/CreateTicket',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(formData),
            headers: { 'RequestVerificationToken': antiForgeryToken },
            success: (response) => {
                if (response.success) {
                    toastr.success(response.message || 'แจ้งปัญหาสำเร็จแล้ว');
                    supportModal.modal('hide');
                } else {
                    toastr.error(response.message || 'เกิดข้อผิดพลาดในการสร้าง Ticket');
                }
            },
            error: () => toastr.error('ไม่สามารถส่งข้อมูลได้ กรุณาลองใหม่อีกครั้ง'),
            complete: () => submitButton.prop('disabled', false).find('.spinner-border').addClass('d-none')
        });
    });

    // =================================================================
    // TAB 2: ITEM WITHDRAWAL/PURCHASE (เบิก/ขออุปกรณ์)
    // =================================================================

    const renderStockItems = (items) => {
        stockItemsContainer.empty();
        if (!items || items.length === 0) {
            stockItemsContainer.html('<p class="text-center text-muted mt-4">ไม่พบรายการอุปกรณ์ในสต็อก</p>');
            return;
        }
        items.forEach(item => {
            const itemHtml = `
                <div class="card mb-2 shadow-sm">
                    <div class="card-body p-2 d-flex align-items-center">
                        <div class="flex-grow-1">
                            <h6 class="card-title mb-1">${item.name}</h6>
                            <small class="text-muted">คงเหลือ: ${item.quantity} ${item.unit}</small>
                        </div>
                        <button class="btn btn-sm btn-outline-primary add-to-cart-btn" 
                                data-id="${item.id}" 
                                data-name="${item.name}" 
                                data-max="${item.quantity}"
                                ${item.quantity <= 0 ? 'disabled' : ''}>
                            <i class="bi bi-cart-plus"></i> เพิ่ม
                        </button>
                    </div>
                </div>`;
            stockItemsContainer.append(itemHtml);
        });
    };

    const loadStockItems = () => {
        stockItemsContainer.html('<div class="text-center mt-4"><div class="spinner-border text-primary" role="status"><span class="visually-hidden">Loading...</span></div></div>');
        $.getJSON('/Support/GetStockItems')
            .done(data => {
                allStockItems = data;
                renderStockItems(allStockItems);
            })
            .fail(() => {
                stockItemsContainer.html('<p class="text-center text-danger mt-4">ไม่สามารถโหลดข้อมูลอุปกรณ์ได้</p>');
            });
    };

    const updateCartView = () => {
        cartItemsContainer.empty();
        let totalItems = 0;
        if (cart.length === 0) {
            cartItemsContainer.html('<p class="text-center text-muted small">ตะกร้าของคุณว่างอยู่</p>');
        } else {
            cart.forEach(item => {
                totalItems += item.quantity;
                const cartItemHtml = `
                    <div class="d-flex justify-content-between align-items-center mb-2">
                        <span class="small">${item.name}</span>
                        <div class="input-group input-group-sm" style="max-width: 120px;">
                            <button class="btn btn-outline-secondary update-cart-qty-btn" type="button" data-id="${item.id}" data-change="-1">-</button>
                            <input type="text" class="form-control text-center" value="${item.quantity}" readonly>
                            <button class="btn btn-outline-secondary update-cart-qty-btn" type="button" data-id="${item.id}" data-change="1">+</button>
                        </div>
                    </div>`;
                cartItemsContainer.append(cartItemHtml);
            });
        }
        cartItemCount.text(totalItems);
        submitRequestBtn.prop('disabled', cart.length === 0);
    };

    stockItemsContainer.on('click', '.add-to-cart-btn', function () {
        const btn = $(this);
        const item = {
            id: btn.data('id'),
            name: btn.data('name'),
            max: btn.data('max')
        };
        const existingItem = cart.find(ci => ci.id === item.id);
        if (existingItem) {
            if (existingItem.quantity < item.max) {
                existingItem.quantity++;
            } else {
                toastr.warning(`ไม่สามารถเบิก ${item.name} เกินจำนวนที่มีในสต็อกได้`);
            }
        } else {
            cart.push({ ...item, quantity: 1 });
        }
        updateCartView();
    });

    cartItemsContainer.on('click', '.update-cart-qty-btn', function () {
        const btn = $(this);
        const itemId = btn.data('id');
        const change = parseInt(btn.data('change'));
        const itemInCart = cart.find(ci => ci.id === itemId);

        if (itemInCart) {
            const newQty = itemInCart.quantity + change;
            if (newQty > 0 && newQty <= itemInCart.max) {
                itemInCart.quantity = newQty;
            } else if (newQty > itemInCart.max) {
                toastr.warning(`ไม่สามารถเบิก ${itemInCart.name} เกินจำนวนที่มีในสต็อกได้`);
            } else {
                cart = cart.filter(ci => ci.id !== itemId); // Remove item if quantity is 0
            }
        }
        updateCartView();
    });

    stockItemSearch.on('keyup', function () {
        const searchTerm = $(this).val().toLowerCase();
        const filteredItems = allStockItems.filter(item => item.name.toLowerCase().includes(searchTerm));
        renderStockItems(filteredItems);
    });

    submitRequestBtn.on('click', function () {
        // Logic to submit withdrawal request
        const btn = $(this);
        btn.prop('disabled', true).find('.spinner-border').removeClass('d-none');

        const requestData = {
            Items: cart.map(item => ({ ItemId: item.id, Quantity: item.quantity }))
        };

        $.ajax({
            url: '/Support/CreateWithdrawalRequest',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(requestData),
            headers: { 'RequestVerificationToken': antiForgeryToken },
            success: (response) => {
                if (response.success) {
                    toastr.success(response.message || 'ส่งคำขอเบิกสำเร็จ');
                    cart = [];
                    updateCartView();
                    supportModal.modal('hide');
                } else {
                    toastr.error(response.message || 'เกิดข้อผิดพลาดในการส่งคำขอ');
                }
            },
            error: () => toastr.error('ไม่สามารถส่งข้อมูลได้'),
            complete: () => btn.prop('disabled', false).find('.spinner-border').addClass('d-none')
        });
    });

    purchaseRequestForm.on('submit', function (e) {
        e.preventDefault();
        const form = $(this);
        if (!this.checkValidity()) {
            e.stopPropagation();
            form.addClass('was-validated');
            return;
        }
        form.removeClass('was-validated');
        const btn = form.find('button[type="submit"]');
        btn.prop('disabled', true).find('.spinner-border').removeClass('d-none');

        const requestData = {
            ItemName: form.find('[name="ItemName"]').val(),
            Quantity: parseInt(form.find('[name="Quantity"]').val()),
            Specification: form.find('[name="Specification"]').val(),
            Reason: form.find('[name="Reason"]').val()
        };

        $.ajax({
            url: '/Support/CreatePurchaseRequest',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(requestData),
            headers: { 'RequestVerificationToken': antiForgeryToken },
            success: (response) => {
                if (response.success) {
                    toastr.success(response.message || 'ส่งคำขอจัดซื้อสำเร็จ');
                    form[0].reset();
                    $('#purchase-request-form-collapse').collapse('hide');
                } else {
                    toastr.error(response.message || 'เกิดข้อผิดพลาดในการส่งคำขอ');
                }
            },
            error: () => toastr.error('ไม่สามารถส่งข้อมูลได้'),
            complete: () => btn.prop('disabled', false).find('.spinner-border').addClass('d-none')
        });
    });


    // =================================================================
    // TAB 3: TICKET HISTORY (ประวัติ)
    // =================================================================

    const loadMyTickets = () => {
        historyListContainer.html('<div class="text-center p-4"><div class="spinner-border" role="status"><span class="visually-hidden">Loading...</span></div></div>');
        $.get('/Support/GetMyTickets').done(data => historyListContainer.html(data)).fail(() => historyListContainer.html('<p class="text-danger text-center p-4">ไม่สามารถโหลดประวัติได้</p>'));
    };

    historyListContainer.on('click', '.view-history-detail-btn', function () {
        const ticketId = $(this).data('id');
        historyDetailView.html('<div class="text-center p-4"><div class="spinner-border" role="status"></div></div>').show();
        historyListView.hide();

        $.get(`/Support/GetTicketDetailPartial/${ticketId}`).done(data => historyDetailView.html(data)).fail(() => historyDetailView.html('<p class="text-danger text-center p-4">ไม่สามารถโหลดข้อมูลได้</p>'));
    });

    historyDetailView.on('click', '#back-to-history-list', function () {
        historyDetailView.hide().empty();
        historyListView.show();
    });

    // =================================================================
    // MODAL EVENT HANDLING
    // =================================================================

    supportModal.on('hidden.bs.modal', function () {
        createTicketForm[0].reset();
        createTicketForm.removeClass('was-validated');
        fileListContainer.empty();
        purchaseRequestForm[0].reset();
        purchaseRequestForm.removeClass('was-validated');
        cart = [];
        updateCartView();
        historyDetailView.hide().empty();
        historyListView.show();
    });

    $('button[data-bs-toggle="pill"]').on('show.bs.tab', function (e) {
        const targetTab = $(e.target).attr('id');
        if (targetTab === 'v-pills-history-tab' && historyListContainer.is(':empty')) {
            loadMyTickets();
        }
        if (targetTab === 'v-pills-request-tab' && stockItemsContainer.is(':empty')) {
            loadStockItems();
        }
    });

});