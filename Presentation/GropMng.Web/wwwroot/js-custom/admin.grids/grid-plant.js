(function (window) {
    'use strict';

    function getConfig() {
        return window.gropPlantGridConfig || {};
    }

    function escapeHtml(value) {
        return window.GropAdminTable && window.GropAdminTable.escapeHtml
            ? window.GropAdminTable.escapeHtml(value)
            : String(value ?? '');
    }

    function renderFamily(data) {
        if (!data) {
            return '<span class="text-muted">-</span>';
        }

        return escapeHtml(data);
    }

    function renderCategory(data) {
        var config = getConfig();
        var categoryTexts = config.categoryTexts || {};
        var localized = window.GropAdminTable
            ? window.GropAdminTable.localizeByMap(data, categoryTexts, '')
            : (categoryTexts[data] || data || '');

        return '<span class="badge bg-label-primary">' + escapeHtml(localized) + '</span>';
    }

    function renderFlags(data, type, row) {
        var config = getConfig();
        var flags = [];

        if (row && row.isEdible) {
            flags.push('<span class="badge bg-label-success me-1">' + escapeHtml(config.edibleText || '') + '</span>');
        }

        if (row && row.isMedicinal) {
            flags.push('<span class="badge bg-label-info me-1">' + escapeHtml(config.medicinalText || '') + '</span>');
        }

        if (row && row.isToxic) {
            flags.push('<span class="badge bg-label-danger me-1">' + escapeHtml(config.toxicText || '') + '</span>');
        }

        if (flags.length === 0) {
            return '<span class="text-muted">' + escapeHtml(config.noneText || '') + '</span>';
        }

        return flags.join('');
    }

    function renderActions(id) {
        var config = getConfig();
        var safeId = escapeHtml(id);
        var editUrl = String(config.editUrl || '').replace(/\/$/, '');
        var deleteUrl = String(config.deleteUrl || '');

        return '<a class="btn btn-sm btn-outline-primary me-1" href="' + editUrl + '/' + safeId + '"><i class="bx bx-edit-alt"></i></a>' +
            '<form class="d-inline js-delete-form" action="' + escapeHtml(deleteUrl) + '" method="post" data-grop-delete="true" data-confirm-icon="warning" data-confirm-title="' + escapeHtml(config.deleteTitleText || '') + '" data-confirm-text="' + escapeHtml(config.deleteConfirmText || '') + '" data-confirm-button-text="' + escapeHtml(config.deleteText || '') + '" data-cancel-button-text="' + escapeHtml(config.cancelText || '') + '">' +
            '<input type="hidden" name="__RequestVerificationToken" value="' + escapeHtml(config.token || '') + '" />' +
            '<input type="hidden" name="id" value="' + safeId + '" />' +
            '<button type="submit" class="btn btn-sm btn-outline-danger"><i class="bx bx-trash"></i></button>' +
            '</form>';
    }

    window.GropPlantGridRenderers = {
        renderFamily: renderFamily,
        renderCategory: renderCategory,
        renderFlags: renderFlags,
        renderActions: renderActions
    };
})(window);
