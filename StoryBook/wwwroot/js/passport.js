(function () {
    "use strict";

    const storageKey = "storybook.passport";
    const stateVersion = 1;
    const allowedSources = ["dinosaurs", "aquarium"];
    const supportedLanguages = ["zh-TW", "en"];
    const fallbackLanguage = "zh-TW";
    const languageStorageKey = "storybook.language";
    const slugPattern = /^[a-z0-9]+(?:-[a-z0-9]+)*$/;

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", initializePassport);
    } else {
        initializePassport();
    }

    function initializePassport() {
        const detailRegion = document.querySelector("[data-passport-story]");

        if (detailRegion instanceof HTMLElement) {
            initializeDetailCompletion(detailRegion);
        }
    }

    function initializeDetailCompletion(region) {
        const button = region.querySelector("[data-passport-complete]");
        const status = region.querySelector("[data-passport-status]");
        const story = readStory(region);
        let currentLanguage = readStoredLanguage();

        if (!(button instanceof HTMLButtonElement) || !(status instanceof HTMLElement) || !story) {
            return;
        }

        applyLanguage(currentLanguage);
        refreshDetailState("initial");

        button.addEventListener("click", () => {
            const readResult = readPassportState();

            if (readResult.storageStatus === "read-blocked") {
                updateStatus("read-blocked");
                return;
            }

            if (readResult.state.completedStories.some((item) => isSameStory(item, story))) {
                updateStatus("already-completed");
                region.setAttribute("data-passport-completed", "true");
                button.setAttribute("aria-pressed", "true");
                return;
            }

            const nextState = {
                version: stateVersion,
                completedStories: readResult.state.completedStories.concat([{ source: story.source, slug: story.slug }])
            };
            const writeStatus = writePassportState(nextState);

            if (writeStatus === "write-blocked") {
                updateStatus("write-blocked");
                return;
            }

            region.setAttribute("data-passport-completed", "true");
            button.setAttribute("aria-pressed", "true");
            updateStatus("completed");
        });

        document.querySelectorAll("[data-language-option]").forEach((languageButton) => {
            languageButton.addEventListener("click", () => {
                currentLanguage = normalizeLanguage(languageButton.getAttribute("data-language-option"));
                applyLanguage(currentLanguage);
                refreshDetailState("language");
            });
        });

        function refreshDetailState(reason) {
            const readResult = readPassportState();

            if (readResult.storageStatus === "read-blocked") {
                updateStatus("read-blocked");
                return;
            }

            if (readResult.state.completedStories.some((item) => isSameStory(item, story))) {
                region.setAttribute("data-passport-completed", "true");
                button.setAttribute("aria-pressed", "true");
                updateStatus(reason === "language" ? "completed" : "completed");
                return;
            }

            region.setAttribute("data-passport-completed", "false");
            button.setAttribute("aria-pressed", "false");

            if (readResult.storageStatus === "invalid-data") {
                updateStatus("invalid-data");
                return;
            }

            updateStatus("incomplete");
        }

        function updateStatus(statusCode) {
            const message = getRegionMessage(region, statusCode, currentLanguage, story.name);

            if (message) {
                status.textContent = message;
            }

            region.setAttribute("data-passport-storage-status", statusCode);
        }

        function applyLanguage(language) {
            const selectedLanguage = normalizeLanguage(language);
            document.documentElement.lang = selectedLanguage;

            region.querySelectorAll("[data-i18n-zh-tw][data-i18n-en]").forEach((element) => {
                const value = getLocalizedAttribute(element, "i18n", selectedLanguage);

                if (value) {
                    element.textContent = value;
                }
            });

            region.querySelectorAll("[data-aria-label-zh-tw][data-aria-label-en]").forEach((element) => {
                const value = getLocalizedAttribute(element, "aria-label", selectedLanguage);

                if (value) {
                    element.setAttribute("aria-label", value);
                }
            });
        }
    }

    function readStory(region) {
        const source = region.getAttribute("data-passport-source") || "";
        const slug = region.getAttribute("data-passport-slug") || "";

        if (!allowedSources.includes(source) || !slugPattern.test(slug)) {
            return null;
        }

        return {
            source,
            slug,
            name: getLocalizedAttribute(region, "passport-name", readStoredLanguage()) || slug
        };
    }

    function readPassportState(knownStories) {
        let rawValue = null;

        try {
            rawValue = window.localStorage.getItem(storageKey);
        } catch {
            return {
                storageStatus: "read-blocked",
                state: createEmptyState(),
                ignoredItemCount: 0
            };
        }

        if (!rawValue) {
            return {
                storageStatus: "available",
                state: createEmptyState(),
                ignoredItemCount: 0
            };
        }

        let parsedValue = null;

        try {
            parsedValue = JSON.parse(rawValue);
        } catch {
            return {
                storageStatus: "invalid-data",
                state: createEmptyState(),
                ignoredItemCount: 0
            };
        }

        const normalized = normalizeState(parsedValue, knownStories);

        return {
            storageStatus: normalized.isValidShape && normalized.ignoredItemCount === 0 ? "available" : "invalid-data",
            state: normalized.state,
            ignoredItemCount: normalized.ignoredItemCount
        };
    }

    function writePassportState(state) {
        const normalized = normalizeState(state);

        try {
            window.localStorage.setItem(storageKey, JSON.stringify({
                version: stateVersion,
                completedStories: normalized.state.completedStories
            }));
        } catch {
            return "write-blocked";
        }

        return "available";
    }

    function normalizeState(value, knownStories) {
        if (!value
            || typeof value !== "object"
            || Array.isArray(value)
            || value.version !== stateVersion
            || !Array.isArray(value.completedStories)) {
            return {
                isValidShape: false,
                state: createEmptyState(),
                ignoredItemCount: 0
            };
        }

        const completedStories = [];
        const seenKeys = new Set();
        let ignoredItemCount = 0;

        value.completedStories.forEach((item) => {
            const normalizedItem = normalizeCompletedItem(item, knownStories);

            if (!normalizedItem) {
                ignoredItemCount += 1;
                return;
            }

            const key = `${normalizedItem.source}:${normalizedItem.slug}`;

            if (seenKeys.has(key)) {
                ignoredItemCount += 1;
                return;
            }

            seenKeys.add(key);
            completedStories.push(normalizedItem);
        });

        return {
            isValidShape: true,
            state: {
                version: stateVersion,
                completedStories
            },
            ignoredItemCount
        };
    }

    function normalizeCompletedItem(item, knownStories) {
        if (!item || typeof item !== "object" || Array.isArray(item)) {
            return null;
        }

        const source = typeof item.source === "string" ? item.source.trim().toLowerCase() : "";
        const slug = typeof item.slug === "string" ? item.slug.trim().toLowerCase() : "";

        if (!allowedSources.includes(source) || !slugPattern.test(slug)) {
            return null;
        }

        if (knownStories instanceof Map && !knownStories.has(`${source}:${slug}`)) {
            return null;
        }

        return { source, slug };
    }

    function createEmptyState() {
        return {
            version: stateVersion,
            completedStories: []
        };
    }

    function isSameStory(left, right) {
        return left.source === right.source && left.slug === right.slug;
    }

    function getRegionMessage(region, statusCode, language, storyName) {
        const value = getLocalizedAttribute(region, `passport-status-${statusCode}`, language);
        return (value || "").replace("{0}", storyName);
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
})();
