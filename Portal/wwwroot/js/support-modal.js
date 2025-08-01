// Wrap in an IIFE (Immediately Invoked Function Expression) to avoid polluting the global scope
(function ($) {
    'use strict';

    // =========================================================================
    // SELECTOR CACHE
    // =========================================================================
    // Storing frequently used jQuery objects to improve performance
    const selectors = {
        supportModal: $('#supportModal'),
        // Problem Tab
        problemForm: $('#createTicketForm'),
        problemSubmitBtn: $('#submit-problem-btn'),
        relatedTicketDropdown: $('#relatedTicketId'),
        titleInput: $('#ticketTitle'),
        descriptionInput: $('#ticketDescription'),
        fileInput: $('#ticketFiles'),
        progressBar: $('#upload-progress-bar'),
        progressBarInner: $('#upload-progress-bar .progress-bar'),
        fileListContainer: $('#file-upload-list'),
        // Request Tab
        stockSearchInput: $('#stock-item-search'),
        stockItemsContainer: $('#stock-items-container'),
        cartItemsContainer: $('#cart-items-container'),
        cartItemCountBadge: $('#cart-item-count'),
        submitRequestBtn: $('#submit-request-btn'),
        purchaseRequestForm: $('#purchaseRequestForm'),
        submitPurchaseBtn: $('#submit-purchase-request-btn'),
        // History Tab
        historyTabBtn: $('#v-pills-history-tab'),
        historyContainer: $('#my-tickets-history-container'),
    };

    // =========================================================================
    // STATE VARIABLES
    // =========================================================================
    let uploadedFileIds = [];
    let cart = {}; // Object to store cart items { itemId: { name, quantity, stock }, ... }

    // =========================================================================
    // UTILITY FUNCTIONS
    // =========================================================================

    /**
     * Initializes the bootstrap-select plugin for a searchable dropdown.
     * @param {jQuery} element The dropdown element.
     * @param {string} url The URL to fetch data from.
     * @param {string} placeholder The placeholder text.
     */
    const initSearchableDropdown = (element, url, placeholder) => {
        element.selectpicker({
            liveSearch: true,
            title: placeholder,
            width: '100%',
            noneResultsText: 'ไม่พบข้อมูล'
        });

        $.getJSON(url, function (data) {
            element.empty();
            if (data && data.length > 0) {
                $.each(data, function (index, item) {
                    element.append($('<option></option>').val(item.value).text(item.text));
                });
            }
            element.selectpicker('refresh');
        }).fail(function () {
            console.error(`Failed to load data for searchable dropdown from ${url}`);
            element.selectpicker('refresh');
        });
    };

    // =========================================================================
    // PROBLEM TAB LOGIC
    // =========================================================================

    /**
     * Handles the file upload process via AJAX for the problem tab.
     * @param {FileList} files The files to upload.
     */
    const handleFileUpload = (files) => {
        // (Existing file upload logic from previous version)
        if (!files || files.length === 0) return;
        selectors.progressBar.show();
        Array.from(files).forEach(file => {
            const formData = new FormData();
            formData.append('file', file);
            const fileId = `file-${Date.now()}`;
            const fileElement = $(`
                <div id="${fileId}" class="alert alert-light p-2 d-flex align-items-center justify-content-between mb-2">
                    <div><i class="bi bi-file-earmark-arrow-up me-2"></i><span class="file-name">${file.name}</span><small class="text-muted ms-2">(${(file.size / 1024).toFixed(2)} KB)</small></div>
                    <div class="upload-status"><span class="spinner-border spinner-border-sm" role="status"></span></div>
                </div>`);
            selectors.fileListContainer.append(fileElement);
            $.ajax({
                url: '/api/FileUpload/Upload', type: 'POST', data: formData, processData: false, contentType: false,
                xhr: function () {
                    const xhr = new window.XMLHttpRequest();
                    xhr.upload.addEventListener('progress', function (evt) {
                        if (evt.lengthComputable) {
                            const percentComplete = Math.round((evt.loaded / evt.total) * 100);
                            selectors.progressBarInner.width(percentComplete + '%').attr('aria-valuenow', percentComplete);
                        }
                    }, false);
                    return xhr;
                },
                success: function (response) {
                    if (response.success && response.data) {
                        uploadedFileIds.push(response.data.id);
                        fileElement.find('.upload-status').html('<i class="bi bi-check-circle-fill text-success"></i>');
                        const removeBtn = $('<button type="button" class="btn btn-sm btn-outline-danger ms-2"><i class="bi bi-x"></i></button>');
                        removeBtn.on('click', function () {
                            fileElement.remove();
                            uploadedFileIds = uploadedFileIds.filter(id => id !== response.data.id);
                        });
                        fileElement.find('.upload-status').append(removeBtn);
                    } else {
                        fileElement.find('.upload-status').html(`<i class="bi bi-exclamation-triangle-fill text-danger" title="${response.message || 'Upload failed'}"></i>`);
                    }
                },
                error: function () { fileElement.find('.upload-status').html('<i class="bi bi-exclamation-triangle-fill text-danger" title="Upload error"></i>'); },
                complete: function () { selectors.progressBar.hide(); selectors.progressBarInner.width('0%'); }
            });
        });
        selectors.fileInput.val('');
    };

    /** Resets the "Report a Problem" form. */
    const resetProblemForm = () => {
        selectors.problemForm[0].reset();
        selectors.problemForm.removeClass('was-validated');
        selectors.relatedTicketDropdown.selectpicker('val', '');
        uploadedFileIds = [];
        selectors.fileListContainer.empty();
    };

    /** Initializes the "Report a Problem" tab. */
    const initProblemTab = () => {
        initSearchableDropdown(selectors.relatedTicketDropdown, '/Lookup/GetMyTickets', 'ค้นหา Ticket เก่า...');
        selectors.fileInput.off('change').on('change', function () { handleFileUpload(this.files); });
        selectors.problemForm.off('submit').on('submit', function (event) {
            event.preventDefault();
            event.stopPropagation();
            if (!this.checkValidity()) {
                $(this).addClass('was-validated');
                return;
            }
            app.showLoading(selectors.problemSubmitBtn);
            const formData = {
                RelatedTicketId: selectors.relatedTicketDropdown.val(),
                Title: selectors.titleInput.val(),
                Description: selectors.descriptionInput.val(),
                UploadedFileIds: uploadedFileIds
            };
            $.ajax({
                url: '/api/SupportTicket/CreateTicket', type: 'POST', contentType: 'application/json',
                data: JSON.stringify(formData),
                headers: { 'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
                success: function (response) {
                    if (response.success) {
                        app.showSuccessToast('แจ้งปัญหาสำเร็จแล้ว');
                        resetProblemForm();
                        selectors.supportModal.modal('hide');
                    } else { app.showErrorAlert(response.message || 'เกิดข้อผิดพลาด'); }
                },
                error: function () { app.showErrorAlert('ไม่สามารถเชื่อมต่อเซิร์ฟเวอร์ได้'); },
                complete: function () { app.hideLoading(selectors.problemSubmitBtn); }
            });
        });
    };

    // =========================================================================
    // REQUEST TAB LOGIC
    // =========================================================================

    /** Renders a single stock item card. */
    const renderStockItem = (item) => {
        const disabled = item.quantityInStock <= 0 ? 'disabled' : '';
        const badge = item.quantityInStock <= 0 ? '<span class="badge bg-danger">ของหมด</span>' : `<span class="badge bg-success">${item.quantityInStock} ชิ้น</span>`;
        return `
            <div class="card mb-2 stock-item" data-id="${item.id}" data-name="${item.name}" data-stock="${item.quantityInStock}">
                <div class="card-body p-2 d-flex justify-content-between align-items-center">
                    <div>
                        <h6 class="card-title mb-0 small">${item.name}</h6>
                        <p class="card-text text-muted small mb-0">${item.description || ''}</p>
                    </div>
                    <div class="d-flex align-items-center">
                        ${badge}
                        <button class="btn btn-sm btn-outline-primary ms-3 add-to-cart-btn" ${disabled}><i class="bi bi-plus"></i></button>
                    </div>
                </div>
            </div>`;
    };

    /** Loads and renders all stock items from the API. */
    const loadStockItems = () => {
        selectors.stockItemsContainer.html('<div class="text-center p-5"><span class="spinner-border"></span></div>');
        $.getJSON('/api/ITInventory/GetStockItemsForWithdrawal', function (items) {
            selectors.stockItemsContainer.empty();
            if (items && items.length > 0) {
                items.forEach(item => selectors.stockItemsContainer.append(renderStockItem(item)));
            } else {
                selectors.stockItemsContainer.html('<p class="text-center text-muted p-5">ไม่พบรายการอุปกรณ์ในสต็อก</p>');
            }
        });
    };

    /** Updates the view of the cart. */
    const updateCartView = () => {
        selectors.cartItemsContainer.empty();
        let totalItems = 0;
        if (Object.keys(cart).length === 0) {
            selectors.cartItemsContainer.html('<p class="text-center text-muted small mt-3">ตะกร้าของคุณว่างอยู่</p>');
            selectors.submitRequestBtn.prop('disabled', true);
        } else {
            for (const itemId in cart) {
                const item = cart[itemId];
                totalItems += item.quantity;
                const cartItemHtml = `
                    <div class="d-flex justify-content-between align-items-center mb-2 small">
                        <span>${item.name}</span>
                        <div class="input-group input-group-sm" style="width: 120px;">
                            <button class="btn btn-outline-secondary cart-qty-btn" type="button" data-id="${itemId}" data-change="-1">-</button>
                            <input type="text" class="form-control text-center cart-qty-input" value="${item.quantity}" readonly>
                            <button class="btn btn-outline-secondary cart-qty-btn" type="button" data-id="${itemId}" data-change="1" ${item.quantity >= item.stock ? 'disabled' : ''}>+</button>
                        </div>
                    </div>`;
                selectors.cartItemsContainer.append(cartItemHtml);
            }
            selectors.submitRequestBtn.prop('disabled', false);
        }
        selectors.cartItemCountBadge.text(totalItems);
    };

    /** Handles adding/removing items from the cart. */
    const updateCart = (itemId, change) => {
        if (!cart[itemId]) return;

        let newQuantity = cart[itemId].quantity + change;
        if (newQuantity <= 0) {
            delete cart[itemId];
        } else if (newQuantity > cart[itemId].stock) {
            newQuantity = cart[itemId].stock; // Cap at max stock
        }
        cart[itemId].quantity = newQuantity;
        updateCartView();
    };

    /** Resets the request tab forms. */
    const resetRequestForm = () => {
        cart = {};
        updateCartView();
        selectors.purchaseRequestForm[0].reset();
        selectors.purchaseRequestForm.removeClass('was-validated');
        selectors.stockSearchInput.val('');
        $('.stock-item').show(); // Show all items
    };

    /** Initializes the "Request Equipment" tab. */
    const initRequestTab = () => {
        loadStockItems();

        // Search functionality
        selectors.stockSearchInput.on('keyup', function () {
            const searchTerm = $(this).val().toLowerCase();
            $('.stock-item').each(function () {
                const itemName = $(this).data('name').toLowerCase();
                $(this).toggle(itemName.includes(searchTerm));
            });
        });

        // Add to cart
        selectors.stockItemsContainer.on('click', '.add-to-cart-btn', function () {
            const itemCard = $(this).closest('.stock-item');
            const itemId = itemCard.data('id');
            if (!cart[itemId]) {
                cart[itemId] = {
                    name: itemCard.data('name'),
                    quantity: 1,
                    stock: itemCard.data('stock')
                };
            } else {
                if (cart[itemId].quantity < cart[itemId].stock) {
                    cart[itemId].quantity++;
                }
            }
            updateCartView();
        });

        // Change quantity in cart
        selectors.cartItemsContainer.on('click', '.cart-qty-btn', function () {
            const itemId = $(this).data('id');
            const change = parseInt($(this).data('change'));
            updateCart(itemId, change);
        });

        // Submit withdrawal request
        selectors.submitRequestBtn.on('click', function () {
            const items = Object.keys(cart).map(id => ({ itemId: id, quantity: cart[id].quantity }));
            if (items.length === 0) return;

            app.showLoading(this);
            $.ajax({
                url: '/api/SupportTicket/CreateWithdrawalRequest',
                type: 'POST', contentType: 'application/json', data: JSON.stringify({ items }),
                headers: { 'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
                success: (res) => {
                    if (res.success) {
                        app.showSuccessToast('ส่งคำขอเบิกสำเร็จ');
                        resetRequestForm();
                        selectors.supportModal.modal('hide');
                    } else { app.showErrorAlert(res.message); }
                },
                error: () => app.showErrorAlert('เกิดข้อผิดพลาด'),
                complete: () => app.hideLoading(this)
            });
        });

        // Submit purchase request
        selectors.purchaseRequestForm.on('submit', function (e) {
            e.preventDefault();
            if (!this.checkValidity()) {
                $(this).addClass('was-validated');
                return;
            }
            app.showLoading(selectors.submitPurchaseBtn);
            const formData = {
                ItemName: $(this).find('[name="ItemName"]').val(),
                Quantity: $(this).find('[name="Quantity"]').val(),
                Specification: $(this).find('[name="Specification"]').val(),
                Reason: $(this).find('[name="Reason"]').val(),
            };
            $.ajax({
                url: '/api/SupportTicket/CreatePurchaseRequest',
                type: 'POST', contentType: 'application/json', data: JSON.stringify(formData),
                headers: { 'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
                success: (res) => {
                    if (res.success) {
                        app.showSuccessToast('ส่งคำขอจัดซื้อสำเร็จ');
                        resetRequestForm();
                        selectors.supportModal.modal('hide');
                    } else { app.showErrorAlert(res.message); }
                },
                error: () => app.showErrorAlert('เกิดข้อผิดพลาด'),
                complete: () => app.hideLoading(selectors.submitPurchaseBtn)
            });
        });
    };

    // =========================================================================
    // HISTORY TAB LOGIC
    // =========================================================================

    /** Renders the ticket history list. */
    const renderHistory = (tickets) => {
        selectors.historyContainer.empty();
        if (tickets && tickets.length > 0) {
            const historyHtml = tickets.map(ticket => `
                <a href="/Support/Details/${ticket.id}" class="list-group-item list-group-item-action">
                    <div class="d-flex w-100 justify-content-between">
                        <h6 class="mb-1">#${ticket.ticketNumber} - ${ticket.title}</h6>
                        <small>${new Date(ticket.createdAt).toLocaleDateString('th-TH')}</small>
                    </div>
                    <p class="mb-1 small">${ticket.categoryName || 'N/A'}</p>
                    <small><span class="badge rounded-pill ${app.getTicketStatusClass(ticket.status)}">${ticket.status}</span></small>
                </a>`).join('');
            selectors.historyContainer.html(`<div class="list-group">${historyHtml}</div>`);
        } else {
            selectors.historyContainer.html('<p class="text-center text-muted p-5">คุณยังไม่มีประวัติการแจ้งเรื่อง</p>');
        }
    };

    /** Loads the user's ticket history. */
    const loadHistory = () => {
        selectors.historyContainer.html('<div class="text-center p-5"><span class="spinner-border"></span></div>');
        $.getJSON('/api/SupportTicket/GetMyHistory', function (tickets) {
            renderHistory(tickets);
        });
    };

    /** Initializes the "My History" tab. */
    const initHistoryTab = () => {
        // Load history only when the tab is shown to save resources
        selectors.historyTabBtn.on('shown.bs.tab', function () {
            loadHistory();
        });
    };

    // =========================================================================
    // MAIN MODAL INITIALIZATION
    // =========================================================================
    $(function () {
        // Initialize all tabs when the modal is shown for the first time
        selectors.supportModal.one('show.bs.modal', function () {
            initProblemTab();
            initRequestTab();
            initHistoryTab();
        });

        // Reset all forms when the modal is hidden
        selectors.supportModal.on('hidden.bs.modal', function () {
            resetProblemForm();
            resetRequestForm();
            // History tab doesn't need resetting as it reloads on show
        });
    });

})(jQuery);
