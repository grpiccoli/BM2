if(!document.getElementById('semaforo').classList.contains('d-none'))
    tippy('#infobtn', {
        content: document.getElementById('infotable').innerHTML,
        allowHTML: true,
        animation: 'scale'
    });