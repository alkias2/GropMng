'use strict'
// IMPORTANT: Clear template customizer localStorage keys after theme config changes.
let config = {
    colors: {
  primary: '#045424',
      secondary: '#69809a',
      success: '#39da8a',
      info: '#00cfdd',
      warning: '#fdac41',
      danger: '#ff5b5c',
      dark: '#495563',
      black: '#000',
      white: '#fff',
      cardColor: '#fff',
      bodyBg: '#f2f2f6',
      bodyColor: '#677788',
      headingColor: '#516377',
      textMuted: '#a8b1bb',
      borderColor: '#e9ecee'
    },
    colors_label: {
      primary: '#04542429',
      secondary: '#69809a29',
      success: '#39da8a29',
      info: '#00cfdd29',
      warning: '#fdac4129',
      danger: '#ff5b5c29',
      dark: '#49556329'
    },
    colors_dark: {
      cardColor: '#283144',
      bodyBg: '#1c222f',
      bodyColor: '#a1b0cb',
      headingColor: '#d8deea',
      textMuted: '#8295ba',
      borderColor: '#36445d'
    },
    enableMenuLocalStorage: !0
  },
  assetsPath = document.documentElement.getAttribute('data-assets-path'),
  templateName = document.documentElement.getAttribute('data-template'),
  rtlSupport = !1

'undefined' != typeof TemplateCustomizer &&
  (window.templateCustomizer = new TemplateCustomizer({
    cssPath: assetsPath + 'vendor/css/',
    themesPath: assetsPath + 'vendor/css/',
    availableThemes: [{ name: 'theme-grop-mng', title: 'GropMng' }],
    displayCustomizer: !0,
    lang:
      localStorage.getItem('templateCustomizer-' + templateName + '--Lang') ||
      'en',
    defaultTheme: 'theme-grop-mng',
    defaultStyle: 'dark',
    controls: ['style', 'themes']
  }))
