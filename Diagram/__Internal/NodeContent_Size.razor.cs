using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace Excubo.Blazor.Diagrams.__Internal
{
    public partial class NodeContent : IDisposable
    {
        [Inject] private IJSRuntime js { get; set; }
        private DotNetObjectReference<NodeContent> js_interop_reference_to_this;
        protected override async Task OnAfterRenderAsync(bool first_render)
        {
            if (first_render)
            {
                if (Node is ContentSizedNodeBase)
                {
                    js_interop_reference_to_this ??= DotNetObjectReference.Create(this);
                    await js.RegisterResizeObserverAsync(element, js_interop_reference_to_this);
                }
            }
            await base.OnAfterRenderAsync(first_render);
        }
        public class Dimensions
        {
            public double Width { get; set; }
            public double Height { get; set; }
        }
        [JSInvokable]
        public void OnResize(Dimensions dimensions)
        {
            (Node as ContentSizedNodeBase).GetSize((dimensions.Width, dimensions.Height));
        }
        public void Dispose()
        {
            if (js_interop_reference_to_this == null)
            {
                return;
            }
            if (element.Id != null)
            {
                js.UnobserveResizesAsync(element);
            }
            js_interop_reference_to_this.Dispose();
        }
    }
}