document.addEventListener("DOMContentLoaded", function () {
    var modal = document.getElementById('modal-action');
    if (modal) {
        var content = modal.querySelector('.modal-content');
        if (content) {
            modal.addEventListener('show.bs.modal', function (event: any) {
                var button = event.relatedTarget; // Button that triggered the modal
                var url = button.href;
                // note that this will replace the content of modal-content everytime the modal is opened
                fetch(url)
                    .then(function (response) {
                        return response.text();
                    })
                    .then(function (body) {
                        content.innerHTML = body;
                    });
            });
            // when the modal is closed
            modal.addEventListener('hidden.bs.modal', function () {
                // remove the bs.modal data attribute from it
                $(modal).removeData('bs.modal');
                // and empty the modal-content element
                $(content).empty();
            });
            $(modal).change(_ => {
                $.validator.unobtrusive.parse('form#modal-form');
            });
        }
    }
});