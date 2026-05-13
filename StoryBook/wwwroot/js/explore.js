(function () {
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

        if (!(input instanceof HTMLInputElement) || items.length === 0) {
            return;
        }

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
            updateStatus(status, getResultMode(query.length > 0 && !isTooShort, hasFilters), visibleCount);
        }
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

    function updateStatus(status, mode, visibleCount) {
        if (!(status instanceof HTMLElement)) {
            return;
        }

        const template = status.getAttribute(`data-${mode}-template-zh-tw`) || status.getAttribute("data-all-template-zh-tw");
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
})();
