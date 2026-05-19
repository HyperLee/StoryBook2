(function () {
    "use strict";

    const supportedLanguages = ["zh-TW", "en"];
    const fallbackLanguage = "zh-TW";
    const languageStorageKey = "storybook.language";
    const fieldCodes = [
        "source",
        "name",
        "diet",
        "living-area",
        "period",
        "discovery-location",
        "summary",
        "detail-link"
    ];

    initializeCompare();

    function initializeCompare() {
        const page = document.querySelector("[data-compare-preserve-state-on-theme-change]");
        const firstSelect = document.querySelector("[data-compare-first-select]");
        const secondSelect = document.querySelector("[data-compare-second-select]");
        const clearButton = document.querySelector("[data-compare-clear-selection]");
        const table = document.querySelector("[data-compare-table]");
        const status = document.querySelector("[data-compare-status]");
        const oneSelectedMessage = document.querySelector("[data-compare-one-selected-message]");
        const duplicateMessage = document.querySelector("[data-compare-duplicate-message]");
        const notEnoughMessage = document.querySelector("[data-compare-not-enough]");
        const readyMessage = document.querySelector("[data-compare-ready-message]");
        const candidates = readCandidates();
        let currentLanguage = readStoredLanguage();

        if (!(page instanceof HTMLElement)
            || !(firstSelect instanceof HTMLSelectElement)
            || !(secondSelect instanceof HTMLSelectElement)) {
            return;
        }

        applyLanguage(currentLanguage);

        document.querySelectorAll("[data-language-option]").forEach((button) => {
            button.addEventListener("click", () => {
                currentLanguage = normalizeLanguage(button.getAttribute("data-language-option"));
                applyLanguage(currentLanguage);
                updateComparison();
            });
        });

        firstSelect.addEventListener("change", updateComparison);
        secondSelect.addEventListener("change", updateComparison);

        if (clearButton instanceof HTMLButtonElement) {
            clearButton.addEventListener("click", () => {
                firstSelect.value = "";
                secondSelect.value = "";
                updateComparison();
                firstSelect.focus();
            });
        }

        updateComparison();

        function updateComparison() {
            const firstId = firstSelect.value;
            const secondId = secondSelect.value;
            const firstCandidate = candidates.get(firstId);
            const secondCandidate = candidates.get(secondId);

            if (candidates.size < 2) {
                showOnly(notEnoughMessage);
                setHidden(table, true);
                return;
            }

            if (!firstId && !secondId) {
                showOnly(status);
                setHidden(table, true);
                return;
            }

            if (!firstId || !secondId) {
                showOnly(oneSelectedMessage);
                setHidden(table, true);
                return;
            }

            if (firstId === secondId) {
                showOnly(duplicateMessage);
                setHidden(table, true);
                return;
            }

            if (!firstCandidate || !secondCandidate) {
                showOnly(status);
                setHidden(table, true);
                return;
            }

            renderComparison(firstCandidate, secondCandidate);
            showOnly(readyMessage);
            setHidden(table, false);
        }

        function showOnly(activeMessage) {
            [status, oneSelectedMessage, duplicateMessage, notEnoughMessage, readyMessage].forEach((message) => {
                setHidden(message, message !== activeMessage);
            });
        }

        function renderComparison(firstCandidate, secondCandidate) {
            fieldCodes.forEach((fieldCode) => {
                const row = document.querySelector(`[data-compare-field="${fieldCode}"]`);

                if (!(row instanceof HTMLTableRowElement)) {
                    return;
                }

                const firstCell = row.querySelector("[data-compare-first-value]");
                const secondCell = row.querySelector("[data-compare-second-value]");
                replaceValue(firstCell, createValueNode(firstCandidate, fieldCode));
                replaceValue(secondCell, createValueNode(secondCandidate, fieldCode));
            });
        }

        function createValueNode(candidate, fieldCode) {
            if (fieldCode === "detail-link") {
                const link = document.createElement("a");
                link.href = candidate.detailHref;
                link.textContent = candidate.detailText;
                return link;
            }

            return document.createTextNode(candidate[fieldCode] || "");
        }

        function replaceValue(cell, node) {
            if (cell instanceof HTMLElement) {
                cell.replaceChildren(node);
            }
        }

        function applyLanguage(language) {
            const selectedLanguage = normalizeLanguage(language);
            document.documentElement.lang = selectedLanguage;

            document.querySelectorAll("[data-i18n-zh-tw][data-i18n-en]").forEach((element) => {
                setElementText(element, getLocalizedAttribute(element, "i18n", selectedLanguage));
            });

            document.querySelectorAll("[data-aria-label-zh-tw][data-aria-label-en]").forEach((element) => {
                setElementAttribute(element, "aria-label", getLocalizedAttribute(element, "aria-label", selectedLanguage));
            });

            document.querySelectorAll("[data-language-option]").forEach((button) => {
                const isSelected = button.getAttribute("data-language-option") === selectedLanguage;
                button.setAttribute("aria-pressed", String(isSelected));
            });

            updateCandidateOptions(firstSelect, selectedLanguage);
            updateCandidateOptions(secondSelect, selectedLanguage);
            refreshLocalizedCandidates(candidates, selectedLanguage);
        }
    }

    function readCandidates() {
        const candidates = new Map();

        document.querySelectorAll("[data-compare-candidate]").forEach((element) => {
            if (!(element instanceof HTMLElement)) {
                return;
            }

            const id = element.getAttribute("data-candidate-id") || "";

            if (!id) {
                return;
            }

            candidates.set(id, createCandidate(element, fallbackLanguage));
        });

        return candidates;
    }

    function createCandidate(element, language) {
        const selectedLanguage = normalizeLanguage(language);

        return {
            source: getLocalizedAttribute(element, "source", selectedLanguage),
            name: getLocalizedAttribute(element, "name", selectedLanguage),
            diet: getLocalizedAttribute(element, "diet", selectedLanguage),
            "living-area": getLocalizedAttribute(element, "living-area", selectedLanguage),
            period: getLocalizedAttribute(element, "period", selectedLanguage),
            "discovery-location": getLocalizedAttribute(element, "discovery-location", selectedLanguage),
            summary: getLocalizedAttribute(element, "summary", selectedLanguage),
            detailText: getLocalizedAttribute(element, "detail-text", selectedLanguage),
            detailHref: element.getAttribute("data-detail-href") || "#"
        };
    }

    function refreshLocalizedCandidates(candidates, language) {
        document.querySelectorAll("[data-compare-candidate]").forEach((element) => {
            if (!(element instanceof HTMLElement)) {
                return;
            }

            const id = element.getAttribute("data-candidate-id") || "";

            if (id && candidates.has(id)) {
                candidates.set(id, createCandidate(element, language));
            }
        });
    }

    function updateCandidateOptions(select, language) {
        Array.from(select.options).forEach((option) => {
            const label = getLocalizedAttribute(option, "option-name", language);

            if (label) {
                option.textContent = label;
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

    function getLocalizedAttribute(element, prefix, language) {
        const suffix = normalizeLanguage(language) === "en" ? "en" : "zh-tw";
        return element.getAttribute(`data-${prefix}-${suffix}`)
            || element.getAttribute(`data-${prefix}-zh-tw`)
            || "";
    }

    function setElementText(element, value) {
        if (value) {
            element.textContent = value;
        }
    }

    function setElementAttribute(element, attributeName, value) {
        if (value) {
            element.setAttribute(attributeName, value);
        }
    }

    function setHidden(element, hidden) {
        if (element instanceof HTMLElement) {
            element.hidden = hidden;
        }
    }
})();
