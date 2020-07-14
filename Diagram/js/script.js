window.Excubo = window.Excubo || {};
window.Excubo.Diagrams = window.Excubo.Diagrams || {
    position: (e) => { return { 'Left': e.offsetLeft, 'Top': e.offsetTop } },
    size: (e) => { return { 'Width': e.clientWidth, 'Height': e.clientHeight } },
    observer: new ResizeObserver((es) => {
        for (const e of es) {
            let el = Array.from(new Set(e.target.attributes)).find((e) => e.name.startsWith('_bl_')).name;
            let r = window.Excubo.Diagrams.references[el];
            if (r != undefined) {
                r.invokeMethodAsync('OnResize', { 'Width': e.contentRect.width, 'Height': e.contentRect.height }).catch(() => { });
            }
        }
    }),
    references: {},
    observeResizes: (el, id, r) => {
        window.Excubo.Diagrams.references[id] = r;
        window.Excubo.Diagrams.observer.observe(el)
    },
    unobserveResizes: (el, id) => {
        delete window.Excubo.Diagrams.references[id];
        window.Excubo.Diagrams.observer.unobserve(el)
    }
};