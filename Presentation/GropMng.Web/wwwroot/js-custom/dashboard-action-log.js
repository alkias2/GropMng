/**
 * dashboard-action-log.js
 * Handles "Done" quick-log and "Skip" actions from the owner dashboard action list.
 *
 * Skip strategy (server-side / DB-backed via /action-log/skip):
 *   - "Skip for today"   -> ActiveUntilDate = today (row reappears tomorrow)
 *   - "Skip until next"  -> ActiveUntilDate = today + FrequencyDays - 1
 */
(function () {
    'use strict';

    function getToken() {
        return $('input[name="__RequestVerificationToken"]').first().val();
    }

    function removeRow($row, $container) {
        $row.addClass('opacity-50');
        $row.fadeOut(400, function () {
            $(this).remove();
            updateEmptyState($container);
        });
    }

    function updateEmptyState($container) {
        var $listGroup = $container.find('.list-group');
        var visible = $listGroup.find('.list-group-item:visible').length;

        if ($listGroup.length && visible === 0) {
            $listGroup.replaceWith(
                '<p class="text-muted mb-0" id="actions-empty-msg">' +
                ($container.data('empty-text') || 'All done for today!') +
                '</p>'
            );
        }
    }

    function showToast(type, message) {
        var $toastContainer = $('#dashboard-toast-container');
        if ($toastContainer.length === 0) {
            $toastContainer = $('<div id="dashboard-toast-container" class="position-fixed bottom-0 end-0 p-3" style="z-index:1100"></div>');
            $('body').append($toastContainer);
        }

        var bgClass = type === 'success' ? 'bg-success' : 'bg-danger';
        var $toast = $(
            '<div class="toast align-items-center text-white ' + bgClass + ' border-0" role="alert" aria-live="assertive" aria-atomic="true">' +
            '  <div class="d-flex">' +
            '    <div class="toast-body">' + $('<span>').text(message).html() + '</div>' +
            '    <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>' +
            '  </div>' +
            '</div>'
        );

        $toastContainer.append($toast);
        var bsToast = new bootstrap.Toast($toast[0], { delay: 3500 });
        bsToast.show();
        $toast.on('hidden.bs.toast', function () {
            $(this).remove();
        });
    }

    function bindDoneButtons($container) {
        $container.on('click', '.btn-action-done', function () {
            var $btn = $(this);
            if ($btn.prop('disabled')) {
                return;
            }

            var url = $btn.data('url');
            var plantInstanceId = $btn.data('plant-instance-id');
            var $row = $btn.closest('.list-group-item');

            $btn.prop('disabled', true)
                .html('<span class="spinner-border spinner-border-sm" role="status"></span>');

            $.ajax({
                url: url,
                type: 'POST',
                data: {
                    PlantInstanceId: plantInstanceId,
                    __RequestVerificationToken: getToken()
                },
                success: function (response) {
                    if (response && response.success) {
                        removeRow($row, $container);
                        return;
                    }

                    var msg = (response && response.message) ? response.message : 'An error occurred.';
                    $btn.prop('disabled', false).html('<i class="bx bx-check"></i> ' + ($btn.data('label-done') || 'Done'));
                    showToast('error', msg);
                },
                error: function () {
                    $btn.prop('disabled', false).html('<i class="bx bx-check"></i> ' + ($btn.data('label-done') || 'Done'));
                    showToast('error', 'Request failed. Please try again.');
                }
            });
        });
    }

    function bindSkipButtons($container) {
        $container.on('click', '.btn-action-skip', function () {
            var $btn = $(this);
            var plantInstanceId = $btn.data('plant-instance-id');
            var actionType = $btn.data('action-type');
            var plantName = $btn.data('plant-name');
            var frequencyDays = $btn.data('frequency-days');
            var $row = $btn.closest('.list-group-item');

            var $modal = $('#skipActionModal');
            $modal.find('.skip-plant-name').text(plantName);
            $modal.data('plant-instance-id', plantInstanceId);
            $modal.data('action-type', actionType);
            $modal.data('frequency-days', frequencyDays);
            $modal.data('$row', $row);
            $modal.data('$container', $container);

            new bootstrap.Modal($modal[0]).show();
        });

        $(document).on('click', '#btnSkipToday', function () {
            executeSkip('today');
        });

        $(document).on('click', '#btnSkipNext', function () {
            executeSkip('next');
        });
    }

    function executeSkip(mode) {
        var $modal = $('#skipActionModal');
        var plantInstanceId = $modal.data('plant-instance-id');
        var actionType = $modal.data('action-type');
        var frequencyDays = $modal.data('frequency-days') || 0;
        var $row = $modal.data('$row');
        var $container = $modal.data('$container');

        $modal.find('#btnSkipToday, #btnSkipNext').prop('disabled', true);

        $.ajax({
            url: '/action-log/skip',
            type: 'POST',
            data: {
                PlantInstanceId: plantInstanceId,
                ActionType: actionType,
                SkipMode: mode,
                FrequencyDays: frequencyDays,
                __RequestVerificationToken: getToken()
            },
            success: function (response) {
                var modalInstance = bootstrap.Modal.getInstance($modal[0]);
                if (modalInstance) {
                    modalInstance.hide();
                }
                $modal.find('#btnSkipToday, #btnSkipNext').prop('disabled', false);

                if (response && response.success) {
                    removeRow($row, $container);
                    return;
                }

                var msg = (response && response.message) ? response.message : 'An error occurred.';
                showToast('error', msg);
            },
            error: function () {
                var modalInstance = bootstrap.Modal.getInstance($modal[0]);
                if (modalInstance) {
                    modalInstance.hide();
                }
                $modal.find('#btnSkipToday, #btnSkipNext').prop('disabled', false);
                showToast('error', 'Request failed. Please try again.');
            }
        });
    }

    $(function () {
        var $container = $('#dashboard-today-actions');
        if (!$container.length) {
            return;
        }

        bindDoneButtons($container);
        bindSkipButtons($container);
    });
})();
