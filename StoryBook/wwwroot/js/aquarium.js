(function () {
    initializeSearch();

    function initializeSearch() {
        const input = document.querySelector("[data-aquarium-search-input]");
        const items = Array.from(document.querySelectorAll("[data-aquarium-search-item]"));
        const noResults = document.querySelector("[data-aquarium-no-results]");
        const tooShort = document.querySelector("[data-aquarium-too-short]");
        const clearButton = document.querySelector("[data-aquarium-clear-search]");

        if (!(input instanceof HTMLInputElement) || items.length === 0) {
            return;
        }

        input.addEventListener("input", updateSearchResults);

        if (clearButton instanceof HTMLButtonElement) {
            clearButton.addEventListener("click", () => {
                input.value = "";
                updateSearchResults();
                input.focus();
            });
        }

        updateSearchResults();

        function updateSearchResults() {
            const rawQuery = input.value;
            const query = normalize(rawQuery);
            const hasTypedText = rawQuery.trim().length > 0;
            const isTooShort = hasTypedText && query.length < 2;
            let visibleCount = 0;

            items.forEach((item) => {
                const text = normalize(item.getAttribute("data-aquarium-search-text"));
                const isVisible = query.length === 0 || isTooShort || text.includes(query);
                item.hidden = !isVisible;

                if (isVisible) {
                    visibleCount += 1;
                }
            });

            if (tooShort instanceof HTMLElement) {
                tooShort.hidden = !isTooShort;
            }

            if (noResults instanceof HTMLElement) {
                noResults.hidden = isTooShort || query.length === 0 || visibleCount > 0;
            }
        }
    }

    function normalize(value) {
        return (value || "")
            .normalize("NFKC")
            .toLocaleLowerCase()
            .replace(/[^\p{L}\p{N}]+/gu, "");
    }
})();
