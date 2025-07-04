// File: wwwroot/js/support-modal.js (Complete, Robust, and Refactored)
(function ($) {
    'use strict';

    // --- State Management ---
    let allStockItems = [];
    let cart = {}; // { itemId: { itemData, quantity } }
    let myTicketsCache = []; // Cache for ticket history

    // --- Main Initialization Function ---
    const initializeSupportModal = () => {
        const supportModalEl = document.getElementById('supportModal');
        if (!supportModalEl) return;

        // Initialize all logic modules
        problemTabLogic.init();
        historyTabLogic.init();
        requestTabLogic.init();

        // Setup modal-wide event listeners
        supportModalEl.addEventListener('hidden.bs.modal', resetAllStates);
    };

    // --- Utility Functions ---
    const escapeHtml = (unsafe) => unsafe ? unsafe.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;").replace(/"/g, "&quot;").replace(/'/g, "&#039;") : '';
    const getCsrfToken = () => document.querySelector('input[name="__RequestVerificationToken"]')?.value;

    // --- Tab: แจ้งปัญหา ---
    const problemTabLogic = (() => {
        const form = document.getElementById('createTicketForm');
        const titleInput = document.getElementById('ticketTitle');
        const descriptionInput = document.getElementById('ticketDescription');
        const submitButton = document.getElementById('main-submit-button');
        const problemTab = document.getElementById('problem-tab');

        const handleSubmission = (e) => {
            e.preventDefault();
            if (!form) return;

            const spinner = submitButton?.querySelector('.spinner-border');
            if (submitButton) submitButton.disabled = true;
            spinner?.classList.remove('d-none');

            fetch(form.action, { method: 'POST', body: new FormData(form) })
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
                .finally(() => {
                    if (submitButton) submitButton.disabled = false;
                    spinner?.classList.add('d-none');
                });
        };

        return {
            init: () => {
                if (form) form.addEventListener('submit', handleSubmission);
                else console.error('Problem reporting form not found.');
            },
            prefillForReopen: (ticket) => {
                if (titleInput) titleInput.value = `[แจ้งซ้ำ] ${ticket.title}`;
                if (descriptionInput) descriptionInput.value = `อ้างอิงถึง Ticket เดิม: ${ticket.ticketNumber}\n\n-----------------\nกรุณาอธิบายปัญหาที่เกิดขึ้นอีกครั้ง:\n`;
                if (problemTab) bootstrap.Tab.getOrCreateInstance(problemTab).show();
                descriptionInput?.focus();
            },
            reset: () => {
                form?.reset();
            }
        };
    })();

    // --- Tab: ประวัติของฉัน ---
    const historyTabLogic = (() => {
        const tab = document.getElementById('history-tab');
        const container = document.getElementById('my-tickets-history-container');

        const load = () => {
            if (!container) return;
            container.innerHTML = `<div class="text-center p-5"><div class="spinner-border text-primary"></div><p class="mt-2">กำลังโหลดประวัติ...</p></div>`;
            fetch('/Lookup/GetMySupportTickets')
                .then(res => res.json())
                .then(data => {
                    if (data.success) {
                        myTicketsCache = data.data;
                        render(myTicketsCache);
                    } else throw new Error(data.message);
                })
                .catch(err => {
                    container.innerHTML = `<div class="text-center p-5 text-danger"><i class="bi bi-exclamation-triangle-fill fs-2"></i><p>โหลดข้อมูลไม่สำเร็จ</p></div>`;
                    console.error('Failed to load history:', err);
                });
        };

        const render = (tickets) => {
            if (!container) return;
            if (!tickets || tickets.length === 0) {
                container.innerHTML = `<div class="text-center p-5 text-muted"><i class="bi bi-journal-x fs-2"></i><p>ไม่พบประวัติการแจ้งปัญหา</p></div>`;
                return;
            }
            const rows = tickets.map(t => {
                const statusMap = { 1: { text: 'เปิด', class: 'bg-primary' }, 2: { text: 'กำลังดำเนินการ', class: 'bg-warning text-dark' }, 3: { text: 'แก้ไขแล้ว', class: 'bg-success' }, 4: { text: 'ปิด', class: 'bg-secondary' } };
                const isClosed = (t.status === 3 || t.status === 4);
                const statusInfo = statusMap[t.status] || { text: 'ไม่ทราบ', class: 'bg-dark' };
                return `
                <tr>
                    <td>${t.ticketNumber}</td>
                    <td>${escapeHtml(t.title)}</td>
                    <td><span class="badge ${statusInfo.class}">${escapeHtml(t.statusName)}</span></td>
                    <td>${new Date(t.createdAt).toLocaleString('th-TH')}</td>
                    <td class="text-center text-nowrap">
                        ${isClosed ? `<button class="btn btn-sm btn-outline-danger reopen-ticket-btn" data-ticket-id="${t.id}" title="แจ้งปัญหานี้อีกครั้ง"><i class="bi bi-arrow-repeat"></i></button>` : ''}
                        <button class="btn btn-sm btn-outline-primary ms-1" title="ดูรายละเอียด"><i class="bi bi-search"></i></button>
                    </td>
                </tr>`;
            }).join('');
            container.innerHTML = `<div class="table-responsive"><table class="table table-hover table-sm"><thead><tr><th>หมายเลข</th><th>หัวข้อ</th><th>สถานะ</th><th>วันที่แจ้ง</th><th class="text-center">ดู</th></tr></thead><tbody>${rows}</tbody></table></div>`;
        };

        const handleHistoryClick = (e) => {
            const reopenButton = e.target.closest('.reopen-ticket-btn');
            if (reopenButton) {
                const ticketId = parseInt(reopenButton.dataset.ticketId, 10);
                const ticketToReopen = myTicketsCache.find(t => t.id === ticketId);
                if (ticketToReopen) {
                    problemTabLogic.prefillForReopen(ticketToReopen);
                }
            }
        };

        return {
            init: () => {
                if (tab) tab.addEventListener('shown.bs.tab', load);
                else console.error('History tab element not found.');
                if (container) container.addEventListener('click', handleHistoryClick);
                else console.error('History container element not found.');
            }
        };
    })();

    // --- Tab: เบิก/ขออุปกรณ์ ---
    const requestTabLogic = (() => {
        const tab = document.getElementById('request-tab');
        const search = document.getElementById('stock-item-search');
        const itemsContainer = document.getElementById('stock-items-container');
        const cartContainer = document.getElementById('cart-items-container');
        const cartCountBadge = document.getElementById('cart-item-count');
        const submitRequestBtn = document.getElementById('submit-request-btn');
        const purchaseForm = document.getElementById('purchaseRequestForm');
        const purchaseSubmitBtn = document.getElementById('submit-purchase-request-btn');

        const loadStock = () => {
            if (!itemsContainer) return;
            itemsContainer.innerHTML = `<div class="text-center p-5"><div class="spinner-border text-primary"></div><p class="mt-2">กำลังโหลดรายการ...</p></div>`;
            fetch('/Lookup/GetITStockItems')
                .then(res => res.json())
                .then(data => {
                    if (data.success) {
                        allStockItems = data.data;
                        renderStock();
                    } else throw new Error(data.message || 'Could not load stock items.');
                })
                .catch(err => {
                    itemsContainer.innerHTML = `<div class="text-center p-5 text-danger"><i class="bi bi-exclamation-triangle-fill fs-2"></i><p>โหลดข้อมูลไม่สำเร็จ</p></div>`;
                    console.error('Failed to load stock items:', err);
                });
        };

        const renderStock = (filter = '') => {
            if (!itemsContainer) return;
            const lowerFilter = filter.toLowerCase();
            const filteredItems = allStockItems.filter(item => item.name.toLowerCase().includes(lowerFilter));
            if (filteredItems.length === 0) {
                itemsContainer.innerHTML = `<div class="text-center p-5 text-muted"><i class="bi bi-box-seam fs-2"></i><p>ไม่พบรายการที่ค้นหา</p></div>`;
                return;
            }
            itemsContainer.innerHTML = filteredItems.map(item => `
                <div class="card mb-2 shadow-sm">
                    <div class="card-body d-flex align-items-center p-2">
                        <div class="flex-grow-1">
                            <h6 class="card-title mb-1">${escapeHtml(item.name)}</h6>
                            <p class="card-text small text-muted mb-0">${escapeHtml(item.specification || 'ไม่มีรายละเอียด')}</p>
                        </div>
                        <div class="text-end ms-3">
                            <span class="badge bg-secondary">คงเหลือ: ${item.quantity}</span>
                            <button class="btn btn-sm btn-primary ms-2 add-to-cart-btn" data-item-id="${item.itemId}" ${item.quantity <= 0 ? 'disabled' : ''}>
                                <i class="bi bi-plus-lg"></i> เพิ่ม
                            </button>
                        </div>
                    </div>
                </div>`).join('');
        };

        const renderCart = () => {
            if (!cartContainer || !cartCountBadge || !submitRequestBtn) return;
            const cartItems = Object.values(cart);
            if (cartItems.length === 0) {
                cartContainer.innerHTML = `<div class="text-center text-muted pt-5"><p><i class="bi bi-cart-x fs-2"></i></p><p>ตะกร้าของคุณว่างอยู่</p></div>`;
            } else {
                cartContainer.innerHTML = cartItems.map(({ itemData, quantity }) => `
                    <div class="d-flex justify-content-between align-items-center mb-2">
                        <div>
                            <p class="mb-0 small fw-bold">${escapeHtml(itemData.name)}</p>
                            <p class="mb-0 small text-muted">คงเหลือ: ${itemData.quantity}</p>
                        </div>
                        <div class="d-flex align-items-center">
                            <input type="number" class="form-control form-control-sm cart-quantity-input" value="${quantity}" min="1" max="${itemData.quantity}" data-item-id="${itemData.itemId}" style="width: 60px;">
                            <button class="btn btn-sm btn-outline-danger ms-2 remove-from-cart-btn" data-item-id="${itemData.itemId}"><i class="bi bi-trash"></i></button>
                        </div>
                    </div>`).join('');
            }
            cartCountBadge.textContent = cartItems.reduce((sum, { quantity }) => sum + quantity, 0);
            submitRequestBtn.disabled = cartItems.length === 0;
        };

        const addToCart = (itemId) => {
            const item = allStockItems.find(i => i.itemId === itemId);
            if (!item) return;
            if (cart[itemId]) {
                if (cart[itemId].quantity < item.quantity) cart[itemId].quantity++;
                else window.app.showErrorToast('ไม่สามารถเบิกเกินจำนวนคงเหลือ');
            } else {
                cart[itemId] = { itemData: item, quantity: 1 };
            }
            renderCart();
        };

        const updateCartQuantity = (itemId, newQuantity) => {
            const item = allStockItems.find(i => i.itemId === itemId);
            if (!item || !cart[itemId]) return;
            if (newQuantity > item.quantity) {
                cart[itemId].quantity = item.quantity;
                window.app.showErrorToast('ปรับจำนวนเป็นค่าสูงสุดที่เบิกได้');
            } else if (newQuantity < 1) {
                cart[itemId].quantity = 1;
            } else {
                cart[itemId].quantity = newQuantity;
            }
            renderCart();
        };

        const removeFromCart = (itemId) => {
            delete cart[itemId];
            renderCart();
        };

        const handleSubmitRequest = () => {
            const token = getCsrfToken();
            if (!token) { window.app.showErrorAlert('เกิดข้อผิดพลาด', { text: 'ไม่พบ Security Token' }); return; }
            const payload = { items: Object.values(cart).map(c => ({ itemId: c.itemData.itemId, quantity: c.quantity })) };
            if (payload.items.length === 0) { window.app.showErrorToast('กรุณาเพิ่มรายการในตะกร้าก่อน'); return; }

            if (submitRequestBtn) {
                submitRequestBtn.disabled = true;
                submitRequestBtn.innerHTML = `<span class="spinner-border spinner-border-sm me-2"></span>กำลังส่ง...`;
            }
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
                .catch(err => window.app.showErrorAlert('เกิดข้อผิดพลาดในการเชื่อมต่อ'))
                .finally(() => {
                    if (submitRequestBtn) {
                        submitRequestBtn.disabled = false;
                        submitRequestBtn.innerHTML = `<i class="bi bi-check-circle me-2"></i>ส่งคำขอเบิก`;
                    }
                });
        };

        const handlePurchaseSubmit = (e) => {
            e.preventDefault();
            if (!purchaseForm || !purchaseSubmitBtn) return;
            const token = getCsrfToken();
            if (!token) { window.app.showErrorAlert('เกิดข้อผิดพลาด', { text: 'ไม่พบ Security Token' }); return; }
            const payload = Object.fromEntries(new FormData(purchaseForm).entries());
            if (!payload.ItemName || !payload.Reason) { window.app.showErrorToast('กรุณากรอกข้อมูลที่จำเป็นให้ครบถ้วน'); return; }

            purchaseSubmitBtn.disabled = true;
            purchaseSubmitBtn.innerHTML = `<span class="spinner-border spinner-border-sm me-2"></span>กำลังส่ง...`;
            fetch('/Support/CreatePurchase', { method: 'POST', headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': token }, body: JSON.stringify(payload) })
                .then(res => res.json())
                .then(data => {
                    if (data.success) {
                        bootstrap.Modal.getInstance(document.getElementById('supportModal')).hide();
                        window.app.showSuccessAlert('ส่งคำขอจัดซื้อสำเร็จ!', { html: `Ticket ของคุณคือ <b>${data.data.ticketNumber}</b>` });
                    } else {
                        window.app.showErrorAlert('ส่งคำขอไม่สำเร็จ', { html: data.errors?.join('<br>') || data.message });
                    }
                })
                .catch(err => window.app.showErrorAlert('เกิดข้อผิดพลาดในการเชื่อมต่อ'))
                .finally(() => {
                    purchaseSubmitBtn.disabled = false;
                    purchaseSubmitBtn.innerHTML = `<i class="bi bi-send me-2"></i>ส่งคำขอจัดซื้อ`;
                });
        };

        const handleRequestTabClick = (e) => {
            const target = e.target;
            if (target.matches('.add-to-cart-btn')) addToCart(parseInt(target.dataset.itemId));
            else if (target.closest('.remove-from-cart-btn')) {
                const button = target.closest('.remove-from-cart-btn');
                removeFromCart(parseInt(button.dataset.itemId));
            }
        };

        const handleRequestTabInput = (e) => {
            if (e.target.matches('.cart-quantity-input')) {
                updateCartQuantity(parseInt(e.target.dataset.itemId), parseInt(e.target.value));
            }
        };

        return {
            init: () => {
                if (tab) tab.addEventListener('shown.bs.tab', loadStock);
                if (search) search.addEventListener('input', (e) => renderStock(e.target.value));
                if (submitRequestBtn) submitRequestBtn.addEventListener('click', handleSubmitRequest);
                if (itemsContainer) itemsContainer.addEventListener('click', handleRequestTabClick);
                if (cartContainer) {
                    cartContainer.addEventListener('click', handleRequestTabClick);
                    cartContainer.addEventListener('change', handleRequestTabInput);
                }
                if (purchaseForm) purchaseForm.addEventListener('submit', handlePurchaseSubmit);
            },
            reset: () => {
                cart = {};
                allStockItems = [];
                if (itemsContainer) itemsContainer.innerHTML = '';
                if (search) search.value = '';
                if (purchaseForm) purchaseForm.reset();
                const collapseEl = document.getElementById('purchase-request-form-collapse');
                if (collapseEl) bootstrap.Collapse.getOrCreateInstance(collapseEl).hide();
                renderCart();
            }
        };
    })();

    const resetAllStates = () => {
        problemTabLogic.reset();
        requestTabLogic.reset();
        myTicketsCache = [];
        const problemTabEl = document.getElementById('problem-tab');
        if (problemTabEl) bootstrap.Tab.getOrCreateInstance(problemTabEl).show();
    };

    document.addEventListener('DOMContentLoaded', initializeSupportModal);

})(jQuery);
