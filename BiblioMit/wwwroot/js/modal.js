document.addEventListener("DOMContentLoaded", function () {
    var modal = document.getElementById('modal-action');
    if (modal) {
        var content = modal.querySelector('.modal-content');
        if (content) {
            modal.addEventListener('show.bs.modal', function (event) {
                var button = event.relatedTarget;
                var url = button.href;
                fetch(url)
                    .then(function (response) {
                    return response.text();
                })
                    .then(function (body) {
                    content.innerHTML = body;
                });
            });
            modal.addEventListener('hidden.bs.modal', function () {
                $(modal).removeData('bs.modal');
                $(content).empty();
            });
            $(modal).change(_ => {
                $.validator.unobtrusive.parse('form#modal-form');
            });
        }
    }
});
//# sourceMappingURL=modal.js.map