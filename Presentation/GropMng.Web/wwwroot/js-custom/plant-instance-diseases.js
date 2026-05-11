/**
 * plant-instance-diseases.js
 * Manages the Disease Records tab on the PlantInstance Edit page.
 */

var GropPlantDiseases = (function () {
    'use strict';

    var _cfg = null;
    var _tabLoaded = false;
    var _modalEl = null;
    var _modal = null;

    function getAntiForgeryToken() {
        var input = document.querySelector('input[name="__RequestVerificationToken"]');
        return input ? input.value : '';
    }

    function buildUrlWithId(template, id) {
        return template.replace(/\/0(\/|$)/, '/' + id + '$1');
    }

    function buildUrlWithTwoIds(template, firstId, secondId) {
        var url = template.replace(/\/0(\/|$)/, '/' + firstId + '$1');
        return url.replace(/\/0(\/|$)/, '/' + secondId + '$1');
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
        } else {
            if (isError) console.error(message);
            else console.info(message);
        }
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
            if (typeof window.gropInitFlatpickrInputs === 'function') {
                window.gropInitFlatpickrInputs(_cfg.tabPane);
            }
            if (typeof window.gropInitQuillEditors === 'function') {
                window.gropInitQuillEditors(_cfg.tabPane);
            }
            bindTabEvents();
            bindUploadWidgets();
        })
        .catch(function () {
            if (placeholder) placeholder.innerHTML = '<span class="text-danger small">Error loading disease data.</span>';
        });
    }

    function bindTabEvents() {
        var pane = _cfg.tabPane;

        var addBtn = pane.querySelector('[id^="disease-record-add-btn-"]');
        if (addBtn) {
            addBtn.addEventListener('click', function () { openModal(0, null); });
        }

        pane.addEventListener('click', function (e) {
            var editBtn = e.target.closest('.disease-record-edit-btn');
            if (editBtn) {
                openModal(parseInt(editBtn.dataset.recordId, 10), editBtn.dataset);
                return;
            }

            var deleteBtn = e.target.closest('.disease-record-delete-btn');
            if (deleteBtn) {
                confirmDelete(parseInt(deleteBtn.dataset.recordId, 10));
                return;
            }

            var resolveBtn = e.target.closest('.disease-record-resolve-btn');
            if (resolveBtn) {
                quickResolve(parseInt(resolveBtn.dataset.recordId, 10));
                return;
            }

            var addPhotoBtn = e.target.closest('.disease-photo-add-btn');
            if (addPhotoBtn) {
                addPhoto(parseInt(addPhotoBtn.dataset.recordId, 10));
                return;
            }

            var deletePhotoBtn = e.target.closest('.disease-photo-delete-btn');
            if (deletePhotoBtn) {
                deletePhoto(parseInt(deletePhotoBtn.dataset.recordId, 10), parseInt(deletePhotoBtn.dataset.photoId, 10));
            }
        });
    }

    function bindUploadWidgets() {
        var wrappers = _cfg.tabPane.querySelectorAll('[id^="disease-photo-upload-"]');
        wrappers.forEach(function (wrapper) {
            var uploadId = wrapper.id;
            var fileInput = document.getElementById(uploadId + '-file');
            var preview = document.getElementById(uploadId + '-preview');
            var placeholder = document.getElementById(uploadId + '-placeholder');
            var hidden = document.getElementById(uploadId + '-hidden');
            var statusEl = document.getElementById(uploadId + '-status');

            if (!fileInput || !hidden) return;

            fileInput.addEventListener('change', function () {
                var file = this.files[0];
                if (!file) return;

                statusEl.textContent = _cfg.msgUploading;
                fileInput.disabled = true;

                var formData = new FormData();
                formData.append('file', file);
                formData.append('qqfilename', file.name);
                formData.append('entityType', 'plantinstance');

                fetch(_cfg.uploadUrl, {
                    method: 'POST',
                    body: formData,
                    headers: { 'X-Requested-With': 'XMLHttpRequest' }
                })
                .then(function (r) { return r.json(); })
                .then(function (json) {
                    if (json.success) {
                        hidden.value = json.pictureId;
                        preview.src = json.imageUrl;
                        preview.classList.remove('d-none');
                        if (placeholder) placeholder.classList.add('d-none');
                        statusEl.textContent = '';
                    } else {
                        statusEl.textContent = json.message || _cfg.msgUploadError;
                    }
                })
                .catch(function () {
                    statusEl.textContent = _cfg.msgUploadError;
                })
                .finally(function () {
                    fileInput.disabled = false;
                    fileInput.value = '';
                });
            });
        });
    }

    function openModal(recordId, data) {
        _modalEl = document.getElementById(_cfg.modalId);
        if (!_modalEl) return;

        _modal = bootstrap.Modal.getOrCreateInstance(_modalEl);

        _modalEl.querySelector('#' + _cfg.modalId + '-error').classList.add('d-none');
        _modalEl.querySelector('#' + _cfg.modalId + '-record-id').value = recordId;

        var titleEl = _modalEl.querySelector('#' + _cfg.modalId + '-title');
        if (titleEl) {
            var addTitle = titleEl.getAttribute('data-title-add') || _cfg.titleAdd;
            var editTitle = titleEl.getAttribute('data-title-edit') || _cfg.titleEdit;
            titleEl.textContent = recordId === 0 ? addTitle : editTitle;
        }

        _modalEl.querySelector('#' + _cfg.modalId + '-diseaseid').value = (data && data.diseaseid) || '';

        var detectedInput = _modalEl.querySelector('#' + _cfg.modalId + '-detected');
        var detectedValue = (data && data.detected) || todayIso();
        detectedInput.value = detectedValue;
        if (detectedInput._flatpickr) {
            detectedInput._flatpickr.setDate(detectedValue, true, 'Y-m-d');
        }

        var resolvedInput = _modalEl.querySelector('#' + _cfg.modalId + '-resolved');
        var resolvedValue = (data && data.resolved) || '';
        resolvedInput.value = resolvedValue;
        if (resolvedInput._flatpickr) {
            resolvedInput._flatpickr.setDate(resolvedValue || null, true, 'Y-m-d');
        }

        _modalEl.querySelector('#' + _cfg.modalId + '-severity').value = (data && data.severity) || 'Moderate';
        _modalEl.querySelector('#' + _cfg.modalId + '-outcome').value = (data && data.outcome) || 'Ongoing';
        _modalEl.querySelector('#' + _cfg.modalId + '-treatment').value = (data && data.treatment) || '';

        var notesInput = _modalEl.querySelector('#' + _cfg.modalId + '-notes');
        var notesValue = (data && data.notes) || '';
        notesInput.value = notesValue;
        if (notesInput._gropQuill) {
            notesInput._gropQuill.root.innerHTML = notesValue;
        }

        _modal.show();
    }

    function saveModal() {
        var recordId = parseInt(_modalEl.querySelector('#' + _cfg.modalId + '-record-id').value, 10);
        var diseaseId = _modalEl.querySelector('#' + _cfg.modalId + '-diseaseid').value;
        var detected = _modalEl.querySelector('#' + _cfg.modalId + '-detected').value;
        var resolved = _modalEl.querySelector('#' + _cfg.modalId + '-resolved').value;
        var severity = _modalEl.querySelector('#' + _cfg.modalId + '-severity').value;
        var outcome = _modalEl.querySelector('#' + _cfg.modalId + '-outcome').value;
        var treatment = _modalEl.querySelector('#' + _cfg.modalId + '-treatment').value;
        var notes = _modalEl.querySelector('#' + _cfg.modalId + '-notes').value;
        var errorEl = _modalEl.querySelector('#' + _cfg.modalId + '-error');

        if (!diseaseId || !detected) {
            errorEl.textContent = _cfg.validationRequired;
            errorEl.classList.remove('d-none');
            return;
        }

        var url = recordId === 0
            ? _cfg.createUrl
            : buildUrlWithId(_cfg.updateUrlTemplate, recordId);

        var formData = new FormData();
        formData.append('__RequestVerificationToken', getAntiForgeryToken());
        formData.append('DiseaseId', diseaseId);
        formData.append('DetectedDate', detected);
        if (resolved) formData.append('ResolvedDate', resolved);
        if (severity) formData.append('Severity', severity);
        if (outcome) formData.append('Outcome', outcome);
        if (treatment) formData.append('TreatmentUsed', treatment);
        if (notes) formData.append('Notes', notes);

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

    function confirmDelete(recordId) {
        if (typeof Swal !== 'undefined') {
            Swal.fire({
                title: _cfg.deleteConfirm,
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#d33',
                confirmButtonText: 'OK'
            }).then(function (r) {
                if (r.isConfirmed) deleteRecord(recordId);
            });
        } else if (window.confirm(_cfg.deleteConfirm)) {
            deleteRecord(recordId);
        }
    }

    function deleteRecord(recordId) {
        var url = buildUrlWithId(_cfg.deleteUrlTemplate, recordId);
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

    function quickResolve(recordId) {
        var url = buildUrlWithId(_cfg.quickResolveUrlTemplate, recordId);
        var formData = new FormData();
        formData.append('__RequestVerificationToken', getAntiForgeryToken());

        fetch(url, { method: 'POST', body: formData })
        .then(function (r) { return r.json(); })
        .then(function (result) {
            if (result.success) {
                showToast(_cfg.msgResolveSuccess, false);
                reloadTabContent();
            } else {
                showToast(result.message || _cfg.msgSaveError, true);
            }
        })
        .catch(function () {
            showToast(_cfg.msgNetworkError, true);
        });
    }

    function addPhoto(recordId) {
        var uploadId = 'disease-photo-upload-' + recordId;
        var hidden = document.getElementById(uploadId + '-hidden');
        var notes = document.getElementById(uploadId + '-notes');
        var takenDate = document.getElementById(uploadId + '-takendate');
        var displayOrder = document.getElementById(uploadId + '-displayorder');

        if (!hidden || parseInt(hidden.value, 10) <= 0) {
            showToast(_cfg.msgPhotoRequired, true);
            return;
        }

        var formData = new FormData();
        formData.append('__RequestVerificationToken', getAntiForgeryToken());
        formData.append('PictureId', hidden.value);
        formData.append('TakenDate', takenDate ? takenDate.value : todayIso());
        formData.append('DisplayOrder', displayOrder ? displayOrder.value : '0');
        if (notes && notes.value) formData.append('Notes', notes.value);

        var url = buildUrlWithId(_cfg.addPhotoUrlTemplate, recordId);
        fetch(url, { method: 'POST', body: formData })
        .then(function (r) { return r.json(); })
        .then(function (result) {
            if (result.success) {
                showToast(_cfg.msgPhotoAddSuccess, false);
                reloadTabContent();
            } else {
                showToast(result.message || _cfg.msgSaveError, true);
            }
        })
        .catch(function () {
            showToast(_cfg.msgNetworkError, true);
        });
    }

    function deletePhoto(recordId, photoId) {
        if (typeof Swal !== 'undefined') {
            Swal.fire({
                title: _cfg.deletePhotoConfirm,
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#d33',
                confirmButtonText: 'OK'
            }).then(function (r) {
                if (r.isConfirmed) doDeletePhoto(recordId, photoId);
            });
        } else if (window.confirm(_cfg.deletePhotoConfirm)) {
            doDeletePhoto(recordId, photoId);
        }
    }

    function doDeletePhoto(recordId, photoId) {
        var url = buildUrlWithTwoIds(_cfg.deletePhotoUrlTemplate, recordId, photoId);
        var formData = new FormData();
        formData.append('__RequestVerificationToken', getAntiForgeryToken());

        fetch(url, { method: 'POST', body: formData })
        .then(function (r) { return r.json(); })
        .then(function (result) {
            if (result.success) {
                showToast(_cfg.msgPhotoDeleteSuccess, false);
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

        if (_cfg.activeTab === 'diseases') {
            loadTabContent();
        }
    }

    return { init: init };
}());
