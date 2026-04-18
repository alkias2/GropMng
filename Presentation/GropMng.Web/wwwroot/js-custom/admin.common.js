/**
 * Shared admin helper functions used across the back-office UI.
 *
 * Responsibilities:
 * - keep the legacy global selection array synchronized with the shared DataTables layer
 * - expose Nop-style global helper methods expected by Razor views and partials
 * - attach anti-forgery tokens to AJAX payloads
 * - initialize Flatpickr inputs with a consistent cross-device configuration
 */
(function (window, document, $) {
    'use strict';

    // Abort immediately when the browser context or jQuery is not available.
    if (!window || !$) {
        return;
    }

    // selectedIds is intentionally kept as a global legacy variable because multiple
    // admin views still reference it directly from inline scripts.
    // The exact name must not change unless all views are refactored together.
    window.selectedIds = window.selectedIds || [];

    // ============================================================================
    // Selection state helpers
    // ============================================================================

    /**
     * Synchronizes the legacy global selection array with the per-table selection state
     * maintained by the shared GropAdminTable infrastructure.
     *
     * @param {string} tableSelector - CSS selector of the target DataTable.
     */
    function syncGlobalSelectedIds(tableSelector) {
        if (window.GropAdminTable && typeof window.GropAdminTable.getSelectedIds === 'function') {
            window.selectedIds = window.GropAdminTable.getSelectedIds(tableSelector);
        }
    }

    /**
     * Clears the master checkbox and resets all tracked selected ids for a grid.
     * This helper is called after destructive or refresh actions to avoid stale selection state.
     *
     * @param {string} tableSelector - CSS selector of the target DataTable.
     */
    window.clearMasterCheckbox = function (tableSelector) {
        $(tableSelector).find('thead input[type="checkbox"]').prop('checked', false);

        if (window.GropAdminTable && typeof window.GropAdminTable.clearSelectedIds === 'function') {
            window.GropAdminTable.clearSelectedIds(tableSelector);
        }

        window.selectedIds = [];
    };

    /**
     * Updates the visual state of the master checkbox based on the current row selection.
     * Supports the checked and indeterminate states used by the shared admin grids.
     *
     * @param {string} tableSelector - CSS selector of the target DataTable.
     */
    window.updateMasterCheckbox = function (tableSelector) {
        var $table = $(tableSelector);
        var $master = $table.find('thead input[type="checkbox"]');
        var $checkboxes = $table.find('tbody input[type="checkbox"]');
        var checkedCount = $checkboxes.filter(':checked').length;

        $master.prop('checked', $checkboxes.length > 0 && checkedCount === $checkboxes.length);
        $master.prop('indeterminate', checkedCount > 0 && checkedCount < $checkboxes.length);

        syncGlobalSelectedIds(tableSelector);
    };

    // ============================================================================
    // DataTables refresh helpers
    // ============================================================================

    /**
     * Rebinds and redraws a client-side DataTable using the current in-memory data source.
     * This is primarily used when server-side reloading is not needed.
     *
     * @param {string} tableSelector - CSS selector of the target DataTable.
     * @param {boolean} isMasterCheckBoxUsed - Indicates whether selection cleanup is required.
     */
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

    /**
     * Reloads a DataTable from its AJAX source and optionally clears row selection.
     * When the shared GropAdminTable wrapper is available, that path is preferred.
     *
     * @param {string} tableSelector - CSS selector of the target DataTable.
     * @param {boolean} isMasterCheckBoxUsed - Indicates whether selection cleanup is required.
     */
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

    /**
     * Recalculates DataTable column widths after layout changes such as tab switches,
     * collapsing cards, or responsive viewport updates.
     *
     * @param {string} tableSelector - CSS selector of the target DataTable.
     */
    window.updateTableWidth = function (tableSelector) {
        if ($.fn.DataTable.isDataTable(tableSelector)) {
            $(tableSelector).DataTable().columns.adjust();
        }
    };

    // ============================================================================
    // Shared feedback and security helpers
    // ============================================================================

    /**
     * Displays a shared alert message inside a grop-alert container.
     * Falls back to direct DOM manipulation when the shared table helper is unavailable.
     *
     * @param {string} alertId - The id of the alert element without the leading # character.
     * @param {string} message - Message text to display.
     */
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

    /**
     * Adds the ASP.NET Core anti-forgery token to an outgoing AJAX payload.
     * This keeps POST requests compatible with the application's CSRF protection.
     *
     * @param {Object} data - The payload object that will be sent to the server.
     * @returns {Object} The same payload object enriched with the verification token.
     */
    window.addAntiForgeryToken = function (data) {
        var postData = data || {};
        var tokenInput = $('input[name="__RequestVerificationToken"]').first();

        if (tokenInput.length) {
            postData.__RequestVerificationToken = tokenInput.val();
        }

        return postData;
    };

    // ============================================================================
    // Flatpickr initialization
    // ============================================================================

    /**
     * Initializes all shared date and datetime inputs that opt into Flatpickr.
     * The configuration intentionally disables the native mobile picker so the UI stays
     * consistent across desktop and touch devices.
     */
    function initializeFlatpickrInputs() {
        if (typeof window.flatpickr !== 'function') {
            return;
        }

        // Shared defaults for every admin date picker instance.
        var commonFlatpickrOptions = {
            allowInput: true,
            monthSelectorType: 'static',
            disableMobile: true,
            position: 'auto left',
            appendTo: document.body
        };

        // Date-only controls.
        document.querySelectorAll('.flatpickr-date').forEach(function (element) {
            if (element._flatpickr) {
                return;
            }

            window.flatpickr(element, Object.assign({}, commonFlatpickrOptions, {
                dateFormat: 'Y-m-d'
            }));
        });

        // Date-and-time controls using a 24-hour clock format.
        document.querySelectorAll('.flatpickr-datetime').forEach(function (element) {
            if (element._flatpickr) {
                return;
            }

            window.flatpickr(element, Object.assign({}, commonFlatpickrOptions, {
                enableTime: true,
                time_24hr: true,
                dateFormat: 'Y-m-d H:i'
            }));
        });
    }

    // Initialize shared date controls once the DOM is ready.
    $(function () {
        initializeFlatpickrInputs();
    });
})(window, document, window.jQuery);
