window.KinoSeating = {
    init: function () {
        const seats = document.querySelectorAll('.seat');

        seats.forEach((seat, index) => {
            // staggered intro animation
            seat.style.animationDelay = (index * 0.015) + 's';
            seat.classList.add('seat--intro');

            // small flash on click (doesn't interfere with Blazor click handler)
            seat.addEventListener('click', () => {
                seat.classList.remove('seat--clickflash');
                // force reflow to restart animation
                void seat.offsetWidth;
                seat.classList.add('seat--clickflash');
            });
        });
    }
};
