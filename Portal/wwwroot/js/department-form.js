// wwwroot/js/Department/department-form.js
// [NEW] This is a merged and improved file for the Department form.

(function ($) {
    'use strict';

    /**
     * Re-indexes form inputs for correct model binding after an item is removed or added.
     * @param {jQuery} container - The container element (e.g., #sectionContainer).
     * @param {string} prefix - The model binding prefix (e.g., 'SectionViewModels').
     */
    function reindexSections(container, prefix) {
        container.find('.child-item').each(function (index, item) {
            $(item).find('input, select, textarea, span[data-valmsg-for]').each(function () {
                const $this = $(this);
                const attrsToUpdate = ['name', 'id', 'data-valmsg-for'];

                attrsToUpdate.forEach(attr => {
                    const oldAttrValue = $this.attr(attr);
                    if (oldAttrValue) {
                        const newAttrValue = oldAttrValue.replace(new RegExp(`${prefix}\\[\\d+\\]`, 'g'), `${prefix}[${index}]`)
                            .replace(new RegExp(`${prefix}_\\d+__`, 'g'), `${prefix}_${index}__`);
                        $this.attr(attr, newAttrValue);
                    }
                });
            });
        });
    }

    /**
     * Manages the dynamic addition and removal of Section items in the form.
     */
    function setupDynamicSectionItems() {
        const templateHtml = $('#sectionTemplate').html();
        if (!templateHtml) {
            console.error('Section template not found.');
            return;
        }

        const container = $('#sectionContainer');
        const form = container.closest('form');

        // Add button click handler
        $(document).on('click', '#add-section-btn', function () {
            const newIndex = container.find('.child-item').length;
            const newItemHtml = templateHtml.replace(/\{index\}/g, newIndex);
            container.append(newItemHtml);

            // Re-parse unobtrusive validation
            form.removeData('validator').removeData('unobtrusiveValidation');
            $.validator.unobtrusive.parse(form);
        });

        // Remove button click handler (delegated)
        container.on('click', '.remove-section-btn', function () {
            $(this).closest('.child-item').remove();
            // Re-index after removal to ensure sequence is correct (0, 1, 2, ...)
            reindexSections(container, 'SectionViewModels');

            // Re-parse unobtrusive validation
            form.removeData('validator').removeData('unobtrusiveValidation');
            $.validator.unobtrusive.parse(form);
        });
    }

    /**
     * Sets up the cascading dropdown functionality between Company and Division.
     */
    function setupCascadingDropdowns() {
        $('#CompanyId').change(function () {
            const companyId = $(this).val();
            const $divisionDropdown = $('#DivisionId');

            $divisionDropdown.empty().append('<option value="">-- กำลังโหลด... --</option>').prop('disabled', true);

            if (companyId) {
                $.getJSON('/Lookup/GetSelectListDivisionsByCompany', { companyId: companyId }, function (data) {
                    $divisionDropdown.empty().append('<option value="">-- เลือกสายงาน --</option>');
                    $.each(data, function (index, item) {
                        $divisionDropdown.append($('<option></option>').val(item.value).text(item.text));
                    });
                    $divisionDropdown.prop('disabled', false);
                }).fail(function () {
                    $divisionDropdown.empty().append('<option value="">-- ไม่สามารถโหลดข้อมูลได้ --</option>');
                });
            } else {
                $divisionDropdown.empty().append('<option value="">-- เลือกบริษัทก่อน --</option>');
            }
        });
    }

    // Initialize all functionalities when the document is ready
    $(document).ready(function () {
        setupCascadingDropdowns();
        setupDynamicSectionItems();

        // The form submission is now handled by the global site.js handler
        // by adding data-ajax="true" and data-confirm="true" to the <form> tag.
    });

})(jQuery);
