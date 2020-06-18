using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace Excubo.Blazor.Diagrams.__Internal
{
    /// <summary>
    /// INTERNAL component, do not use as a user of Excubo.Blazor.Diagrams!
    /// </summary>
    public partial class NodeContent : ComponentBase
    {
        /// <summary>
        /// Horizontal position of the node content
        /// </summary>
        [Parameter] public double X { get; set; }
        /// <summary>
        /// Vertical position of the node content
        /// </summary>
        [Parameter] public double Y { get; set; }
        /// <summary>
        /// Horizontal size of the node content
        /// </summary>
        [Parameter] public double Width { get; set; }
        /// <summary>
        /// Vertical size of the node content
        /// </summary>
        [Parameter] public double Height { get; set; }
        /// <summary>
        /// Scale of the node content
        /// </summary>
        [Parameter] public double Zoom { get; set; }
        /// <summary>
        /// Actual content
        /// </summary>
        [Parameter] public RenderFragment ChildContent { get; set; }
        /// <summary>
        /// Callback that tells the diagram how large the node must be
        /// </summary>
        [Parameter] public Action<double[]> SizeCallback { get; set; }
        public void TriggerRender(double x, double y, double width, double height, double zoom)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Zoom = zoom;
            StateHasChanged();
        }
        protected override async Task OnAfterRenderAsync(bool first_render)
        {
            if (first_render)
            {
                var result = await js.GetDimensionsAsync(element); // TODO: handle content size changes.
                SizeCallback?.Invoke(result);
            }
            await base.OnAfterRenderAsync(first_render);
        }
    }
}
