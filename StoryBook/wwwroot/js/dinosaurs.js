(function () {
    initializeSearch();
    initializeImageModal();

    function initializeSearch() {
        const input = document.querySelector("[data-dino-search-input]");
        const items = Array.from(document.querySelectorAll("[data-dino-search-item]"));
        const noResults = document.querySelector("[data-dino-no-results]");
        const clearButton = document.querySelector("[data-dino-clear-search]");

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
            const query = normalize(input.value);
            let visibleCount = 0;

            items.forEach((item) => {
                const text = normalize(item.getAttribute("data-dino-search-text"));
                const isVisible = query.length === 0 || text.includes(query);
                item.hidden = !isVisible;

                if (isVisible) {
                    visibleCount += 1;
                }
            });

            if (noResults instanceof HTMLElement) {
                noResults.hidden = visibleCount > 0;
            }
        }
    }

    function initializeImageModal() {
    let activeModal = null;
    let activeTrigger = null;
    let activeBackdrop = null;

    const openButtons = document.querySelectorAll("[data-dino-modal-open]");

    openButtons.forEach((button) => {
        button.addEventListener("click", () => {
            const modalId = button.getAttribute("data-dino-modal-open");
            const modal = modalId ? document.getElementById(modalId) : null;

            if (modal) {
                openModal(modal, button);
            }
        });
    });

    document.querySelectorAll("[data-dino-modal-close]").forEach((button) => {
        button.addEventListener("click", closeModal);
    });

    document.querySelectorAll("[data-dino-modal]").forEach((modal) => {
        modal.addEventListener("click", (event) => {
            if (event.target === modal) {
                closeModal();
            }
        });
    });

    function openModal(modal, trigger) {
        activeModal = modal;
        activeTrigger = trigger;
        activeBackdrop = document.createElement("div");
        activeBackdrop.className = "modal-backdrop fade show";
        activeBackdrop.setAttribute("data-dino-modal-backdrop", "true");
        activeBackdrop.addEventListener("click", closeModal);
        document.body.appendChild(activeBackdrop);

        modal.style.display = "block";
        modal.removeAttribute("aria-hidden");
        modal.setAttribute("aria-modal", "true");
        modal.classList.add("show");
        document.body.classList.add("modal-open");
        document.addEventListener("keydown", handleKeydown);

        const closeButton = modal.querySelector("[data-dino-modal-close]");
        if (closeButton instanceof HTMLElement) {
            closeButton.focus();
        }
    }

    function closeModal() {
        if (!activeModal) {
            return;
        }

        activeModal.classList.remove("show");
        activeModal.style.display = "none";
        activeModal.setAttribute("aria-hidden", "true");
        activeModal.removeAttribute("aria-modal");
        document.body.classList.remove("modal-open");
        document.removeEventListener("keydown", handleKeydown);

        if (activeBackdrop) {
            activeBackdrop.remove();
            activeBackdrop = null;
        }

        if (activeTrigger instanceof HTMLElement) {
            activeTrigger.focus();
        }

        activeModal = null;
        activeTrigger = null;
    }

    function handleKeydown(event) {
        if (event.key === "Escape") {
            closeModal();
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
