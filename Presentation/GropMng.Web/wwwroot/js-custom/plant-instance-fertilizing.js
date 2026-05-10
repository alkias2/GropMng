/**
 * plant-instance-fertilizing.js
 * Manages the Fertilizing Schedule tab on the PlantInstance Edit page.
 */

var GropPlantFertilizing = (function () {
    'use strict';

    var _cfg = null;
    var _tabLoaded = false;
    var _modalEl = null;
    var _modal = null;

    function getAntiForgeryToken() {
        var input = document.querySelector('input[name="__RequestVerificationToken"]');
        return input ? input.value : '';
    }

    function buildUrlWithScheduleId(template, scheduleId) {
        return template.replace(/\/0(\/|$)/, '/' + scheduleId + '$1');
    }

    function showToast(message, isError) {
        if (typeof Swal !== 'undefined') {
            Swal.fire({
                toast: true,
                position: 'top-end',
                icon: isError ? 'error' : 'success',
                title: message,
                showConfirmButton: false,
                timer: 3000,
                timerProgressBar: true
            });
        } else {
            if (isError) console.error(message);
            else console.info(message);
        }
    }

    function decodeHtmlEntities(value) {
        if (!value) return '';

        var decoded = value;
        for (var i = 0; i < 2; i++) {
            if (decoded.indexOf('&') === -1) break;
            var el = document.createElement('textarea');
            el.innerHTML = decoded;
            decoded = el.value;
        }

        return decoded;
    }

    function normalizeDecimal(value) {
        if (value === null || value === undefined) return '';

        var normalized = String(value).trim().replace(/\s+/g, '');
        if (!normalized) return '';

        var lastComma = normalized.lastIndexOf(',');
        var lastDot = normalized.lastIndexOf('.');

        if (lastComma >= 0 && lastDot >= 0) {
            if (lastComma > lastDot) {
                normalized = normalized.replace(/\./g, '').replace(',', '.');
            } else {
                normalized = normalized.replace(/,/g, '');
            }
        } else {
            normalized = normalized.replace(',', '.');
        }

        if (normalized.charAt(0) === '.') {
            normalized = '0' + normalized;
        } else if (normalized.indexOf('-.') === 0) {
            normalized = normalized.replace('-.', '-0.');
        }

        if (!/^-?\d+(\.\d+)?$/.test(normalized)) {
            return null;
        }

        return normalized;
    }

    function loadTabContent() {
        if (_tabLoaded) return;

        var placeholder = _cfg.tabPane.querySelector('.tab-lazy-placeholder');
        var spinner = placeholder ? placeholder.querySelector('.spinner-border') : null;
        if (spinner) spinner.classList.remove('d-none');

        fetch(_cfg.tabUrl, {
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        })
        .then(function (r) { return r.text(); })
        .then(function (html) {
            _cfg.tabPane.innerHTML = html;
            _tabLoaded = true;
            bindTabEvents();
        })
        .catch(function () {
            if (placeholder) placeholder.innerHTML = '<span class="text-danger small">Error loading fertilizing data.</span>';
        });
    }

    function bindTabEvents() {
        var pane = _cfg.tabPane;

        var addBtn = pane.querySelector('[id^="fertilizing-add-btn-"]');
        if (addBtn) {
            addBtn.addEventListener('click', function () { openModal(0, null); });
        }

        pane.addEventListener('click', function (e) {
            var editBtn = e.target.closest('.fertilizing-edit-btn');
            if (editBtn) {
                openModal(parseInt(editBtn.dataset.scheduleId, 10), editBtn.dataset);
                return;
            }

            var deleteBtn = e.target.closest('.fertilizing-delete-btn');
            if (deleteBtn) {
                confirmDelete(parseInt(deleteBtn.dataset.scheduleId, 10));
                return;
            }

            var deleteLogBtn = e.target.closest('.fertilizing-log-delete-btn');
            if (deleteLogBtn) {
                confirmDeleteLog(parseInt(deleteLogBtn.dataset.logId, 10));
            }
        });
    }

    function openModal(scheduleId, data) {
        _modalEl = document.getElementById(_cfg.modalId);
        if (!_modalEl) return;

        _modal = bootstrap.Modal.getOrCreateInstance(_modalEl);

        _modalEl.querySelector('#' + _cfg.modalId + '-error').classList.add('d-none');
        _modalEl.querySelector('#' + _cfg.modalId + '-schedule-id').value = scheduleId;

        var titleEl = _modalEl.querySelector('#' + _cfg.modalId + '-title');
        if (titleEl) {
            var title = scheduleId === 0 ? _cfg.titleAdd : _cfg.titleEdit;
            titleEl.textContent = decodeHtmlEntities(title);
        }

        _modalEl.querySelector('#' + _cfg.modalId + '-fertilizerid').value = (data && data.fertilizerid) || '';
        _modalEl.querySelector('#' + _cfg.modalId + '-season').value = (data && data.season) || 'Spring';
        _modalEl.querySelector('#' + _cfg.modalId + '-frequency').value = (data && data.frequency) || '14';
        _modalEl.querySelector('#' + _cfg.modalId + '-quantity').value = normalizeDecimal((data && data.quantity) || '');
        _modalEl.querySelector('#' + _cfg.modalId + '-unit').value = (data && data.unit) || '';
        _modalEl.querySelector('#' + _cfg.modalId + '-notes').value = (data && data.notes) || '';
        _modalEl.querySelector('#' + _cfg.modalId + '-dilution').value = (data && data.dilution) || '';

        _modal.show();
    }

    function saveModal() {
        var scheduleId = parseInt(_modalEl.querySelector('#' + _cfg.modalId + '-schedule-id').value, 10);
        var fertilizerId = _modalEl.querySelector('#' + _cfg.modalId + '-fertilizerid').value;
        var season = _modalEl.querySelector('#' + _cfg.modalId + '-season').value;
        var frequency = _modalEl.querySelector('#' + _cfg.modalId + '-frequency').value;
        var quantityInput = _modalEl.querySelector('#' + _cfg.modalId + '-quantity').value;
        var quantity = normalizeDecimal(quantityInput);
        var unit = _modalEl.querySelector('#' + _cfg.modalId + '-unit').value;
        var notes = _modalEl.querySelector('#' + _cfg.modalId + '-notes').value;
        var dilution = _modalEl.querySelector('#' + _cfg.modalId + '-dilution').value;
        var errorEl = _modalEl.querySelector('#' + _cfg.modalId + '-error');

        if (!fertilizerId || !season || !frequency || parseInt(frequency, 10) < 1) {
            errorEl.textContent = _cfg.validationRequired;
            errorEl.classList.remove('d-none');
            return;
        }

        if (quantityInput && quantity === null) {
            errorEl.textContent = _cfg.validationRequired;
            errorEl.classList.remove('d-none');
            return;
        }

        var url = scheduleId === 0
            ? _cfg.createUrl
            : buildUrlWithScheduleId(_cfg.updateUrlTemplate, scheduleId);

        var formData = new FormData();
        formData.append('__RequestVerificationToken', getAntiForgeryToken());
        formData.append('FertilizerId', fertilizerId);
        formData.append('Season', season);
        formData.append('FrequencyDays', frequency);
        if (quantity) formData.append('Quantity', quantity);
        if (unit) formData.append('Unit', unit);
        if (notes) formData.append('Notes', notes);
        if (dilution) formData.append('DilutionInstructions', dilution);

        fetch(url, { method: 'POST', body: formData })
        .then(function (r) { return r.json(); })
        .then(function (result) {
            if (result.success) {
                _modal.hide();
                showToast(_cfg.msgSaveSuccess, false);
                reloadTabContent();
            } else {
                errorEl.textContent = result.message || _cfg.msgSaveError;
                errorEl.classList.remove('d-none');
            }
        })
        .catch(function () {
            errorEl.textContent = _cfg.msgNetworkError;
            errorEl.classList.remove('d-none');
        });
    }

    function confirmDelete(scheduleId) {
        if (typeof Swal !== 'undefined') {
            Swal.fire({
                title: _cfg.deleteConfirm,
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#d33',
                confirmButtonText: 'OK'
            }).then(function (r) {
                if (r.isConfirmed) deleteSchedule(scheduleId);
            });
        } else if (window.confirm(_cfg.deleteConfirm)) {
            deleteSchedule(scheduleId);
        }
    }

    function deleteSchedule(scheduleId) {
        var url = buildUrlWithScheduleId(_cfg.deleteUrlTemplate, scheduleId);

        var formData = new FormData();
        formData.append('__RequestVerificationToken', getAntiForgeryToken());

        fetch(url, { method: 'POST', body: formData })
        .then(function (r) { return r.json(); })
        .then(function (result) {
            if (result.success) {
                showToast(_cfg.msgDeleteSuccess, false);
                reloadTabContent();
            } else {
                showToast(result.message || _cfg.msgDeleteError, true);
            }
        })
        .catch(function () {
            showToast(_cfg.msgNetworkError, true);
        });
    }

    function confirmDeleteLog(logId) {
        if (typeof Swal !== 'undefined') {
            Swal.fire({
                title: _cfg.deleteLogConfirm,
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#d33',
                confirmButtonText: 'OK'
            }).then(function (r) {
                if (r.isConfirmed) deleteLog(logId);
            });
        } else if (window.confirm(_cfg.deleteLogConfirm)) {
            deleteLog(logId);
        }
    }

    function deleteLog(logId) {
        var url = buildUrlWithScheduleId(_cfg.deleteLogUrlTemplate, logId);

        var formData = new FormData();
        formData.append('__RequestVerificationToken', getAntiForgeryToken());

        fetch(url, { method: 'POST', body: formData })
        .then(function (r) { return r.json(); })
        .then(function (result) {
            if (result.success) {
                showToast(_cfg.msgDeleteLogSuccess, false);
                reloadTabContent();
            } else {
                showToast(result.message || _cfg.msgDeleteError, true);
            }
        })
        .catch(function () {
            showToast(_cfg.msgNetworkError, true);
        });
    }

    function reloadTabContent() {
        _tabLoaded = false;
        _cfg.tabPane.innerHTML =
            '<div class="tab-lazy-placeholder text-center py-4">' +
            '<span class="spinner-border spinner-border-sm text-primary" role="status"></span>' +
            '</div>';
        loadTabContent();
    }

    function init(cfg) {
        _cfg = cfg;

        document.addEventListener('click', function (e) {
            if (e.target && e.target.id === _cfg.modalId + '-save-btn') {
                saveModal();
            }
        });

        if (_cfg.tabBtn) {
            _cfg.tabBtn.addEventListener('shown.bs.tab', function () {
                loadTabContent();
            });
        }

        if (_cfg.activeTab === 'fertilizing') {
            loadTabContent();
        }
    }

    return { init: init };
}());
