// site.js - Global JavaScript for the application
(function () {
    'use strict';

    const App = {
        // ------------------------------
        // 🔧 INITIALIZATION
        // ------------------------------

        init: function () {
            this.setupLayout();
            this.initializePlugins();
            this.setupGlobalEventListeners();
        },

        setupLayout: function () {
            this.calculateStickyPositions();
            window.addEventListener('resize', this.calculateStickyPositions.bind(this));
        },

        calculateStickyPositions: function () {
            const header = document.querySelector('header');
            const footer = 0;
            const main = document.querySelector('main');

            const headerHeight = header ? header.offsetHeight : 0;
            const mainPaddingTop = main ? parseFloat(window.getComputedStyle(main).paddingTop) : 0;
            const headerHeightWithPadding = headerHeight + mainPaddingTop;

            // Sticky Top Stack
            let topOffset = headerHeightWithPadding;
            document.querySelectorAll('.setStickyTop').forEach(el => {
                el.style.top = `${topOffset}px`;
                el.style.position = 'sticky';
                el.style.zIndex = '1020';
                el.style.backdropFilter = 'blur(5px)';
                topOffset += el.offsetHeight;
            });

            // Sticky Bottom Stack
            let bottomOffset = footer ? footer.offsetHeight : 0;
            document.querySelectorAll('.setStickyBottom').forEach(el => {
                el.style.bottom = `${bottomOffset}px`;
                el.style.position = 'sticky';
                el.style.zIndex = '1020';
                el.style.backdropFilter = 'blur(5px)';
                bottomOffset += el.offsetHeight;
            });
        },

        // ------------------------------
        // 🔌 PLUGINS
        // ------------------------------

        initializePlugins: function () {
            this.initializeTooltips();
            this.initializePopovers();
        },

        initializeTooltips: function () {
            const tooltipElements = document.querySelectorAll('[data-bs-toggle="tooltip"]');
            tooltipElements.forEach(el => {
                const instance = bootstrap.Tooltip.getInstance(el);
                if (instance) instance.dispose();
                new bootstrap.Tooltip(el, { trigger: 'hover focus' });
            });
        },

        initializePopovers: function () {
            const popoverElements = document.querySelectorAll('[data-bs-toggle="popover"]');
            popoverElements.forEach(el => {
                const instance = bootstrap.Popover.getInstance(el);
                if (instance) instance.dispose();
                new bootstrap.Popover(el);
            });
        },

        // ------------------------------
        // 🌐 EVENT LISTENERS
        // ------------------------------

        setupGlobalEventListeners: function () {
            $(document).ajaxComplete(() => this.initializePlugins());
            $(document).on('shown.bs.modal', () => this.initializePlugins());
        },

        // ------------------------------
        // 🧱 DYNAMIC FORM MANAGEMENT
        // ------------------------------

        manageDynamicItems: function (options) {
            const container = $(options.containerSelector);
            let itemIndex = container.find(options.itemSelector).length;

            const updateItemIndexes = () => {
                container.find(options.itemSelector).each(function (index) {
                    const $item = $(this);
                    if (options.titleSelector) {
                        $item.find(options.titleSelector).text(`${options.itemTitle} ${index + 1}`);
                    }
                    $item.find('[name]').each(function () {
                        const $el = $(this);
                        const name = $el.attr('name').replace(/\[\d+\]/, `[${index}]`);
                        $el.attr('name', name);
                    });
                    $item.find('[data-valmsg-for]').each(function () {
                        const $el = $(this);
                        const valFor = $el.attr('data-valmsg-for').replace(/\[\d+\]/, `[${index}]`);
                        $el.attr('data-valmsg-for', valFor);
                    });
                    $item.find('label[for]').each(function () {
                        const $label = $(this);
                        const forAttr = $label.attr('for').replace(/_\d+__/, `_${index}__`);
                        $label.attr('for', forAttr);
                    });
                });
                itemIndex = container.find(options.itemSelector).length;
            };

            const revalidateForm = () => {
                const $form = $('form');
                $form.removeData('validator').removeData('unobtrusiveValidation');
                $.validator.unobtrusive.parse($form);
            };

            $(document).on('click', options.addButtonSelector, function () {
                const newItem = options.template
                    .replace(/{index}/g, itemIndex)
                    .replace(/{indexDisplay}/g, itemIndex + 1);
                container.append(newItem);
                updateItemIndexes();
                revalidateForm();
                $(options.focusSelector).focus();
                if (options.onAdd) options.onAdd();
            });

            $(document).on('click', options.removeButtonSelector, function () {
                const $item = $(this).closest(options.itemSelector);
                $item.remove();
                updateItemIndexes();
                revalidateForm();
                if (options.onRemove) options.onRemove();
            });

            if (options.clearButtonSelector) {
                $(document).on('click', options.clearButtonSelector, function () {
                    const items = container.find(options.itemSelector);
                    if (items.length > 1) {
                        items.slice(1).remove();
                        updateItemIndexes();
                        revalidateForm();
                        if (options.onClear) options.onClear();
                        App.showSuccessToast('ล้างข้อมูลสาขาทั้งหมดเรียบร้อยแล้ว (ยกเว้นรายการแรก)');
                    }
                });
            }
        },

        // ------------------------------
        // 📤 FORM SUBMISSION
        // ------------------------------

        handleFormSubmission: function (options) {
            const $form = $(options.formSelector);
            const $btn = $form.find('button[type="submit"]');
            const $spinner = $btn.find('.spinner-border');

            $form.on('submit', function (e) {
                e.preventDefault();
                if (!$form.attr('data-ajax')) return;

                $btn.prop('disabled', true);
                $spinner.removeClass('d-none');

                $.ajax({
                    url: $form.attr('action'),
                    method: $form.attr('method'),
                    data: $form.serialize(),
                    complete: () => {
                        $btn.prop('disabled', false);
                        $spinner.addClass('d-none');
                    },
                    success: (response) => {
                        if (response.success) {
                            App.showSuccessAlert(response.message || 'ดำเนินการสำเร็จ', {
                                onAction: () => {
                                    if (options.successUrl) {
                                        window.location.href = options.successUrl;
                                    }
                                }
                            });
                        } else {
                            App.handleApiError(response);
                        }
                    },
                    error: (xhr) => {
                        if (xhr.status === 400 && xhr.responseJSON) {
                            App.handleApiError(xhr.responseJSON);
                        } else {
                            App.showErrorAlert('เกิดข้อผิดพลาดในการเชื่อมต่อกับเซิร์ฟเวอร์');
                        }
                    }
                });
            });
        },

        handleApiError: function (response) {
            const $form = $('form');
            const $validationSummary = $form.find('.validation-summary-errors');
            $validationSummary.removeClass('d-none').empty();

            if (response.errors?.length) {
                response.errors.forEach(error => {
                    $validationSummary.append($('<li></li>').text(error));
                });
            }

            if (response.message) {
                this.showErrorToast(response.message);
            }
        },

        // ------------------------------
        // 🧰 UTILITIES
        // ------------------------------

        togglePasswordVisibility: function (inputSelector, toggleIconSelector) {
            const input = document.querySelector(inputSelector);
            const icon = document.querySelector(toggleIconSelector);

            if (!input) return;

            if (input.type === 'password') {
                input.type = 'text';
                if (icon) icon.classList.replace('bi-eye', 'bi-eye-slash');
            } else {
                input.type = 'password';
                if (icon) icon.classList.replace('bi-eye-slash', 'bi-eye');
            }
        },

        debounce: function (func, wait = 300) {
            let timeout;
            return function (...args) {
                clearTimeout(timeout);
                timeout = setTimeout(() => func.apply(this, args), wait);
            };
        },

        downloadFile: function (url, fileName = 'download') {
            const link = document.createElement('a');
            link.href = url;
            link.download = fileName;
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
        },

        copyToClipboard: function (text) {
            navigator.clipboard.writeText(text)
                .then(() => this.showSuccessToast('คัดลอกข้อความแล้ว'))
                .catch(() => this.showErrorToast('ไม่สามารถคัดลอกข้อความได้'));
        },

        // ------------------------------
        // 🔔 ALERTS & TOASTS
        // ------------------------------

        showConfirmDialog: function (options) {
            return Swal.fire({
                title: options.title || 'ยืนยันการดำเนินการ',
                html: options.text || options.html || 'คุณแน่ใจหรือไม่?',
                icon: options.icon || 'warning',
                showCancelButton: true,
                confirmButtonColor: options.confirmColor || '#d33',
                cancelButtonColor: options.cancelColor || '#3085d6',
                confirmButtonText: options.confirmText || 'ตกลง',
                cancelButtonText: options.cancelText || 'ยกเลิก',
                draggable: true
            });
        },

        showErrorAlert: function (message) {
            return Swal.fire({
                icon: 'error',
                title: 'เกิดข้อผิดพลาด',
                html: message,
                confirmButtonText: 'ตกลง',
                confirmButtonColor: '#3085d6',
                draggable: true
            });
        },

        showSuccessAlert: function (message, options = {}) {
            return Swal.fire({
                icon: 'success',
                title: 'สำเร็จ',
                html: message || options.text || options.html,
                timer: options.timer || null,
                showConfirmButton: options.showConfirmButton ?? true,
                confirmButtonText: options.confirmButtonText || 'ตกลง',
                confirmButtonColor: options.confirmButtonColor || '#3085d6',
                allowOutsideClick: options.allowOutsideClick ?? false,
                allowEscapeKey: options.allowEscapeKey ?? false,
                timerProgressBar: !!options.timer,
                didClose: options.onClose || null,
                draggable: true
            }).then((result) => {
                if ((result.isConfirmed || result.dismiss === Swal.DismissReason.timer) && typeof options.onAction === 'function') {
                    options.onAction();
                }
            });
        },

        showInfoAlert: function (message, options = {}) {
            return Swal.fire({
                icon: 'info',
                title: options.title || 'ข้อมูล',
                html: message || options.text || options.html ,
                timer: options.timer || null,
                showConfirmButton: options.showConfirmButton ?? true,
                confirmButtonText: options.confirmButtonText || 'ตกลง',
                confirmButtonColor: options.confirmButtonColor || '#3085d6',
                allowOutsideClick: options.allowOutsideClick ?? false,
                allowEscapeKey: options.allowEscapeKey ?? false,
                timerProgressBar: !!options.timer,
                didClose: options.onClose || null,
                draggable: true
            }).then((result) => {
                if ((result.isConfirmed || result.dismiss === Swal.DismissReason.timer) && typeof options.onAction === 'function') {
                    options.onAction();
                }
            });
        },

        showSuccessToast: function (message) {
            toastr.success(message, 'สำเร็จ', {
                positionClass: 'toast-bottom-left',
                timeOut: 3000
            });
        },

        showErrorToast: function (message) {
            toastr.error(message, 'ผิดพลาด', {
                positionClass: 'toast-bottom-left',
                timeOut: 3000
            });
        },

        // ------------------------------
        // 📊 DATATABLE
        // ------------------------------

        setupDataTable: function (tableSelector, options = {}) {
            const defaults = {
                language: {
                    url: '//cdn.datatables.net/plug-ins/2.0.3/i18n/th.json'
                },
                responsive: true
            };
            return $(tableSelector).DataTable({ ...defaults, ...options });
        }
    };

    // Public API
    window.app = App;

    // Initialize
    document.addEventListener('DOMContentLoaded', () => App.init());
})();
