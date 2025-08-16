// =================================================================================
// site.js: Global JavaScript for the Portal application
// This file contains shared functions for layout, plugins, modals, data tables,
// and system-wide confirmation dialogs for all CRUD operations.
// Version: 3.1 (Fixed modal header issue)
// =================================================================================

(function ($) {
    'use strict';

    /**
     * The main application object that encapsulates all global functions.
     * @namespace app
     */
    const app = {

        // =========================================================================
        // A. Layout & Utility Functions (Alphabetical Order)
        // =========================================================================

        /**
         * Dynamically calculates and sets the 'top' and 'bottom' CSS properties
         * for elements with 'setStickyTop' or 'setStickyBottom' classes.
         */
        calculateStickyPositions: function () {
            const header = document.querySelector('header');
            const main = document.querySelector('main');
            const headerHeight = header ? header.offsetHeight : 0;
            const mainPaddingTop = main ? parseFloat(window.getComputedStyle(main).paddingTop) : 0;
            const topStartPosition = headerHeight + mainPaddingTop;

            let topOffset = topStartPosition;
            document.querySelectorAll('.setStickyTop').forEach(el => {
                el.style.top = `${topOffset}px`;
                el.style.position = 'sticky';
                el.style.zIndex = '1020';
            });

            let bottomOffset = 0;
            document.querySelectorAll('.setStickyBottom').forEach(el => {
                el.style.bottom = `${bottomOffset}px`;
                el.style.position = 'sticky';
                el.style.zIndex = '1020';
            });
        },

        /**
         * Copies a given string to the user's clipboard.
         * @param {string} text - The text to copy.
         */
        copyToClipboard: function (text) {
            navigator.clipboard.writeText(text)
                .then(() => this.showSuccessToast('คัดลอกข้อความแล้ว'))
                .catch(() => this.showErrorToast('ไม่สามารถคัดลอกข้อความได้'));
        },

        /**
         * Creates a debounced function that delays invoking the provided function.
         * @param {Function} func - The function to debounce.
         * @param {number} [wait=300] - The number of milliseconds to delay.
         * @returns {Function} The new debounced function.
         */
        debounce: function (func, wait = 300) {
            let timeout;
            return function (...args) {
                clearTimeout(timeout);
                timeout = setTimeout(() => func.apply(this, args), wait);
            };
        },

        /**
         * Triggers a browser download for a file from a given URL.
         * @param {string} url - The URL of the file to download.
         * @param {string} [fileName='download'] - The desired name for the downloaded file.
         */
        downloadFile: function (url, fileName = 'download') {
            const link = document.createElement('a');
            link.href = url;
            link.download = fileName;
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
        },

        getTicketStatusClass: function (status) {
            // ===== [ปรับปรุง] ให้รองรับชื่อสถานะที่เป็นข้อความเต็ม =====
            switch (status) {
                case 'Open':
                case 'เปิด':
                    return 'bg-primary-subtle text-primary-emphasis';
                case 'In Progress':
                case 'กำลังดำเนินการ':
                    return 'bg-warning-subtle text-warning-emphasis';
                case 'Resolved':
                case 'แก้ไขแล้ว':
                    return 'bg-success-subtle text-success-emphasis';
                case 'Closed':
                case 'ปิดงาน':
                    return 'bg-secondary-subtle text-secondary-emphasis';
                case 'Cancelled':
                case 'ยกเลิก':
                    return 'bg-dark-subtle text-dark-emphasis';
                default:
                    return 'bg-light text-dark';
            }
        },

        // =========================================================================
        // B. Form & AJAX Handling (Alphabetical Order)
        // =========================================================================

        /**
         * A generic handler for submitting forms via AJAX.
         * @param {HTMLFormElement} form - The form element to submit.
         */
        handleAjaxForm: function (form) {
            const $form = $(form);
            const $submitButton = $form.find('button[type="submit"]');
            const $spinner = $submitButton.find('.spinner-border');

            if (!$form.valid()) {
                this.showErrorToast('กรุณากรอกข้อมูลให้ครบถ้วน');
                return;
            }

            $submitButton.prop('disabled', true);
            $spinner.removeClass('d-none');

            $.ajax({
                url: $form.attr('action'),
                type: $form.attr('method'),
                data: $form.serialize(),
                success: (response) => {
                    if (response.success) {
                        const successUrl = $form.data('success-url');
                        this.showSuccessAlert(response.message || 'บันทึกข้อมูลสำเร็จ', {
                            onAction: () => {
                                if (successUrl) {
                                    window.location.href = successUrl;
                                }
                            }
                        });
                    } else {
                        this.showErrorAlert(response.message || 'เกิดข้อผิดพลาดในการบันทึกข้อมูล');
                    }
                },
                error: (xhr) => {
                    this.showErrorAlert(`เกิดข้อผิดพลาด: ${xhr.statusText}`);
                },
                complete: () => {
                    $submitButton.prop('disabled', false);
                    $spinner.addClass('d-none');
                }
            });
        },

        /**
         * Handles dynamic addition and removal of form items.
         * @param {object} options - Configuration for managing dynamic items.
         */
        manageDynamicItems: function (options) {
            const container = $(options.containerSelector);
            const revalidateForm = () => {
                const $form = container.closest('form');
                $form.removeData('validator').removeData('unobtrusiveValidation');
                $.validator.unobtrusive.parse($form);
            };

            $(document).on('click', options.addButtonSelector, () => {
                const newIndex = container.find(options.itemSelector).length;
                const newItemHtml = options.template.replace(/\{index\}/g, newIndex);
                container.append(newItemHtml);
                revalidateForm();
                if (options.focusSelector) $(options.focusSelector).focus();
                if (options.onAdd) options.onAdd();
            });

            container.on('click', options.removeButtonSelector, function () {
                $(this).closest(options.itemSelector).remove();
                revalidateForm();
                if (options.onRemove) options.onRemove();
            });
        },

        /**
         * Toggles the visibility of a password input field.
         * @param {string} inputSelector - The CSS selector for the password input.
         * @param {string} toggleSelector - The CSS selector for the toggle button/icon.
         */
        togglePasswordVisibility: function (inputSelector, toggleSelector) {
            const input = $(inputSelector);
            const icon = $(toggleSelector).find('i');
            if (input.length) {
                const isPassword = input.attr('type') === 'password';
                input.attr('type', isPassword ? 'text' : 'password');
                icon.toggleClass('bi-eye-slash bi-eye');
            }
        },

        // =========================================================================
        // C. Initialization & Event Listeners (Alphabetical Order)
        // =========================================================================

        /**
         * Main initialization function for the application.
         */
        init: function () {
            this.setupLayout();
            this.initializePlugins();
            this.setupGlobalEventListeners();
        },

        /**
         * Initializes third-party plugins like Tooltips and Popovers.
         */
        initializePlugins: function () {
            const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
            tooltipTriggerList.map(el => new bootstrap.Tooltip(el));

            const popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
            popoverTriggerList.map(el => new bootstrap.Popover(el));
        },

        /**
         * Sets up global event listeners for the application.
         */
        setupGlobalEventListeners: function () {
            $(document).on('ajaxComplete shown.bs.modal', () => this.initializePlugins());
        },

        /**
         * Sets up layout-related functionalities.
         */
        setupLayout: function () {
            this.calculateStickyPositions();
            window.addEventListener('resize', this.debounce(this.calculateStickyPositions.bind(this)));
        },

        // =========================================================================
        // D. UI Components (Modals, Alerts, Toasts) (Alphabetical Order)
        // =========================================================================

        showLoading: function (buttonElement) {
            const $btn = $(buttonElement);
            $btn.prop('disabled', true);
            $btn.find('.spinner-border').removeClass('d-none');
        },

        hideLoading: function (buttonElement) {
            const $btn = $(buttonElement);
            $btn.prop('disabled', false);
            $btn.find('.spinner-border').addClass('d-none');
        },

        /**
         * Initializes a DataTable with standardized settings for the project.
         * @param {string} selector - The CSS selector for the table element.
         * @param {object} [options={}] - Custom options to override the defaults.
         * @returns {object} The initialized DataTable instance.
         */
        setupDataTable: function (selector, options = {}) {
            const defaultOptions = {
                language: { url: '//cdn.datatables.net/plug-ins/2.0.8/i18n/th.json' },
                responsive: true
            };
            return $(selector).DataTable({ ...defaultOptions, ...options });
        },

        /**
         * Shows a confirmation dialog using SweetAlert2.
         * @param {object} options - Custom SweetAlert2 options.
         * @returns {Promise} A promise that resolves with the dialog result.
         */
        showConfirmDialog: function (options) {
            const defaultOptions = {
                title: 'ยืนยันการดำเนินการ',
                html: 'คุณแน่ใจหรือไม่?',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#3085d6',
                cancelButtonColor: '#d33',
                confirmButtonText: 'ยืนยัน',
                cancelButtonText: 'ยกเลิก',
            };
            return Swal.fire({ ...defaultOptions, ...options });
        },

        /**
         * Shows an error alert using SweetAlert2.
         * @param {string} message - The error message to display.
         */
        showErrorAlert: function (message) {
            Swal.fire({
                icon: 'error',
                title: 'เกิดข้อผิดพลาด',
                html: message,
            });
        },

        /**
         * Shows an error toast notification for non-blocking feedback.
         * @param {string} message - The error message to display.
         */
        showErrorToast: function (message) {
            toastr.error(message, 'ผิดพลาด');
        },

        /**
         * Displays a global modal and loads content into it asynchronously.
         * @param {object} options - Configuration for the modal.
         * @param {string} options.url - The URL to load content from.
         * @param {string} options.title - The title to display in the modal header.
         * @param {string} [options.size='lg'] - The modal size ('sm', 'lg', 'xl').
         */
        showGlobalModal: async function (options) {
            const { url, title, size = 'lg' } = options;
            const modalEl = document.getElementById('globalModal');
            const modalDialog = document.getElementById('globalModalDialog');
            const modalContent = document.getElementById('globalModalContent');
            if (!modalEl) return;

            modalDialog.className = `modal-dialog modal-dialog-centered modal-dialog-scrollable modal-${size}`;

            const headerHtml = `
                <div class="modal-header">
                    <h5 class="modal-title">${title}</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>`;

            const loadingBodyHtml = `
                <div class="modal-body text-center p-4">
                    <div class="spinner-border text-primary" role="status">
                        <span class="visually-hidden">Loading...</span>
                    </div>
                </div>`;

            modalContent.innerHTML = headerHtml + loadingBodyHtml;

            const modal = new bootstrap.Modal(modalEl);
            modal.show();

            try {
                const response = await fetch(url);
                if (!response.ok) throw new Error(`Network error: ${response.statusText}`);
                const partialHtml = await response.text();

                // **THE FIX IS HERE:** Reconstruct the content with the header and the fetched partial view HTML.
                modalContent.innerHTML = headerHtml + partialHtml;

            } catch (error) {
                console.error('Failed to load modal content:', error);
                const errorBodyHtml = `
                    <div class="modal-body">
                        <p class="text-danger">ไม่สามารถโหลดข้อมูลได้</p>
                    </div>`;
                modalContent.innerHTML = headerHtml.replace(title, "Error") + errorBodyHtml;
            }
        },

        /**
         * Shows an info alert using SweetAlert2.
         * @param {string} message - The info message to display.
         * @param {object} [options={}] - Custom SweetAlert2 options.
         */
        showInfoAlert: function (message, options = {}) {
            const defaultOptions = {
                icon: 'info',
                title: 'แจ้งเพื่อทราบ',
                html: message,
            };
            return Swal.fire({ ...defaultOptions, ...options });
        },

        /**
         * Shows a success alert using SweetAlert2, with a callback on action.
         * @param {string} message - The success message to display.
         * @param {object} [options={}] - Custom options, including an onAction callback.
         */
        showSuccessAlert: function (message, options = {}) {
            const defaultOptions = {
                icon: 'success',
                title: 'สำเร็จ',
                html: message,
            };
            Swal.fire({ ...defaultOptions, ...options }).then((result) => {
                if (result.isConfirmed && typeof options.onAction === 'function') {
                    options.onAction();
                }
            });
        },

        /**
         * Shows a success toast notification for non-blocking feedback.
         * @param {string} message - The success message to display.
         */
        showSuccessToast: function (message) {
            toastr.success(message, 'สำเร็จ');
        },
    };

    // Expose the app object to the global window scope
    window.app = app;

    // Initialize the application once the DOM is ready
    $(document).ready(function () {
        app.init();

        // --- SYSTEM-WIDE CONFIRMATION EVENT LISTENERS ---

        $(document).on('submit', 'form[data-confirm="true"]', function (e) {
            e.preventDefault();
            const form = this;
            const isEdit = $('input[name="Id"]', form).val() > 0;
            app.showConfirmDialog({
                title: 'ยืนยันการบันทึกข้อมูล?',
                text: isEdit ? 'คุณต้องการบันทึกการเปลี่ยนแปลงใช่หรือไม่?' : 'คุณต้องการสร้างรายการใหม่นี้ใช่หรือไม่?',
                icon: 'question',
                confirmButtonText: 'ยืนยัน',
            }).then((result) => {
                if (result.isConfirmed) {
                    if ($(form).data('ajax') === true) {
                        app.handleAjaxForm(form);
                    } else {
                        form.submit();
                    }
                }
            });
        });

        $(document).on('click', '.delete-btn', function (e) {
            e.preventDefault();
            const $button = $(this);
            const url = $button.data('url');
            const id = $button.data('id');
            const redirectUrl = $button.data('redirect');
            const token = $('input[name="__RequestVerificationToken"]').val();

            app.showConfirmDialog({
                title: 'ยืนยันการลบข้อมูล?',
                text: 'ข้อมูลที่ถูกลบจะไม่สามารถกู้คืนได้!',
                icon: 'warning',
                confirmButtonText: 'ใช่, ลบเลย',
                confirmButtonColor: '#d33',
            }).then((result) => {
                if (result.isConfirmed) {
                    $.ajax({
                        url: url,
                        type: 'POST',
                        data: { id: id, __RequestVerificationToken: token },
                        success: (response) => {
                            if (response.success) {
                                app.showSuccessAlert(response.message || 'ลบข้อมูลสำเร็จ', {
                                    onAction: () => {
                                        if (redirectUrl) {
                                            window.location.href = redirectUrl;
                                        } else {
                                            window.location.reload();
                                        }
                                    }
                                });
                            } else {
                                app.showErrorAlert(response.message || 'เกิดข้อผิดพลาดในการลบข้อมูล');
                            }
                        },
                        error: (xhr) => app.showErrorAlert(`เกิดข้อผิดพลาด: ${xhr.statusText}`)
                    });
                }
            });
        });
    });

})(jQuery);