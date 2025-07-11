// File: wwwroot/js/support-modal.js
// Description: Controls the functionality of the redesigned Help Center modal.
(function ($) {
    'use strict';

    // --- State Management ---
    let allStockItems = [];
    let cart = {}; // { itemId: { itemData, quantity } }
    let myTicketsCache = [];

    // --- Utility Functions ---
    const getCsrfToken = () => document.querySelector('input[name="__RequestVerificationToken"]')?.value;
    const escapeHtml = (unsafe) => unsafe ? unsafe.toString().replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;").replace(/"/g, "&quot;").replace(/'/g, "&#039;") : '';
    const toggleSpinner = (button, show) => {
        const spinner = button.querySelector('.spinner-border');
        if (show) {
            button.disabled = true;
            spinner?.classList.remove('d-none');
        } else {
            button.disabled = false;
            spinner?.classList.add('d-none');
        }
    };

    // --- Module: Problem Tab ---
    const problemTabLogic = (() => {
        const form = document.getElementById('createTicketForm');
        const submitButton = document.getElementById('submit-problem-btn');

        const handleSubmit = (e) => {
            e.preventDefault();
            if (!form.checkValidity()) {
                e.stopPropagation();
                form.classList.add('was-validated');
                return;
            }
            toggleSpinner(submitButton, true);
            fetch('/Support/Create', {
                method: 'POST',
                body: new FormData(form)
            })
                .then(res => res.json())
                .then(data => {
                    if (data.success) {
                        bootstrap.Modal.getInstance(document.getElementById('supportModal')).hide();
                        window.app.showSuccessAlert('ส่งเรื่องสำเร็จ!', { html: `Ticket ของคุณคือ <b>${data.data.ticketNumber}</b>` });
                    } else {
                        window.app.showErrorAlert('ส่งเรื่องไม่สำเร็จ', { html: data.errors?.join('<br>') || data.message });
                    }
                })
                .catch(err => window.app.showErrorAlert('เกิดข้อผิดพลาด', { text: err.message }))
                .finally(() => toggleSpinner(submitButton, false));
        };

        return {
            init: () => form?.addEventListener('submit', handleSubmit),
            prefill: (ticket) => {
                document.getElementById('ticketTitle').value = `[แจ้งซ้ำ] ${ticket.title}`;
                document.getElementById('ticketDescription').value = `อ้างอิงถึง Ticket เดิม: ${ticket.ticketNumber}\n\n-----------------\n`;
                bootstrap.Tab.getOrCreateInstance(document.getElementById('v-pills-problem-tab')).show();
                setTimeout(() => document.getElementById('ticketDescription').focus(), 150);
            },
            reset: () => {
                form?.reset();
                form?.classList.remove('was-validated');
            }
        };
    })();

    // --- Module: History Tab ---
    const historyTabLogic = (() => {
        const tab = document.getElementById('v-pills-history-tab');
        const listContainer = document.getElementById('my-tickets-history-container');
        const listView = document.getElementById('history-list-view');
        const detailView = document.getElementById('history-detail-view');

        const showListView = () => {
            if (detailView) detailView.style.display = 'none';
            if (listView) listView.style.display = 'block';
            if (detailView) detailView.innerHTML = '';
        };

        const showDetailView = () => {
            if (listView) listView.style.display = 'none';
            if (detailView) detailView.style.display = 'block';
        };

        const loadHistory = () => {
            listContainer.innerHTML = `<div class="text-center p-5"><div class="spinner-border text-primary"></div></div>`;
            fetch('/Lookup/GetMySupportTickets')
                .then(res => res.json())
                .then(data => {
                    if (data.success) {
                        myTicketsCache = data.data;
                        renderList(myTicketsCache);
                    } else throw new Error(data.message);
                })
                .catch(() => listContainer.innerHTML = `<div class="alert alert-warning">ไม่สามารถโหลดประวัติได้</div>`);
        };

        const renderList = (tickets) => {
            if (!tickets || tickets.length === 0) {
                listContainer.innerHTML = `<div class="text-center p-4 text-muted border bg-light rounded"><i class="bi bi-journal-x fs-2"></i><p class="mt-2 mb-0">ไม่พบประวัติการแจ้งปัญหา</p></div>`;
                return;
            }
            const rows = tickets.map(t => `
                <a href="#" class="list-group-item list-group-item-action view-details-btn" data-id="${t.id}">
                    <div class="d-flex w-100 justify-content-between">
                        <h6 class="mb-1">${escapeHtml(t.title)}</h6>
                        <small class="text-muted">${new Date(t.createdAt).toLocaleDateString('th-TH')}</small>
                    </div>
                    <p class="mb-1 small">${escapeHtml(t.ticketNumber)}</p>
                    <small><span class="badge bg-info-subtle text-info-emphasis rounded-pill">${escapeHtml(t.statusName)}</span></small>
                </a>`).join('');
            listContainer.innerHTML = `<div class="list-group">${rows}</div>`;
        };

        const loadDetails = (ticketId) => {
            showDetailView();
            detailView.innerHTML = `<div class="text-center p-5"><div class="spinner-border text-primary"></div></div>`;
            fetch(`/Lookup/GetTicketDetails?id=${ticketId}`)
                .then(res => res.json())
                .then(response => {
                    if (response.success) renderDetails(response.data);
                    else throw new Error(response.message);
                })
                .catch(() => detailView.innerHTML = `<div class="alert alert-warning">ไม่สามารถโหลดรายละเอียดได้</div>`);
        };

        const renderDetails = (ticket) => {
            const historyHtml = ticket.history.map(h => `
                <div class="d-flex position-relative mb-3">
                    <div class="flex-shrink-0"><i class="bi bi-person-circle fs-3 text-secondary"></i></div>
                    <div class="flex-grow-1 ms-3">
                        <h6 class="mt-0 mb-1">${escapeHtml(h.actionDescription)} <small class="text-muted fw-normal">- ${escapeHtml(h.actionBy)}</small></h6>
                        ${h.comment ? `<p class="mb-1 small">${escapeHtml(h.comment)}</p>` : ''}
                        <small class="text-muted">${new Date(h.actionDate).toLocaleString('th-TH')}</small>
                    </div>
                </div>`).join('<hr class="my-2 border-light">');

            detailView.innerHTML = `
                <button type="button" class="btn btn-outline-secondary btn-sm mb-3" id="back-to-history-list"><i class="bi bi-arrow-left"></i> กลับ</button>
                <div class="card border-0">
                    <div class="card-header bg-light d-flex justify-content-between align-items-center">
                        <h5 class="mb-0">Ticket: ${ticket.ticketNumber}</h5>
                        <span class="badge bg-primary rounded-pill">${ticket.status}</span>
                    </div>
                    <div class="card-body">
                        <h5 class="card-title">${escapeHtml(ticket.title)}</h5>
                        <p class="card-text">${escapeHtml(ticket.description)}</p>
                        <hr>
                        <h6>ประวัติ</h6>
                        <div class="mt-3" style="max-height: 250px; overflow-y: auto;">${historyHtml || '<p class="text-muted small">ยังไม่มีประวัติ</p>'}</div>
                    </div>
                </div>`;
            document.getElementById('back-to-history-list').addEventListener('click', showListView);
        };

        return {
            init: () => {
                tab?.addEventListener('shown.bs.tab', loadHistory);
                listView?.addEventListener('click', (e) => {
                    const link = e.target.closest('.view-details-btn');
                    if (link) {
                        e.preventDefault();
                        loadDetails(link.dataset.id);
                    }
                });
            },
            reset: () => {
                myTicketsCache = [];
                if (listContainer) listContainer.innerHTML = '';
                showListView();
            }
        };
    })();

    // --- Module: Request Tab ---
    const requestTabLogic = (() => {
        const tab = document.getElementById('v-pills-request-tab');
        const searchInput = document.getElementById('stock-item-search');
        const itemsContainer = document.getElementById('stock-items-container');
        const cartContainer = document.getElementById('cart-items-container');
        const cartCountBadge = document.getElementById('cart-item-count');
        const submitRequestBtn = document.getElementById('submit-request-btn');
        const purchaseForm = document.getElementById('purchaseRequestForm');
        const submitPurchaseBtn = document.getElementById('submit-purchase-request-btn');

        const loadStock = () => {
            if (allStockItems.length > 0) {
                renderStock();
                return;
            }
            itemsContainer.innerHTML = `<div class="text-center p-5"><div class="spinner-border text-primary"></div></div>`;
            fetch('/Lookup/GetITStockItems')
                .then(res => res.json())
                .then(data => {
                    if (data.success) {
                        allStockItems = data.data;
                        renderStock();
                    } else throw new Error(data.message);
                })
                .catch(() => itemsContainer.innerHTML = `<div class="alert alert-warning">ไม่สามารถโหลดรายการได้</div>`);
        };

        const renderStock = () => {
            const filter = searchInput.value.toLowerCase();
            const filteredItems = allStockItems.filter(item => item.name.toLowerCase().includes(filter));
            itemsContainer.innerHTML = filteredItems.length === 0
                ? `<div class="text-center p-4 text-muted border bg-light rounded"><p class="mb-0">ไม่พบรายการที่ค้นหา</p></div>`
                : filteredItems.map(item => `
                    <div class="d-flex justify-content-between align-items-center p-2 border-bottom">
                        <div>
                            <h6 class="mb-0 small">${escapeHtml(item.name)}</h6>
                            <small class="text-muted">คงเหลือ: ${item.quantity}</small>
                        </div>
                        <button class="btn btn-sm btn-outline-primary add-to-cart-btn" data-item-id="${item.itemId}" ${item.quantity <= 0 ? 'disabled' : ''}><i class="bi bi-plus"></i></button>
                    </div>`).join('');
        };

        const renderCart = () => {
            const cartItems = Object.values(cart);
            cartCountBadge.textContent = cartItems.reduce((sum, { quantity }) => sum + quantity, 0);
            submitRequestBtn.disabled = cartItems.length === 0;
            if (cartItems.length === 0) {
                cartContainer.innerHTML = `<div class="text-center text-muted p-4"><p class="mb-0">ตะกร้าว่างเปล่า</p></div>`;
                return;
            }
            cartContainer.innerHTML = cartItems.map(({ itemData, quantity }) => `
                <div class="d-flex justify-content-between align-items-center mb-2">
                    <span class="small">${escapeHtml(itemData.name)}</span>
                    <div class="input-group input-group-sm" style="width: 120px;">
                        <button class="btn btn-outline-secondary cart-quantity-btn" data-item-id="${itemData.itemId}" data-change="-1">-</button>
                        <input type="text" class="form-control text-center" value="${quantity}" readonly>
                        <button class="btn btn-outline-secondary cart-quantity-btn" data-item-id="${itemData.itemId}" data-change="1">+</button>
                    </div>
                </div>`).join('');
        };

        const updateCart = (itemId, change) => {
            const item = allStockItems.find(i => i.itemId == itemId);
            if (!item) return;

            let currentQuantity = cart[itemId] ? cart[itemId].quantity : 0;
            let newQuantity = currentQuantity + change;

            if (newQuantity > item.quantity) {
                window.app.showErrorToast('ไม่สามารถเบิกเกินจำนวนคงเหลือ');
                newQuantity = item.quantity;
            }

            if (newQuantity <= 0) {
                delete cart[itemId];
            } else {
                cart[itemId] = { itemData: item, quantity: newQuantity };
            }
            renderCart();
        };

        const handleSubmitRequest = () => {
            const token = getCsrfToken();
            const payload = { items: Object.values(cart).map(c => ({ itemId: c.itemData.itemId, quantity: c.quantity })) };
            toggleSpinner(submitRequestBtn, true);
            fetch('/Support/CreateWithdrawal', { method: 'POST', headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': token }, body: JSON.stringify(payload) })
                .then(res => res.json())
                .then(data => {
                    if (data.success) {
                        bootstrap.Modal.getInstance(document.getElementById('supportModal')).hide();
                        window.app.showSuccessAlert('ส่งคำขอเบิกสำเร็จ!', { html: `Ticket ของคุณคือ <b>${data.data.ticketNumber}</b>` });
                    } else {
                        window.app.showErrorAlert('ส่งคำขอไม่สำเร็จ', { html: data.errors?.join('<br>') || data.message });
                    }
                })
                .catch(() => window.app.showErrorAlert('เกิดข้อผิดพลาดในการเชื่อมต่อ'))
                .finally(() => toggleSpinner(submitRequestBtn, false));
        };

        const handlePurchaseSubmit = (e) => {
            e.preventDefault();
            if (!purchaseForm.checkValidity()) { e.stopPropagation(); purchaseForm.classList.add('was-validated'); return; }
            toggleSpinner(submitPurchaseBtn, true);
            const payload = Object.fromEntries(new FormData(purchaseForm).entries());
            fetch('/Support/CreatePurchase', { method: 'POST', headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': getCsrfToken() }, body: JSON.stringify(payload) })
                .then(res => res.json())
                .then(data => {
                    if (data.success) {
                        bootstrap.Modal.getInstance(document.getElementById('supportModal')).hide();
                        window.app.showSuccessAlert('ส่งคำขอจัดซื้อสำเร็จ!', { html: `Ticket ของคุณคือ <b>${data.data.ticketNumber}</b>` });
                    } else {
                        window.app.showErrorAlert('ส่งคำขอไม่สำเร็จ', { html: data.errors?.join('<br>') || data.message });
                    }
                })
                .catch(() => window.app.showErrorAlert('เกิดข้อผิดพลาดในการเชื่อมต่อ'))
                .finally(() => toggleSpinner(submitPurchaseBtn, false));
        };

        return {
            init: () => {
                tab?.addEventListener('shown.bs.tab', loadStock);
                searchInput?.addEventListener('input', renderStock);
                submitRequestBtn?.addEventListener('click', handleSubmitRequest);
                purchaseForm?.addEventListener('submit', handlePurchaseSubmit);
                itemsContainer?.addEventListener('click', (e) => {
                    const btn = e.target.closest('.add-to-cart-btn');
                    if (btn) updateCart(btn.dataset.itemId, 1);
                });
                cartContainer?.addEventListener('click', (e) => {
                    const btn = e.target.closest('.cart-quantity-btn');
                    if (btn) updateCart(btn.dataset.itemId, parseInt(btn.dataset.change));
                });
            },
            reset: () => {
                cart = {};
                allStockItems = [];
                if (itemsContainer) itemsContainer.innerHTML = '';
                if (searchInput) searchInput.value = '';
                if (purchaseForm) {
                    purchaseForm.reset();
                    purchaseForm.classList.remove('was-validated');
                }
                const collapseEl = document.getElementById('purchase-request-form-collapse');
                if (collapseEl) bootstrap.Collapse.getOrCreateInstance(collapseEl).hide();
                renderCart();
            }
        };
    })();

    // --- Global Modal Control ---
    const resetAllStates = () => {
        problemTabLogic.reset();
        requestTabLogic.reset();
        historyTabLogic.reset();
        // Set the first tab as active
        const firstTab = document.getElementById('v-pills-problem-tab');
        if (firstTab) bootstrap.Tab.getOrCreateInstance(firstTab).show();
    };

    // --- Initializer ---
    const supportModalEl = document.getElementById('supportModal');
    if (supportModalEl) {
        problemTabLogic.init();
        requestTabLogic.init();
        historyTabLogic.init();
        supportModalEl.addEventListener('hidden.bs.modal', resetAllStates);
    }

})(jQuery);
