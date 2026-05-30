(function () {
    "use strict";

    const supportedLanguages = ["zh-TW", "en"];
    const fallbackLanguage = "zh-TW";
    const languageStorageKey = "storybook.language";

    initializeJourneyLanguage();

    function initializeJourneyLanguage() {
        let currentLanguage = readStoredLanguage();

        applyLanguage(currentLanguage);

        document.querySelectorAll("[data-language-option]").forEach((button) => {
            button.addEventListener("click", () => {
                const requestedLanguage = normalizeLanguage(button.getAttribute("data-language-option"));

                try {
                    window.localStorage.setItem(languageStorageKey, requestedLanguage);
                } catch {
                    // Storage can be blocked; the current page still applies the selected language.
                }

                currentLanguage = requestedLanguage;
                applyLanguage(currentLanguage);
            });
        });

        window.addEventListener("storage", (event) => {
            if (event.key === languageStorageKey) {
                currentLanguage = normalizeLanguage(event.newValue);
                applyLanguage(currentLanguage);
            }
        });
    }

    function readStoredLanguage() {
        try {
            return normalizeLanguage(window.localStorage.getItem(languageStorageKey));
        } catch {
            return fallbackLanguage;
        }
    }

    function normalizeLanguage(language) {
        return supportedLanguages.includes(language || "") ? language : fallbackLanguage;
    }

    function applyLanguage(language) {
        const selectedLanguage = normalizeLanguage(language);
        document.documentElement.lang = selectedLanguage;

        document.querySelectorAll("[data-i18n-zh-tw][data-i18n-en]").forEach((element) => {
            setElementText(element, getLocalizedValue(element, "i18n", selectedLanguage));
        });

        document.querySelectorAll("[data-aria-label-zh-tw][data-aria-label-en]").forEach((element) => {
            setElementAttribute(element, "aria-label", getLocalizedValue(element, "ariaLabel", selectedLanguage));
        });

        document.querySelectorAll("[data-alt-zh-tw][data-alt-en]").forEach((element) => {
            setElementAttribute(element, "alt", getLocalizedValue(element, "alt", selectedLanguage));
        });

        document.querySelectorAll("[data-language-option]").forEach((button) => {
            const isSelected = button.getAttribute("data-language-option") === selectedLanguage;
            button.setAttribute("aria-pressed", String(isSelected));
        });
    }

    function getLocalizedValue(element, prefix, language) {
        const suffix = normalizeLanguage(language) === "en" ? "En" : "ZhTw";
        const fallbackValue = element.dataset[`${prefix}ZhTw`];
        return element.dataset[`${prefix}${suffix}`] || fallbackValue || "";
    }

    function setElementText(element, value) {
        if (value) {
            element.textContent = value;
        }
    }

    function setElementAttribute(element, attribute, value) {
        if (value) {
            element.setAttribute(attribute, value);
        }
    }
})();
