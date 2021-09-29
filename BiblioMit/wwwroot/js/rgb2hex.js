const rgb2hex = (rgb) => `#${arr2hex(rgb.match(/^rgb\((\d+),\s*(\d+),\s*(\d+)\)$/).slice(1))}`;
const arr2hex = (arr) => arr.map((n) => parseInt(n, 10).toString(16).padStart(2, '0')).join('');
function pickHex(color1, color2, weight) {
    var w1 = weight;
    var w2 = 1 - w1;
    var rgb = [Math.round(color1[0] * w1 + color2[0] * w2),
        Math.round(color1[1] * w1 + color2[1] * w2),
        Math.round(color1[2] * w1 + color2[2] * w2)];
    return arr2hex(rgb);
}
//# sourceMappingURL=rgb2hex.js.map