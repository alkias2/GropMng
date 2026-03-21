(function (window, $) {
    'use strict';

    if (!window || !$) {
        return;
    }

    function escapeHtml(value) {
        return String(value)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;');
    }

    function formatString(template) {
        const values = Array.prototype.slice.call(arguments, 1);
        if (!template) {
            return '';
        }

        return template.replace(/\{(\d+)\}/g, function (match, index) {
            return values[index] !== undefined ? values[index] : match;
        });
    }

    function localizeByMap(value, map, emptyValue) {
        if (value === null || value === undefined || value === '') {
            return emptyValue ?? '';
        }

        return map && map[value] ? map[value] : value;
    }

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

    function createDataTable(selector, options) {
        const settings = $.extend(true, {
            processing: true,
            serverSide: true,
            responsive: true
        }, options || {});

        settings.language = $.extend(true, buildLanguage(settings.localizedTexts), settings.language || {});
        delete settings.localizedTexts;

        const table = $(selector).DataTable(settings);
        table.on('draw', function () {
            window.GropSwal?.bindConfirmForms("form[data-grop-confirm='true'], form[data-grop-delete='true']");
        });

        return table;
    }

    function bindFilterButtons(table, options) {
        const config = options || {};

        if (config.applyButtonSelector) {
            $(config.applyButtonSelector).on('click', function () {
                table.ajax.reload();
            });
        }

        if (config.clearButtonSelector) {
            $(config.clearButtonSelector).on('click', function () {
                if (typeof config.clearCallback === 'function') {
                    config.clearCallback();
                }

                table.ajax.reload();
            });
        }
    }

    window.GropAdminTable = {
        buildLanguage: buildLanguage,
        bindFilterButtons: bindFilterButtons,
        createDataTable: createDataTable,
        formatString: formatString,
        localizeByMap: localizeByMap
    };
})(window, window.jQuery);