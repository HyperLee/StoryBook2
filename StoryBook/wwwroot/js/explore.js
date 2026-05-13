(function () {
    const supportedLanguages = ["zh-TW", "en"];
    const fallbackLanguage = "zh-TW";
    const languageStorageKey = "storybook.language";

    initializeExploreControls();

    function initializeExploreControls() {
        const input = document.querySelector("[data-explore-search-input]");
        const clearButton = document.querySelector("[data-explore-clear-search]");
        const clearFiltersButton = document.querySelector("[data-explore-clear-filters]");
        const status = document.querySelector("[data-explore-result-status]");
        const tooShort = document.querySelector("[data-explore-too-short]");
        const noResults = document.querySelector("[data-explore-no-results]");
        const items = Array.from(document.querySelectorAll("[data-explore-result-item]"));
        const filterButtons = Array.from(document.querySelectorAll("[data-explore-filter-value]"));
        const selectedFacets = new Map();
        let currentLanguage = readStoredLanguage(languageStorageKey);

        if (!(input instanceof HTMLInputElement) || items.length === 0) {
            return;
        }

        applyLanguage(currentLanguage);
        initializeLanguageButtons((language) => {
            currentLanguage = language;
            applyLanguage(currentLanguage);
            updateResults();
        });

        input.addEventListener("input", updateResults);

        if (clearButton instanceof HTMLButtonElement) {
            clearButton.addEventListener("click", () => {
                input.value = "";
                updateResults();
                input.focus();
            });
        }

        filterButtons.forEach((button) => {
            button.addEventListener("click", () => {
                const group = button.closest("[data-explore-filter-group]");

                if (!(group instanceof HTMLElement)) {
                    return;
                }

                const groupCode = group.getAttribute("data-explore-filter-group") || "";
                const valueCode = button.getAttribute("data-explore-filter-value") || "";

                if (!groupCode || !valueCode) {
                    return;
                }

                selectedFacets.set(groupCode, valueCode);
                updateFilterButtons(filterButtons, selectedFacets);
                updateResults();
            });
        });

        if (clearFiltersButton instanceof HTMLButtonElement) {
            clearFiltersButton.addEventListener("click", () => {
                selectedFacets.clear();
                updateFilterButtons(filterButtons, selectedFacets);
                updateResults();
            });
        }

        updateResults();

        function updateResults() {
            const rawQuery = input.value;
            const query = normalize(rawQuery);
            const hasTypedText = rawQuery.trim().length > 0;
            const isTooShort = hasTypedText && query.length < 2;
            const hasFilters = selectedFacets.size > 0;
            let visibleCount = 0;

            items.forEach((item) => {
                const searchText = normalize(item.getAttribute("data-explore-search-text"));
                const matchesSearch = query.length === 0 || isTooShort || searchText.includes(query);
                const matchesFilters = itemMatchesFilters(item, selectedFacets);
                const isVisible = matchesSearch && matchesFilters;
                item.hidden = !isVisible;

                if (isVisible) {
                    visibleCount += 1;
                }
            });

            setHidden(tooShort, !isTooShort);
            setHidden(noResults, isTooShort || (!hasFilters && query.length === 0) || visibleCount > 0);
            updateStatus(status, getResultMode(query.length > 0 && !isTooShort, hasFilters), visibleCount, currentLanguage);
        }
    }

    function initializeLanguageButtons(onLanguageChanged) {
        document.querySelectorAll("[data-language-option]").forEach((button) => {
            button.addEventListener("click", () => {
                const requestedLanguage = normalizeLanguage(button.getAttribute("data-language-option"));

                try {
                    const storage = window.localStorage;
                    storage.setItem(languageStorageKey, requestedLanguage);
                } catch {
                    // Storage can be blocked; the current page still applies the selected language.
                }

                onLanguageChanged(requestedLanguage);
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

    function updateFilterButtons(filterButtons, selectedFacets) {
        filterButtons.forEach((button) => {
            const group = button.closest("[data-explore-filter-group]");
            const groupCode = group instanceof HTMLElement ? group.getAttribute("data-explore-filter-group") || "" : "";
            const valueCode = button.getAttribute("data-explore-filter-value") || "";
            const isSelected = selectedFacets.get(groupCode) === valueCode;

            button.setAttribute("aria-pressed", String(isSelected));
            button.setAttribute("data-explore-filter-active", String(isSelected));
        });
    }

    function itemMatchesFilters(item, selectedFacets) {
        if (selectedFacets.size === 0) {
            return true;
        }

        const facets = new Set(String(item.getAttribute("data-explore-facets") || "").split(/\s+/).filter(Boolean));

        for (const [groupCode, valueCode] of selectedFacets.entries()) {
            if (!facets.has(`${groupCode}:${valueCode}`)) {
                return false;
            }
        }

        return true;
    }

    function getResultMode(hasSearch, hasFilters) {
        if (hasSearch && hasFilters) {
            return "intersection";
        }

        if (hasSearch) {
            return "search";
        }

        return hasFilters ? "filter" : "all";
    }

    function updateStatus(status, mode, visibleCount, language) {
        if (!(status instanceof HTMLElement)) {
            return;
        }

        const suffix = normalizeLanguage(language) === "en" ? "en" : "zh-tw";
        const template = status.getAttribute(`data-${mode}-template-${suffix}`) || status.getAttribute("data-all-template-zh-tw");
        status.textContent = (template || "目前顯示 {0} 位故事朋友。").replace("{0}", String(visibleCount));
    }

    function setHidden(element, hidden) {
        if (element instanceof HTMLElement) {
            element.hidden = hidden;
        }
    }

    function normalize(value) {
        return (value || "")
            .normalize("NFKC")
            .toLocaleLowerCase()
            .replace(/[^\p{L}\p{N}]+/gu, "");
    }

    function getLocalizedValue(element, prefix, language) {
        const languageSuffix = normalizeLanguage(language) === "en" ? "en" : "zhTw";
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
