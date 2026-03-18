"use strict";

(function () {
  var html = document.documentElement;
  var assetsPath = html.getAttribute("data-assets-path") || "/";
  var templateName = html.getAttribute("data-template") || "vertical-menu-template-starter";
  var storagePrefix = "templateCustomizer-" + templateName + "--";
  var styleKey = storagePrefix + "Style";
  var themeKey = storagePrefix + "Theme";
  var desiredTheme = "theme-grop-mng";

  function resolveStyle(styleValue) {
    if (styleValue === "system") {
      return window.matchMedia("(prefers-color-scheme: dark)").matches ? "dark" : "light";
    }

    return styleValue === "dark" ? "dark" : "light";
  }

  function applyThemeStyle(styleValue) {
    var effectiveStyle = resolveStyle(styleValue);
    var isDark = effectiveStyle === "dark";
    var coreHref = assetsPath + "vendor/css/core" + (isDark ? "-dark" : "") + ".css";
    var themeHref = assetsPath + "vendor/css/" + desiredTheme + (isDark ? "-dark" : "") + ".css";

    var coreLink = document.querySelector(".template-customizer-core-css");
    var themeLink = document.querySelector(".template-customizer-theme-css");

    if (coreLink) coreLink.setAttribute("href", coreHref);
    if (themeLink) themeLink.setAttribute("href", themeHref);

    html.classList.remove("light-style", "dark-style");
    html.classList.add(isDark ? "dark-style" : "light-style");
    html.setAttribute("data-theme", desiredTheme);
  }

  function setInitialState() {
    try {
      localStorage.setItem(themeKey, desiredTheme);
      if (!localStorage.getItem(styleKey)) {
        localStorage.setItem(styleKey, "dark");
      }
    } catch (e) {
      // Ignore localStorage access errors and still apply runtime styles.
    }

    var savedStyle = "dark";
    try {
      savedStyle = localStorage.getItem(styleKey) || "dark";
    } catch (e) {
      savedStyle = "dark";
    }

    applyThemeStyle(savedStyle);
  }

  function bindStyleSwitcher() {
    var switcher = document.querySelector(".dropdown-style-switcher");
    if (!switcher) return;

    switcher.addEventListener(
      "click",
      function (event) {
        var item = event.target.closest(".dropdown-item[data-theme]");
        if (!item) return;

        var nextStyle = item.getAttribute("data-theme") || "dark";

        event.preventDefault();
        event.stopPropagation();
        if (event.stopImmediatePropagation) {
          event.stopImmediatePropagation();
        }

        try {
          localStorage.setItem(themeKey, desiredTheme);
          localStorage.setItem(styleKey, nextStyle);
        } catch (e) {
          // Ignore localStorage access errors and continue.
        }

        applyThemeStyle(nextStyle);
      },
      true
    );
  }

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", function () {
      setInitialState();
      bindStyleSwitcher();
    });
  } else {
    setInitialState();
    bindStyleSwitcher();
  }
})();
