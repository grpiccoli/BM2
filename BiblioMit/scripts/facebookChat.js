var chatbox = document.getElementById('fb-customer-chat');
chatbox.setAttribute("page_id", "1350128371792150");
chatbox.setAttribute("attribution", "page_inbox");
window.fbAsyncInit = function () {
    FB.init({
        xfbml: true,
        version: 'v11.0'
    });
};

(function (d, s, id) {
    var js, fjs = d.getElementsByTagName(s)[0];
    if (d.getElementById(id)) return;
    js = d.createElement(s); js.id = id;
    var lan = document.getElementsByTagName("html")[0].getAttribute("lang");
    var l = "";
    switch (lan) {
        case "en":
            l = "en_GB";
            break;
        //case "es":
        default:
            l = "es_LA";
            break;
    }
    js.src = 'https://connect.facebook.net/'+l+'/sdk/xfbml.customerchat.js';
    fjs.parentNode.insertBefore(js, fjs);
}(document, 'script', 'facebook-jssdk'));