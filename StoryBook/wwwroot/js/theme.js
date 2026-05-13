(function () {
    "use strict";

    const allowedModes = ["light", "dark", "system"];
    const defaultMode = "system";
    const safeEffectiveTheme = "light";
    const fallbackStorageKey = "storybook.theme";

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", initializeTheme);
    } else {
        initializeTheme();
    }

    function initializeTheme() {
        const selector = document.querySelector("[data-theme-selector]");
        const storageKey = selector?.getAttribute("data-theme-storage-key") || fallbackStorageKey;
        const selectedMode = readStoredMode(storageKey);

        applyTheme(selectedMode, selector);

        if (!selector) {
            return;
        }

        selector.querySelectorAll("[data-theme-option]").forEach((option) => {
            option.addEventListener("change", () => {
                if (!(option instanceof HTMLInputElement) || !option.checked) {
                    return;
                }

                const requestedMode = normalizeMode(option.value);
                writeStoredMode(storageKey, requestedMode);
                applyTheme(requestedMode, selector);
            });
        });
    }

    function readStoredMode(storageKey) {
        try {
            return normalizeMode(window.localStorage.getItem(storageKey));
        } catch {
            return defaultMode;
        }
    }

    function writeStoredMode(storageKey, mode) {
        try {
            window.localStorage.setItem(storageKey, normalizeMode(mode));
        } catch {
            // Storage can be blocked; the current page still applies the selected mode.
        }
    }

    function normalizeMode(value) {
        const normalized = String(value || "").trim().toLowerCase();
        return allowedModes.includes(normalized) ? normalized : defaultMode;
    }

    function resolveEffectiveTheme(mode) {
        const selectedMode = normalizeMode(mode);

        if (selectedMode === "light" || selectedMode === "dark") {
            return selectedMode;
        }

        try {
            return window.matchMedia?.("(prefers-color-scheme: dark)").matches ? "dark" : safeEffectiveTheme;
        } catch {
            return safeEffectiveTheme;
        }
    }

    function applyTheme(mode, selector) {
        const selectedMode = normalizeMode(mode);
        const effectiveTheme = resolveEffectiveTheme(selectedMode);
        const root = document.documentElement;

        root.setAttribute("data-bs-theme", effectiveTheme);
        root.setAttribute("data-storybook-theme-mode", selectedMode);
        root.setAttribute("data-storybook-effective-theme", effectiveTheme);

        if (!selector) {
            return;
        }

        selector.setAttribute("data-storybook-theme-mode", selectedMode);
        selector.setAttribute("data-storybook-effective-theme", effectiveTheme);

        selector.querySelectorAll("[data-theme-option]").forEach((option) => {
            const isSelected = option.getAttribute("data-theme-option") === selectedMode;

            if (option instanceof HTMLInputElement) {
                option.checked = isSelected;
            }

            option.setAttribute("aria-checked", String(isSelected));
        });
    }
})();
