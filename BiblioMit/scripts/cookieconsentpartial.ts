(function () {
    document
        .querySelector("#cookieConsent button[data-cookie-string]")
        .addEventListener("click", function (_e) {
            document.cookie = this.dataset.cookieString;
    }, false);
})();