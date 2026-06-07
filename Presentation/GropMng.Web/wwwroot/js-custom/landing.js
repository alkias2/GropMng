/**
 * GropMng — Public Landing Page
 * Handles only what our landing page actually contains:
 *   - Navbar scroll shadow
 *   - Hero parallax (desktop only, when #hero-animation exists)
 */
'use strict';

(function () {
  const nav = document.querySelector('.layout-navbar');
  const heroAnimation = document.getElementById('hero-animation');
  const animationImg = document.querySelectorAll('.hero-dashboard-img');
  const animationElements = document.querySelectorAll('.hero-elements-img');

  // Navbar shadow on scroll
  if (nav) {
    window.addEventListener('scroll', function () {
      if (window.scrollY > 10) {
        nav.classList.add('shadow');
      } else {
        nav.classList.remove('shadow');
      }
    });
  }

  // Hero parallax — desktop only, only when the hero section exists
  const mediaQueryXL = 1200;
  if (screen.width >= mediaQueryXL && heroAnimation) {
    heroAnimation.addEventListener('mousemove', function (e) {
      animationElements.forEach(function (layer) {
        layer.style.transform = 'translateZ(1rem)';
      });
      animationImg.forEach(function (layer) {
        var x = (window.innerWidth - e.pageX * 2) / 100;
        var y = (window.innerHeight - e.pageY * 2) / 100;
        layer.style.transform = 'perspective(1200px) rotateX(' + y + 'deg) rotateY(' + x + 'deg) scale3d(1, 1, 1)';
      });
    });

    heroAnimation.addEventListener('mouseout', function () {
      animationElements.forEach(function (layer) {
        layer.style.transform = 'translateZ(0)';
      });
      animationImg.forEach(function (layer) {
        layer.style.transform = 'perspective(1200px) scale(1) rotateX(0) rotateY(0)';
      });
    });
  }
})();
