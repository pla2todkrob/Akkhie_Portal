// =================================================================================
// division-form.js: Handles all dynamic interactions on the Division create/edit form.
// This includes managing the dynamic addition and removal of Department items.
// =================================================================================

$(document).ready(function () {
    /**
     * Manages the dynamic addition and removal of Department items in the form.
     * This function leverages the global app.manageDynamicItems for consistency.
     */
    function setupDynamicDepartmentItems() {
        const templateHtml = $('#departmentTemplate').html();
        if (!templateHtml) {
            console.error('Department template not found.');
            return;
        }

        app.manageDynamicItems({
            containerSelector: '#departmentContainer',
            itemSelector: '.child-item',
            addButtonSelector: '#add-department-btn',
            removeButtonSelector: '.remove-department-btn',
            template: templateHtml,
            focusSelector: '.child-item:last input[name*="Name"]',
            onAdd: () => {
                // Use Toastr for quick, non-blocking feedback as per system design.
                app.showSuccessToast('เพิ่มรายการฝ่ายใหม่เรียบร้อย');
            },
            onRemove: () => {
                app.showSuccessToast('ลบรายการฝ่ายเรียบร้อย');
            }
        });
    }

    // Initialize all functionalities for the form.
    setupDynamicDepartmentItems();
});
