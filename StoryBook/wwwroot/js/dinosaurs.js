(function () {
    const supportedLanguages = ["zh-TW", "en"];
    const fallbackLanguage = "zh-TW";

    initializeLanguage();
    initializeSearch();
    initializeImageModal();

    function initializeLanguage() {
        const switcher = document.querySelector("[data-language-storage-key]");
        const storageKey = switcher?.getAttribute("data-language-storage-key") || "storybook.language";
        const storedLanguage = readStoredLanguage(storageKey);

        applyLanguage(storedLanguage, storageKey);

        document.querySelectorAll("[data-language-option]").forEach((button) => {
            button.addEventListener("click", () => {
                const requestedLanguage = normalizeLanguage(button.getAttribute("data-language-option"));
                localStorage.setItem(storageKey, requestedLanguage);
                applyLanguage(requestedLanguage, storageKey);
            });
        });
    }

    function readStoredLanguage(storageKey) {
        try {
            return normalizeLanguage(localStorage.getItem(storageKey));
        } catch {
            return fallbackLanguage;
        }
    }

    function normalizeLanguage(language) {
        return supportedLanguages.includes(language || "") ? language : fallbackLanguage;
    }

    function applyLanguage(language, storageKey) {
        const selectedLanguage = normalizeLanguage(language);
        document.documentElement.lang = selectedLanguage;

        document.querySelectorAll("[data-i18n-zh-tw][data-i18n-en]").forEach((element) => {
            setText(element, selectedLanguage);
        });

        document.querySelectorAll("[data-alt-zh-tw][data-alt-en]").forEach((element) => {
            setAttribute(element, "alt", selectedLanguage, "alt");
        });

        document.querySelectorAll("[data-aria-label-zh-tw][data-aria-label-en]").forEach((element) => {
            setAttribute(element, "aria-label", selectedLanguage, "ariaLabel");
        });

        document.querySelectorAll("[data-placeholder-zh-tw][data-placeholder-en]").forEach((element) => {
            setAttribute(element, "placeholder", selectedLanguage, "placeholder");
        });

        document.querySelectorAll("[data-language-option]").forEach((button) => {
            const isSelected = button.getAttribute("data-language-option") === selectedLanguage;
            button.setAttribute("aria-pressed", String(isSelected));
        });

        try {
            localStorage.setItem(storageKey, selectedLanguage);
        } catch {
            // Browsers can block storage; the page still uses the safe fallback language.
        }
    }

    function setText(element, language) {
        const value = getLocalizedValue(element, "i18n", language);
        if (value) {
            element.textContent = value;
        }
    }

    function setAttribute(element, attributeName, language, prefix) {
        const value = getLocalizedValue(element, prefix, language);
        if (value) {
            element.setAttribute(attributeName, value);
        }
    }

    function getLocalizedValue(element, prefix, language) {
        const languageSuffix = language === "en" ? "en" : "zhTw";
        const fallbackValue = element.dataset[`${prefix}ZhTw`];
        return element.dataset[`${prefix}${capitalize(languageSuffix)}`] || fallbackValue || "";
    }

    function capitalize(value) {
        return value.charAt(0).toUpperCase() + value.slice(1);
    }

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
