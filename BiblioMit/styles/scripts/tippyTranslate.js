var esp = $("html").attr("lang") == "es";
tippy('.title', {
    content: esp ? 'Traduciendo...' : 'Translating...',
    onShow: function(instance) {
        translate(e.target.innerHTML)
            .then(t => instance.setContent(t))
            .catch((e) => instance.setContent(e))
    }
});