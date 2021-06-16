async function translate(text) {
    var lang = document.querySelector("html").getAttribute("lang");
    var token = document.querySelector("input[name='__RequestVerificationToken']");
    if (lang && token) {
        var data = {
            text: text.replace(/[\n\r]+/g, " ").replace(/&nbsp;/g, " "),
            to: lang,
            '__RequestVerificationToken': token.value
        };
        return await fetch('/home/translate', {
            method: 'post',
            headers: new Headers({ 'Content-Type': 'application/x-www-form-urlencoded;charset=UTF-8' }),
            body: JSON.stringify(data),
        }).then(r => r.text());
    }
}
$("#search").submit(_ => {
    if ($("#src").val() === "") {
        alert("Seleccione a lo menos 1 repositorio para la búsqueda");
        return false;
    }
    if ($("#q").val() === "") {
        alert("Ingrese algún termino para la búsqueda");
        return false;
    }
    else
        true;
});
function empty() {
    var x = $("#fondos").val();
    if (x === "") {
        alert("Seleccione al menos una fuente de fondos concursables a buscar");
        return false;
    }
    var y = $("#estado").val();
    if (y === "") {
        alert("Seleccione al menos un estado de fondo para la búsqueda");
        return false;
    }
}
//# sourceMappingURL=bibliomit.js.map