// Screen size detection for responsive layout
let componentRef = null;

window.setupResizeListener = (component) => {
    componentRef = component;
    window.addEventListener('resize', onWindowResize);
};

window.onWindowResize = () => {
    if (componentRef) {
        componentRef.invokeMethodAsync('OnWindowResize');
    }
};

window.getIsSmallScreen = () => {
    return window.innerWidth < 768; // md breakpoint is 768px in Tailwind
};
