(function (window, document, $) {
    'use strict';

    if (!window || !$) {
        return;
    }

    //selectedIds - This variable will be used on views. It can not be renamed
    window.selectedIds = window.selectedIds || [];

    function syncGlobalSelectedIds(tableSelector) {
        if (window.GropAdminTable && typeof window.GropAdminTable.getSelectedIds === 'function') {
            window.selectedIds = window.GropAdminTable.getSelectedIds(tableSelector);
        }
    }

    window.clearMasterCheckbox = function (tableSelector) {
        $(tableSelector).find('thead input[type="checkbox"]').prop('checked', false);

        if (window.GropAdminTable && typeof window.GropAdminTable.clearSelectedIds === 'function') {
            window.GropAdminTable.clearSelectedIds(tableSelector);
        }

        window.selectedIds = [];
    };

    window.updateMasterCheckbox = function (tableSelector) {
        var $table = $(tableSelector);
        var $master = $table.find('thead input[type="checkbox"]');
        var $checkboxes = $table.find('tbody input[type="checkbox"]');
        var checkedCount = $checkboxes.filter(':checked').length;

        $master.prop('checked', $checkboxes.length > 0 && checkedCount === $checkboxes.length);
        $master.prop('indeterminate', checkedCount > 0 && checkedCount < $checkboxes.length);

        syncGlobalSelectedIds(tableSelector);
    };

    window.updateTableSrc = function (tableSelector, isMasterCheckBoxUsed) {
        if (!$.fn.DataTable.isDataTable(tableSelector)) {
            return;
        }

        var table = $(tableSelector).DataTable();
        var dataSrc = table.data();
        table.clear().rows.add(dataSrc).draw();
        table.columns.adjust();

        if (isMasterCheckBoxUsed) {
            window.clearMasterCheckbox(tableSelector);
        }
    };

    window.updateTable = function (tableSelector, isMasterCheckBoxUsed) {
        if (window.GropAdminTable && typeof window.GropAdminTable.refreshTable === 'function') {
            if (isMasterCheckBoxUsed) {
                window.clearMasterCheckbox(tableSelector);
            }

            window.GropAdminTable.refreshTable(tableSelector, true);
            return;
        }

        if (!$.fn.DataTable.isDataTable(tableSelector)) {
            return;
        }

        $(tableSelector).DataTable().ajax.reload();
        $(tableSelector).DataTable().columns.adjust();

        if (isMasterCheckBoxUsed) {
            window.clearMasterCheckbox(tableSelector);
        }
    };

    window.updateTableWidth = function (tableSelector) {
        if ($.fn.DataTable.isDataTable(tableSelector)) {
            $(tableSelector).DataTable().columns.adjust();
        }
    };

    window.showAlert = function (alertId, message) {
        if (window.GropAdminTable && typeof window.GropAdminTable.showAlert === 'function') {
            window.GropAdminTable.showAlert(alertId, message);
            return;
        }

        var $alert = $('#' + alertId);
        if (!$alert.length) {
            return;
        }

        $alert.find('.grop-alert-message').text(message || '');
        $alert.removeClass('d-none').show();
    };

    window.addAntiForgeryToken = function (data) {
        var postData = data || {};
        var tokenInput = $('input[name="__RequestVerificationToken"]').first();

        if (tokenInput.length) {
            postData.__RequestVerificationToken = tokenInput.val();
        }

        return postData;
    };

    function initializeFlatpickrInputs() {
        if (typeof window.flatpickr !== 'function') {
            return;
        }

        document.querySelectorAll('.flatpickr-date').forEach(function (element) {
            if (element._flatpickr) {
                return;
            }

            window.flatpickr(element, {
                dateFormat: 'Y-m-d',
                allowInput: true,
                monthSelectorType: 'static'
            });
        });

        document.querySelectorAll('.flatpickr-datetime').forEach(function (element) {
            if (element._flatpickr) {
                return;
            }

            window.flatpickr(element, {
                enableTime: true,
                dateFormat: 'Y-m-d H:i',
                allowInput: true,
                monthSelectorType: 'static'
            });
        });
    }

    $(function () {
        initializeFlatpickrInputs();
    });
})(window, document, window.jQuery);
