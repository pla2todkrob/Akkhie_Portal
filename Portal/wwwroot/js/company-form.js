// company-form.js: Handles all dynamic interactions on the Company create/edit form.

$(document).ready(function () {
    /**
     * Re-indexes form inputs within a container for correct model binding after an item is removed.
     * @param {jQuery} container - The container element (e.g., #branchContainer).
     * @param {string} prefix - The model binding prefix (e.g., 'CompanyBranchViewModels').
     */
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

    /**
     * Adds a new child item from a template to a container.
     * @param {string} containerSelector - The selector for the container.
     * @param {string} templateSelector - The selector for the template script/div.
     */
    function addChildItem(containerSelector, templateSelector) {
        const container = $(containerSelector);
        const templateHtml = $(templateSelector).html();
        const newIndex = container.find('.child-item').length;
        const newItemHtml = templateHtml.replace(/--i--/g, newIndex);

        container.append(newItemHtml);

        // Re-parse the form validator to include new elements
        const form = container.closest('form');
        form.removeData('validator').removeData('unobtrusiveValidation');
        $.validator.unobtrusive.parse(form);
    }

    // Event handler for all "add" buttons
    $('.add-child-item').on('click', function () {
        const containerSelector = $(this).data('container');
        const templateSelector = $(this).data('template');
        addChildItem(containerSelector, templateSelector);
    });

    // Delegated event handler for removing items
    $('#SaveForm').on('click', '.remove-child-item', function () {
        const itemToRemove = $(this).closest('.child-item');
        const container = itemToRemove.parent();
        const isBranchContainer = container.is('#branchContainer');
        const isDivisionContainer = container.is('#divisionContainer');

        itemToRemove.remove();

        // Re-index the remaining items
        if (isBranchContainer) {
            reindexItems(container, 'CompanyBranchViewModels');
        } else if (isDivisionContainer) {
            reindexItems(container, 'DivisionViewModels');
        }
    });
});
