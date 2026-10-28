document.addEventListener("submit", function (event) {
    const form = event.target;
    const message = form.getAttribute("data-confirm");
    if (message && !window.confirm(message)) event.preventDefault();
});
document.querySelectorAll(".alert").forEach((alert) => {
    window.setTimeout(() => {
        if (alert.classList.contains("show")) {
            alert.classList.remove("show");
            window.setTimeout(() => alert.remove(), 250);
        }
    }, 6000);
});
