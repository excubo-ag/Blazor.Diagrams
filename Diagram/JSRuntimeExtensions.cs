using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace Excubo.Blazor.Diagrams
{
    internal static class JSRuntimeExtensions
    {
        private class Position
        {
            public double Left { get; set; }
            public double Top { get; set; }
        }
        private class Dimension
        {
            public double Width { get; set; }
            public double Height { get; set; }
        }
        public static ValueTask InitializeJsAsync(this IJSRuntime js)
        {
            return js.InvokeVoidAsync("eval", "window.Excubo=window.Excubo||{};window.Excubo.Diagrams=window.Excubo.Diagrams||{position:n=>({Left:n.offsetLeft,Top:n.offsetTop}),size:n=>({Width:n.clientWidth,Height:n.clientHeight}),observer:new ResizeObserver(n=>{for(const t of n){let i=Array.from(new Set(t.target.attributes)).find(n=>n.name.startsWith('_bl_')).name,n=window.Excubo.Diagrams.references[i];n!=undefined&&n.invokeMethodAsync('OnResize',{Width:t.contentRect.width,Height:t.contentRect.height}).catch(()=>{})}}),references:{},observeResizes:(n,t,i)=>{window.Excubo.Diagrams.references[t]=i,window.Excubo.Diagrams.observer.observe(n)},unobserveResizes:(n,t)=>{delete window.Excubo.Diagrams.references[t],window.Excubo.Diagrams.observer.unobserve(n)}};");
        }
        public static async Task<(double Left, double Top)> GetPositionAsync(this IJSRuntime js, ElementReference element)
        {
            var position = await js.InvokeAsync<Position>("Excubo.Diagrams.position", element);
            return (position.Left, position.Top);
        }
        public static async Task<(double Width, double Height)> GetDimensionsAsync(this IJSRuntime js, ElementReference element)
        {
            var dimensions = await js.InvokeAsync<Dimension>("Excubo.Diagrams.size", element);
            return (dimensions.Width, dimensions.Height);
        }
        public static async Task RegisterResizeObserverAsync<T>(this IJSRuntime js, ElementReference element, DotNetObjectReference<T> reference) where T : class
        {
            await js.InvokeVoidAsync("Excubo.Diagrams.observeResizes", element, element.Id, reference);
        }
        public static async Task UnobserveResizesAsync(this IJSRuntime js, ElementReference element)
        {
            await js.InvokeVoidAsync("Excubo.Diagrams.unobserveResizes", element, element.Id);
        }
    }
}
