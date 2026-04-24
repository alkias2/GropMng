/**
 * Inline editing for the Locale Resources admin grid.
 *
 * Responsibilities:
 * - Render Edit / Delete action buttons in the DataTables actions column.
 * - Replace a row with editable inputs on Edit click.
 * - POST to UpdateLocaleResource / DeleteLocaleResource via AJAX.
 * - Show the Add Resource panel and POST to AddLocaleResource.
 * - Uses window.localeResourceConfig (injected by the Razor view).
 */
(function (window, document, $) {
    'use strict';

    if (!window || !$) {
        return;
    }

    // ============================================================================
    // Config (injected by Razor view)
    // ============================================================================

    var cfg = window.localeResourceConfig || {};
    var texts = cfg.texts || {};

    function getToken() {
        return $('input[name="__RequestVerificationToken"]').first().val();
    }

    function escHtml(value) {
        if (window.GropAdminTable && typeof window.GropAdminTable.escapeHtml === 'function') {
            return window.GropAdminTable.escapeHtml(value);
        }
        return String(value)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;');
    }

    function refreshTable() {
        if (window.updateTable) {
            window.updateTable(cfg.tableSelector, false);
        } else if ($.fn.DataTable && $.fn.DataTable.isDataTable(cfg.tableSelector)) {
            $(cfg.tableSelector).DataTable().ajax.reload(null, false);
        }
    }

    function showTableError(message) {
        var $alert = $('#localeResourceTableError');
        if (!$alert.length) {
            $alert = $('<div id="localeResourceTableError" class="alert alert-danger mt-2" role="alert"></div>');
            $(cfg.tableSelector).closest('.card').before($alert);
        }
        $alert.text(message).removeClass('d-none');
        window.setTimeout(function () { $alert.addClass('d-none'); }, 6000);
    }

    // ============================================================================
    // DataTables custom render — called by Table.cshtml via RenderCustom
    // ============================================================================

    /**
     * Renders Edit and Delete buttons for a row.
     * DataTables passes (data, type, row) — data is the cell value (id), row is the full row object.
     */
    window.localeResource_renderActions = function (data, type, row) {
        if (type !== 'display') { return ''; }
        var id = escHtml(row.id);
        var editText = escHtml(texts.edit || 'Edit');
        var deleteText = escHtml(texts.delete || 'Delete');
        return '<button class="btn btn-sm btn-icon btn-outline-secondary locale-resource-edit-btn me-1" ' +
                   'data-id="' + id + '" title="' + editText + '"><i class="bx bx-pencil"></i></button>' +
               '<button class="btn btn-sm btn-icon btn-outline-danger locale-resource-delete-btn" ' +
                   'data-id="' + id + '" title="' + deleteText + '"><i class="bx bx-trash"></i></button>';
    };

    // ============================================================================
    // Inline row editing
    // ============================================================================

    /**
     * Replaces a standard data row with an editable row.
     * @param {jQuery} $tr - The <tr> element.
     * @param {object} row - The DataTables row data object.
     */
    function enterEditMode($tr, row) {
        // Prevent double-edit
        if ($tr.hasClass('locale-resource-editing')) {
            return;
        }
        $tr.addClass('locale-resource-editing');

        var nameVal  = escHtml(row.resourceName);
        var valueVal = escHtml(row.resourceValue);

        var $cells = $tr.find('td');

        // Col 0: ID — keep as-is
        // Col 1: ResourceName — make editable
        $cells.eq(1).html(
            '<input type="text" class="form-control form-control-sm locale-resource-name-input" ' +
            'value="' + nameVal + '" maxlength="400" />'
        );
        // Col 2: ResourceValue — make editable
        $cells.eq(2).html(
            '<input type="text" class="form-control form-control-sm locale-resource-value-input" ' +
            'value="' + valueVal + '" />'
        );
        // Col 3: Actions — replace with Save/Cancel
        var saveText = escHtml(texts.save || 'Save');
        var cancelText = escHtml(texts.cancel || 'Cancel');
        $cells.eq(3).html(
            '<button class="btn btn-sm btn-success locale-resource-save-btn me-1" ' +
                'data-id="' + escHtml(row.id) + '" ' +
                'data-language-id="' + escHtml(row.languageId) + '">' +
                '<i class="bx bx-save me-1"></i>' + saveText +
            '</button>' +
            '<button class="btn btn-sm btn-outline-secondary locale-resource-cancel-btn">' +
                cancelText +
            '</button>'
        );
    }

    /**
     * Exits edit mode by refreshing the table from the server.
     */
    function exitEditMode() {
        refreshTable();
    }

    /**
     * Saves an inline-edited row.
     * @param {jQuery} $tr - The <tr> element in edit mode.
     */
    function saveRow($tr) {
        var $saveBtn = $tr.find('.locale-resource-save-btn');
        var id = parseInt($saveBtn.data('id'), 10);
        var languageId = parseInt($saveBtn.data('language-id'), 10);
        var resourceName  = $tr.find('.locale-resource-name-input').val().trim();
        var resourceValue = $tr.find('.locale-resource-value-input').val().trim();

        if (!resourceName) {
            alert('Resource name is required.');
            return;
        }

        $saveBtn.prop('disabled', true);

        $.ajax({
            url: cfg.urlUpdate,
            type: 'POST',
            data: {
                Id: id,
                LanguageId: languageId,
                ResourceName: resourceName,
                ResourceValue: resourceValue,
                __RequestVerificationToken: getToken()
            },
            dataType: 'json',
            success: function (response) {
                if (response && response.success) {
                    exitEditMode();
                } else {
                    $saveBtn.prop('disabled', false);
                    showTableError('Update failed.');
                }
            },
            error: function (jqXHR) {
                $saveBtn.prop('disabled', false);
                var msg = 'Update failed.';
                try {
                    var body = JSON.parse(jqXHR.responseText);
                    if (body && body.errors) {
                        msg = Object.values(body.errors).flat().join(' ');
                    }
                } catch (e) { /* ignore */ }
                showTableError(msg);
            }
        });
    }

    function confirmDeleteResource() {
        if (window.GropSwal && typeof window.GropSwal.confirm === 'function') {
            return window.GropSwal.confirm({
                icon: 'warning',
                confirmButtonClass: 'btn btn-danger me-2',
                cancelButtonClass: 'btn btn-outline-secondary'
            });
        }

        return Promise.resolve(window.confirm((window.gropCommonTexts && window.gropCommonTexts.deleteText) || 'Are you sure?'));
    }

    /**
     * Deletes a resource row after confirmation.
     * @param {number} id - The resource ID.
     */
    async function deleteRow(id) {
        var isConfirmed = await confirmDeleteResource();
        if (!isConfirmed) {
            return;
        }

        $.ajax({
            url: cfg.urlDelete,
            type: 'POST',
            data: {
                id: id,
                __RequestVerificationToken: getToken()
            },
            dataType: 'json',
            success: function (response) {
                if (response && response.success) {
                    refreshTable();
                } else {
                    showTableError('Delete failed.');
                }
            },
            error: function (jqXHR) {
                var msg = 'Delete failed.';
                try {
                    var body = JSON.parse(jqXHR.responseText);
                    if (body && body.errors) {
                        msg = Object.values(body.errors).flat().join(' ');
                    }
                } catch (e) { /* ignore */ }
                showTableError(msg);
            }
        });
    }

    // ============================================================================
    // Add Resource panel
    // ============================================================================

    function showAddPanel() {
        $('#addResourceErrors').addClass('d-none').text('');
        $('#addResourceName').val('');
        $('#addResourceValue').val('');
        $('#addResourcePanel').removeClass('d-none');
        $('#addResourceName').focus();
    }

    function hideAddPanel() {
        $('#addResourcePanel').addClass('d-none');
    }

    function saveNewResource() {
        var name  = $('#addResourceName').val().trim();
        var value = $('#addResourceValue').val().trim();

        if (!name) {
            $('#addResourceErrors').text('Resource name is required.').removeClass('d-none');
            return;
        }

        var $btn = $('#btnSaveNewResource').prop('disabled', true);

        $.ajax({
            url: cfg.urlAdd,
            type: 'POST',
            data: {
                LanguageId: cfg.languageId,
                ResourceName: name,
                ResourceValue: value,
                __RequestVerificationToken: getToken()
            },
            dataType: 'json',
            success: function (response) {
                $btn.prop('disabled', false);
                if (response && response.success) {
                    hideAddPanel();
                    refreshTable();
                } else {
                    $('#addResourceErrors').text('Save failed.').removeClass('d-none');
                }
            },
            error: function (jqXHR) {
                $btn.prop('disabled', false);
                var msg = 'Save failed.';
                try {
                    var body = JSON.parse(jqXHR.responseText);
                    if (body && body.errors) {
                        msg = Object.values(body.errors).flat().join(' ');
                    }
                } catch (e) { /* ignore */ }
                $('#addResourceErrors').text(msg).removeClass('d-none');
            }
        });
    }

    // ============================================================================
    // Event bindings
    // ============================================================================

    $(function () {
        // Add Resource toggle
        $(document).on('click', '#btnAddResource', function () {
            showAddPanel();
        });
        $(document).on('click', '#btnCancelNewResource', function () {
            hideAddPanel();
        });
        $(document).on('click', '#btnSaveNewResource', function () {
            saveNewResource();
        });
        $('#addResourceName, #addResourceValue').on('keydown', function (e) {
            if (e.key === 'Enter') { saveNewResource(); }
            if (e.key === 'Escape') { hideAddPanel(); }
        });

        // Delegate row-level events to the table body (rows are redrawn after each AJAX reload)
        $(document).on('click', cfg.tableSelector + ' .locale-resource-edit-btn', function () {
            var $btn = $(this);
            var $tr  = $btn.closest('tr');
            var id   = parseInt($btn.data('id'), 10);
            var dt   = $(cfg.tableSelector).DataTable();
            var row  = dt.row($tr).data();
            if (row) {
                enterEditMode($tr, row);
            }
        });

        $(document).on('click', cfg.tableSelector + ' .locale-resource-save-btn', function () {
            var $tr = $(this).closest('tr');
            saveRow($tr);
        });

        $(document).on('click', cfg.tableSelector + ' .locale-resource-cancel-btn', function () {
            exitEditMode();
        });

        $(document).on('click', cfg.tableSelector + ' .locale-resource-delete-btn', function () {
            var id = parseInt($(this).data('id'), 10);
            deleteRow(id);
        });
    });

})(window, document, jQuery);
