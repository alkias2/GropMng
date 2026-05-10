/**
 * plant-instance-watering.js
 * Manages the Watering Schedule tab on the PlantInstance Edit page.
 * Handles: lazy tab load, add/edit modal, save, delete.
 */

var GropPlantWatering = (function () {
    'use strict';

    var _cfg = null;
    var _tabLoaded = false;
    var _modalEl = null;
    var _modal = null;

    // ── Helpers ────────────────────────────────────────────────────

    function getAntiForgeryToken() {
        var input = document.querySelector('input[name="__RequestVerificationToken"]');
        return input ? input.value : '';
    }

    function buildUrlWithScheduleId(template, scheduleId) {
        // Replace trailing /0 or /0/action with the real id
        return template.replace(/\/0(\/|$)/, '/' + scheduleId + '$1');
    }

    function showToast(message, isError) {
        // Try Swal if available, else fallback to alert
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
            if (!isError) console.info(message);
            else console.error(message);
        }
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

    // ── Tab Content Loading ─────────────────────────────────────────

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
            if (placeholder) placeholder.innerHTML = '<span class="text-danger small">Error loading watering data.</span>';
        });
    }

    // ── Event Binding (after tab content is loaded) ─────────────────

    function bindTabEvents() {
        var pane = _cfg.tabPane;

        // Add button
        var addBtn = pane.querySelector('[id^="watering-add-btn-"]');
        if (addBtn) {
            addBtn.addEventListener('click', function () { openModal(0, null); });
        }

        // Edit buttons (delegated)
        pane.addEventListener('click', function (e) {
            var editBtn = e.target.closest('.watering-edit-btn');
            if (editBtn) {
                openModal(parseInt(editBtn.dataset.scheduleId, 10), editBtn.dataset);
                return;
            }

            var deleteBtn = e.target.closest('.watering-delete-btn');
            if (deleteBtn) {
                confirmDelete(parseInt(deleteBtn.dataset.scheduleId, 10));
                return;
            }

            var deleteLogBtn = e.target.closest('.watering-log-delete-btn');
            if (deleteLogBtn) {
                confirmDeleteLog(parseInt(deleteLogBtn.dataset.logId, 10));
            }
        });
    }

    // ── Modal ───────────────────────────────────────────────────────

    function openModal(scheduleId, data) {
        var modalId = _cfg.modalId;
        _modalEl = document.getElementById(modalId);
        if (!_modalEl) return;

        _modal = bootstrap.Modal.getOrCreateInstance(_modalEl);

        // Reset
        _modalEl.querySelector('#' + modalId + '-error').classList.add('d-none');
        _modalEl.querySelector('#' + modalId + '-schedule-id').value = scheduleId;

        // Title
        var titleEl = _modalEl.querySelector('#' + modalId + '-title');
        if (titleEl) titleEl.textContent = scheduleId === 0
            ? _modalEl.querySelector('[data-title-add]')?.dataset.titleAdd || ''
            : _modalEl.querySelector('[data-title-edit]')?.dataset.titleEdit || '';

        // Populate fields
        _modalEl.querySelector('#' + modalId + '-season').value    = (data && data.season)    || 'Spring';
        _modalEl.querySelector('#' + modalId + '-frequency').value = (data && data.frequency) || '3';
        _modalEl.querySelector('#' + modalId + '-amount').value    = (data && data.amount)    || '';
        _modalEl.querySelector('#' + modalId + '-timeofday').value = (data && data.timeofday) || '';
        _modalEl.querySelector('#' + modalId + '-notes').value     = (data && data.notes)     || '';

        _modal.show();
    }

    function saveModal() {
        var modalId = _cfg.modalId;
        var scheduleId  = parseInt(_modalEl.querySelector('#' + modalId + '-schedule-id').value, 10);
        var season      = _modalEl.querySelector('#' + modalId + '-season').value;
        var frequency   = _modalEl.querySelector('#' + modalId + '-frequency').value;
        var amountInput = _modalEl.querySelector('#' + modalId + '-amount').value;
        var amount      = normalizeDecimal(amountInput);
        var timeOfDay   = _modalEl.querySelector('#' + modalId + '-timeofday').value;
        var notes       = _modalEl.querySelector('#' + modalId + '-notes').value;
        var errorEl     = _modalEl.querySelector('#' + modalId + '-error');

        if (!season || !frequency || parseInt(frequency, 10) < 1) {
            errorEl.textContent = 'Season and frequency are required.';
            errorEl.classList.remove('d-none');
            return;
        }

        if (amountInput && amount === null) {
            errorEl.textContent = 'Invalid decimal amount.';
            errorEl.classList.remove('d-none');
            return;
        }

        var url = scheduleId === 0
            ? _cfg.createUrl
            : buildUrlWithScheduleId(_cfg.updateUrlTemplate, scheduleId);

        var formData = new FormData();
        formData.append('__RequestVerificationToken', getAntiForgeryToken());
        formData.append('Season', season);
        formData.append('FrequencyDays', frequency);
        if (amount) formData.append('WaterAmountL', amount);
        if (timeOfDay) formData.append('TimeOfDay', timeOfDay);
        if (notes) formData.append('Notes', notes);

        fetch(url, { method: 'POST', body: formData })
        .then(function (r) { return r.json(); })
        .then(function (result) {
            if (result.success) {
                _modal.hide();
                showToast(_cfg.msgSaveSuccess, false);
                reloadTabContent();
            } else {
                errorEl.textContent = result.message || 'Error saving schedule.';
                errorEl.classList.remove('d-none');
            }
        })
        .catch(function () {
            errorEl.textContent = 'Network error.';
            errorEl.classList.remove('d-none');
        });
    }

    // ── Delete ──────────────────────────────────────────────────────

    function confirmDelete(scheduleId) {
        var confirmed = typeof Swal !== 'undefined'
            ? Swal.fire({
                title: _cfg.deleteConfirm,
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#d33',
                confirmButtonText: 'OK'
            }).then(function (r) { if (r.isConfirmed) deleteSchedule(scheduleId); })
            : (window.confirm(_cfg.deleteConfirm) && deleteSchedule(scheduleId));

        return confirmed;
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
                showToast(result.message || 'Error deleting schedule.', true);
            }
        })
        .catch(function () { showToast('Network error.', true); });
    }

    function confirmDeleteLog(logId) {
        var confirmed = typeof Swal !== 'undefined'
            ? Swal.fire({
                title: _cfg.deleteLogConfirm,
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#d33',
                confirmButtonText: 'OK'
            }).then(function (r) { if (r.isConfirmed) deleteLog(logId); })
            : (window.confirm(_cfg.deleteLogConfirm) && deleteLog(logId));

        return confirmed;
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
                showToast(result.message || 'Error deleting watering log.', true);
            }
        })
        .catch(function () { showToast('Network error.', true); });
    }

    // ── Reload Tab ──────────────────────────────────────────────────

    function reloadTabContent() {
        _tabLoaded = false;
        _cfg.tabPane.innerHTML =
            '<div class="tab-lazy-placeholder text-center py-4">' +
            '<span class="spinner-border spinner-border-sm text-primary" role="status"></span>' +
            '</div>';
        loadTabContent();
    }

    // ── Public Init ─────────────────────────────────────────────────

    function init(cfg) {
        _cfg = cfg;

        // Bind save button in modal
        document.addEventListener('click', function (e) {
            if (e.target && e.target.id === _cfg.modalId + '-save-btn') {
                saveModal();
            }
        });

        // Lazy load on tab click
        if (_cfg.tabBtn) {
            _cfg.tabBtn.addEventListener('shown.bs.tab', function () {
                loadTabContent();
            });
        }

        // If watering is the active tab on page load, load immediately
        if (_cfg.activeTab === 'watering') {
            loadTabContent();
        }
    }

    return { init: init };
}());
