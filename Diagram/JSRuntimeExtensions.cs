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
            return js.InvokeVoidAsync("eval", "window.Excubo=window.Excubo||{};window.Excubo.Diagrams=window.Excubo.Diagrams||{position:n=>{const t=n.getBoundingClientRect();return{Left:t.left,Top:t.top}},size:n=>{const t=n.getBoundingClientRect();return{Width:t.width,Height:t.height}},ro:new ResizeObserver(n=>{for(const t of n){let i=Array.from(new Set(t.target.attributes)).find(n=>n.name.startsWith('_bl_')).name.substring(4),n=window.Excubo.Diagrams.rs[i];n!=undefined&&n.Ref.invokeMethodAsync('OnResize',{Width:t.contentRect.width,Height:t.contentRect.height}).catch(()=>{})}}),mo:new MutationObserver(()=>{for(const t in window.Excubo.Diagrams.rs){var n=window.Excubo.Diagrams.rs[t];if(n!=undefined&&n.Parents!=undefined){const t=n.Element.getBoundingClientRect(),i=t.left,r=t.top,u=t.width,f=t.height;(n.Left!=i||n.Top!=r||n.Width!=u||n.Height!=f)&&(n.Left=i,n.Top=r,n.Width=u,n.Height=f,n.Ref.invokeMethodAsync('OnMove',{Left:i,Top:r,Width:u,Height:f}).catch(()=>{}))}}}),rs:{},observeResizes:(n,t,i)=>{if(n!=undefined){const r=window.Excubo.Diagrams,u=n.getBoundingClientRect();r.rs[t]={Element:n,Ref:i,Width:u.width,Height:u.height};r.ro.observe(n)}},unobserveResizes:(n,t)=>{if(n!=undefined){const i=window.Excubo.Diagrams;delete i.rs[t];i.ro.unobserve(n)}},observeMoves:(n,t,i)=>{if(n!=undefined){const u=window.Excubo.Diagrams,r=n.getBoundingClientRect();for(u.rs[t]={Element:n,Ref:i,Left:r.left,Top:r.top,Width:r.width,Height:r.height,Parents:[]};n.parentElement!=null;)u.rs[t].Parents.push(n.parentElement),u.mo.observe(n.parentElement,{attributes:!0}),n=n.parentElement}},unobserveMoves:(n,t)=>{if(n!=undefined){const i=window.Excubo.Diagrams,r=i.rs[t].Parents;delete i.rs[t];const u=n=>{for(const t in i.rs){const r=i.rs[t];if(r!=undefined&&r.Parents!=undefined&&r.Parents.includes(n))return!0}return!1};for(;;){const n=r.pop();if(n==undefined)break;u(n)||i.mo.unobserve(n)}}}};");
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
        public static async Task RegisterMoveObserverAsync<T>(this IJSRuntime js, ElementReference element, DotNetObjectReference<T> reference) where T : class
        {
            await js.InvokeVoidAsync("Excubo.Diagrams.observeMoves", element, element.Id, reference);
        }
        public static async Task UnobserveMovesAsync(this IJSRuntime js, ElementReference element)
        {
            await js.InvokeVoidAsync("Excubo.Diagrams.unobserveMoves", element, element.Id);
        }
    }
}