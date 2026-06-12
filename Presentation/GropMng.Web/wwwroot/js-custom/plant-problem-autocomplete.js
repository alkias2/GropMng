/**
 * plant-problem-autocomplete.js
 *
 * Autocomplete search against DiseaseKnowledge API.
 * Toggles NotifyAdmin checkbox visibility and loads info panel when a disease is selected.
 *
 * Dependencies: jQuery
 */

(function () {
    'use strict';

    // Debounce helper
    function debounce(fn, delay) {
        var timer;
        return function () {
            var context = this;
            var args = arguments;
            clearTimeout(timer);
            timer = setTimeout(function () { fn.apply(context, args); }, delay);
        };
    }

    /**
     * Initialize autocomplete on a problem form within a modal or container.
     *
     * @param {Element|string} container  - DOM element or selector containing the form fields
     * @param {object} opts
     * @param {string} opts.searchUrl      - URL for DiseaseKnowledge/Search?q= (e.g. '/my-garden/disease-knowledge/search')
     * @param {string} opts.infoPanelUrl   - URL for DiseaseKnowledge/InfoPanel/{id} (e.g. '/my-garden/disease-knowledge/info-panel/')
     */
    function init(container, opts) {
        if (typeof container === 'string') {
            container = document.querySelector(container);
        }
        if (!container) return;

        opts = opts || {};

        var nameInput = container.querySelector('.problem-name-input');
        var suggestionsEl = container.querySelector('.autocomplete-suggestions');
        var diseaseIdInput = container.querySelector('.problem-disease-knowledge-id');
        var notifyAdminWrapper = container.querySelector('.notify-admin-wrapper');
        var infoPanel = container.querySelector('.disease-info-panel');
        var infoContent = container.querySelector('.disease-info-content');

        if (!nameInput || !suggestionsEl) return;

        var currentIndex = -1;

        // Clear selection
        function clearSelection() {
            if (diseaseIdInput) diseaseIdInput.value = '';
            if (notifyAdminWrapper) notifyAdminWrapper.classList.remove('d-none');
            if (infoPanel) infoPanel.classList.add('d-none');
            if (infoContent) infoContent.innerHTML = '';
        }

        // Hide suggestions
        function hideSuggestions() {
            suggestionsEl.classList.add('d-none');
            suggestionsEl.innerHTML = '';
            currentIndex = -1;
        }

        // Position suggestions below the input
        function positionSuggestions() {
            var rect = nameInput.getBoundingClientRect();
            var offsetParent = suggestionsEl.offsetParent;
            var parentRect = offsetParent ? offsetParent.getBoundingClientRect() : { top: 0, left: 0 };
            suggestionsEl.style.top = (rect.bottom - parentRect.top + 4) + 'px';
            suggestionsEl.style.left = (rect.left - parentRect.left) + 'px';
            suggestionsEl.style.width = rect.width + 'px';
        }

        // Select a disease entry
        function selectItem(item) {
            nameInput.value = item.commonName;
            if (diseaseIdInput) diseaseIdInput.value = item.id;
            if (notifyAdminWrapper) notifyAdminWrapper.classList.add('d-none');

            // Load info panel
            if (infoPanel && infoContent && opts.infoPanelUrl) {
                var url = opts.infoPanelUrl + '/' + item.id;
                fetch(url, { headers: { 'X-Requested-With': 'XMLHttpRequest' } })
                    .then(function (r) { return r.text(); })
                    .then(function (html) {
                        infoContent.innerHTML = html;
                        infoPanel.classList.remove('d-none');
                    })
                    .catch(function () {
                        // Info panel load failed — silently ignore
                    });
            }

            hideSuggestions();
        }

        // Render suggestion items
        function renderSuggestions(items) {
            suggestionsEl.innerHTML = '';
            currentIndex = -1;

            if (!items || items.length === 0) {
                suggestionsEl.classList.add('d-none');
                return;
            }

            items.forEach(function (item, i) {
                var div = document.createElement('div');
                div.className = 'autocomplete-item px-3 py-2 cursor-pointer';
                div.style.cursor = 'pointer';
                div.innerHTML = '<strong>' + escapeHtml(item.commonName) + '</strong>' +
                    (item.scientificName ? ' <small class="text-muted">' + escapeHtml(item.scientificName) + '</small>' : '');

                div.addEventListener('click', function () { selectItem(item); });
                div.addEventListener('mouseenter', function () { currentIndex = i; });
                suggestionsEl.appendChild(div);
            });

            positionSuggestions();
            suggestionsEl.classList.remove('d-none');
        }

        function escapeHtml(str) {
            var div = document.createElement('div');
            div.textContent = str;
            return div.innerHTML;
        }

        // Debounced search
        var doSearch = debounce(function () {
            var term = nameInput.value.trim();
            if (term.length < 2) {
                hideSuggestions();
                return;
            }

            var url = opts.searchUrl + '?q=' + encodeURIComponent(term);
            fetch(url, { headers: { 'X-Requested-With': 'XMLHttpRequest' } })
                .then(function (r) { return r.json(); })
                .then(function (data) { renderSuggestions(data); })
                .catch(function () { hideSuggestions(); });
        }, 300);

        // Input event
        nameInput.addEventListener('input', function () {
            if (diseaseIdInput && diseaseIdInput.value && nameInput.value !== nameInput.dataset.lastSelected) {
                clearSelection();
            }
            doSearch();
        });

        // Keyboard navigation
        nameInput.addEventListener('keydown', function (e) {
            var items = suggestionsEl.querySelectorAll('.autocomplete-item');
            if (!items.length) return;

            if (e.key === 'ArrowDown') {
                e.preventDefault();
                currentIndex = Math.min(currentIndex + 1, items.length - 1);
                items.forEach(function (item, i) {
                    item.style.backgroundColor = i === currentIndex ? '#f0f0f0' : '';
                });
            } else if (e.key === 'ArrowUp') {
                e.preventDefault();
                currentIndex = Math.max(currentIndex - 1, 0);
                items.forEach(function (item, i) {
                    item.style.backgroundColor = i === currentIndex ? '#f0f0f0' : '';
                });
            } else if (e.key === 'Enter') {
                e.preventDefault();
                if (currentIndex >= 0 && currentIndex < items.length) {
                    items[currentIndex].click();
                }
            } else if (e.key === 'Escape') {
                hideSuggestions();
            }
        });

        // Click outside to close
        document.addEventListener('click', function (e) {
            if (!suggestionsEl.contains(e.target) && e.target !== nameInput) {
                hideSuggestions();
            }
        });

        // Info panel toggle
        if (infoPanel) {
            var toggleBtn = infoPanel.querySelector('.disease-info-toggle');
            if (toggleBtn) {
                toggleBtn.addEventListener('click', function () {
                    var content = infoPanel.querySelector('.disease-info-content');
                    var icon = toggleBtn.querySelector('i');
                    if (content) content.classList.toggle('d-none');
                    if (icon) {
                        icon.classList.toggle('bx-chevron-up');
                        icon.classList.toggle('bx-chevron-down');
                    }
                });
            }
        }

        // Store last selected for dirty detection
        if (diseaseIdInput && diseaseIdInput.value) {
            nameInput.dataset.lastSelected = nameInput.value;
        }

        // Expose public API
        container._problemAutocomplete = {
            clear: clearSelection,
            getSelectedId: function () { return diseaseIdInput ? diseaseIdInput.value : ''; }
        };
    }

    /**
     * Initializes click handlers on disease knowledge detail badges in the problems tab.
     * Fetches the detail modal partial and displays it using Bootstrap's modal API.
     *
     * @param {Element|string} container - DOM element or selector containing the problem cards
     */
    function initDetailBadges(container) {
        if (typeof container === 'string') {
            container = document.querySelector(container);
        }
        if (!container) return;

        var detailUrlRoot = window.GropPlantProblemAutocompleteDetailUrl || '/my-garden/disease-knowledge/detail/';

        container.addEventListener('click', function (e) {
            var btn = e.target.closest('.disease-knowledge-detail-btn');
            if (!btn) return;

            var knowledgeId = btn.getAttribute('data-disease-knowledge-id');
            if (!knowledgeId) return;

            e.preventDefault();

            // Remove any previously injected modal
            var existingModal = document.getElementById('disease-knowledge-detail-modal');
            if (existingModal) {
                existingModal.remove();
            }

            // Fetch the modal partial
            fetch(detailUrlRoot + knowledgeId, {
                headers: { 'X-Requested-With': 'XMLHttpRequest' }
            })
                .then(function (response) {
                    if (!response.ok) throw new Error('Failed to load detail');
                    return response.text();
                })
                .then(function (html) {
                    // Inject into body
                    var temp = document.createElement('div');
                    temp.innerHTML = html;
                    var modalEl = temp.firstElementChild;
                    document.body.appendChild(modalEl);

                    // Show via Bootstrap modal API
                    var modal = new bootstrap.Modal(modalEl);
                    modal.show();

                    // Clean up after hide
                    modalEl.addEventListener('hidden.bs.modal', function () {
                        modalEl.remove();
                    });
                })
                .catch(function (err) {
                    console.error('DiseaseKnowledge detail modal failed:', err);
                });
        });
    }

    // Expose globally
    window.GropPlantProblemAutocomplete = { init: init, initDetailBadges: initDetailBadges };
})();
