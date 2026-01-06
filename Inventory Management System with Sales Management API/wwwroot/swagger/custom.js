(function () {
    const TOKEN_KEY = "swagger_jwt_token";

    // Intercept Swagger XHR calls
    const open = XMLHttpRequest.prototype.open;
    XMLHttpRequest.prototype.open = function () {
        this.addEventListener("load", function () {
            try {
                if (this.responseURL.includes("/api/User/Login")) {
                    const res = JSON.parse(this.responseText);
                    if (res.token) {
                        localStorage.setItem(TOKEN_KEY, res.token);
                        console.log("JWT saved from login");
                    }
                }
            } catch (e) { }
        });
        open.apply(this, arguments);
    };

    // Wait until Swagger UI is ready
    const timer = setInterval(() => {
        if (window.ui) {
            const token = localStorage.getItem(TOKEN_KEY);
            if (token) {
                window.ui.preauthorizeApiKey("Bearer", token);
                console.log("JWT auto-filled in Swagger");
            }
            clearInterval(timer);
        }
    }, 500);
})();
