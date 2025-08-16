// wwwroot/js/Section/formEvent.js
(function ($) {
    'use strict';

    function setupCascadingDropdowns() {
        const $company = $('#CompanyId');
        const $division = $('#DivisionId');
        const $department = $('#DepartmentId');

        // When Company changes
        $company.change(function () {
            const companyId = $(this).val();

            // Reset and disable subsequent dropdowns
            $division.empty().append('<option value="">-- เลือกสายงาน --</option>').prop('disabled', true);
            $department.empty().append('<option value="">-- เลือกฝ่าย --</option>').prop('disabled', true);

            if (companyId) {
                $division.empty().append('<option value="">-- กำลังโหลด... --</option>');
                $.getJSON('/Lookup/GetSelectListDivisionsByCompany', { companyId: companyId }, function (data) {
                    $division.empty().append('<option value="">-- เลือกสายงาน --</option>');
                    $.each(data, function (index, item) {
                        $division.append($('<option></option>').val(item.value).text(item.text));
                    });
                    // Restore selected value if it exists (for Edit page)
                    const selectedDivisionId = $division.data('selected-id');
                    if (selectedDivisionId) {
                        $division.val(selectedDivisionId);
                    }
                    $division.prop('disabled', false).trigger('change'); // Trigger change to load departments
                });
            }
        });

        // When Division changes
        $division.change(function () {
            const divisionId = $(this).val();

            // Reset department dropdown
            $department.empty().append('<option value="">-- เลือกฝ่าย --</option>').prop('disabled', true);

            if (divisionId) {
                $department.empty().append('<option value="">-- กำลังโหลด... --</option>');
                $.getJSON('/Lookup/GetSelectListDepartmentsByDivision', { id: divisionId }, function (data) {
                    $department.empty().append('<option value="">-- เลือกฝ่าย --</option>');
                    $.each(data, function (index, item) {
                        $department.append($('<option></option>').val(item.value).text(item.text));
                    });
                    // Restore selected value if it exists (for Edit page)
                    const selectedDepartmentId = $department.data('selected-id');
                    if (selectedDepartmentId) {
                        $department.val(selectedDepartmentId);
                    }
                    $department.prop('disabled', false);
                });
            }
        });
    }

    $(document).ready(function () {
        // Store the initial selected values from the model for the Edit page
        $('#DivisionId').data('selected-id', $('#DivisionId').val());
        $('#DepartmentId').data('selected-id', $('#DepartmentId').val());

        setupCascadingDropdowns();

        // Trigger the change event on page load if a company is already selected
        if ($('#CompanyId').val()) {
            $('#CompanyId').trigger('change');
        }
    });

})(jQuery);
