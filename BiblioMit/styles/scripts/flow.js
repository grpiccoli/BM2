var CryptoJS = require("crypto-js");
const fetch = require("node-fetch");
(async () => {
    const args = process.argv.slice(2);

    const sign = (msg, key) => {
        return CryptoJS.HmacSHA256(msg, key).toString();
    };

    const body = args[2];
    const form = body.replace(/=|&/g, "");
    const s = sign(form, args[1]);

    switch (args[0]) {
        case "sign":
            console.log(s);
            break;
        default:
            await fetch(`https://sandbox.flow.cl/api/${args[0]}`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/x-www-form-urlencoded; charset=utf-8' },
                body: `${body}&s=${s}`
            })
                .then(r => r.json())
                .then(json => console.log(json));
            break;
    };
})();