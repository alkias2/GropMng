/**
 * dashboard-section-loader.js
 * Handles AJAX loading of dashboard section panels (watering / fertilizing / diseases)
 * and populates the GardenSpot filter dropdown from the loaded panel's embedded options.
 */
(function () {
    'use strict';

    var activeSection = 'watering';

    function notifyStateChanged() {
        if (window.jQuery) {
            window.jQuery(document).trigger('dashboard:state-changed');
        }
    }

    // ---------------------------------------------------------------------------
    // Section loading
    // ---------------------------------------------------------------------------

    function loadSection(section) {
        var panel = document.getElementById('dashboard-panel-content');
        if (!panel) return;

        var url = '/Home/Dashboard' + capitalize(section) + 'Panel';

        panel.innerHTML =
            '<div class="p-4 text-center">' +
            '<div class="spinner-border text-primary" role="status">' +
            '<span class="visually-hidden">Loading\u2026</span>' +
            '</div></div>';

        fetch(url, { headers: { 'X-Requested-With': 'XMLHttpRequest' } })
            .then(function (r) {
                if (!r.ok) throw new Error('HTTP ' + r.status);
                return r.text();
            })
            .then(function (html) {
                panel.innerHTML = html;
                activeSection = section;
                updateButtonStates();
                refreshFilter();
                notifyStateChanged();
            })
            .catch(function () {
                panel.innerHTML =
                    '<div class="p-4"><p class="text-danger mb-0">Error loading content. Please refresh the page.</p></div>';
            });
    }

    function capitalize(s) {
        return s.charAt(0).toUpperCase() + s.slice(1);
    }

    function updateButtonStates() {
        document.querySelectorAll('.dashboard-section-btn').forEach(function (btn) {
            btn.classList.toggle('active', btn.dataset.section === activeSection);
        });

        // Show bulk action button that matches the active tab.
        var $waterAllBtn = document.getElementById('btn-water-all');
        if ($waterAllBtn) {
            $waterAllBtn.classList.toggle('d-none', activeSection !== 'watering');
        }

        var $fertilizeAllBtn = document.getElementById('btn-fertilize-all');
        if ($fertilizeAllBtn) {
            $fertilizeAllBtn.classList.toggle('d-none', activeSection !== 'fertilizing');
        }

        notifyStateChanged();
    }

    // ---------------------------------------------------------------------------
    // GardenSpot filter
    // ---------------------------------------------------------------------------

    function refreshFilter() {
        var optionsEl = document.getElementById('dashboard-spot-options');
        var menu = document.getElementById('dashboardSpotFilterMenu');
        var label = document.getElementById('dashboardSpotFilterLabel');
        var wrapper = document.getElementById('dashboard-spot-filter-wrapper');

        if (!menu) return;
        menu.innerHTML = '';

        if (!optionsEl) {
            if (wrapper) wrapper.classList.add('d-none');
            return;
        }

        var options = [];
        try { options = JSON.parse(optionsEl.textContent || '[]'); } catch (e) { return; }

        // Hide filter when only "All" option (or none at all)
        var hasFilter = options.length > 1;
        if (wrapper) wrapper.classList.toggle('d-none', !hasFilter);
        if (!hasFilter) return;

        // Populate dropdown items
        options.forEach(function (opt) {
            var a = document.createElement('a');
            a.className = 'dropdown-item' + (opt.value === '' ? ' active' : '');
            a.href = 'javascript:void(0);';
            a.dataset.spotValue = opt.value;
            a.textContent = opt.text;
            a.addEventListener('click', function () {
                applyFilter(opt.value, opt.text);
                menu.querySelectorAll('.dropdown-item').forEach(function (i) { i.classList.remove('active'); });
                a.classList.add('active');
            });
            menu.appendChild(a);
        });

        // Reset label to "All spots"
        if (label && options.length > 0) {
            label.textContent = options[0].text;
        }
    }

    function applyFilter(value, text) {
        var label = document.getElementById('dashboardSpotFilterLabel');
        if (label) label.textContent = text;

        document.querySelectorAll('[data-spot-id]').forEach(function (card) {
            card.style.display = (!value || card.dataset.spotId === value) ? '' : 'none';
        });

        notifyStateChanged();
    }

    // ---------------------------------------------------------------------------
    // Init
    // ---------------------------------------------------------------------------

    document.addEventListener('DOMContentLoaded', function () {
        // Bootstrap filter from the server-rendered watering panel
        refreshFilter();

        // Show initial bulk-action button based on active tab.
        var $waterAllBtn = document.getElementById('btn-water-all');
        if ($waterAllBtn) {
            $waterAllBtn.classList.toggle('d-none', activeSection !== 'watering');
        }

        var $fertilizeAllBtn = document.getElementById('btn-fertilize-all');
        if ($fertilizeAllBtn) {
            $fertilizeAllBtn.classList.toggle('d-none', activeSection !== 'fertilizing');
        }

        notifyStateChanged();

        document.querySelectorAll('.dashboard-section-btn').forEach(function (btn) {
            btn.addEventListener('click', function () {
                var section = this.dataset.section;
                if (section !== activeSection) {
                    loadSection(section);
                }
            });
        });
    });

}());
