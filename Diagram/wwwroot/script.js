
window.Excubo = window.Excubo || {};
window.Excubo.Blazor = window.Excubo.Blazor || {};
window.Excubo.Blazor.Diagrams = {
    GetPosition: function (el) {
        return [el.offsetLeft, el.offsetTop];
    },
    GetDimensions: function (el) {
        return [el.clientWidth, el.clientHeight];
    }
};