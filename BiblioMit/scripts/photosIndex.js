jQuery(document).ready(function () {
    var lang = $("html").attr("lang");
    fetch(`/api/static/json/${lang}/getphotos.json`).then(r => r.json())
        .then(data => {
            jQuery("#my_nanogallery2").nanogallery2({
                items: data,
                thumbnailWidth: 'auto',
                thumbnailHeight: 170,
                locationHash: false,
                thumbnailL1GutterWidth: 40,
                thumbnailL1GutterHeight: 40,
                thumbnailStacks: 4,
                thumbnailStacksTranslateZ: 0.3,
                thumbnailStacksRotateX: 0.9,
                galleryBuildInit2: 'perspective_900px|perspective-origin_50% 50%',
                thumbnailL1HoverEffect2: 'thumbnail_translateZ_0px_10px_easeOutQuad_400 | thumbnail_rotateX_0deg_10deg_easeOutBack_200 | thumbnail_rotateX_10deg_0deg_delay250_keyframe_hoverin_easeOutBack_400',
                thumbnailHoverEffect2: 'label_font-size_1em_1.5em|title_color_#000_#fff|image_scale_1.00_1.10_5000|image_rotateZ_0deg_4deg_5000',
                viewerTools: {
                    topLeft: 'label',
                    topRight: 'custom1,fullscreenButton,closeButton'
                },
                icons: { viewerCustomTool1: '' },
                fnImgToolbarCustDisplay: myImgToolbarCustDisplay,
                viewerToolbar: {
                    standard: 'minimizeButton, label, fullscreenButton, downloadButton, infoButton'
                }
            });
        });
});
jQuery('#btnsearch2').on('click', function () {
    jQuery("#my_nanogallery2").nanogallery2('search2', jQuery('#ngy2search').val(), jQuery('#ngy2search2').val());
    alert('found: ' + jQuery("#my_nanogallery2").nanogallery2('search2Execute'));
});

function myImgToolbarCustDisplay($customToolbarElements, item) {
    if (item.customData.myCounter == undefined) {
        item.customData.myCounter = 1;
    }
    var s = '<div style="color:#f00;">photo Id: ' + item.GetId();
    s += ' views: ' + item.customData.myCounter++ + '</div>';
    $customToolbarElements.html(s);
}