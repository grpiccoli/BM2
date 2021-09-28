$('td[contenteditable=true]').focus(function () {
    $(this).data("initialText", $(this).html());
}).blur(function () {
    if ($(this).data("initialText") !== $(this).html()) {
        var $cell = $(this).parent();
        var id = $cell.attr('id');
        var values = [];
        $cell.children('td').each(function () {
            values.push($(this).html().replace(/^\s*/, '').replace(/\s*$/, ''));
        });
        var url = $("#constrainer2").data('url');
        url = url.replace("P0", id).replace("A1", values[2]).replace("C2", values[3])
            .replace("C3", values[4]).replace("C4", values[5]).replace("C5", values[6]).replace("C6", values[7]);
        $.post(url, { "__RequestVerificationToken": $("input[name='__RequestVerificationToken']").val() }, function (r) {
            alert('cambios guardados en la base de datos. Presione Ctrl+Z para deshacer');
        });
    }
});
//# sourceMappingURL=columnasColumnas.js.map