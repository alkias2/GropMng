/// <reference path="../js/jquery-3.7.1.min.js" />
/// <reference path="../js/datatables.min.js" />

/**
 * GropAdminTable - Shared DataTables infrastructure for admin grids.
 * 
 * Features:
 * - Per-table selection state management (no global shared arrays)
 * - Defensive response handling for AJAX errors
 * - Safe HTML escaping for all renders
 * - Refresh/update hooks for post-action cleanup
 * - Bootstrap 5 integration
 * - Localization support
 */
(function (window) {
    'use strict';

    function initialize($) {
        if (!window || !$) {
            return false;
        }

        if (window.GropAdminTable) {
            return true;
        }

    // ============================================================================
    // Private Data: Per-table selection state (not global)
    // ============================================================================

    /**
     * Storage for per-table selection state.
     * Key: table selector (e.g., '#plants-grid'), Value: Set of selected IDs
     * @private
     */
    const tableSelectionState = new Map();

    // ============================================================================
    // Private Utility Functions
    // ============================================================================

    /**
     * Escapes HTML special characters to prevent XSS attacks.
     * @param {*} value - The value to escape
     * @returns {string} The escaped HTML-safe value
     */
    function escapeHtml(value) {
        return String(value)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;');
    }

    /**
     * Formats a template string with positional arguments {0}, {1}, etc.
     * @param {string} template - The template string
     * @param {...*} args - Arguments to substitute
     * @returns {string} The formatted string
     */
    function formatString(template) {
        const values = Array.prototype.slice.call(arguments, 1);
        if (!template) {
            return '';
        }

        return template.replace(/\{(\d+)\}/g, function (match, index) {
            return values[index] !== undefined ? values[index] : match;
        });
    }

    /**
     * Maps a value using a key-value map, with optional fallback.
     * @param {*} value - The value to map
     * @param {Object} map - The mapping dictionary
     * @param {*} emptyValue - The fallback value if mapping fails
     * @returns {*} The mapped value or fallback
     */
    function localizeByMap(value, map, emptyValue) {
        if (value === null || value === undefined || value === '') {
            return emptyValue ?? '';
        }

        return map && map[value] ? map[value] : value;
    }

    /**
     * Builds a DataTables language configuration object.
     * @param {Object} localizedTexts - Localized text overrides
     * @returns {Object} The language configuration
     */
    function buildLanguage(localizedTexts) {
        const texts = localizedTexts || {};
        const commonTexts = window.gropCommonTexts || {};

        return {
            processing: `<div class="spinner-border text-primary" role="status"><span class="visually-hidden">${escapeHtml(texts.processing || commonTexts.loadingText || 'Loading...')}</span></div>`,
            lengthMenu: texts.lengthMenu || '_MENU_',
            zeroRecords: texts.zeroRecords || '',
            emptyTable: texts.emptyTable || '',
            info: texts.info || '',
            infoEmpty: texts.infoEmpty || '',
            infoFiltered: texts.infoFiltered || '',
            search: texts.search || '',
            paginate: {
                first: texts.first || '',
                last: texts.last || '',
                next: texts.next || '',
                previous: texts.previous || ''
            }
        };
    }

    /**
     * Converts response shape and values to DataTables-compatible format.
     * Supports both camelCase and PascalCase payloads.
     * @param {Object} response - The server response object
     * @returns {Object|null} Normalized response or null if response is invalid
     * @private
     */
    function normalizeResponse(response) {
        if (!response || typeof response !== 'object') {
            return null;
        }

        const drawRaw = response.draw ?? response.Draw;
        const recordsTotalRaw = response.recordsTotal ?? response.RecordsTotal;
        const recordsFilteredRaw = response.recordsFiltered ?? response.RecordsFiltered;
        const dataRaw = response.data ?? response.Data;

        const draw = Number.parseInt(drawRaw, 10);
        const recordsTotal = Number.parseInt(recordsTotalRaw, 10);
        const recordsFiltered = Number.parseInt(recordsFilteredRaw, 10);

        return {
            draw: Number.isFinite(draw) ? draw : 0,
            recordsTotal: Number.isFinite(recordsTotal) ? recordsTotal : 0,
            recordsFiltered: Number.isFinite(recordsFiltered) ? recordsFiltered : 0,
            data: Array.isArray(dataRaw) ? dataRaw : []
        };
    }

    /**
     * Validates that an AJAX response has the required DataTables fields.
     * @param {Object} response - The normalized server response object
     * @returns {boolean} True if response is valid, false otherwise
     * @private
     */
    function isValidResponse(response) {
        if (!response || typeof response !== 'object') {
            console.error('GropAdminTable: Invalid response type. Expected object.');
            return false;
        }

        if (!Array.isArray(response.data)) {
            console.error('GropAdminTable: Response.data must be an array.');
            return false;
        }

        if (!Number.isFinite(response.draw)) {
            console.error('GropAdminTable: Response.draw must be a number.');
            return false;
        }

        if (!Number.isFinite(response.recordsTotal) || !Number.isFinite(response.recordsFiltered)) {
            console.error('GropAdminTable: recordsTotal/recordsFiltered must be numbers.');
            return false;
        }

        return true;
    }

    /**
     * Gets or initializes the selection state for a table.
     * @param {string} tableSelector - The CSS selector or ID of the table
     * @returns {Set<*>} The Set of selected row IDs for this table
     * @private
     */
    function getSelectionState(tableSelector) {
        if (!tableSelectionState.has(tableSelector)) {
            tableSelectionState.set(tableSelector, new Set());
        }
        return tableSelectionState.get(tableSelector);
    }

    /**
     * Clears the selection state for a table.
     * @param {string} tableSelector - The CSS selector or ID of the table
     * @private
     */
    function clearSelectionState(tableSelector) {
        tableSelectionState.set(tableSelector, new Set());
    }

    // ============================================================================
    // Public API: DataTables Setup
    // ============================================================================

    /**
     * Creates and initializes a DataTable with enhanced error handling.
     * @param {string} selector - CSS selector for the table element
     * @param {Object} options - DataTables initialization options
     * @returns {DataTables.Api} The initialized DataTable instance
     */
    function createDataTable(selector, options) {
        const settings = $.extend(true, {
            processing: true,
            serverSide: true,
            responsive: true
        }, options || {});

        settings.language = $.extend(true, buildLanguage(settings.localizedTexts), settings.language || {});
        delete settings.localizedTexts;

        // Add defensive response handler
        if (!settings.ajax) {
            throw new Error('GropAdminTable: ajax configuration is required in options');
        }

        const originalAjax = typeof settings.ajax === 'string' 
            ? { url: settings.ajax, type: 'POST' } 
            : settings.ajax;

        const enhancedAjax = {
            ...originalAjax,
            dataFilter: function (data, type) {
                try {
                    const parsedResponse = JSON.parse(data);
                    const response = normalizeResponse(parsedResponse);
                    if (!isValidResponse(response)) {
                        console.error('GropAdminTable: Response validation failed. Using empty data.');
                        return JSON.stringify({
                            draw: response && Number.isFinite(response.draw) ? response.draw : 0,
                            recordsTotal: 0,
                            recordsFiltered: 0,
                            data: []
                        });
                    }
                    return JSON.stringify(response);
                } catch (e) {
                    console.error('GropAdminTable: Failed to parse response JSON:', e);
                    return JSON.stringify({
                        draw: 0,
                        recordsTotal: 0,
                        recordsFiltered: 0,
                        data: []
                    });
                }
            },
            error: function (xhr, error, thrown) {
                const commonTexts = window.gropCommonTexts || {};
                console.error('GropAdminTable: AJAX error — HTTP ' + xhr.status + ' ' + xhr.statusText
                    + ' | textStatus=' + error
                    + ' | responseText=' + (xhr.responseText ? xhr.responseText.substring(0, 300) : '(empty)'));
                window.GropSwal?.showError?.(commonTexts.networkErrorText || 'Failed to load data. Please try again.');
            }
        };

        settings.ajax = enhancedAjax;

        const table = $(selector).DataTable(settings);
        const processingSelector = `${selector}_processing`;

        function setProcessingVisible(isVisible) {
            const $processing = $(processingSelector);
            if ($processing.length === 0) {
                return;
            }

            if (isVisible) {
                $processing.stop(true, true).show();
            } else {
                $processing.stop(true, true).hide();
            }
        }

        // Defensive UI fix: ensure processing overlay is shown only during active requests.
        setProcessingVisible(false);
        table.on('preXhr.dt', function () {
            setProcessingVisible(true);
        });
        table.on('xhr.dt error.dt draw.dt', function () {
            setProcessingVisible(false);
        });
        table.on('processing.dt', function (e, dtSettings, processing) {
            setProcessingVisible(!!processing);
        });

        // Bind post-draw event for confirmations and selection restoration
        table.on('draw', function () {
            window.GropSwal?.bindConfirmForms("form[data-grop-confirm='true'], form[data-grop-delete='true']");
            
            // Restore selection state after draw
            const selectedIds = getSelectionState(selector);
            if (selectedIds.size > 0) {
                restoreSelectionState(selector, table, selectedIds);
            }
        });

        // Store reference to the table instance for selection management
        $(selector).data('gropTable', table);
        $(selector).data('gropTableSelector', selector);

        return table;
    }

    /**
     * Binds filter/search button click handlers.
     * @param {DataTables.Api} table - The DataTable instance
     * @param {Object} options - Configuration options
     * @param {string} options.applyButtonSelector - Selector for apply/search button
     * @param {string} options.clearButtonSelector - Selector for clear button
     * @param {Function} options.clearCallback - Callback when clearing filters
     */
    function bindFilterButtons(table, options) {
        const config = options || {};

        if (config.applyButtonSelector) {
            $(config.applyButtonSelector).off('click').on('click', function () {
                const tableSelector = '#' + table.table().node().id;
                clearSelectedIds(tableSelector);
                window.selectedIds = [];
                table.ajax.reload();
                return false;
            });
        }

        if (config.clearButtonSelector) {
            $(config.clearButtonSelector).off('click').on('click', function () {
                const tableSelector = '#' + table.table().node().id;
                if (typeof config.clearCallback === 'function') {
                    config.clearCallback();
                }

                clearSelectedIds(tableSelector);
                window.selectedIds = [];
                table.ajax.reload();
                return false;
            });
        }
    }

    /**
     * Clears a set of filter inputs by HTML element ID.
     * Automatically resets flatpickr-backed controls when present.
     * @param {Array<string>} filterIds - Array of HTML element IDs.
     */
    function clearFilterInputs(filterIds) {
        if (!Array.isArray(filterIds)) {
            return;
        }

        filterIds.forEach(function (filterId) {
            if (!filterId) {
                return;
            }

            const element = document.getElementById(filterId);
            if (!element) {
                return;
            }

            if (element._flatpickr) {
                element._flatpickr.clear();
            } else if (element.tagName === 'SELECT') {
                $(element).val('');
            } else if (element.type === 'checkbox') {
                element.checked = false;
            } else {
                element.value = '';
            }
        });
    }

    /**
     * Gets the current selection state for a table.
     * @param {string} tableSelector - The CSS selector for the table
     * @returns {Array<*>} Array of selected row IDs
     */
    function getSelectedIds(tableSelector) {
        const state = getSelectionState(tableSelector);
        return Array.from(state);
    }

    /**
     * Sets the selection state for a table.
     * @param {string} tableSelector - The CSS selector for the table
     * @param {Array<*>} ids - Array of IDs to select
     */
    function setSelectedIds(tableSelector, ids) {
        const state = getSelectionState(tableSelector);
        state.clear();
        if (Array.isArray(ids)) {
            ids.forEach(id => state.add(id));
        }
    }

    /**
     * Adds IDs to the current selection.
     * @param {string} tableSelector - The CSS selector for the table
     * @param {Array<*>} ids - Array of IDs to add
     */
    function addSelectedIds(tableSelector, ids) {
        const state = getSelectionState(tableSelector);
        if (Array.isArray(ids)) {
            ids.forEach(id => state.add(id));
        }
    }

    /**
     * Removes IDs from the current selection.
     * @param {string} tableSelector - The CSS selector for the table
     * @param {Array<*>} ids - Array of IDs to remove
     */
    function removeSelectedIds(tableSelector, ids) {
        const state = getSelectionState(tableSelector);
        if (Array.isArray(ids)) {
            ids.forEach(id => state.delete(id));
        }
    }

    /**
     * Clears all selections for a table.
     * @param {string} tableSelector - The CSS selector for the table
     */
    function clearSelectedIds(tableSelector) {
        clearSelectionState(tableSelector);
        window.selectedIds = [];
    }

    /**
     * Binds checkbox selection management for a DataTable.
     * Handles row checkboxes, master checkbox, and draw restore.
     * After calling this, use getSelectedIds(tableSelector) to read the current selection.
     * Fires 'grop:selectionChanged' event on the table element after each change.
     * @param {string} tableSelector - CSS selector for the table
     * @param {string} cbClass - CSS class of row checkboxes (e.g., 'dt-checkbox')
     * @param {Object} [options] - Options
     * @param {string} [options.masterSelector] - Selector for the master/select-all checkbox
     */
    function bindSelectionHandlers(tableSelector, cbClass, options) {
        const config = options || {};
        const checkboxClass = String(cbClass || 'checkboxGroups').split(' ')[0];
        const masterSelector = config.masterSelector || (tableSelector + '_wrapper .mastercheckbox, ' + tableSelector + ' thead input.mastercheckbox');
        const rowCheckboxSelector = 'tbody input[type="checkbox"].' + checkboxClass;

        $(tableSelector).off('change.gropSelection');
        $(tableSelector).off('click.gropSelection mousedown.gropSelection mouseup.gropSelection', rowCheckboxSelector);
        if (masterSelector) {
            $(document).off('change.gropSelectionMaster', masterSelector);
            $(document).off('click.gropSelectionMaster mousedown.gropSelectionMaster mouseup.gropSelectionMaster', masterSelector);
        }
        $(tableSelector).off('draw.dt.gropSelection');

        $(tableSelector).on('click.gropSelection mousedown.gropSelection mouseup.gropSelection', rowCheckboxSelector, function (event) {
            event.stopPropagation();
        });

        if (masterSelector) {
            $(document).on('click.gropSelectionMaster mousedown.gropSelectionMaster mouseup.gropSelectionMaster', masterSelector, function (event) {
                event.stopPropagation();
            });
        }

        function notifyChanged() {
            const ids = getSelectedIds(tableSelector);
            window.selectedIds = ids.slice();
            $(tableSelector).trigger('grop:selectionChanged', [ids]);
        }

        function syncMaster() {
            if (!masterSelector) {
                return;
            }

            const total = $(tableSelector).find(rowCheckboxSelector).length;
            const checked = $(tableSelector).find(rowCheckboxSelector + ':checked').length;
            $(masterSelector)
                .prop('checked', total > 0 && total === checked)
                .prop('indeterminate', checked > 0 && total > checked);
        }

        $(tableSelector).on('change.gropSelection', rowCheckboxSelector, function () {
            const val = String($(this).val());
            if ($(this).is(':checked')) {
                addSelectedIds(tableSelector, [val]);
            } else {
                removeSelectedIds(tableSelector, [val]);
            }

            syncMaster();
            notifyChanged();
        });

        if (masterSelector) {
            $(document).on('change.gropSelectionMaster', masterSelector, function () {
                const shouldSelect = $(this).is(':checked');
                const ids = [];

                $(tableSelector).find(rowCheckboxSelector).each(function () {
                    $(this).prop('checked', shouldSelect);
                    ids.push(String($(this).val()));
                });

                if (shouldSelect) {
                    setSelectedIds(tableSelector, ids);
                } else {
                    clearSelectedIds(tableSelector);
                }

                syncMaster();
                notifyChanged();
            });
        }

        $(tableSelector).on('draw.dt.gropSelection', function () {
            const selectedIds = getSelectionState(tableSelector);
            $(tableSelector).find(rowCheckboxSelector).each(function () {
                $(this).prop('checked', selectedIds.has(String($(this).val())));
            });

            syncMaster();
            notifyChanged();
        });

        syncMaster();
    }

    /**
     * Shows a previously hidden grop-alert element with an optional message.
     * @param {string} alertId - The id attribute of the alert element (without #)
     * @param {string} [message] - Optional message to display inside the alert
     */
    function showAlert(alertId, message) {
        const $alert = $('#' + alertId);
        if (!$alert.length) return;
        if (message) {
            $alert.find('.grop-alert-message').text(message);
        }
        $alert.removeClass('d-none').show();
    }

    /**
     * Wires up single-row and bulk-delete AJAX handlers for a DataTable.
     * Reads default confirmation texts from window.gropCommonTexts (set in _Layout).
     * Configuration overrides are optional — omit them to use the global defaults.
     *
     * @param {string} tableSelector - CSS selector for the table (e.g. '#appLogsTable')
     * @param {Object} config
     * @param {string} [config.deleteUrl]                  - POST URL for single-row delete
     * @param {string} [config.deleteSelectedUrl]          - POST URL for bulk delete
     * @param {string} [config.deleteSelectedButtonSelector] - Selector of the "Delete selected" button
     * @param {string} [config.selectedCountSelector]      - Selector of the badge showing count
     * @param {string} [config.alertId]                    - ID of a <grop-alert> element for errors
     * @param {string} [config.deleteTitle]                - Override confirm dialog title
     * @param {string} [config.deleteText]                 - Override confirm dialog single-item text
     * @param {string} [config.deleteItemsText]            - Override confirm dialog multi-item text template ({0})
     * @param {string} [config.deleteButtonText]           - Override confirm button label
     * @param {string} [config.cancelButtonText]           - Override cancel button label
     */
    function bindDeleteHandlers(tableSelector, config) {
        const cfg = config || {};
        const texts = window.gropCommonTexts || {};
        const token = function () { return $('input[name="__RequestVerificationToken"]').first().val(); };

        const title      = cfg.deleteTitle       || texts.deleteTitle       || texts.deleteButtonText || 'Delete';
        const text       = cfg.deleteText        || texts.deleteText        || 'Are you sure?';
        const itemsText  = cfg.deleteItemsText   || texts.deleteItemsText   || 'Are you sure you want to delete {0} items?';
        const btnConfirm = cfg.deleteButtonText  || texts.deleteButtonText  || texts.yesButtonText || 'Delete';
        const btnCancel  = cfg.cancelButtonText  || texts.cancelButtonText  || texts.noButtonText || 'Cancel';

        // Update selected-count badge and toggle delete-selected button
        $(tableSelector).on('grop:selectionChanged', function (e, ids) {
            if (cfg.selectedCountSelector) $(cfg.selectedCountSelector).text(ids.length);
            if (cfg.deleteSelectedButtonSelector) $(cfg.deleteSelectedButtonSelector).prop('disabled', ids.length === 0);
        });

        // Single row delete — triggered by any .btn-delete[data-id] inside the table
        if (cfg.deleteUrl) {
            $(tableSelector).on('click', '.btn-delete', async function () {
                var id = $(this).data('id');
                if (!await window.GropSwal.confirm({ icon: 'warning', title: title, text: text, confirmButtonText: btnConfirm, cancelButtonText: btnCancel })) return;
                $.post(cfg.deleteUrl, { id: id, __RequestVerificationToken: token() })
                    .done(function () {
                        window.GropAdminTable.removeSelectedIds(tableSelector, [String(id)]);
                        window.GropAdminTable.refreshTable(tableSelector, false);
                    })
                    .fail(function (jqXHR, textStatus, errorThrown) {
                        if (cfg.alertId) showAlert(cfg.alertId, errorThrown);
                    });
            });
        }

        // Bulk delete — triggered by the configured button
        if (cfg.deleteSelectedUrl && cfg.deleteSelectedButtonSelector) {
            $(cfg.deleteSelectedButtonSelector).on('click', async function () {
                var ids = getSelectedIds(tableSelector);
                if (!ids.length) return;
                var confirmText = ids.length === 1 ? text : formatString(itemsText, ids.length);
                if (!await window.GropSwal.confirm({ icon: 'warning', title: title, text: confirmText, confirmButtonText: btnConfirm, cancelButtonText: btnCancel })) return;
                $.ajax({
                    url: cfg.deleteSelectedUrl,
                    type: 'POST',
                    data: { selectedIds: ids, __RequestVerificationToken: token() },
                    traditional: true,
                    error: function (jqXHR, textStatus, errorThrown) { if (cfg.alertId) showAlert(cfg.alertId, errorThrown); },
                    complete: function (jqXHR) {
                        if (jqXHR.status === 204) return;
                        clearSelectedIds(tableSelector);
                        refreshTable(tableSelector, false);
                    }
                });
            });
        }
    }

    /**
     * Restores the visual selection state on table rows after a draw.
     * @param {string} tableSelector - The CSS selector for the table
     * @param {DataTables.Api} table - The DataTable instance
     * @param {Set<*>} selectedIds - Set of selected IDs
     * @private
     */
    function restoreSelectionState(tableSelector, table, selectedIds) {
        if (!selectedIds || selectedIds.size === 0) return;

        table.rows().every(function () {
            const rowData = this.data();
            const rowId = rowData && rowData.id ? rowData.id : null;
            
            if (rowId !== null && rowId !== undefined && selectedIds.has(String(rowId))) {
                $(this.node()).find('input[type="checkbox"]').prop('checked', true);
                $(this.node()).addClass('selected');
            }
        });
    }

    /**
     * Refreshes (reloads) the table data from the server.
     * @param {string} tableSelector - The CSS selector for the table
     * @param {boolean} resetPaging - Whether to reset to page 1 (default: true)
     */
    function refreshTable(tableSelector, resetPaging) {
        const table = $(tableSelector).data('gropTable');
        if (!table) {
            console.warn(`GropAdminTable: Table not found for selector: ${tableSelector}`);
            return;
        }

        if (resetPaging !== false) {
            table.page('first');
        }

        table.ajax.reload();
    }

    /**
     * Handles a successful action and refreshes the table.
     * Clears selections and shows a success message.
     * @param {string} tableSelector - The CSS selector for the table
     * @param {string} message - Optional success message to display
     * @param {boolean} resetPaging - Whether to reset to page 1 (default: true)
     */
    function handleActionSuccess(tableSelector, message, resetPaging) {
        if (message) {
            window.GropSwal?.showSuccess?.(message);
        }

        clearSelectedIds(tableSelector);
        refreshTable(tableSelector, resetPaging);
    }

    /**
     * Handles an action error.
     * @param {string} message - Error message to display
     * @param {*} error - Optional error object or details
     */
    function handleActionError(message, error) {
        const texts = window.gropCommonTexts || {};
        console.error('GropAdminTable action error:', error);
        window.GropSwal?.showError?.(message || texts.networkErrorText || 'An error occurred. Please try again.');
    }

    // ============================================================================
    // Public API: Render Helpers
    // ============================================================================

    /**
     * Renders an HTML-safe link with escaped text content.
     * @param {string} href - The link URL
     * @param {*} text - The link text (will be escaped)
     * @param {Object} options - Optional attributes (class, title, etc.)
     * @returns {string} The HTML for the link
     */
    function renderLink(href, text, options) {
        const config = options || {};
        const classList = config.class ? ` class="${escapeHtml(config.class)}"` : '';
        const title = config.title ? ` title="${escapeHtml(config.title)}"` : '';
        const target = config.target ? ` target="${escapeHtml(config.target)}"` : '';
        
        return `<a href="${escapeHtml(href)}"${classList}${title}${target}>${escapeHtml(text)}</a>`;
    }

    /**
     * Renders an HTML-safe button with icon.
     * @param {*} iconHtml - The icon HTML (e.g., '<i class="bx bx-pencil"></i>')
     * @param {Object} options - Button options (class, title, onClick, etc.)
     * @returns {string} The HTML for the button
     */
    function renderButton(iconHtml, options) {
        const config = options || {};
        const classList = config.class || 'btn btn-sm btn-primary';
        const title = config.title ? ` title="${escapeHtml(config.title)}"` : '';
        const onclick = config.onClick ? ` onclick="${escapeHtml(config.onClick)}"` : '';
        
        return `<button type="button" class="${classList}"${title}${onclick}>${iconHtml}</button>`;
    }

    /**
     * Renders a Bootstrap badge with optional custom class.
     * @param {*} text - Badge text (will be escaped)
     * @param {string} badgeClass - Bootstrap badge class (e.g., 'bg-success')
     * @returns {string} The HTML for the badge
     */
    function renderBadge(text, badgeClass) {
        const cls = badgeClass || 'bg-primary';
        return `<span class="badge ${escapeHtml(cls)}">${escapeHtml(text)}</span>`;
    }

    /**
     * Renders a formatted date string.
     * @param {*} dateValue - Date value (ISO string, timestamp, or Date object)
     * @param {string} format - Optional format function name or pattern
     * @returns {string} The formatted date
     */
    function renderDate(dateValue, format) {
        if (!dateValue) return '';

        try {
            const date = new Date(dateValue);
            if (isNaN(date.getTime())) {
                console.warn('GropAdminTable: Invalid date value:', dateValue);
                return '';
            }

            return format ? date.toLocaleDateString(format) : date.toLocaleDateString();
        } catch (e) {
            console.warn('GropAdminTable: Error formatting date:', e);
            return '';
        }
    }

    /**
     * Renders a status indicator (Yes/No, Active/Inactive, etc.).
     * @param {boolean} value - The boolean value
     * @param {Object} options - Label options
     * @param {string} options.trueLabel - Text for true (default: 'Yes')
     * @param {string} options.falseLabel - Text for false (default: 'No')
     * @param {string} options.trueClass - CSS class for true (default: 'badge bg-success')
     * @param {string} options.falseClass - CSS class for false (default: 'badge bg-danger')
     * @returns {string} The HTML for the status indicator
     */
    function renderStatus(value, options) {
        const config = options || {};
        const texts = window.gropCommonTexts || {};
        const label = value
            ? (config.trueLabel || texts.yesButtonText || 'Yes')
            : (config.falseLabel || texts.noButtonText || 'No');
        const cssClass = value ? (config.trueClass || 'badge bg-success') : (config.falseClass || 'badge bg-danger');
        
        return `<span class="${escapeHtml(cssClass)}">${escapeHtml(label)}</span>`;
    }

    // ============================================================================
    // Public API Exposure
    // ============================================================================

        window.GropAdminTable = {
            // Core DataTables functions
            buildLanguage: buildLanguage,
            bindFilterButtons: bindFilterButtons,
            clearFilterInputs: clearFilterInputs,
            createDataTable: createDataTable,

            // Selection management (per-table, not global)
            getSelectedIds: getSelectedIds,
            setSelectedIds: setSelectedIds,
            addSelectedIds: addSelectedIds,
            removeSelectedIds: removeSelectedIds,
            clearSelectedIds: clearSelectedIds,
            bindSelectionHandlers: bindSelectionHandlers,
            bindDeleteHandlers: bindDeleteHandlers,
            showAlert: showAlert,

            // Table refresh and action handlers
            refreshTable: refreshTable,
            handleActionSuccess: handleActionSuccess,
            handleActionError: handleActionError,

            // Utility functions
            formatString: formatString,
            localizeByMap: localizeByMap,
            escapeHtml: escapeHtml,

            // Render helpers
            renderLink: renderLink,
            renderButton: renderButton,
            renderBadge: renderBadge,
            renderDate: renderDate,
            renderStatus: renderStatus
        };

        return true;
    }

    // Defer initialization until jQuery is available
    var retryCount = 0;
    var maxRetries = 200; // Increased from 100 to allow more time

    function attemptInitialize() {
        if (window.jQuery) {
            if (initialize(window.jQuery)) {
                return;
            }
        }

        retryCount += 1;
        if (retryCount <= maxRetries) {
            window.setTimeout(attemptInitialize, 25); // Reduced delay from 50ms to 25ms for responsiveness
        }
        else {
            console.warn('GropAdminTable initialization skipped: jQuery is unavailable after 5 seconds.');
        }
    }

    // Start initialization attempt immediately when script loads
    attemptInitialize();
})(window);
