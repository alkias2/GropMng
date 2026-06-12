/**
 * plant-problem-schedule.js
 *
 * Auto-calculates NextDueDate from StartDate + FrequencyValue + FrequencyUnit.
 * Used in schedule create/edit modals.
 *
 * Dependencies: none
 */

(function () {
    'use strict';

    /**
     * Calculate the next due date based on start date, frequency value, and frequency unit.
     *
     * @param {Date}   startDate      - JavaScript Date object for the start date
     * @param {number} frequencyValue - e.g. 7
     * @param {string} frequencyUnit  - "Days" | "Weeks" | "Months"
     * @returns {Date} The calculated next due date
     */
    function calculateNextDueDate(startDate, frequencyValue, frequencyUnit) {
        var result = new Date(startDate.getTime());

        switch (frequencyUnit) {
            case 'Days':
                result.setDate(result.getDate() + frequencyValue);
                break;
            case 'Weeks':
                result.setDate(result.getDate() + frequencyValue * 7);
                break;
            case 'Months':
                result.setMonth(result.getMonth() + frequencyValue);
                break;
            default:
                return result;
        }

        return result;
    }

    /**
     * Format a Date as yyyy-MM-dd for input[type=date] display.
     */
    function formatDate(date) {
        var yyyy = date.getFullYear();
        var mm = String(date.getMonth() + 1).padStart(2, '0');
        var dd = String(date.getDate()).padStart(2, '0');
        return yyyy + '-' + mm + '-' + dd;
    }

    /**
     * Format a Date as dd/MM/yyyy for hint display.
     */
    function formatDisplayDate(date) {
        var dd = String(date.getDate()).padStart(2, '0');
        var mm = String(date.getMonth() + 1).padStart(2, '0');
        var yyyy = date.getFullYear();
        return dd + '/' + mm + '/' + yyyy;
    }

    /**
     * Initialize schedule auto-calculation on a container (modal).
     *
     * @param {Element|string} container - DOM element containing schedule form fields
     */
    function init(container) {
        if (typeof container === 'string') {
            container = document.querySelector(container);
        }
        if (!container) return;

        var startDateInput = container.querySelector('.schedule-start-date');
        var frequencyValueInput = container.querySelector('.schedule-frequency-value');
        var frequencyUnitSelect = container.querySelector('.schedule-frequency-unit');
        var nextDueDateInput = container.querySelector('.schedule-next-due-date');
        var hintEl = container.querySelector('.schedule-next-due-hint');

        if (!startDateInput || !frequencyValueInput || !frequencyUnitSelect) return;

        function updateNextDueDate() {
            var startValue = startDateInput.value;
            var freqValue = parseInt(frequencyValueInput.value, 10);
            var freqUnit = frequencyUnitSelect.value;

            if (!startValue || !freqValue || freqValue < 1 || !freqUnit) {
                if (nextDueDateInput) nextDueDateInput.value = '';
                if (hintEl) hintEl.textContent = '';
                return;
            }

            var startDate = new Date(startValue + 'T00:00:00');
            var nextDue = calculateNextDueDate(startDate, freqValue, freqUnit);

            if (nextDueDateInput) {
                nextDueDateInput.value = formatDate(nextDue);
            }

            if (hintEl) {
                hintEl.textContent = 'Επόμενη εκτέλεση: ' + formatDisplayDate(nextDue);
            }
        }

        // Initial calculation
        updateNextDueDate();

        // Bind events
        startDateInput.addEventListener('change', updateNextDueDate);
        frequencyValueInput.addEventListener('input', updateNextDueDate);
        frequencyValueInput.addEventListener('change', updateNextDueDate);
        frequencyUnitSelect.addEventListener('change', updateNextDueDate);
    }

    // Expose globally
    window.GropPlantProblemSchedule = { init: init };
})();