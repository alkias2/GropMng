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
(function (window, $) {
    'use strict';

    if (!window || !$) {
        return;
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

        return {
            processing: `<div class="spinner-border text-primary" role="status"><span class="visually-hidden">${escapeHtml(texts.processing || 'Loading...')}</span></div>`,
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
                console.error('GropAdminTable: AJAX error', { xhr, error, thrown });
                window.GropSwal?.showError?.('Failed to load data. Please try again.');
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
                table.ajax.reload();
            });
        }

        if (config.clearButtonSelector) {
            $(config.clearButtonSelector).off('click').on('click', function () {
                if (typeof config.clearCallback === 'function') {
                    config.clearCallback();
                }

                table.ajax.reload();
            });
        }
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
            
            if (rowId && selectedIds.has(rowId)) {
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
        console.error('GropAdminTable action error:', error);
        window.GropSwal?.showError?.(message || 'An error occurred. Please try again.');
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
        const label = value ? (config.trueLabel || 'Yes') : (config.falseLabel || 'No');
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
        createDataTable: createDataTable,

        // Selection management (per-table, not global)
        getSelectedIds: getSelectedIds,
        setSelectedIds: setSelectedIds,
        addSelectedIds: addSelectedIds,
        removeSelectedIds: removeSelectedIds,
        clearSelectedIds: clearSelectedIds,

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

})(window, window.jQuery);
