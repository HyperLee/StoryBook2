// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

(function () {
    "use strict";

    const supportedLanguages = ["zh-TW", "en"];
    const fallbackLanguage = "zh-TW";
    const fallbackStorageKey = "storybook.language";

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", initializeLanguage);
    } else {
        initializeLanguage();
    }

    function initializeLanguage() {
        const switcher = document.querySelector("[data-language-storage-key]");
        const storageKey = switcher?.getAttribute("data-language-storage-key") || fallbackStorageKey;
        applyLanguage(readStoredLanguage(storageKey));

        document.querySelectorAll("[data-language-option]").forEach((button) => {
            button.addEventListener("click", () => {
                const language = normalizeLanguage(button.getAttribute("data-language-option"));

                try {
                    window.localStorage.setItem(storageKey, language);
                } catch {
                    // Storage can be blocked; the current page still applies the requested language.
                }

                applyLanguage(language);
            });
        });
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

    function applyLanguage(language) {
        const selectedLanguage = normalizeLanguage(language);
        document.documentElement.lang = selectedLanguage;

        document.querySelectorAll("[data-i18n-zh-tw][data-i18n-en]").forEach((element) => {
            setElementText(element, getLocalizedValue(element, "i18n", selectedLanguage));
        });

        document.querySelectorAll("[data-alt-zh-tw][data-alt-en]").forEach((element) => {
            setElementAttribute(element, "alt", getLocalizedValue(element, "alt", selectedLanguage));
        });

        document.querySelectorAll("[data-aria-label-zh-tw][data-aria-label-en]").forEach((element) => {
            setElementAttribute(element, "aria-label", getLocalizedValue(element, "ariaLabel", selectedLanguage));
        });

        document.querySelectorAll("[data-placeholder-zh-tw][data-placeholder-en]").forEach((element) => {
            setElementAttribute(element, "placeholder", getLocalizedValue(element, "placeholder", selectedLanguage));
        });

        document.querySelectorAll("[data-language-option]").forEach((button) => {
            const isSelected = button.getAttribute("data-language-option") === selectedLanguage;
            button.setAttribute("aria-pressed", String(isSelected));
        });
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

    function setElementAttribute(element, attribute, value) {
        if (value) {
            element.setAttribute(attribute, value);
        }
    }

    function capitalize(value) {
        return value.charAt(0).toUpperCase() + value.slice(1);
    }
})();
