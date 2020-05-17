async function translate(text: string) {
    var lang = $("html").attr("lang");
    var data = {
        text: text.replace(/[\n\r]+/g, " ").replace(/&nbsp;/g, " "),
        to: lang,
        '__RequestVerificationToken': $("input[name='__RequestVerificationToken']").val() };
    return await fetch('/home/translate', {
        method: 'post',
        headers: new Headers({ 'Content-Type': 'application/x-www-form-urlencoded;charset=UTF-8' }),
        body: JSON.stringify(data),
    }).then(r => r.text());
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
    else true;
});
//$(document).ready(function () {
//    $(".title").tooltip(function () {
//        $.post("/home/translate?text=" + $(this).html(), {},
//            function (response) {
//                $(this)
//                    .tooltip('hide')
//                    .attr('data-original-title', response)
//                    //.tooltip('fixTitle')
//                    .tooltip('show');
//            });
//    });
//    $(".title").mouseout(function () {
//        $(this).tooltip();
//        $('.ui-tooltip').hide();
//    });
//});
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