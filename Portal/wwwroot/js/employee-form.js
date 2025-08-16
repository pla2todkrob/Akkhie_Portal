// wwwroot/js/employee/employee-form.js
(function ($) {
    'use strict';

    function setupCascadingDropdowns() {
        const $company = $('#CompanyId');
        const $division = $('#DivisionId');
        const $department = $('#DepartmentId');
        const $section = $('#SectionId');

        // When Company changes -> Populate Divisions
        $company.change(function () {
            const companyId = $(this).val();

            $division.empty().append('<option value="">-- เลือกสายงาน --</option>').prop('disabled', true);
            $department.empty().append('<option value="">-- เลือกฝ่าย --</option>').prop('disabled', true);
            $section.empty().append('<option value="">-- เลือกแผนก --</option>').prop('disabled', true);

            if (companyId) {
                $division.empty().append('<option value="">-- กำลังโหลด... --</option>');
                $.getJSON('/Lookup/GetSelectListDivisionsByCompany', { companyId: companyId }, function (data) {
                    $division.empty().append('<option value="">-- เลือกสายงาน --</option>');
                    $.each(data, function (index, item) {
                        $division.append($('<option></option>').val(item.value).text(item.text));
                    });

                    const selectedId = $division.data('selected-id');
                    if (selectedId) {
                        $division.val(selectedId);
                    }
                    $division.prop('disabled', false).trigger('change');
                });
            }
        });

        // When Division changes -> Populate Departments
        $division.change(function () {
            const divisionId = $(this).val();

            $department.empty().append('<option value="">-- เลือกฝ่าย --</option>').prop('disabled', true);
            $section.empty().append('<option value="">-- เลือกแผนก --</option>').prop('disabled', true);

            if (divisionId) {
                $department.empty().append('<option value="">-- กำลังโหลด... --</option>');
                $.getJSON('/Lookup/GetSelectListDepartmentsByDivision', { id: divisionId }, function (data) {
                    $department.empty().append('<option value="">-- เลือกฝ่าย --</option>');
                    $.each(data, function (index, item) {
                        $department.append($('<option></option>').val(item.value).text(item.text));
                    });

                    const selectedId = $department.data('selected-id');
                    if (selectedId) {
                        $department.val(selectedId);
                    }
                    $department.prop('disabled', false).trigger('change');
                });
            }
        });

        // When Department changes -> Populate Sections
        $department.change(function () {
            const departmentId = $(this).val();

            $section.empty().append('<option value="">-- เลือกแผนก --</option>').prop('disabled', true);

            if (departmentId) {
                $section.empty().append('<option value="">-- กำลังโหลด... --</option>');
                $.getJSON('/Lookup/GetSelectListSectionsByDepartment', { id: departmentId }, function (data) {
                    $section.empty().append('<option value="">-- เลือกแผนก --</option>');
                    $.each(data, function (index, item) {
                        $section.append($('<option></option>').val(item.value).text(item.text));
                    });

                    const selectedId = $section.data('selected-id');
                    if (selectedId) {
                        $section.val(selectedId);
                    }
                    $section.prop('disabled', false);
                });
            }
        });
    }

    $(document).ready(function () {
        // Store initial selected IDs for the Edit page
        $('#DivisionId').data('selected-id', $('#DivisionId').val());
        $('#DepartmentId').data('selected-id', $('#DepartmentId').val());
        $('#SectionId').data('selected-id', $('#SectionId').val());

        setupCascadingDropdowns();

        // Trigger the change event on page load if a company is already selected
        if ($('#CompanyId').val()) {
            $('#CompanyId').trigger('change');
        }
    });

})(jQuery);
