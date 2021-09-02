function setStyles(lightPage) {
    document.querySelector("nav").classList.toggle("bg-dark", lightPage);
    var foot = document.querySelector("footer a").classList;
    foot.toggle("text-dark", lightPage);
    foot.toggle("text-light", !lightPage);
    document.querySelector("img[alt='epic solutions']").src = lightPage ? "epic_hor.svg" : "epic_light_hor.svg";
}
function getAverageColourAsRGB(img) {
    var canvas = document.createElement('canvas'), context = canvas.getContext && canvas.getContext('2d'), rgb = { r: 102, g: 102, b: 102 }, pixelInterval = 5, count = 0, i = -4, data, length;
    if (!context) {
        alert('Your browser does not support CANVAS');
        return rgb;
    }
    var height = canvas.height = img.naturalHeight || img.offsetHeight || img.height, width = canvas.width = img.naturalWidth || img.offsetWidth || img.width;
    context.drawImage(img, 0, 0);
    try {
        data = context.getImageData(0, 0, width, height);
    }
    catch (e) {
        alert(e);
        return rgb;
    }
    data = data.data;
    length = data.length;
    while ((i += pixelInterval * 4) < length) {
        count++;
        rgb.r += data[i];
        rgb.g += data[i + 1];
        rgb.b += data[i + 2];
    }
    rgb.r = Math.floor(rgb.r / count);
    rgb.g = Math.floor(rgb.g / count);
    rgb.b = Math.floor(rgb.b / count);
    return rgb;
}
function setContrast() {
    var carousel = document.querySelector(".carousel-item.active");
    var style = getComputedStyle(carousel);
    var bgimg = style.getPropertyValue('background-image');
    var src = bgimg.replace(/^[^"]+"/g, '').replace(/".*/g, '');
    var img = document.createElement('img');
    img.src = src;
    img.height = screen.height;
    img.width = screen.width;
    var rgb = getAverageColourAsRGB(img);
    var brightness = Math.round((rgb.r * 299 +
        rgb.g * 587 +
        rgb.b * 114) / 1000);
    var isLight = brightness > 250;
    $("nav").toggleClass("navbar-dark", !isLight).toggleClass("navbar-light", isLight);
}
//# sourceMappingURL=home.js.map