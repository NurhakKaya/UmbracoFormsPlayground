/*
 * Light/dark theme toggle. The initial data-theme is set by the inline script in <head>
 * (master.cshtml) so the first paint is already correct; this file wires up the switch.
 * Every localStorage access is guarded: the site must work with storage blocked.
 */
(function () {
    'use strict';

    var KEY = 'theme-preference';
    var root = document.documentElement;
    var button = document.getElementById('themeToggle');
    var query = window.matchMedia ? window.matchMedia('(prefers-color-scheme: dark)') : null;

    function readStored() {
        try {
            var value = window.localStorage.getItem(KEY);
            return value === 'light' || value === 'dark' ? value : null;
        } catch (e) {
            return null;
        }
    }

    function writeStored(value) {
        try {
            window.localStorage.setItem(KEY, value);
        } catch (e) { /* storage blocked: the choice just won't persist */ }
    }

    function systemTheme() {
        return query && query.matches ? 'dark' : 'light';
    }

    function apply(theme) {
        root.setAttribute('data-theme', theme);
        root.style.colorScheme = theme;
        if (button) {
            button.setAttribute('aria-checked', theme === 'dark' ? 'true' : 'false');
        }
    }

    apply(root.getAttribute('data-theme') || readStored() || systemTheme());

    if (button) {
        button.addEventListener('click', function () {
            var next = root.getAttribute('data-theme') === 'dark' ? 'light' : 'dark';
            apply(next);
            writeStored(next);
        });
    }

    // Follow the OS only while the user has not made an explicit choice
    if (query) {
        var onSystemChange = function () {
            if (!readStored()) { apply(systemTheme()); }
        };
        if (query.addEventListener) { query.addEventListener('change', onSystemChange); }
        else if (query.addListener) { query.addListener(onSystemChange); }
    }

    // Keep other open tabs in sync
    window.addEventListener('storage', function (e) {
        if (e.key === KEY) { apply(readStored() || systemTheme()); }
    });
})();
