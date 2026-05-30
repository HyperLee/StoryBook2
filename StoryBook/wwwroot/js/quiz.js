(function () {
    "use strict";

    const page = document.querySelector("[data-quiz-page]");

    if (!page) {
        return;
    }

    const feedback = page.querySelector("[data-quiz-feedback]:not(:empty)");

    if (feedback instanceof HTMLElement) {
        feedback.focus();
    }

    page.querySelectorAll("[data-quiz-answer-form]").forEach((form) => {
        form.addEventListener("submit", () => {
            const submit = form.querySelector("[data-quiz-submit]");

            if (submit instanceof HTMLButtonElement) {
                submit.setAttribute("aria-busy", "true");
                submit.disabled = true;
            }
        });
    });
})();
