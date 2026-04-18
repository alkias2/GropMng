(function (window, $) {
    'use strict';

    function getConfig() {
        return window.gropAppLogGridConfig || {};
    }

    function escapeHtml(value) {
        return window.GropAdminTable && window.GropAdminTable.escapeHtml
            ? window.GropAdminTable.escapeHtml(value)
            : String(value ?? '');
    }

    function renderRowSelector(id) {
        return '<input type="checkbox" class="form-check-input row-selector" data-id="' + escapeHtml(id) + '" />';
    }

    function renderLevel(level) {
        var config = getConfig();
        var badgeMap = config.levelBadge || {};
        var levelTexts = config.levelTexts || {};
        var localizedLevel = window.GropAdminTable
            ? window.GropAdminTable.localizeByMap(level, levelTexts, '')
            : (levelTexts[level] || level || '');
        var color = badgeMap[level] || 'secondary';

        return '<span class="badge bg-label-' + escapeHtml(color) + '">' + escapeHtml(localizedLevel) + '</span>';
    }

    function renderTimestamp(data) {
        if (!data) {
            return '';
        }

        var config = getConfig();
        var culture = config.currentCulture || undefined;
        return new Date(data).toLocaleString(culture);
    }

    function renderActions(id) {
        var config = getConfig();
        var safeId = escapeHtml(id);
        var detailsUrl = String(config.detailsUrl || '').replace(/\/$/, '');

        return '<a class="btn btn-sm btn-icon btn-outline-primary me-1" href="' + detailsUrl + '/' + safeId + '" title="' + escapeHtml(config.detailsText || '') + '"><i class="bx bx-show"></i></a>' +
            '<button class="btn btn-sm btn-icon btn-outline-danger btn-delete" data-id="' + safeId + '" title="' + escapeHtml(config.deleteText || '') + '"><i class="bx bx-trash"></i></button>';
    }

    function init() {
        var config = getConfig();
        var tableSelector = '#appLogsTable';
        var maxRetries = 40;
        var retryDelayMs = 100;

        function bindWhenReady(retry) {
            var $table = $(tableSelector);
            if ($table.length === 0 || !$.fn.DataTable || !$.fn.DataTable.isDataTable(tableSelector)) {
                if (retry < maxRetries) {
                    window.setTimeout(function () {
                        bindWhenReady(retry + 1);
                    }, retryDelayMs);
                }

                return;
            }

            wireTable($table);
        }

        function wireTable($table) {

            var table = $table.DataTable();
            table.order([5, 'desc']).draw(false);

            var selectedIds = new Set(window.GropAdminTable.getSelectedIds(tableSelector) || []);

            function syncSelectionState() {
                window.GropAdminTable.setSelectedIds(tableSelector, Array.from(selectedIds));
            }

            function updateSelectAllVisibleState() {
                var visibleCheckboxes = $(tableSelector + ' tbody .row-selector');
                var totalVisible = visibleCheckboxes.length;

            if (totalVisible === 0) {
                $('#selectAllVisible').prop('checked', false).prop('indeterminate', false);
                return;
            }

            var checkedVisible = visibleCheckboxes.filter(':checked').length;
            $('#selectAllVisible')
                .prop('checked', checkedVisible === totalVisible)
                .prop('indeterminate', checkedVisible > 0 && checkedVisible < totalVisible);
        }

            function updateSelectionUi() {
                $('#selectedCount').text(selectedIds.size);
                $('#btnDeleteSelected').prop('disabled', selectedIds.size === 0);
                updateSelectAllVisibleState();
            }

            function reapplyRowSelections() {
                $(tableSelector + ' tbody .row-selector').each(function () {
                    var id = Number($(this).data('id'));
                    $(this).prop('checked', selectedIds.has(id));
                });

                updateSelectionUi();
            }

            table.on('draw', function () {
                reapplyRowSelections();
            });

            window.GropAdminTable.bindFilterButtons(table, {
                applyButtonSelector: '#btnApplyFilters',
                clearButtonSelector: '#btnClearFilters',
                clearCallback: function () {
                    document.getElementById('levelFilter').value = '';
                    if (window.gropAppLogDatePickers) {
                        window.gropAppLogDatePickers.from.clear();
                        window.gropAppLogDatePickers.to.clear();
                    }
                }
            });

            $table.on('change', '.row-selector', function () {
                var id = Number($(this).data('id'));
                if ($(this).is(':checked')) {
                    selectedIds.add(id);
                } else {
                    selectedIds.delete(id);
                }

                syncSelectionState();
                updateSelectionUi();
            });

            $('#selectAllVisible').on('change', function () {
                var shouldCheck = $(this).is(':checked');

                $(tableSelector + ' tbody .row-selector').each(function () {
                    var id = Number($(this).data('id'));
                    $(this).prop('checked', shouldCheck);

                    if (shouldCheck) {
                        selectedIds.add(id);
                    } else {
                        selectedIds.delete(id);
                    }
                });

                syncSelectionState();
                updateSelectionUi();
            });

            $('#btnDeleteSelected').on('click', async function () {
                if (selectedIds.size === 0) {
                    return;
                }

                var selectedArray = Array.from(selectedIds);
                var confirmationText = selectedArray.length === 1
                    ? config.deleteSelectedConfirmSingle
                    : window.GropAdminTable.formatString(config.deleteSelectedConfirmMultiple, selectedArray.length);

                var confirmed = await window.GropSwal.confirm({
                    icon: 'warning',
                    title: config.deleteSelectedTitle,
                    text: confirmationText,
                    confirmButtonText: config.deleteSelectedConfirmButton,
                    cancelButtonText: config.cancelText
                });

                if (!confirmed) {
                    return;
                }

                $.ajax({
                    url: config.deleteSelectedUrl,
                    type: 'POST',
                    traditional: true,
                    data: {
                        selectedIds: selectedArray,
                        __RequestVerificationToken: config.token
                    },
                    success: function () {
                        selectedIds.clear();
                        syncSelectionState();
                        $('#selectAllVisible').prop('checked', false).prop('indeterminate', false);
                        table.ajax.reload(null, false);
                        window.GropSwal.notify({ icon: 'success', title: config.deleteSelectedSuccess });
                    },
                    error: function (xhr) {
                        var message = xhr && xhr.responseJSON && xhr.responseJSON.message
                            ? xhr.responseJSON.message
                            : config.deleteSelectedError;
                        window.GropSwal.alert({ icon: 'error', title: config.deleteSelectedTitle, text: message });
                    }
                });
            });

            $table.on('click', '.btn-delete', async function () {
                var id = $(this).data('id');
                var confirmed = await window.GropSwal.confirm({
                    icon: 'warning',
                    title: config.deleteSingleTitle,
                    text: config.deleteSingleConfirm,
                    confirmButtonText: config.deleteText,
                    cancelButtonText: config.cancelText
                });

                if (!confirmed) {
                    return;
                }

                $.ajax({
                    url: config.deleteUrl,
                    type: 'POST',
                    data: {
                        id: id,
                        __RequestVerificationToken: config.token
                    },
                    success: function () {
                        selectedIds.delete(Number(id));
                        syncSelectionState();
                        table.ajax.reload(null, false);
                        window.GropSwal.notify({ icon: 'success', title: config.deleteSingleSuccess });
                    },
                    error: function () {
                        window.GropSwal.alert({ icon: 'error', title: config.deleteSingleTitle, text: config.deleteSingleError });
                    }
                });
            });

            syncSelectionState();
            updateSelectionUi();
        }

        bindWhenReady(0);
    }

    window.GropAppLogGridRenderers = {
        renderRowSelector: renderRowSelector,
        renderLevel: renderLevel,
        renderTimestamp: renderTimestamp,
        renderActions: renderActions
    };

    window.GropAppLogGrid = {
        init: init
    };
})(window, window.jQuery);
