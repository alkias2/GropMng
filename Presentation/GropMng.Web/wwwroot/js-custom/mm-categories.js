(function (window, document) {
    'use strict';

    // Controls the two-pane main-category/subcategory UI without exposing globals.
    function initializeCategoryPane() {
        var pane = document.querySelector('.category-pane');
        if (!pane) {
            return;
        }

        var mainButtons = Array.prototype.slice.call(pane.querySelectorAll('.category-main-item'));
        var detailPanels = Array.prototype.slice.call(pane.querySelectorAll('.category-detail-panel'));
        var defaultMainCategoryKey = pane.getAttribute('data-default-main-category');

        function setActiveMainCategory(mainCategoryKey) {
            mainButtons.forEach(function (button) {
                var isActive = button.getAttribute('data-main-category') === mainCategoryKey;
                button.classList.toggle('is-active', isActive);
                button.setAttribute('aria-selected', isActive ? 'true' : 'false');
                button.setAttribute('aria-expanded', isActive ? 'true' : 'false');
            });

            detailPanels.forEach(function (panel) {
                var isActive = panel.getAttribute('data-detail-category') === mainCategoryKey;
                panel.hidden = !isActive;
            });
        }

        mainButtons.forEach(function (button) {
            button.addEventListener('click', function () {
                setActiveMainCategory(button.getAttribute('data-main-category'));
            });
        });

        if (defaultMainCategoryKey) {
            setActiveMainCategory(defaultMainCategoryKey);
        }
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initializeCategoryPane);
    } else {
        initializeCategoryPane();
    }
})(window, document);
