/* file-details styles */

(function () {
    'use strict';
  
    // swiper with pagination
    var swiper = new Swiper(".pagination", {
        pagination: {
            el: ".swiper-pagination",
            clickable: true,
        },
        loop: true,
        autoplay: {
            delay: 1500,
            disableOnInteraction: false
        },
        navigation: {
          nextEl: ".swiper-button-next",
          prevEl: ".swiper-button-prev",
        },
        slidesPerView: 1,
        loop: true,
        spaceBetween: 20,
        breakpoints: {
          400: {
            slidesPerView: 2,
          },
          500: {
            slidesPerView: 3,
          },
          700: {
            slidesPerView: 4,
          },
          992: {
            slidesPerView: 3,
          },
          1400: {
            slidesPerView: 4,
          },
        },
    });
  
  })();