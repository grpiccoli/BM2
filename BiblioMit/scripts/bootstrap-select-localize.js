﻿! function (e, o) {
    void 0 === e && void 0 !== window && (e = window), "function" == typeof define && define.amd ? define(["jquery"], function (e) {
        return o(e)
    }) : "object" == typeof module && module.exports ? module.exports = o(require("jquery")) : o(e.jQuery)
}(this, function (e) {
    var lang = $("html").attr("lang");
    if(lang == "es")
        e.fn.selectpicker.defaults = {
            noneSelectedText: "No hay selecci\xf3n",
            noneResultsText: "No hay resultados {0}",
            countSelectedText: "Seleccionados {0} de {1}",
            maxOptionsText: ["L\xedmite alcanzado ({n} {var} max)", "L\xedmite del grupo alcanzado({n} {var} max)", ["elementos", "element"]],
            multipleSeparator: ", ",
            selectAllText: "Seleccionar Todos",
            deselectAllText: "Desmarcar Todos"
        }
});