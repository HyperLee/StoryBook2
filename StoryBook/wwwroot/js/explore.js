(function () {
    initializeSearch();

    function initializeSearch() {
        const input = document.querySelector("[data-explore-search-input]");
        const clearButton = document.querySelector("[data-explore-clear-search]");
        const status = document.querySelector("[data-explore-result-status]");
        const tooShort = document.querySelector("[data-explore-too-short]");
        const noResults = document.querySelector("[data-explore-no-results]");
        const items = Array.from(document.querySelectorAll("[data-explore-result-item]"));

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

        updateResults();

        function updateResults() {
            const rawQuery = input.value;
            const query = normalize(rawQuery);
            const hasTypedText = rawQuery.trim().length > 0;
            const isTooShort = hasTypedText && query.length < 2;
            let visibleCount = 0;

            items.forEach((item) => {
                const searchText = normalize(item.getAttribute("data-explore-search-text"));
                const isVisible = query.length === 0 || isTooShort || searchText.includes(query);
                item.hidden = !isVisible;

                if (isVisible) {
                    visibleCount += 1;
                }
            });

            setHidden(tooShort, !isTooShort);
            setHidden(noResults, isTooShort || query.length === 0 || visibleCount > 0);
            updateStatus(status, query.length > 0 && !isTooShort, visibleCount);
        }
    }

    function updateStatus(status, hasSearch, visibleCount) {
        if (!(status instanceof HTMLElement)) {
            return;
        }

        const template = hasSearch
            ? status.getAttribute("data-search-template-zh-tw")
            : status.getAttribute("data-all-template-zh-tw");
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
