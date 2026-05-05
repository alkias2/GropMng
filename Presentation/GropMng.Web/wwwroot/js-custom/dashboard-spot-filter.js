/**
 * dashboard-spot-filter.js
 * Client-side GardenSpot filter for the Dashboard Watering and Fertilizing tabs.
 *
 * Behaviour:
 *  - Each action card has a data-spot-id attribute rendered server-side.
 *  - Selecting a spot from the dropdown hides cards that don't match.
 *  - Selecting "All" (value="") restores all cards.
 *  - The dropdown button label updates to reflect the current filter.
 */
(function () {
    'use strict';

    function applySpotFilter(tab, spotId) {
        var gridId = tab === 'watering' ? 'dashboard-watering-grid' : 'dashboard-fertilizing-grid';
        var grid = document.getElementById(gridId);
        if (!grid) return;

        var cards = grid.querySelectorAll('[data-spot-id]');
        cards.forEach(function (card) {
            if (spotId === '' || card.getAttribute('data-spot-id') === spotId) {
                card.style.display = '';
            } else {
                card.style.display = 'none';
            }
        });
    }

    function initTab(tab) {
        var items = document.querySelectorAll('[data-dashboard-spot-filter-item="' + tab + '"]');
        var labelEl = document.getElementById(tab + 'SpotFilterLabel');

        items.forEach(function (item) {
            item.addEventListener('click', function (e) {
                e.preventDefault();

                var spotId = this.getAttribute('data-spot-filter-value');
                var label = this.getAttribute('data-spot-filter-label');

                // Update active state
                items.forEach(function (i) { i.classList.remove('active'); });
                this.classList.add('active');

                // Update button label
                if (labelEl) labelEl.textContent = label;

                applySpotFilter(tab, spotId);
            });
        });
    }

    document.addEventListener('DOMContentLoaded', function () {
        initTab('watering');
        initTab('fertilizing');
    });
})();
