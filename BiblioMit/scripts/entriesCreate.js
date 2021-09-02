$(document).ready(function () {
    $("#DeclarationType").change(function () {
        $('#tipo').html(this.value);
        $('.btnSubir').removeClass('d-none');
    });
    // Step show event
    $("#smartwizard").on("showStep", function (_e, _anchorObject, _stepNumber, _stepDirection, stepPosition) {
        //alert("You are on step "+stepNumber+" now");
        if (stepPosition === 'first') {
            $("#prev-btn").addClass('disabled');
        } else if (stepPosition === 'final') {
            $("#next-btn").addClass('disabled');
        } else {
            $("#prev-btn").removeClass('disabled');
            $("#next-btn").removeClass('disabled');
        }
    });

    // Toolbar extra buttons
    var btnFinish = $('<button></button>').text('Subir')
        .addClass('btn btn-info d-none btnSubir')
        .on('click', function () { $('#formCreate').submit(); });

    // Smart Wizard
    $('#smartwizard').smartWizard({
        selected: 0,
        theme: 'default',
        transitionEffect: 'fade',
        showStepURLhash: false,
        lang: {
            next: 'Siguiente',
            previous: 'Anterior'
        },
        toolbarSettings: {
            toolbarPosition: 'both',
            toolbarButtonPosition: 'end',
            toolbarExtraButtons: [btnFinish]
        }
    });

    // External Button Events
    $("#reset-btn").on("click", function () {
        // Reset wizard
        $('#smartwizard').smartWizard("reset");
        return true;
    });

    $("#prev-btn").on("click", function () {
        // Navigate previous
        $('#smartwizard').smartWizard("prev");
        return true;
    });

    $("#next-btn").on("click", function () {
        // Navigate next
        $('#smartwizard').smartWizard("next");
        return true;
    });

    // Set selected theme on page refresh
    $("#theme_selector").change();
});

$(window).load(function () {
    var error = document.getElementById("params").dataset.error;
    if (error != "") {
        alert(error);
    }
});