$(function () {
    'use strict';

    // Initialize DataTable for the ticket list
    app.setupDataTable('#ticketTable', {
        // Order by the 'Created At' column descending by default
        order: [[5, 'desc']],
        columnDefs: [
            {
                targets: 6, // The 'Manage' column
                orderable: false,
                searchable: false
            },
            {
                targets: 5, // The 'Created At' column
                render: function (data, type, row, meta) {
                    // 'data' is the original display text from the <td>, e.g., "7/7/2568 09:04"
                    // 'type' is the purpose of the render ('display', 'sort', 'filter')

                    if (type === 'sort') {
                        // For sorting, get the value from the 'data-sort' attribute of the <td>.
                        // 'meta.settings.aoData[meta.row].nTr' provides the raw <tr> DOM element.
                        const trNode = meta.settings.aoData[meta.row].nTr;
                        // Find the 6th cell (index 5) and get its data-sort attribute.
                        return $(trNode).find('td:eq(5)').data('sort');
                    }

                    // For 'display', 'filter', and other types, just return the original display data.
                    return data;
                }
            }
        ]
    });
});
