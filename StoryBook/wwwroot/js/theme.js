(function () {
    "use strict";

    const allowedModes = ["light", "dark", "system"];
    const defaultMode = "system";
    const safeEffectiveTheme = "light";
    const fallbackStorageKey = "storybook.theme";
    const languageStorageKey = "storybook.language";
    const supportedLanguages = ["zh-TW", "en"];
    const fallbackLanguage = "zh-TW";

    let currentMode = defaultMode;

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", initializeTheme);
    } else {
        initializeTheme();
    }

    function initializeTheme() {
        const selector = document.querySelector("[data-theme-selector]");
        const storageKey = selector?.getAttribute("data-theme-storage-key") || fallbackStorageKey;
        currentMode = readStoredMode(storageKey);

        applyTheme(currentMode, selector);
        initializeSystemPreferenceSync(selector);
        initializeThemeLanguage(selector);

        if (!selector) {
            return;
        }

        selector.querySelectorAll("[data-theme-option]").forEach((option) => {
            option.addEventListener("change", () => {
                if (!(option instanceof HTMLInputElement) || !option.checked) {
                    return;
                }

                currentMode = normalizeMode(option.value);
                writeStoredMode(storageKey, currentMode);
                applyTheme(currentMode, selector);
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

    function initializeSystemPreferenceSync(selector) {
        const query = getSystemPreferenceQuery();

        if (!query) {
            return;
        }

        const handleChange = () => {
            if (currentMode === "system") {
                applyTheme(currentMode, selector);
            }
        };

        if (typeof query.addEventListener === "function") {
            query.addEventListener("change", handleChange);
            return;
        }

        if (typeof query.addListener === "function") {
            query.addListener(handleChange);
        }
    }

    function getSystemPreferenceQuery() {
        try {
            return typeof window.matchMedia === "function"
                ? window.matchMedia("(prefers-color-scheme: dark)")
                : null;
        } catch {
            return null;
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

        applySelectorLanguage(selector, readStoredLanguage(getLanguageStorageKey(selector)));
    }

    function initializeThemeLanguage(selector) {
        if (!selector) {
            return;
        }

        const storageKey = getLanguageStorageKey(selector);
        applySelectorLanguage(selector, readStoredLanguage(storageKey));

        document.querySelectorAll("[data-language-option]").forEach((button) => {
            button.addEventListener("click", () => {
                const requestedLanguage = normalizeLanguage(button.getAttribute("data-language-option"));

                try {
                    window.localStorage.setItem(storageKey, requestedLanguage);
                } catch {
                    // Storage can be blocked; the selector still uses the requested language.
                }

                applySelectorLanguage(selector, requestedLanguage);
            });
        });
    }

    function getLanguageStorageKey(selector) {
        return selector?.getAttribute("data-theme-language-storage-key") || languageStorageKey;
    }

    function readStoredLanguage(storageKey) {
        try {
            return normalizeLanguage(window.localStorage.getItem(storageKey));
        } catch {
            return fallbackLanguage;
        }
    }

    function normalizeLanguage(language) {
        return supportedLanguages.includes(language || "") ? language : fallbackLanguage;
    }

    function applySelectorLanguage(selector, language) {
        const selectedLanguage = normalizeLanguage(language);

        selector.querySelectorAll("[data-i18n-zh-tw][data-i18n-en]").forEach((element) => {
            setElementText(element, getLocalizedValue(element, "i18n", selectedLanguage));
        });

        selector.querySelectorAll("[data-theme-label]").forEach((element) => {
            setElementText(element, getLocalizedValue(element, "themeLabel", selectedLanguage));
        });

        selector.querySelectorAll("[data-theme-description]").forEach((element) => {
            setElementText(element, getLocalizedValue(element, "themeDescription", selectedLanguage));
        });

        document.querySelectorAll("[data-language-option]").forEach((button) => {
            const isSelected = button.getAttribute("data-language-option") === selectedLanguage;
            button.setAttribute("aria-pressed", String(isSelected));
        });

        updateSelectedStatus(selector, selectedLanguage);
    }

    function updateSelectedStatus(selector, language) {
        const status = selector.querySelector("[data-theme-selected-status]");
        const selectedInput = selector.querySelector(`[data-theme-option="${currentMode}"]`);
        const selectedLabel = selectedInput instanceof HTMLElement
            ? selectedInput.parentElement?.querySelector("[data-theme-label]")
            : null;

        if (!status || !selectedLabel) {
            return;
        }

        const template = getLocalizedValue(status, "themeSelectedStatus", language);
        const label = selectedLabel.textContent?.trim() || currentMode;
        setElementText(status, template.replace("{0}", label));
    }

    function getLocalizedValue(element, prefix, language) {
        const languageSuffix = language === "en" ? "en" : "zhTw";
        const fallbackValue = element.dataset[`${prefix}ZhTw`];
        return element.dataset[`${prefix}${capitalize(languageSuffix)}`] || fallbackValue || "";
    }

    function setElementText(element, value) {
        if (value) {
            element.textContent = value;
        }
    }

    function capitalize(value) {
        return value.charAt(0).toUpperCase() + value.slice(1);
    }
})();
