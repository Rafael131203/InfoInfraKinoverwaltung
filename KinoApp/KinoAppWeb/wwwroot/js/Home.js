// Home.razor.js – KinoApp landing page logic
// Hero slider + (optional) movie filters

// 1) Global function Blazor calls
//    Home.razor.cs: await JS.InvokeVoidAsync("kinoInitHero");
window.kinoInitHero = function () {
    console.log("kinoInitHero called");
    if (window.KinoHome && typeof window.KinoHome.init === "function") {
        window.KinoHome.init();
    }
};

// 2) Page module with actual logic
window.KinoHome = (function () {

    // Public init – called from kinoInitHero
    function init() {
        initHeroSlider();
        // If filters are now Blazor-only, comment this out:
        // initMovieFilters();
    }

    /* =========================================
       HERO SLIDER + SHUTTER ANIMATION
    ========================================= */
    function initHeroSlider() {
        const slider = document.querySelector('[data-slider="hero"]');
        if (!slider) return;

        // Avoid double-initializing the same slider element
        if (slider.dataset.kinoInitialized === "true") {
            return;
        }
        slider.dataset.kinoInitialized = "true";

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
        const SHUTTER_DURATION_MS = 900; // keep for CSS sync if needed

        function setActiveSlide(index) {
            slides.forEach(slide =>
                slide.classList.remove("hero-slide--active")
            );
            dots.forEach(dot =>
                dot.classList.remove("hero-slider__dot--active")
            );

            const newSlide = slides[index];
            const newDot = dots[index];

            if (newSlide) newSlide.classList.add("hero-slide--active");
            if (newDot) newDot.classList.add("hero-slider__dot--active");

            currentIndex = index;
        }

        // Shutter is now purely a visual effect over the *new* slide
        function playShutterAnimation() {
            if (!shutterTop || !shutterBottom) {
                return;
            }

            shutterTop.classList.remove("hero-slider__shutter--play-top");
            shutterBottom.classList.remove("hero-slider__shutter--play-bottom");

            // force reflow so animation can restart
            // eslint-disable-next-line no-unused-expressions
            shutterTop.offsetWidth;

            shutterTop.classList.add("hero-slider__shutter--play-top");
            shutterBottom.classList.add("hero-slider__shutter--play-bottom");
        }

        function goToSlide(targetIndex, useShutter = true) {
            const total = slides.length;
            if (!total) return;

            let index = targetIndex;
            if (index < 0) index = total - 1;
            if (index >= total) index = 0;
            if (index === currentIndex) return;

            // 1) Immediately switch slide
            setActiveSlide(index);

            // 2) Then play shutter over the new slide
            if (useShutter) {
                playShutterAnimation();
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
       Only used if you still filter purely in JS.
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

    // expose public API
    return {
        init
    };
})();
