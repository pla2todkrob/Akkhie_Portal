// File: wwwroot/js/support-modal.js (Fixed)
(function ($) {
    'use strict';

    // --- Function to initialize the entire modal logic ---
    const initializeSupportModal = () => {
        const supportModalEl = document.getElementById('supportModal');
        if (!supportModalEl) {
            // If the modal doesn't exist on this page, do nothing.
            return;
        }

        const createTicketForm = document.getElementById('createTicketForm');
        const categorySelect = document.getElementById('ticketCategoryId');
        const historyTab = document.getElementById('history-tab');
        const historyTabPane = document.getElementById('history-tab-pane');

        // Ensure all required elements exist before proceeding
        if (!createTicketForm || !categorySelect || !historyTab || !historyTabPane) {
            console.error("One or more required elements for the support modal are missing.");
            return;
        }

        const populateCategories = () => {
            categorySelect.innerHTML = '<option value="">กำลังโหลดข้อมูลหมวดหมู่...</option>';
            categorySelect.disabled = true;

            fetch('/Lookup/GetSupportCategories')
                .then(response => {
                    if (!response.ok) {
                        throw new Error('Network response was not ok');
                    }
                    return response.json();
                })
                .then(data => {
                    categorySelect.innerHTML = '<option value="">กรุณาเลือกหมวดหมู่</option>';
                    if (data && data.success && Array.isArray(data.data)) {
                        data.data.forEach(category => {
                            const option = new Option(category.name, category.id);
                            categorySelect.add(option);
                        });
                    } else {
                        throw new Error(data.message || 'Invalid data format for categories.');
                    }
                })
                .catch(error => {
                    console.error('Failed to load support categories:', error);
                    categorySelect.innerHTML = `<option value="">ไม่สามารถโหลดข้อมูลได้</option>`;
                })
                .finally(() => {
                    categorySelect.disabled = false;
                });
        };

        const loadTicketHistory = () => {
            historyTabPane.innerHTML = '<div class="d-flex justify-content-center p-5"><div class="spinner-border text-primary" role="status"><span class="visually-hidden">Loading...</span></div></div>';

            fetch('/Lookup/GetMySupportTickets')
                .then(response => response.json())
                .then(data => {
                    if (data && data.success && Array.isArray(data.data)) {
                        renderTicketHistory(data.data);
                    } else {
                        throw new Error(data.message || 'Could not load ticket history.');
                    }
                })
                .catch(error => {
                    console.error('Failed to load ticket history:', error);
                    historyTabPane.innerHTML = `<div class="alert alert-danger text-center">${error.message}</div>`;
                });
        };

        const renderTicketHistory = (tickets) => {
            if (tickets.length === 0) {
                historyTabPane.innerHTML = '<div class="text-center text-muted p-5"><i class="bi bi-ticket-detailed fs-1"></i><p class="mt-2">คุณยังไม่มีประวัติการแจ้งเรื่อง</p></div>';
                return;
            }

            let tableHtml = `
                <div class="table-responsive">
                    <table class="table table-hover table-sm">
                        <thead class="table-light">
                            <tr>
                                <th>หมายเลข</th>
                                <th>หัวข้อ</th>
                                <th>สถานะ</th>
                                <th>วันที่แจ้ง</th>
                            </tr>
                        </thead>
                        <tbody>`;

            tickets.forEach(ticket => {
                const statusBadge = getStatusBadge(ticket.status);
                const formattedDate = new Date(ticket.createdAt).toLocaleString('th-TH', {
                    year: 'numeric', month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit'
                });

                tableHtml += `
                    <tr>
                        <td><a href="#" class="fw-bold text-decoration-none">${ticket.ticketNumber}</a></td>
                        <td>${ticket.title}</td>
                        <td>${statusBadge}</td>
                        <td class="text-muted small">${formattedDate}</td>
                    </tr>`;
            });

            tableHtml += `</tbody></table></div>`;
            historyTabPane.innerHTML = tableHtml;
        };

        const getStatusBadge = (status) => {
            switch (status) {
                case 'Open': return '<span class="badge bg-primary">เปิด</span>';
                case 'InProgress': return '<span class="badge bg-warning text-dark">กำลังดำเนินการ</span>';
                case 'Resolved': return '<span class="badge bg-success">แก้ไขแล้ว</span>';
                case 'Closed': return '<span class="badge bg-secondary">ปิด</span>';
                default: return `<span class="badge bg-light text-dark">${status}</span>`;
            }
        };

        // --- Event Listeners ---
        supportModalEl.addEventListener('show.bs.modal', () => populateCategories());
        supportModalEl.addEventListener('hidden.bs.modal', () => {
            createTicketForm.classList.remove('was-validated');
            createTicketForm.reset();
        });

        historyTab.addEventListener('show.bs.tab', () => loadTicketHistory());

        createTicketForm.addEventListener('submit', function (event) {
            event.preventDefault();
            event.stopPropagation();

            if (!createTicketForm.checkValidity()) {
                createTicketForm.classList.add('was-validated');
                return;
            }

            const submitButton = createTicketForm.querySelector('button[type="submit"]');
            const spinner = submitButton.querySelector('.spinner-border');

            submitButton.disabled = true;
            spinner.classList.remove('d-none');

            const formData = new FormData(createTicketForm);

            fetch(createTicketForm.action, {
                method: 'POST',
                body: formData,
            })
                .then(response => response.json())
                .then(data => {
                    if (data.success) {
                        bootstrap.Modal.getInstance(supportModalEl).hide();
                        window.app.showSuccessAlert('ส่งเรื่องสำเร็จ!', {
                            html: `Ticket ของคุณคือ <b>${data.data.ticketNumber}</b><br>เราจะดำเนินการโดยเร็วที่สุด`
                        });
                    } else {
                        const errorMessages = data.errors ? data.errors.join('<br>') : (data.message || 'เกิดข้อผิดพลาดที่ไม่รู้จัก');
                        window.app.showErrorAlert('ส่งเรื่องไม่สำเร็จ', { html: errorMessages });
                    }
                })
                .catch(error => {
                    console.error('Form submission error:', error);
                    window.app.showErrorAlert('เกิดข้อผิดพลาดในการเชื่อมต่อ', { text: 'กรุณาลองใหม่อีกครั้ง' });
                })
                .finally(() => {
                    submitButton.disabled = false;
                    spinner.classList.add('d-none');
                });
        });
    };

    // Initialize the modal logic when the DOM is ready.
    document.addEventListener('DOMContentLoaded', initializeSupportModal);

})(jQuery);
