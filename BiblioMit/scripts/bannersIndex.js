$(".btn-banner").each(function () {
    $(this).attr("url", $(this).attr("href"));
    $(this).removeAttr("href");
});

function select(select, text) {
    var value = select.find("option:contains('" + text + "')").val();
    select.val(value).change();
}

var captionEdit = document.getElementById('caption-edit');
tippy('h1,h5', {
    hideOnClick: 'toggle',
    trigger: 'click',
    allowHTML: true,
    interactive: true,
    content: captionEdit.innerHTML,
    onTrigger: function(instance, event) {
        tippy.hideAll({ exclude: instance });
        var $content = $(captionEdit.innerHTML);
        var caption = $(event.target).closest(".caption");
        console.log(caption)
        $content.find("#Id").val(caption.attr("id")).change();
        $content.find("#Title").val(caption.find("h1").html()).change();
        $content.find("#Subtitle").val(caption.find("h5").html()).change();
        $content.find("#Color").val(rgb2hex(caption.css("color"))).change();
        select($content.find("#Position"), caption.data("position"));
        select($content.find("#Lang"), caption.data("lang"));
        instance.setContent($content[0]);
    }
});

var btnEdit = document.getElementById('btn-edit');
tippy('.btn-banner', {
    hideOnClick: 'toggle',
    trigger: 'click',
    allowHTML: true,
    interactive: true,
    content: btnEdit.innerHTML,
    onTrigger: function(instance, event) {
        tippy.hideAll({ exclude: instance });
        var $content = $(btnEdit.innerHTML);
        $content.find("#Id").val(event.target.id).change();
        $content.find("#Title").val(event.target.innerHTML).change();
        $content.find("#Uri").val(event.target.getAttribute("url")).change();
        instance.setContent($content[0]);
    }
});

var imgEdit = document.getElementById('img-edit');
tippy('#btn-img-edit', {
    hideOnClick: 'toggle',
    trigger: 'click',
    allowHTML: true,
    interactive: true,
    content: imgEdit.innerHTML,
    onTrigger: function(instance, event) {
        tippy.hideAll({ exclude: instance });
    }
});