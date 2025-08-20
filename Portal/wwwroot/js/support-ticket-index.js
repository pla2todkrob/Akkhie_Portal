$(function () {
    'use strict';

    // ฟังก์ชันสำหรับตั้งค่า DataTable
    const initializeDataTable = (tableSelector) => {
        if ($.fn.DataTable.isDataTable(tableSelector)) {
            return; // ไม่ต้องสร้างซ้ำถ้ามีอยู่แล้ว
        }

        app.setupDataTable(tableSelector, {
            order: [[4, 'desc']], // เรียงตามวันที่แจ้งล่าสุด
            columnDefs: [
                { targets: 5, orderable: false, searchable: false }, // คอลัมน์ "จัดการ"
                {
                    targets: 4, // คอลัมน์ "วันที่แจ้ง"
                    render: function (data, type, row, meta) {
                        if (type === 'sort') {
                            const trNode = meta.settings.aoData[meta.row].nTr;
                            return $(trNode).find('td:eq(4)').data('sort');
                        }
                        return data;
                    }
                }
            ]
        });
    };

    // ตั้งค่า DataTable สำหรับ Tab แรกที่ Active อยู่
    const activeTabPane = $('#ticketStatusTabsContent .tab-pane.active');
    if (activeTabPane.length > 0) {
        const initialTable = activeTabPane.find('.ticket-table');
        if (initialTable.length > 0) {
            initializeDataTable(initialTable);
        }
    }

    // Event listener สำหรับเมื่อมีการเปลี่ยน Tab
    $('#ticketStatusTabs button[data-bs-toggle="tab"]').on('shown.bs.tab', function (event) {
        const targetPaneId = $(event.target).data('bs-target');
        const tableInTab = $(targetPaneId).find('.ticket-table');
        if (tableInTab.length > 0) {
            initializeDataTable(tableInTab);
        }
    });
});
