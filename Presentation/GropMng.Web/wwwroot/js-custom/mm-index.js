(function (window, document) {
    'use strict';

    var monthNames = [
        'Ιανουάριος', 'Φεβρουάριος', 'Μάρτιος', 'Απρίλιος', 'Μάιος', 'Ιούνιος',
        'Ιούλιος', 'Αύγουστος', 'Σεπτέμβριος', 'Οκτώβριος', 'Νοέμβριος', 'Δεκέμβριος'
    ];

    // Enables flatpickr on date inputs when the library is available on the page.
    function initializeDatePickers() {
        if (typeof window.flatpickr !== 'function') {
            return;
        }

        var dateInputs = document.querySelectorAll('.date-only-picker');
        dateInputs.forEach(function (input) {
            window.flatpickr(input, {
                dateFormat: 'Y-m-d',
                allowInput: true
            });
        });
    }

    // Handles month/year quick navigation and submits the filter form with computed range.
    function initializeQuickDateNavigation() {
        var filterForm = document.getElementById('filterForm');
        var fromDateInput = document.getElementById('fromDate');
        var toDateInput = document.getElementById('toDate');
        var currentYearElement = document.getElementById('currentYear');
        var currentMonthElement = document.getElementById('currentMonth');

        if (!filterForm || !fromDateInput || !toDateInput || !currentYearElement || !currentMonthElement) {
            return;
        }

        var currentYear = parseInt((currentYearElement.textContent || '').trim(), 10);
        var currentMonth = parseInt(currentMonthElement.getAttribute('data-current-month') || '', 10);

        if (Number.isNaN(currentYear)) {
            currentYear = new Date().getFullYear();
        }

        if (Number.isNaN(currentMonth) || currentMonth < 1 || currentMonth > 12) {
            var monthByNameIndex = monthNames.indexOf((currentMonthElement.textContent || '').trim());
            currentMonth = monthByNameIndex >= 0 ? monthByNameIndex + 1 : (new Date().getMonth() + 1);
        }

        function updateDateFiltersAndSubmit() {
            currentYearElement.textContent = String(currentYear);
            currentMonthElement.textContent = monthNames[currentMonth - 1];
            currentMonthElement.setAttribute('data-current-month', String(currentMonth));

            var firstDay = new Date(currentYear, currentMonth - 1, 1);
            var lastDay = new Date(currentYear, currentMonth, 0);

            var fromDateStr = firstDay.getFullYear() + '-'
                + String(firstDay.getMonth() + 1).padStart(2, '0') + '-'
                + String(firstDay.getDate()).padStart(2, '0');
            var toDateStr = lastDay.getFullYear() + '-'
                + String(lastDay.getMonth() + 1).padStart(2, '0') + '-'
                + String(lastDay.getDate()).padStart(2, '0');

            fromDateInput.value = fromDateStr;
            toDateInput.value = toDateStr;
            filterForm.submit();
        }

        var yearButtons = document.querySelectorAll('.quick-nav-year');
        yearButtons.forEach(function (button) {
            button.addEventListener('click', function () {
                var delta = parseInt(button.getAttribute('data-delta') || '0', 10);
                if (!Number.isNaN(delta) && delta !== 0) {
                    currentYear += delta;
                    updateDateFiltersAndSubmit();
                }
            });
        });

        var monthButtons = document.querySelectorAll('.quick-nav-month');
        monthButtons.forEach(function (button) {
            button.addEventListener('click', function () {
                var delta = parseInt(button.getAttribute('data-delta') || '0', 10);
                if (Number.isNaN(delta) || delta === 0) {
                    return;
                }

                currentMonth += delta;
                if (currentMonth > 12) {
                    currentMonth = 1;
                    currentYear += 1;
                } else if (currentMonth < 1) {
                    currentMonth = 12;
                    currentYear -= 1;
                }

                updateDateFiltersAndSubmit();
            });
        });
    }

    // Provides tag chips + autocomplete while preserving selected tags in hidden JSON field.
    function initializeTagSearch() {
        var tagSearchInput = document.getElementById('tagSearchInput');
        var tagAutocompleteList = document.getElementById('tagAutocompleteList');
        var tagFilterHidden = document.getElementById('tagFilterHidden');
        var selectedTagsContainer = document.getElementById('selectedTagsContainer');
        var filterForm = document.getElementById('filterForm');

        if (!tagSearchInput || !tagAutocompleteList || !tagFilterHidden || !selectedTagsContainer || !filterForm) {
            return;
        }

        var selectedTags = [];
        var searchTimeoutId = 0;

        function parseInitialTags() {
            var initialTagFilter = (tagFilterHidden.value || '').trim();
            if (!initialTagFilter) {
                return;
            }

            try {
                var parsed = JSON.parse(initialTagFilter);
                if (Array.isArray(parsed)) {
                    selectedTags = parsed.filter(function (tag) {
                        return typeof tag === 'string' && tag.trim().length > 0;
                    });
                    return;
                }
            } catch (error) {
                // Fall back to comma-separated parsing for backward compatibility.
            }

            selectedTags = initialTagFilter
                .split(',')
                .map(function (tag) { return tag.trim(); })
                .filter(function (tag) { return tag.length > 0; });
        }

        function syncHiddenTagFilter() {
            tagFilterHidden.value = JSON.stringify(selectedTags);
        }

        function createTagChip(tag, index) {
            var chip = document.createElement('span');
            chip.className = 'badge text-white d-flex align-items-center gap-2 selected-tag-chip';
            chip.style.fontWeight = '500';

            var tagText = document.createElement('span');
            tagText.className = 'selected-tag-chip';
            tagText.textContent = tag;

            var removeButton = document.createElement('button');
            removeButton.type = 'button';
            removeButton.className = 'btn-close';
            removeButton.setAttribute('data-tag-index', String(index));
            removeButton.style.cursor = 'pointer';
            removeButton.style.padding = '0';
            removeButton.style.marginLeft = '0.25rem';

            chip.appendChild(tagText);
            chip.appendChild(removeButton);
            return chip;
        }

        function renderSelectedTags() {
            selectedTagsContainer.innerHTML = '';
            selectedTags.forEach(function (tag, index) {
                selectedTagsContainer.appendChild(createTagChip(tag, index));
            });
            syncHiddenTagFilter();
        }

        function hideAutocomplete() {
            tagAutocompleteList.style.display = 'none';
            tagAutocompleteList.innerHTML = '';
        }

        function renderAutocomplete(matches) {
            tagAutocompleteList.innerHTML = '';

            if (!matches.length) {
                hideAutocomplete();
                return;
            }

            matches.slice(0, 10).forEach(function (tag) {
                var option = document.createElement('button');
                option.type = 'button';
                option.className = 'list-group-item list-group-item-action tag-option';
                option.setAttribute('data-tag', tag);
                option.textContent = tag;
                tagAutocompleteList.appendChild(option);
            });

            tagAutocompleteList.style.display = 'block';
        }

        async function searchTags(query) {
            try {
                var response = await window.fetch('/Mm/SearchTags?query=' + encodeURIComponent(query));
                if (!response.ok) {
                    throw new Error('Request failed with status ' + response.status);
                }

                var tags = await response.json();
                if (!Array.isArray(tags)) {
                    hideAutocomplete();
                    return;
                }

                var matches = tags.filter(function (tag) {
                    return typeof tag === 'string' && !selectedTags.includes(tag);
                });

                renderAutocomplete(matches);
            } catch (error) {
                hideAutocomplete();
                window.console.error('Error searching tags:', error);
            }
        }

        selectedTagsContainer.addEventListener('click', function (event) {
            var removeButton = event.target.closest('[data-tag-index]');
            if (!removeButton) {
                return;
            }

            event.preventDefault();
            event.stopPropagation();

            var index = parseInt(removeButton.getAttribute('data-tag-index') || '-1', 10);
            if (index >= 0 && index < selectedTags.length) {
                selectedTags.splice(index, 1);
                renderSelectedTags();
            }
        });

        tagSearchInput.addEventListener('input', function () {
            var query = tagSearchInput.value.trim();

            if (searchTimeoutId) {
                window.clearTimeout(searchTimeoutId);
            }

            if (query.length < 2) {
                hideAutocomplete();
                return;
            }

            searchTimeoutId = window.setTimeout(function () {
                searchTags(query);
            }, 300);
        });

        tagAutocompleteList.addEventListener('click', function (event) {
            var option = event.target.closest('.tag-option');
            if (!option) {
                return;
            }

            event.preventDefault();

            var selectedTag = option.getAttribute('data-tag');
            if (selectedTag && !selectedTags.includes(selectedTag)) {
                selectedTags.push(selectedTag);
                renderSelectedTags();
                tagSearchInput.value = '';
                hideAutocomplete();
            }
        });

        tagSearchInput.addEventListener('blur', function () {
            window.setTimeout(function () {
                hideAutocomplete();
            }, 200);
        });

        tagSearchInput.addEventListener('keydown', function (event) {
            if (event.key === 'Backspace' && !tagSearchInput.value && selectedTags.length > 0) {
                selectedTags.pop();
                renderSelectedTags();
            }
        });

        filterForm.addEventListener('submit', function () {
            syncHiddenTagFilter();
        });

        parseInitialTags();
        renderSelectedTags();
    }

    function initializePage() {
        initializeDatePickers();
        initializeQuickDateNavigation();
        initializeTagSearch();
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initializePage);
    } else {
        initializePage();
    }
})(window, document);
