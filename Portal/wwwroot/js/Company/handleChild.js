// handleChild.js
(function ($) {
    'use strict';

    const childTemplate = `
        <div class="col child-item">
            <div class="card border-0 shadow-sm h-100">
                <div class="card-header">
                    <div class="d-flex justify-content-end">
                        <button type="button" class="btn btn-sm btn-outline-danger remove-child">
                            <i class="bi bi-trash me-1"></i>ลบ
                        </button>
                    </div>
                </div>
                <div class="card-body">
                    <div class="row g-3">
                        <div class="col-md-9 col-lg-8">
                            <div class="form-floating">
                                <input name="CompanyBranchViewModels[{index}].Name"
                                       id="CompanyBranchViewModels_{index}__Name"
                                       class="form-control"
                                       placeholder="ชื่อสาขา"
                                       data-val="true"
                                       data-val-required="กรุณากรอกชื่อสาขา"
                                       data-val-maxlength="ชื่อสาขาต้องไม่เกิน 100 ตัวอักษร"
                                       data-val-maxlength-max="100"
                                       required>
                                <label for="CompanyBranchViewModels_{index}__Name">ชื่อสาขา</label>
                                <span class="text-danger small field-validation-valid"
                                      data-valmsg-for="CompanyBranchViewModels[{index}].Name"
                                      data-valmsg-replace="true"></span>
                            </div>
                        </div>
                        <div class="col-md-3 col-lg-4">
                            <div class="form-floating">
                                <input name="CompanyBranchViewModels[{index}].BranchCode"
                                       id="CompanyBranchViewModels_{index}__BranchCode"
                                       class="form-control"
                                       placeholder="รหัสสาขา"
                                       data-val="true"
                                       data-val-required="กรุณากรอกรหัสสาขา"
                                       data-val-maxlength="รหัสสาขาต้องไม่เกิน 10 ตัวอักษร"
                                       data-val-maxlength-max="10"
                                       required>
                                <label for="CompanyBranchViewModels_{index}__BranchCode">รหัสสาขา</label>
                                <span class="text-danger small field-validation-valid"
                                      data-valmsg-for="CompanyBranchViewModels[{index}].BranchCode"
                                      data-valmsg-replace="true"></span>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    `;

    $(document).ready(function () {
        app.manageDynamicItems({
            containerSelector: '#childContainer',
            itemSelector: '.child-item',
            addButtonSelector: '#add-child',
            removeButtonSelector: '.remove-child',
            clearButtonSelector: '#clear-child',
            template: childTemplate,
            focusSelector: '.child-item:last input[name*="Name"]',
            minItems: 1,
            minItemsError: 'ต้องมีอย่างน้อย 1 รายการ',
            deleteConfirmTitle: 'ยืนยันการลบรายการ',
            deleteConfirmText: 'คุณต้องการลบรายการนี้ใช่หรือไม่?',
            deleteConfirmButton: 'ลบ',
            onAdd: function () {
                app.showSuccessToast('เพิ่มรายการใหม่เรียบร้อยแล้ว');
            },
            onRemove: function () {
                app.showSuccessToast('ลบรายการเรียบร้อยแล้ว');
            }
        });
    });
})(jQuery);
