// wwwroot/js/Division/handleChild.js
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
                        <div class="form-floating">
                            <input name="DepartmentViewModels[{index}].Name"
                                       id="DepartmentViewModels_{index}__Name"
                                       class="form-control"
                                       placeholder="ชื่อฝ่าย"
                                       data-val="true"
                                       data-val-required="กรุณากรอกชื่อฝ่าย"
                                       data-val-maxlength="ชื่อฝ่ายต้องไม่เกิน 100 ตัวอักษร"
                                       data-val-maxlength-max="100"
                                       required>
                            <label for="DepartmentViewModels_{index}__Name">ชื่อฝ่าย</label>
                                <span class="text-danger small field-validation-valid"
                                      data-valmsg-for="DepartmentViewModels[{index}].Name"
                                      data-valmsg-replace="true"></span>
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
            template: childTemplate,
            focusSelector: '.child-item:last input[name*="Name"]',
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
