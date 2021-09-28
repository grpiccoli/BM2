var availableDate: any = Array.from($(".tableFilter.calendarFilter").map(function () {
    return this.id
}));

$('.tableFilter.calendarFilter').datepicker({
    format: "dd-mm-yy",
    endDate: "today",
    todayBtn: "linked",
    clearBtn: true,
    language: "es",
    daysOfWeekHighlighted: "0,6",
    todayHighlight: true,
    beforeShowDay: function (date) {
        var ymd = date.getFullYear() + "-" + (date.getMonth() + 1) + "-" + date.getDate();
        if ($.inArray(ymd, availableDate) !== -1) {
            return "green";
        } else {
            return false;
        }
    }
});

$(".calendarShow").click(function () { $(this).datepicker('show'); });

function tableFilter(v: any) {
    var data = document.getElementById("#filter").dataset;
    var tag = v.tagName;
    var val = data.val;
    var url = data.url;
    var name = $(v).attr('id');
    if (tag === "DIV") {
        url = url.replace("SRT", name);
        url = url.replace("JSVAL", val);
    } else {
        url = url.replace("SRT", data.srt);
        var obj: any = {};
        if (val !== "") {
            var filters = val.split(';');
            filters.forEach(function (filter) {
                var pair = filter.split(':');
                obj[pair[0]] = pair[1];
            });
        }
        console.log(val);
        if (tag === "INPUT") {
            if ($(v).attr("type") === "checkbox") {
                obj[name] = $(v).first().val();
            } else if ($(v).hasClass('hasDatepicker')) {
                obj[name] = $(v).first().val();
                //url = url.replace("JSVAL", $(v)[0].value);
            } else {
                obj[name] = $(v).val();
                //url = url.replace("JSVAL", name + ":" + $(v).value);
            }
        } else if (tag === "SELECT") {
            obj[name] = $(v).find(":selected").map(function () { return $(this).val() }).get().join(',');
            //url = url.replace("JSVAL", $(v).find(":selected").map(function () { return this.value }).get().join(","));
        }
        var out: any = [];
        $.each(obj, function (index, value) {
            out.push(index.toString() + ':' + value);
        });
        url = url.replace("JSVAL", out.join(';'));
    }
    window.location.href = url;
}

$(".tableFilter.calendarFilter,.dropdownFilter,.checkboxFilter").change(tableFilter);

$(".sortingFilter").click(tableFilter);