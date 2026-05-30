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
        const passportPage = document.querySelector("[data-passport-page]");

        if (detailRegion instanceof HTMLElement) {
            initializeDetailCompletion(detailRegion);
        }

        if (passportPage instanceof HTMLElement) {
            initializePassportPage(passportPage);
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

    function initializePassportPage(page) {
        const storyLookup = readStoryLookup();
        const badgeElements = Array.from(page.querySelectorAll("[data-passport-badge]"));
        const readList = page.querySelector("[data-passport-read-list]");
        const summary = page.querySelector("[data-passport-summary]");
        const emptyState = page.querySelector("[data-passport-empty]");
        const storageWarning = page.querySelector("[data-passport-storage-warning]");
        let currentLanguage = readStoredLanguage();

        if (!(readList instanceof HTMLOListElement) || !(summary instanceof HTMLElement)) {
            return;
        }

        applyPassportPageLanguage(currentLanguage);
        renderPassportPage();

        document.querySelectorAll("[data-language-option]").forEach((languageButton) => {
            languageButton.addEventListener("click", () => {
                currentLanguage = normalizeLanguage(languageButton.getAttribute("data-language-option"));

                try {
                    window.localStorage.setItem(languageStorageKey, currentLanguage);
                } catch {
                    // Storage can be blocked; the page still applies the selected language.
                }

                applyPassportPageLanguage(currentLanguage);
                renderPassportPage();
            });
        });

        function renderPassportPage() {
            const readResult = readPassportState(storyLookup);
            const completedStories = sortCompletedStories(readResult.state.completedStories, storyLookup);
            const totalCount = storyLookup.size;

            updateSummary(completedStories.length, totalCount);
            updateStorageWarning(readResult);
            renderReadList(completedStories);
            renderBadges(completedStories, totalCount);

            if (emptyState instanceof HTMLElement) {
                emptyState.hidden = completedStories.length > 0;
            }
        }

        function updateSummary(completedCount, totalCount) {
            const template = getLocalizedAttribute(page, "passport-summary-template", currentLanguage);
            const fallback = currentLanguage === "en"
                ? `You have met ${completedCount} of ${totalCount} story friends.`
                : `你已經認識 ${completedCount} / ${totalCount} 位故事朋友。`;
            summary.textContent = template
                ? template.replace("{0}", String(completedCount)).replace("{1}", String(totalCount))
                : fallback;
        }

        function updateStorageWarning(readResult) {
            if (!(storageWarning instanceof HTMLElement)) {
                return;
            }

            const shouldShowWarning = readResult.storageStatus === "read-blocked" || readResult.storageStatus === "invalid-data";
            storageWarning.hidden = !shouldShowWarning;

            if (shouldShowWarning) {
                storageWarning.textContent = getLocalizedAttribute(page, `passport-storage-warning-${readResult.storageStatus}`, currentLanguage);
            }
        }

        function renderReadList(completedStories) {
            readList.replaceChildren();

            completedStories.forEach((completedStory) => {
                const story = storyLookup.get(`${completedStory.source}:${completedStory.slug}`);

                if (!story) {
                    return;
                }

                const item = document.createElement("li");
                const link = document.createElement("a");
                const source = document.createElement("span");
                const summaryText = document.createElement("p");

                link.href = story.href;
                link.textContent = getStoryText(story, "name", currentLanguage);
                source.className = "passport-read-list__source";
                source.textContent = getStoryText(story, "source", currentLanguage);
                summaryText.textContent = getStoryText(story, "summary", currentLanguage);

                item.append(link, source, summaryText);
                readList.append(item);
            });
        }

        function renderBadges(completedStories, totalCount) {
            const completedKeys = new Set(completedStories.map((story) => `${story.source}:${story.slug}`));

            badgeElements.forEach((badgeElement) => {
                if (!(badgeElement instanceof HTMLElement)) {
                    return;
                }

                const label = badgeElement.querySelector("[data-passport-badge-label]");
                const description = badgeElement.querySelector("[data-passport-badge-description]");
                const status = badgeElement.querySelector("[data-passport-badge-status]");
                const isUnlocked = isBadgeUnlocked(badgeElement, completedKeys, completedStories.length, totalCount);

                badgeElement.setAttribute("data-passport-badge-state", isUnlocked ? "unlocked" : "locked");

                if (label instanceof HTMLElement) {
                    label.textContent = getLocalizedAttribute(badgeElement, "i18n-label", currentLanguage);
                }

                if (description instanceof HTMLElement) {
                    description.textContent = getLocalizedAttribute(badgeElement, "i18n-description", currentLanguage);
                }

                if (status instanceof HTMLElement) {
                    status.textContent = getLocalizedAttribute(status, isUnlocked ? "i18n-unlocked" : "i18n-locked", currentLanguage);
                }
            });
        }
    }

    function readStoryLookup() {
        const stories = new Map();

        document.querySelectorAll("[data-passport-story-item]").forEach((element) => {
            if (!(element instanceof HTMLElement)) {
                return;
            }

            const source = element.getAttribute("data-passport-source") || "";
            const slug = element.getAttribute("data-passport-slug") || "";
            const id = `${source}:${slug}`;

            if (!allowedSources.includes(source) || !slugPattern.test(slug)) {
                return;
            }

            stories.set(id, {
                id,
                source,
                slug,
                href: element.getAttribute("data-passport-href") || "#",
                sourceOrder: Number.parseInt(element.getAttribute("data-passport-source-order") || "0", 10),
                storyOrder: Number.parseInt(element.getAttribute("data-passport-story-order") || "0", 10),
                element
            });
        });

        return stories;
    }

    function sortCompletedStories(completedStories, storyLookup) {
        return completedStories
            .filter((story) => storyLookup.has(`${story.source}:${story.slug}`))
            .sort((left, right) => {
                const leftStory = storyLookup.get(`${left.source}:${left.slug}`);
                const rightStory = storyLookup.get(`${right.source}:${right.slug}`);

                return (leftStory?.sourceOrder || 0) - (rightStory?.sourceOrder || 0)
                    || (leftStory?.storyOrder || 0) - (rightStory?.storyOrder || 0)
                    || `${left.source}:${left.slug}`.localeCompare(`${right.source}:${right.slug}`);
            });
    }

    function isBadgeUnlocked(badgeElement, completedKeys, completedCount, totalCount) {
        const milestone = badgeElement.getAttribute("data-passport-badge-milestone");

        if (milestone === "CompletedCountAtLeast") {
            const targetCount = Number.parseInt(badgeElement.getAttribute("data-passport-target-count") || "0", 10);
            return completedCount >= targetCount;
        }

        if (milestone === "CompletedAllInSource") {
            const source = badgeElement.getAttribute("data-passport-source") || "";
            const sourceStories = Array.from(document.querySelectorAll(`[data-passport-story-item][data-passport-source="${source}"]`));
            return sourceStories.length > 0 && sourceStories.every((storyElement) => {
                const slug = storyElement.getAttribute("data-passport-slug") || "";
                return completedKeys.has(`${source}:${slug}`);
            });
        }

        if (milestone === "CompletedAllStories") {
            return totalCount > 0 && completedCount === totalCount;
        }

        return false;
    }

    function applyPassportPageLanguage(language) {
        const selectedLanguage = normalizeLanguage(language);
        document.documentElement.lang = selectedLanguage;

        document.querySelectorAll("[data-i18n-zh-tw][data-i18n-en]").forEach((element) => {
            const value = getLocalizedAttribute(element, "i18n", selectedLanguage);

            if (value) {
                element.textContent = value;
            }
        });

        document.querySelectorAll("[data-aria-label-zh-tw][data-aria-label-en]").forEach((element) => {
            const value = getLocalizedAttribute(element, "aria-label", selectedLanguage);

            if (value) {
                element.setAttribute("aria-label", value);
            }
        });
    }

    function getStoryText(story, fieldName, language) {
        return getLocalizedAttribute(story.element, `i18n-${fieldName}`, language);
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
