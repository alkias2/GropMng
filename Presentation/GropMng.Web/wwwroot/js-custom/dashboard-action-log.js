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

    function resolveActionItem($trigger) {
        var $item = $trigger.closest('[data-dashboard-action-item]');
        if ($item.length) {
            return $item;
        }

        $item = $trigger.closest('[data-skip-key]');
        if ($item.length) {
            return $item;
        }

        $item = $trigger.closest('.list-group-item');
        if ($item.length) {
            return $item;
        }

        $item = $trigger.closest('.col-md-6, .col-lg-4, .col-xl-4');
        if ($item.length) {
            return $item;
        }

        return $trigger.closest('.dashboard-action-card');
    }

    function getActionItems($container) {
        var $items = $container.find('[data-dashboard-action-item]');
        if ($items.length) {
            return $items;
        }

        $items = $container.find('[data-skip-key]');
        if ($items.length) {
            return $items;
        }

        $items = $container.find('.list-group-item');
        if ($items.length) {
            return $items;
        }

        return $container.find('.dashboard-action-card');
    }

    function getToken() {
        var $tokenInput = $('#dashboard-anti-forgery-token input[name="__RequestVerificationToken"]').first();
        if ($tokenInput.length) {
            return $tokenInput.val();
        }

        $tokenInput = $('input[name="__RequestVerificationToken"]').first();
        return $tokenInput.length ? $tokenInput.val() : '';
    }

    function removeRow($row, $container) {
        if (!$row || !$row.length) {
            updateEmptyState($container);
            $(document).trigger('dashboard:action-items-changed');
            return;
        }

        $row.addClass('opacity-50');
        $row.fadeOut(400, function () {
            $(this).remove();
            updateSectionCount($container);
            updateEmptyState($container);
            $(document).trigger('dashboard:action-items-changed');
        });
    }

    function updateSectionCount($container) {
        // Determine action type from remaining items in DOM.
        var actionType = null;
        $container.find('[data-dashboard-action-item] .btn-action-done').each(function () {
            actionType = $(this).data('action-type');
            return false; // break — only need one
        });

        // When the last card has just been removed no items are left in the DOM.
        // Fall back to the currently active section tab to still reach 0.
        if (!actionType) {
            actionType = $('.dashboard-section-btn.active').data('section') || null;
        }

        if (!actionType) return;

        var remaining = $container.find(
            '[data-dashboard-action-item] .btn-action-done[data-action-type="' + actionType + '"]'
        ).length;

        $('[data-count-section="' + actionType + '"]').text(remaining);
    }

    function updateEmptyState($container) {
        var $items = getActionItems($container);
        var total = $items.length;
        var visible = $items.filter(':visible').length;

        if (total === 0) {
            var $content = $container.find('.dashboard-actions-grid').first();
            if (!$content.length) {
                $content = $container.find('.list-group').first();
            }
            if (!$content.length) {
                $content = $container.find('.row').first();
            }

            var emptyHtml =
                '<p class="text-muted mb-0" id="actions-empty-msg">' +
                ($container.data('empty-text') || 'All done for today!') +
                '</p>';

            if ($content.length) {
                $content.replaceWith(emptyHtml);
            } else if (!$container.find('#actions-empty-msg').length) {
                $container.append(emptyHtml);
            }
            return;
        }

        // Keep the grid in place when current filter has no visible cards,
        // so changing filter still works without a page refresh.
        if (visible === 0) {
            $container.find('#actions-empty-msg').remove();
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

    function formatCountMessage(template, count) {
        var safeTemplate = template || '';
        if (safeTemplate.indexOf('{0}') >= 0) {
            return safeTemplate.replace('{0}', count);
        }

        return safeTemplate + ' ' + count;
    }

    function bindDoneButtons($container) {
        $container.on('click', '.btn-action-done', function () {
            var $btn = $(this);
            if ($btn.prop('disabled')) {
                return;
            }

            var url = $btn.data('url');
            var plantInstanceId = $btn.data('plant-instance-id');
            var actionType = $btn.data('action-type');
            var waterAmountL = $btn.data('water-amount-l') || null;
            var fertilizerQuantity = $btn.data('fertilizer-quantity') || null;
            var fertilizerUnit = $btn.data('fertilizer-unit');
            var $row = resolveActionItem($btn);

            $btn.prop('disabled', true)
                .html('<span class="spinner-border spinner-border-sm" role="status"></span>');

            var postData = {
                PlantInstanceId: plantInstanceId,
                __RequestVerificationToken: getToken()
            };

            if (actionType === 'watering' && waterAmountL !== null && waterAmountL !== '') {
                postData.WaterAmountL = waterAmountL;
            }
            if (actionType === 'fertilizing') {
                if (fertilizerQuantity !== null && fertilizerQuantity !== '') {
                    postData.Quantity = fertilizerQuantity;
                }
                if (fertilizerUnit !== undefined && fertilizerUnit !== '') {
                    postData.Unit = fertilizerUnit;
                }
            }

            $.ajax({
                url: url,
                type: 'POST',
                headers: {
                    RequestVerificationToken: getToken()
                },
                data: postData,
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
            var $row = resolveActionItem($btn);

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
            headers: {
                RequestVerificationToken: getToken()
            },
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

    function bindBulkActionButton($container, options) {
        function syncButtonState() {
            var $btn = $(options.buttonSelector);
            if (!$btn.length) {
                return;
            }

            var visibleCount = 0;
            $container.find('[data-dashboard-action-item]:visible').each(function () {
                if ($(this).find('.btn-action-done[data-action-type="' + options.actionType + '"]').length) {
                    visibleCount++;
                }
            });

            $btn.prop('disabled', visibleCount === 0);
        }

        $(document).on('dashboard:state-changed dashboard:action-items-changed', syncButtonState);

        $(document).on('click', options.buttonSelector, function () {
            var $btn = $(this);
            if ($btn.prop('disabled')) {
                return;
            }

            // Collect IDs of visible action items to respect active spot filter.
            var ids = [];
            $container.find('[data-dashboard-action-item]:visible').each(function () {
                var id = $(this).find('.btn-action-done[data-action-type="' + options.actionType + '"]').data('plant-instance-id');
                if (id) {
                    ids.push(id);
                }
            });

            if (ids.length === 0) {
                return;
            }

            $btn.prop('disabled', true)
                .html('<span class="spinner-border spinner-border-sm" role="status"></span>');

            var postData = { __RequestVerificationToken: getToken() };
            $.each(ids, function (i, id) {
                postData['PlantInstanceIds[' + i + ']'] = id;
            });

            $.ajax({
                url: options.url,
                type: 'POST',
                headers: { RequestVerificationToken: getToken() },
                data: postData,
                success: function (response) {
                    $btn.prop('disabled', false)
                        .html(options.iconHtml + ($btn.data('label') || options.defaultLabel));

                    if (response && response.success) {
                        var affectedCount = typeof response.count === 'number' ? response.count : ids.length;

                        // Remove all visible cards of this action type.
                        $container.find('[data-dashboard-action-item]:visible').each(function () {
                            if ($(this).find('.btn-action-done[data-action-type="' + options.actionType + '"]').length) {
                                $(this).addClass('opacity-50').fadeOut(400, function () {
                                    $(this).remove();
                                    updateSectionCount($container);
                                    updateEmptyState($container);
                                    $(document).trigger('dashboard:action-items-changed');
                                });
                            }
                        });

                        if (affectedCount > 0) {
                            var successTemplate = $btn.data('success-template') || options.successTemplate || 'Completed for {0} items.';
                            showToast('success', formatCountMessage(successTemplate, affectedCount));
                        }

                        return;
                    }

                    var msg = (response && response.message) ? response.message : 'An error occurred.';
                    showToast('error', msg);
                },
                error: function () {
                    $btn.prop('disabled', false)
                        .html(options.iconHtml + ($btn.data('label') || options.defaultLabel));
                    showToast('error', 'Request failed. Please try again.');
                }
            });
        });

        syncButtonState();
    }

    $(function () {
        var $container = $('#dashboard-today-actions');
        if (!$container.length) {
            $container = $('#dashboard-panel-content');
        }
        if (!$container.length) {
            return;
        }

        bindDoneButtons($container);
        bindSkipButtons($container);
        bindBulkActionButton($container, {
            buttonSelector: '#btn-water-all',
            actionType: 'watering',
            url: '/action-log/watering-all',
            iconHtml: '<i class="bx bx-droplet me-1"></i>',
            defaultLabel: 'Water all',
            successTemplate: 'Watering recorded for {0} plants.'
        });
        bindBulkActionButton($container, {
            buttonSelector: '#btn-fertilize-all',
            actionType: 'fertilizing',
            url: '/action-log/fertilizing-all',
            iconHtml: '<i class="bx bx-dialpad me-1"></i>',
            defaultLabel: 'Fertilize all',
            successTemplate: 'Fertilizing recorded for {0} plants.'
        });
    });
})();
