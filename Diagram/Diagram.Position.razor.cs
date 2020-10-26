using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace Excubo.Blazor.Diagrams
{
    public partial class Diagram : IDisposable
    {
        private DotNetObjectReference<Diagram> js_interop_reference_to_this;

        [Inject] private IJSRuntime js { get; set; }
        internal double CanvasLeft { get; private set; }
        internal double CanvasTop { get; private set; }
        internal double CanvasWidth { get; private set; }
        internal double CanvasHeight { get; private set; }
        private async Task GetPositionAsync()
        {
            (CanvasLeft, CanvasTop) = await js.GetPositionAsync(canvas);
            (CanvasWidth, CanvasHeight) = await js.GetDimensionsAsync(canvas);
            Nodes.ReRenderIfOffCanvasChanged();
            js_interop_reference_to_this ??= DotNetObjectReference.Create(this);
            await js.RegisterMoveObserverAsync(canvas, js_interop_reference_to_this);
        }
        public class Rect
        {
            public double Left { get; set; }
            public double Top { get; set; }
            public double Width { get; set; }
            public double Height { get; set; }
        }
        [JSInvokable]
        public void OnMove(Rect rect)
        {
            (CanvasLeft, CanvasTop, CanvasWidth, CanvasHeight) = (rect.Left, rect.Top, rect.Width, rect.Height);
            Nodes.ReRenderIfOffCanvasChanged();
        }
        public void Dispose()
        {
            if (js_interop_reference_to_this == null)
            {
                return;
            }
            if (canvas.Id != null)
            {
                js.UnobserveMovesAsync(canvas);
            }
            js_interop_reference_to_this.Dispose();
        }
    }
}
