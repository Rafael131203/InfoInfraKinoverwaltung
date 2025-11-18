// Home.razor.js – KinoApp landing page logic
// Hero slider + shutter + movie filters

window.KinoHome = (function () {
    function init() {
        initHeroSlider();
        initMovieFilters();
    }

    /* =========================================
       HERO SLIDER + SHUTTER ANIMATION
    ========================================= */
    function initHeroSlider() {
        const slider = document.querySelector('[data-slider="hero"]');
        if (!slider) return;

        const slides = Array.from(slider.querySelectorAll(".hero-slide"));
        const dots = Array.from(slider.querySelectorAll(".hero-slider__dot"));
        const prevBtn = slider.querySelector("[data-slider-prev]");
        const nextBtn = slider.querySelector("[data-slider-next]");
        const shutterTop = slider.querySelector(".hero-slider__shutter--top");
        const shutterBottom = slider.querySelector(".hero-slider__shutter--bottom");

        if (!slides.length) return;

        let currentIndex = 0;
        let timerId = null;

        const intervalAttr = slider.getAttribute("data-slider-interval");
        const SLIDE_INTERVAL = Number(intervalAttr) || 7000;
        const SHUTTER_DURATION_MS = 900; // matches CSS shutter animations

        function setActiveSlide(index) {
            slides.forEach((slide) =>
                slide.classList.remove("hero-slide--active")
            );
            dots.forEach((dot) =>
                dot.classList.remove("hero-slider__dot--active")
            );

            const newSlide = slides[index];
            const newDot = dots[index];

            if (newSlide) newSlide.classList.add("hero-slide--active");
            if (newDot) newDot.classList.add("hero-slider__dot--active");

            currentIndex = index;
        }

        function playShutterAnimation(onHalfway) {
            if (!shutterTop || !shutterBottom) {
                if (typeof onHalfway === "function") onHalfway();
                return;
            }

            // Remove classes so animation can restart
            shutterTop.classList.remove("hero-slider__shutter--play-top");
            shutterBottom.classList.remove("hero-slider__shutter--play-bottom");

            // Force reflow to allow re-adding animation classes
            // eslint-disable-next-line no-unused-expressions
            shutterTop.offsetWidth;

            shutterTop.classList.add("hero-slider__shutter--play-top");
            shutterBottom.classList.add("hero-slider__shutter--play-bottom");

            // Trigger slide change roughly mid-animation
            const halfway = SHUTTER_DURATION_MS * 0.4;
            window.setTimeout(() => {
                if (typeof onHalfway === "function") onHalfway();
            }, halfway);
        }

        function goToSlide(targetIndex, useShutter = true) {
            const total = slides.length;
            if (!total) return;

            let index = targetIndex;
            if (index < 0) index = total - 1;
            if (index >= total) index = 0;
            if (index === currentIndex) return;

            const changeSlide = () => setActiveSlide(index);

            if (useShutter) {
                playShutterAnimation(changeSlide);
            } else {
                changeSlide();
            }
        }

        function nextSlide() {
            goToSlide(currentIndex + 1, true);
            resetTimer();
        }

        function prevSlide() {
            goToSlide(currentIndex - 1, true);
            resetTimer();
        }

        function startAutoPlay() {
            stopAutoPlay();
            timerId = window.setInterval(() => {
                goToSlide(currentIndex + 1, true);
            }, SLIDE_INTERVAL);
        }

        function stopAutoPlay() {
            if (timerId !== null) {
                window.clearInterval(timerId);
                timerId = null;
            }
        }

        function resetTimer() {
            startAutoPlay();
        }

        // Initial state
        setActiveSlide(0);
        startAutoPlay();

        // Arrow buttons
        if (nextBtn) {
            nextBtn.addEventListener("click", (e) => {
                e.preventDefault();
                nextSlide();
            });
        }

        if (prevBtn) {
            prevBtn.addEventListener("click", (e) => {
                e.preventDefault();
                prevSlide();
            });
        }

        // Dots
        dots.forEach((dot, index) => {
            dot.addEventListener("click", (e) => {
                e.preventDefault();
                goToSlide(index, true);
                resetTimer();
            });
        });

        // Pause on hover (desktop)
        slider.addEventListener("mouseenter", () => {
            stopAutoPlay();
        });

        slider.addEventListener("mouseleave", () => {
            startAutoPlay();
        });
    }

    /* =========================================
       MOVIE FILTERS (experience + language)
    ========================================= */
    function initMovieFilters() {
        const experienceSelect = document.querySelector(
            '.movie-filters__select[name="experience"]'
        );
        const languageSelect = document.querySelector(
            '.movie-filters__select[name="language"]'
        );
        const movieCards = Array.from(
            document.querySelectorAll(".movie-card")
        );

        if (!experienceSelect || !languageSelect || !movieCards.length) return;

        function applyFilters() {
            const selectedExperience = experienceSelect.value || "all";
            const selectedLanguage = languageSelect.value || "all";

            movieCards.forEach((card) => {
                const cardFormat =
                    card.getAttribute("data-format") || "all";
                const cardLanguage =
                    card.getAttribute("data-language") || "all";

                const matchesExperience =
                    selectedExperience === "all" ||
                    cardFormat === selectedExperience;
                const matchesLanguage =
                    selectedLanguage === "all" ||
                    cardLanguage === selectedLanguage;

                const shouldShow =
                    matchesExperience && matchesLanguage;
                card.hidden = !shouldShow;
            });
        }

        experienceSelect.addEventListener("change", applyFilters);
        languageSelect.addEventListener("change", applyFilters);

        applyFilters();
    }

    // Expose public entry point for app.js
    return {
        init
    };
})();
