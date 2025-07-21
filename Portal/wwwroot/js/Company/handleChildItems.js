$(document).ready(function () {
    // Generic function to handle adding new child items
    function addChildItem(containerSelector, templateSelector) {
        const container = $(containerSelector);
        const templateHtml = $(templateSelector).html();

        // Find the next index for model binding
        let newIndex = container.find('.child-item').length;

        // Replace the placeholder index with the new index
        const newItemHtml = templateHtml.replace(/--i--/g, newIndex);

        container.append(newItemHtml);

        // Re-parse the form validator to include new elements
        const form = container.closest('form');
        form.removeData('validator');
        form.removeData('unobtrusiveValidation');
        $.validator.unobtrusive.parse(form);
    }

    // Generic function to re-index items after removal
    function reindexItems(container, prefix) {
        container.find('.child-item').each(function (index, item) {
            $(item).find('input, select, textarea').each(function () {
                const name = $(this).attr('name');
                if (name) {
                    const newName = name.replace(new RegExp(`${prefix}\\[\\d+\\]`), `${prefix}[${index}]`);
                    $(this).attr('name', newName);
                }
            });
        });
    }

    // Event handler for all "add" buttons with the specific class
    $('.add-child-item').on('click', function () {
        const containerSelector = $(this).data('container');
        const templateSelector = $(this).data('template');
        addChildItem(containerSelector, templateSelector);
    });

    // Event handler for removing items (delegated)
    $('form').on('click', '.remove-child-item', function () {
        const itemToRemove = $(this).closest('.child-item');
        const container = itemToRemove.parent();

        itemToRemove.remove();

        // Determine the prefix and re-index
        if (container.is('#branchContainer')) {
            reindexItems(container, 'CompanyBranchViewModels');
        } else if (container.is('#divisionContainer')) {
            reindexItems(container, 'DivisionViewModels');
        }
    });
});
