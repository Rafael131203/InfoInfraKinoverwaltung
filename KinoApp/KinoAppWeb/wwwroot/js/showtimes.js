// wwwroot/js/showtimes.js
// Simple intersection-based fade/slide-in for showtime cards
function startKinoApp() {
    // Home landing page hooks (hero slider, filters, …)
    if (window.KinoHome && typeof window.KinoHome.init === "function") {
        window.KinoHome.init();
    }

    // Showtimes page animations
    if (window.KinoShowtimes && typeof window.KinoShowtimes.init === "function") {
        window.KinoShowtimes.init();
    }

    // Global behaviours
    initBurgerMenu();
    initDynamicYear();
}


window.KinoShowtimes = (function () {
    let observer;

    function initObserver() {
        const cards = document.querySelectorAll(".js-showtime-card");
        if (!cards.length) return;

        const options = {
            threshold: 0.1
        };

        observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.classList.add("movie-card--visible");
                    observer.unobserve(entry.target);
                }
            });
        }, options);

        cards.forEach(card => observer.observe(card));
    }

    return {
        init: function () {
            // Called once after first render
            initObserver();
        },
        refresh: function () {
            // Called when the list changes (other date)
            if (observer) {
                observer.disconnect();
            }
            initObserver();
        }
    };
})();
