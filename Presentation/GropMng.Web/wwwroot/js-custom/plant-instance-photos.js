/**
 * plant-instance-photos.js
 * Handles PlantInstance photo management: DataTable, add, edit (modal), delete, upload widget.
 *
 * Usage (from Edit.cshtml @section Scripts):
 *   GropPlantPhotos.init({ ...options });
 */

var GropPlantPhotos = (function () {
    'use strict';

    /**
     * Initialises the photo management UI for one PlantInstance.
     * @param {object} opts
     * @param {number} opts.plantInstanceId
     * @param {string} opts.tableId           - id of the <table> element
     * @param {string} opts.addFormId         - id of the add-photo <form>
     * @param {string} opts.addUploadId       - id of the mini-upload wrapper div
     * @param {string} opts.editModalId       - id of the edit <div.modal>
     * @param {string} opts.listUrl           - URL for DataTable AJAX source
     * @param {string} opts.addUrl            - URL for photo add POST
    * @param {string} opts.updateUrl        - URL for photo update POST
    * @param {string} opts.deleteUrl        - URL for photo delete POST
     * @param {string} opts.uploadUrl         - URL for async picture upload
     * @param {string} opts.deleteConfirm     - confirmation message for delete
     * @param {string} opts.msgUploading      - status text while uploading
     * @param {string} opts.msgUploadError    - status text on upload failure
     */
    function init(opts) {
        var table = _initDataTable(opts);
        _initUploadWidget(opts);
        _initAddForm(opts, table);
        _initEditModal(opts, table);
    }

    // ── DataTable ─────────────────────────────────────────────────────────

    function _initDataTable(opts) {
        return $('#' + opts.tableId).DataTable({
            processing : true,
            ajax       : { url: opts.listUrl, type: 'GET', dataSrc: 'data' },
            columns    : [
                {
                    data      : 'thumbnailUrl',
                    orderable : false,
                    render    : function (url) {
                        if (!url) {
                            return '<span class="text-muted">—</span>';
                        }
                        return '<img src="' + url + '" style="width:60px;height:60px;object-fit:cover" class="rounded border" />';
                    }
                },
                { data: 'caption', defaultContent: '<span class="text-muted">—</span>' },
                { data: 'takenDate' },
                { data: 'displayOrder', className: 'text-center' },
                {
                    data      : null,
                    orderable : false,
                    className : 'text-end',
                    render    : function (row) {
                        var rowId = row.id || row.Id || 0;
                        var rowCaption = row.caption || row.Caption || '';
                        var rowTakenDate = row.takenDate || row.TakenDate || '';
                        var rowDisplayOrder = row.displayOrder || row.DisplayOrder || 0;

                        return '<button type="button" class="btn btn-sm btn-outline-primary me-1 btn-edit-photo" '
                             +   'data-id="'           + rowId             + '" '
                             +   'data-caption="'      + _esc(rowCaption)   + '" '
                             +   'data-takendate="'    + rowTakenDate      + '" '
                             +   'data-displayorder="' + rowDisplayOrder   + '">'
                             +   '<i class="bx bx-edit-alt"></i>'
                             + '</button>'
                             + '<button type="button" class="btn btn-sm btn-outline-danger btn-delete-photo" '
                             +   'data-id="' + rowId + '">'
                             +   '<i class="bx bx-trash"></i>'
                             + '</button>';
                    }
                }
            ],
            language: {
                emptyTable: opts.emptyText || 'No photos yet.'
            },
            order    : [[3, 'asc']],
            pageLength: 10,
            dom      : '<"row"<"col-sm-6"l><"col-sm-6"f>>t<"row"<"col-sm-6"i><"col-sm-6"p>>'
        });
    }

    // ── Mini upload widget ────────────────────────────────────────────────

    function _initUploadWidget(opts) {
        var uploadId   = opts.addUploadId;
        var fileInput  = document.getElementById(uploadId + '-file');
        var preview    = document.getElementById(uploadId + '-preview');
        var placeholder= document.getElementById(uploadId + '-placeholder');
        var hidden     = document.getElementById(uploadId + '-hidden');
        var statusEl   = document.getElementById(uploadId + '-status');

        if (!fileInput || !hidden) return;

        fileInput.addEventListener('change', async function () {
            var file = this.files[0];
            if (!file) return;

            statusEl.textContent = opts.msgUploading || 'Uploading…';
            fileInput.disabled   = true;

            var formData = new FormData();
            formData.append('file', file);
            formData.append('qqfilename', file.name);
            formData.append('entityType', 'plantinstance');

            try {
                var resp = await fetch(opts.uploadUrl, {
                    method : 'POST',
                    body   : formData,
                    headers: { 'X-Requested-With': 'XMLHttpRequest' }
                });
                var json = await resp.json();

                if (json.success) {
                    hidden.value          = json.pictureId;
                    preview.src           = json.imageUrl;
                    preview.style.display = '';
                    placeholder.style.display = 'none';
                    statusEl.textContent  = '';
                } else {
                    statusEl.textContent = json.message || (opts.msgUploadError || 'Upload failed.');
                }
            } catch (e) {
                statusEl.textContent = opts.msgUploadError || 'Upload failed.';
            } finally {
                fileInput.disabled = false;
                fileInput.value    = '';
            }
        });
    }

    // ── Add form ──────────────────────────────────────────────────────────

    function _initAddForm(opts, table) {
        var form    = document.getElementById(opts.addFormId);
        var errorEl = document.getElementById(opts.addFormId + '-error');
        var uploadId= opts.addUploadId;

        if (!form) return;

        form.addEventListener('submit', async function (e) {
            e.preventDefault();
            _setError(errorEl, null);

            var hidden = document.getElementById(uploadId + '-hidden');
            if (!hidden || parseInt(hidden.value, 10) <= 0) {
                _setError(errorEl, 'Please upload an image first.');
                return;
            }

            var data = new FormData(form);
            var token = _getAntiforgeryToken(form);

            try {
                var resp = await fetch(opts.addUrl, {
                    method : 'POST',
                    body   : data,
                    headers: {
                        'X-Requested-With'       : 'XMLHttpRequest',
                        'RequestVerificationToken': token
                    }
                });
                var json = await resp.json();

                if (json.success) {
                    table.ajax.reload(null, false);
                    _resetAddForm(form, uploadId);
                } else {
                    _setError(errorEl, json.message || 'Error adding photo.');
                }
            } catch (ex) {
                _setError(errorEl, 'Unexpected error. Please try again.');
            }
        });
    }

    function _resetAddForm(form, uploadId) {
        form.reset();

        var hidden     = document.getElementById(uploadId + '-hidden');
        var preview    = document.getElementById(uploadId + '-preview');
        var placeholder= document.getElementById(uploadId + '-placeholder');
        var statusEl   = document.getElementById(uploadId + '-status');

        if (hidden) hidden.value = '0';
        if (preview) { preview.src = ''; preview.style.display = 'none'; }
        if (placeholder) placeholder.style.display = '';
        if (statusEl) statusEl.textContent = '';
    }

    // ── Edit modal ────────────────────────────────────────────────────────

    function _initEditModal(opts, table) {
        var modalEl  = document.getElementById(opts.editModalId);
        var saveBtn  = document.getElementById(opts.editModalId + '-save-btn');
        var photoIdEl= document.getElementById(opts.editModalId + '-photo-id');
        var captionEl= document.getElementById(opts.editModalId + '-caption');
        var dateEl   = document.getElementById(opts.editModalId + '-takendate');
        var orderEl  = document.getElementById(opts.editModalId + '-displayorder');
        var errorEl  = document.getElementById(opts.editModalId + '-error');

        if (!modalEl || !saveBtn) return;

        var bsModal = new bootstrap.Modal(modalEl);

        // Open modal on edit button click (event delegation on table wrapper)
        document.getElementById('plant-photos-section-' + opts.plantInstanceId)
            .addEventListener('click', function (e) {
                var btn = e.target.closest('.btn-edit-photo');
                if (!btn) return;

                photoIdEl.value  = btn.dataset.id;
                captionEl.value  = btn.dataset.caption !== 'null' ? btn.dataset.caption : '';
                dateEl.value     = btn.dataset.takendate;
                orderEl.value    = btn.dataset.displayorder;
                _setError(errorEl, null);

                bsModal.show();
            });

        // Delete button (event delegation)
        document.getElementById('plant-photos-section-' + opts.plantInstanceId)
            .addEventListener('click', function (e) {
                var btn = e.target.closest('.btn-delete-photo');
                if (!btn) return;

                if (!confirm(opts.deleteConfirm || 'Delete this photo?')) return;

                var photoId  = btn.dataset.id;
                var url      = opts.deleteUrl;
                var token    = _getAntiforgeryToken(document.body);
                var body     = new URLSearchParams({ photoId: photoId });

                fetch(url, {
                    method : 'POST',
                    body   : body,
                    headers: {
                        'Content-Type'           : 'application/x-www-form-urlencoded',
                        'X-Requested-With'       : 'XMLHttpRequest',
                        'RequestVerificationToken': token
                    }
                })
                .then(function (r) { return r.json(); })
                .then(function (json) {
                    if (json.success) {
                        table.ajax.reload(null, false);
                    }
                });
            });

        // Save button in edit modal
        saveBtn.addEventListener('click', async function () {
            _setError(errorEl, null);

            var photoId  = photoIdEl.value;
            var url      = opts.updateUrl;
            var token    = _getAntiforgeryToken(modalEl);

            var body = new URLSearchParams({
                photoId      : photoId,
                Caption      : captionEl.value,
                TakenDate    : dateEl.value,
                DisplayOrder : orderEl.value
            });

            try {
                var resp = await fetch(url, {
                    method : 'POST',
                    body   : body,
                    headers: {
                        'Content-Type'           : 'application/x-www-form-urlencoded',
                        'X-Requested-With'       : 'XMLHttpRequest',
                        'RequestVerificationToken': token
                    }
                });
                var json = await resp.json();

                if (json.success) {
                    bsModal.hide();
                    table.ajax.reload(null, false);
                } else {
                    _setError(errorEl, json.message || 'Error saving changes.');
                }
            } catch (ex) {
                _setError(errorEl, 'Unexpected error. Please try again.');
            }
        });
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    function _getAntiforgeryToken(ctx) {
        var el = (ctx && ctx.querySelector)
            ? ctx.querySelector('[name="__RequestVerificationToken"]')
            : document.querySelector('[name="__RequestVerificationToken"]');
        return el ? el.value : '';
    }

    function _setError(el, msg) {
        if (!el) return;
        if (msg) {
            el.textContent = msg;
            el.classList.remove('d-none');
        } else {
            el.textContent = '';
            el.classList.add('d-none');
        }
    }

    function _esc(str) {
        if (!str) return '';
        return String(str)
            .replace(/&/g, '&amp;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;');
    }

    return { init: init };
}());
