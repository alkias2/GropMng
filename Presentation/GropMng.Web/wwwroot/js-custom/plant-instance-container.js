/**
 * plant-instance-container.js
 * Manages container info and repotting logs in PlantInstance Edit page.
 */

var GropPlantContainer = (function () {
    'use strict';

    var _cfg = null;
    var _tabLoaded = false;

    function getAntiForgeryToken() {
        var input = document.querySelector('input[name="__RequestVerificationToken"]');
        return input ? input.value : '';
    }

    function buildUrlWithId(template, id) {
        return template.replace(/\/0(\/|$)/, '/' + id + '$1');
    }

    function normalizeDecimal(value) {
        if (value == null) return '';
        var text = String(value).trim();
        if (!text) return '';
        return text.replace(',', '.');
    }

    function todayIso() {
        var now = new Date();
        var month = String(now.getMonth() + 1).padStart(2, '0');
        var day = String(now.getDate()).padStart(2, '0');
        return now.getFullYear() + '-' + month + '-' + day;
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
            return;
        }

        if (isError) console.error(message);
        else console.info(message);
    }

    function drawPotDiagrams(root) {
        var canvases = (root || document).querySelectorAll('.grop-pot-diagram__canvas');
        canvases.forEach(function (canvas) {
            drawPotDiagram(canvas);
        });
    }

    function drawPotDiagram(canvas) {
        if (!canvas) return;

        var dBase = parseFloat(canvas.dataset.baseDiameterCm || '0');
        var dTop = parseFloat(canvas.dataset.rimDiameterCm || '0');
        var h = parseFloat(canvas.dataset.heightCm || '0');

        if (!(dBase > 0) || !(dTop > 0) || !(h > 0)) return;

        var W = canvas.width;
        var H = canvas.height;
        var ctx = canvas.getContext('2d');
        if (!ctx) return;

        var BOX = 80;
        var PAD_X = 58;
        var PAD_Y = 34;
        var boxPx = Math.min(W - PAD_X * 2, H - PAD_Y * 2);
        var originX = PAD_X;
        var originY = PAD_Y;
        var cmToPx = boxPx / BOX;

        var coneStroke = '#dbe7ff';
        var dimColor = '#76d9b6';
        var muted = '#9fb0cf';
        var warning = '#f4b4b4';

        ctx.clearRect(0, 0, W, H);

        ctx.strokeStyle = muted;
        ctx.setLineDash([4, 4]);
        ctx.lineWidth = 0.8;
        ctx.strokeRect(originX, originY, boxPx, boxPx);
        ctx.setLineDash([]);

        ctx.fillStyle = '#b8c8e8';
        ctx.font = '600 12px sans-serif';
        ctx.textAlign = 'left';
        ctx.fillText('80 cm', originX + 2, originY - 8);
        ctx.save();
        ctx.translate(originX - 14, originY + boxPx - 2);
        ctx.rotate(-Math.PI / 2);
        ctx.fillText('80 cm', 0, 0);
        ctx.restore();

        var centerX = originX + boxPx / 2;
        var coneBottom = originY + boxPx - 10;
        var coneTop = coneBottom - h * cmToPx;
        var halfBase = (dBase / 2) * cmToPx;
        var halfTop = (dTop / 2) * cmToPx;

        var maxHalf = boxPx / 2 - 2;
        if (halfBase > maxHalf || halfTop > maxHalf || coneTop < originY) {
            ctx.fillStyle = warning;
            ctx.font = '600 12px sans-serif';
            ctx.textAlign = 'center';
            ctx.fillText('Οι διαστάσεις δεν χωρούν στο πλαίσιο 80 cm', W / 2, H / 2);
            return;
        }

        ctx.strokeStyle = coneStroke;
        ctx.lineWidth = 1.5;
        ctx.beginPath();
        ctx.moveTo(centerX - halfBase, coneBottom);
        ctx.lineTo(centerX + halfBase, coneBottom);
        ctx.lineTo(centerX + halfTop, coneTop);
        ctx.lineTo(centerX - halfTop, coneTop);
        ctx.closePath();
        ctx.stroke();

        ctx.beginPath();
        ctx.ellipse(centerX, coneTop, halfTop, Math.max(4, halfTop * 0.22), 0, 0, Math.PI * 2);
        ctx.stroke();

        ctx.beginPath();
        ctx.ellipse(centerX, coneBottom, halfBase, Math.max(4, halfBase * 0.22), 0, 0, Math.PI);
        ctx.stroke();

        ctx.setLineDash([4, 3]);
        ctx.strokeStyle = muted;
        ctx.lineWidth = 1;
        ctx.beginPath();
        ctx.ellipse(centerX, coneBottom, halfBase, Math.max(4, halfBase * 0.22), 0, Math.PI, Math.PI * 2);
        ctx.stroke();
        ctx.setLineDash([]);

        var topDimY = coneTop - 24;
        ctx.strokeStyle = dimColor;
        ctx.lineWidth = 1.2;
        ctx.beginPath();
        ctx.moveTo(centerX - halfTop, topDimY);
        ctx.lineTo(centerX + halfTop, topDimY);
        ctx.stroke();
        ctx.beginPath();
        ctx.moveTo(centerX - halfTop, coneTop - 2);
        ctx.lineTo(centerX - halfTop, topDimY - 5);
        ctx.stroke();
        ctx.beginPath();
        ctx.moveTo(centerX + halfTop, coneTop - 2);
        ctx.lineTo(centerX + halfTop, topDimY - 5);
        ctx.stroke();
        ctx.fillStyle = dimColor;
        ctx.font = '600 12px sans-serif';
        ctx.textAlign = 'center';
        ctx.fillText('Ø κορυφής = ' + dTop.toFixed(2).replace(/\.00$/, '') + ' cm', centerX, topDimY - 8);

        var baseDimY = coneBottom + 24;
        ctx.beginPath();
        ctx.moveTo(centerX - halfBase, baseDimY);
        ctx.lineTo(centerX + halfBase, baseDimY);
        ctx.stroke();
        ctx.beginPath();
        ctx.moveTo(centerX - halfBase, coneBottom + 2);
        ctx.lineTo(centerX - halfBase, baseDimY + 5);
        ctx.stroke();
        ctx.beginPath();
        ctx.moveTo(centerX + halfBase, coneBottom + 2);
        ctx.lineTo(centerX + halfBase, baseDimY + 5);
        ctx.stroke();
        ctx.fillText('Ø βάσης = ' + dBase.toFixed(2).replace(/\.00$/, '') + ' cm', centerX, baseDimY + 15);

        var dimHX = centerX + Math.max(halfBase, halfTop) + 28;
        ctx.beginPath();
        ctx.moveTo(dimHX, coneTop);
        ctx.lineTo(dimHX, coneBottom);
        ctx.stroke();
        ctx.beginPath();
        ctx.moveTo(dimHX - 6, coneTop);
        ctx.lineTo(dimHX + 6, coneTop);
        ctx.stroke();
        ctx.beginPath();
        ctx.moveTo(dimHX - 6, coneBottom);
        ctx.lineTo(dimHX + 6, coneBottom);
        ctx.stroke();
        ctx.save();
        ctx.translate(dimHX + 18, (coneTop + coneBottom) / 2);
        ctx.rotate(-Math.PI / 2);
        ctx.textAlign = 'center';
        ctx.fillText('h = ' + h.toFixed(2).replace(/\.00$/, '') + ' cm', 0, 0);
        ctx.restore();
    }

    function loadTabContent() {
        if (_tabLoaded) return;

        var placeholder = _cfg.tabPane.querySelector('.tab-lazy-placeholder');
        var spinner = placeholder ? placeholder.querySelector('.spinner-border') : null;
        if (spinner) spinner.classList.remove('d-none');

        fetch(_cfg.tabUrl, { headers: { 'X-Requested-With': 'XMLHttpRequest' } })
            .then(function (r) { return r.text(); })
            .then(function (html) {
                _cfg.tabPane.innerHTML = html;
                _tabLoaded = true;
                bindTabEvents();
                drawPotDiagrams(_cfg.tabPane);
            })
            .catch(function () {
                if (placeholder) {
                    var loadErrorText = _cfg.msgLoadError || 'Error loading container data.';
                    placeholder.innerHTML = '<span class="text-danger small">' + loadErrorText + '</span>';
                }
            });
    }

    function bindTabEvents() {
        var pane = _cfg.tabPane;

        var addBtn = pane.querySelector('[id^="repotting-add-btn-"]');
        if (addBtn) {
            addBtn.addEventListener('click', function () { openRepottingModal(0, null); });
        }

        var quickContainerBtn = pane.querySelector('[id^="repotting-quick-container-btn-"]');
        if (quickContainerBtn) {
            quickContainerBtn.addEventListener('click', function () {
                openQuickContainerModal();
            });
        }

        pane.addEventListener('click', function (e) {
            var editBtn = e.target.closest('.repotting-edit-btn');
            if (editBtn) {
                openRepottingModal(parseInt(editBtn.dataset.logId, 10), editBtn.dataset);
                return;
            }

            var deleteBtn = e.target.closest('.repotting-delete-btn');
            if (deleteBtn) {
                confirmDelete(parseInt(deleteBtn.dataset.logId, 10));
            }
        });

        document.addEventListener('click', function (e) {
            if (e.target && e.target.id === _cfg.repottingModalId + '-save-btn') {
                saveRepotting();
                return;
            }

            if (e.target && e.target.id === _cfg.quickContainerModalId + '-save-btn') {
                saveQuickContainer();
            }
        });
    }

    function openRepottingModal(logId, data) {
        var modalEl = document.getElementById(_cfg.repottingModalId);
        if (!modalEl) return;

        var modal = bootstrap.Modal.getOrCreateInstance(modalEl);
        modalEl.querySelector('#' + _cfg.repottingModalId + '-error').classList.add('d-none');
        modalEl.querySelector('#' + _cfg.repottingModalId + '-log-id').value = logId;

        var titleEl = modalEl.querySelector('#' + _cfg.repottingModalId + '-title');
        if (titleEl) {
            var addTitle = titleEl.getAttribute('data-title-add') || _cfg.titleAdd;
            var editTitle = titleEl.getAttribute('data-title-edit') || _cfg.titleEdit;
            titleEl.textContent = logId === 0 ? addTitle : editTitle;
        }

        modalEl.querySelector('#' + _cfg.repottingModalId + '-containerid').value = data && data.containerid ? data.containerid : '';
        modalEl.querySelector('#' + _cfg.repottingModalId + '-soilmixid').value = data && data.soilmixid ? data.soilmixid : '';
        modalEl.querySelector('#' + _cfg.repottingModalId + '-repottedon').value = data && data.repottedon ? data.repottedon : todayIso();
        modalEl.querySelector('#' + _cfg.repottingModalId + '-notes').value = data && data.notes ? data.notes : '';

        modal.show();
    }

    function saveRepotting() {
        var modalEl = document.getElementById(_cfg.repottingModalId);
        if (!modalEl) return;

        var logId = parseInt(modalEl.querySelector('#' + _cfg.repottingModalId + '-log-id').value, 10);
        var containerId = modalEl.querySelector('#' + _cfg.repottingModalId + '-containerid').value;
        var soilMixId = modalEl.querySelector('#' + _cfg.repottingModalId + '-soilmixid').value;
        var repottedOn = modalEl.querySelector('#' + _cfg.repottingModalId + '-repottedon').value;
        var notes = modalEl.querySelector('#' + _cfg.repottingModalId + '-notes').value;
        var errorEl = modalEl.querySelector('#' + _cfg.repottingModalId + '-error');

        if (!repottedOn) {
            errorEl.textContent = _cfg.validationDateRequired;
            errorEl.classList.remove('d-none');
            return;
        }

        var formData = new FormData();
        formData.append('__RequestVerificationToken', getAntiForgeryToken());
        if (containerId) formData.append('NewContainerId', containerId);
        if (soilMixId) formData.append('NewSoilMixId', soilMixId);
        formData.append('RepottedOn', repottedOn);
        if (notes) formData.append('Notes', notes);

        var url = logId === 0 ? _cfg.createUrl : buildUrlWithId(_cfg.updateUrlTemplate, logId);
        fetch(url, { method: 'POST', body: formData })
            .then(function (r) { return r.json(); })
            .then(function (result) {
                if (result.success) {
                    bootstrap.Modal.getOrCreateInstance(modalEl).hide();
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

    function confirmDelete(logId) {
        if (typeof Swal !== 'undefined') {
            Swal.fire({
                title: _cfg.deleteConfirm,
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#d33',
                confirmButtonText: _cfg.deleteConfirmButtonText || 'OK'
            }).then(function (r) {
                if (r.isConfirmed) deleteLog(logId);
            });
        } else if (window.confirm(_cfg.deleteConfirm)) {
            deleteLog(logId);
        }
    }

    function deleteLog(logId) {
        var url = buildUrlWithId(_cfg.deleteUrlTemplate, logId);
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

    function openQuickContainerModal() {
        var modalEl = document.getElementById(_cfg.quickContainerModalId);
        if (!modalEl) return;

        modalEl.querySelector('#' + _cfg.quickContainerModalId + '-error').classList.add('d-none');
        bootstrap.Modal.getOrCreateInstance(modalEl).show();
    }

    function appendDecimalIfPresent(formData, key, value) {
        var normalized = normalizeDecimal(value);
        if (normalized) formData.append(key, normalized);
    }

    function saveQuickContainer() {
        var modalEl = document.getElementById(_cfg.quickContainerModalId);
        if (!modalEl) return;

        var errorEl = modalEl.querySelector('#' + _cfg.quickContainerModalId + '-error');
        errorEl.classList.add('d-none');

        var formData = new FormData();
        formData.append('__RequestVerificationToken', getAntiForgeryToken());
        formData.append('ContainerType', modalEl.querySelector('#' + _cfg.quickContainerModalId + '-type').value);

        var material = modalEl.querySelector('#' + _cfg.quickContainerModalId + '-material').value;
        var color = modalEl.querySelector('#' + _cfg.quickContainerModalId + '-color').value;
        var notes = modalEl.querySelector('#' + _cfg.quickContainerModalId + '-notes').value;
        var hasDrainageHole = modalEl.querySelector('#' + _cfg.quickContainerModalId + '-drainage').checked;

        if (material) formData.append('Material', material);
        if (color) formData.append('Color', color);
        if (notes) formData.append('Notes', notes);
        formData.append('HasDrainageHole', hasDrainageHole ? 'true' : 'false');

        appendDecimalIfPresent(formData, 'BaseCircumferenceCm', modalEl.querySelector('#' + _cfg.quickContainerModalId + '-base').value);
        appendDecimalIfPresent(formData, 'RimCircumferenceCm', modalEl.querySelector('#' + _cfg.quickContainerModalId + '-rim').value);
        appendDecimalIfPresent(formData, 'HeightCm', modalEl.querySelector('#' + _cfg.quickContainerModalId + '-height').value);
        appendDecimalIfPresent(formData, 'LengthCm', modalEl.querySelector('#' + _cfg.quickContainerModalId + '-length').value);
        appendDecimalIfPresent(formData, 'WidthCm', modalEl.querySelector('#' + _cfg.quickContainerModalId + '-width').value);
        appendDecimalIfPresent(formData, 'VolumeL', modalEl.querySelector('#' + _cfg.quickContainerModalId + '-volume').value);

        fetch(_cfg.quickCreateContainerUrl, { method: 'POST', body: formData })
            .then(function (r) { return r.json(); })
            .then(function (result) {
                if (!result.success) {
                    errorEl.textContent = result.message || _cfg.msgContainerCreateError;
                    errorEl.classList.remove('d-none');
                    return;
                }

                var select = document.getElementById(_cfg.repottingModalId + '-containerid');
                if (select && result.data && result.data.id) {
                    var option = document.createElement('option');
                    option.value = String(result.data.id);
                    option.text = result.data.text || String(result.data.id);
                    select.appendChild(option);
                    select.value = option.value;
                }

                bootstrap.Modal.getOrCreateInstance(modalEl).hide();
                showToast(_cfg.msgContainerCreateSuccess, false);
                reloadTabContent();
            })
            .catch(function () {
                errorEl.textContent = _cfg.msgNetworkError;
                errorEl.classList.remove('d-none');
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

        if (_cfg.tabBtn) {
            _cfg.tabBtn.addEventListener('shown.bs.tab', function () {
                loadTabContent();
            });
        }

        if (_cfg.activeTab === 'container') {
            loadTabContent();
        }
    }

    return { init: init };
}());
