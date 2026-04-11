// Theme Manager - handles light/dark mode across all areas
(function () {
    var THEME_KEY = 'app-theme';
    var DARK = 'dark';
    var LIGHT = 'light';

    function getTheme() {
        return localStorage.getItem(THEME_KEY) || LIGHT;
    }

    function applyTheme(theme) {
        document.documentElement.setAttribute('data-bs-theme', theme);
        document.querySelectorAll('[data-theme-toggle]').forEach(function (btn) {
            var icon = btn.querySelector('i');
            if (icon) {
                if (theme === DARK) {
                    icon.className = 'fa-solid fa-sun';
                } else {
                    icon.className = 'fa-solid fa-moon';
                }
            }
            btn.setAttribute('title', theme === DARK ? 'تغییر به حالت روشن' : 'تغییر به حالت تیره');
        });
    }

    function toggleTheme() {
        var next = getTheme() === DARK ? LIGHT : DARK;
        localStorage.setItem(THEME_KEY, next);
        applyTheme(next);
    }

    document.addEventListener('DOMContentLoaded', function () {
        applyTheme(getTheme());
        document.querySelectorAll('[data-theme-toggle]').forEach(function (btn) {
            btn.addEventListener('click', toggleTheme);
        });
    });

    // Public API
    window.themeManager = {
        toggle: toggleTheme,
        getTheme: getTheme,
        apply: applyTheme
    };
})();
