// app.js – KinoApp global behaviour
// Ready-state + burger menu + dynamic year
// Optionally calls Home landing-page JS if loaded

function startKinoApp() {
    // Home landing page hooks (hero slider, filters, …)
    if (window.KinoHome && typeof window.KinoHome.init === "function") {
        window.KinoHome.init();
    }

    // Global behaviours
    initBurgerMenu();
    initDynamicYear();
}

// Blazor-safe DOM ready check
if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", startKinoApp);
} else {
    // DOM already ready (typical for Blazor WASM after hydration)
    startKinoApp();
}

/* =========================================
   BURGER MENU (MOBILE NAV)
========================================= */

function initBurgerMenu() {
    const burger = document.querySelector(".burger");
    const nav = document.querySelector(".main-nav");
    const header = document.querySelector(".site-header");

    if (!burger || !nav || !header) return;

    function setMenuState(isOpen) {
        burger.setAttribute("aria-expanded", String(isOpen));
        burger.classList.toggle("burger--open", isOpen);
        nav.classList.toggle("main-nav--open", isOpen);
        header.classList.toggle("site-header--menu-open", isOpen);
    }

    burger.addEventListener("click", () => {
        const isOpen = burger.getAttribute("aria-expanded") === "true";
        setMenuState(!isOpen);
    });

    // Close on nav link click
    nav.querySelectorAll(".main-nav__link").forEach((link) => {
        link.addEventListener("click", () => setMenuState(false));
    });
}

/* =========================================
   DYNAMIC YEAR IN FOOTER
========================================= */

function initDynamicYear() {
    const yearEl = document.querySelector("[data-year]");
    if (!yearEl) return;

    const now = new Date();
    yearEl.textContent = String(now.getFullYear());
}
