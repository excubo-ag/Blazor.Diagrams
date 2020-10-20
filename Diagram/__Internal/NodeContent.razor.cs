using Microsoft.AspNetCore.Components;
using System;

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
        [Parameter] public Action<(double Width, double Height)> SizeCallback { get; set; }
        /// <summary>
        /// Additional classes for the node content container
        /// </summary>
        [Parameter] public string ContentClasses { get; set; }
        /// <summary>
        /// Additional style for the node content container
        /// </summary>
        [Parameter] public string ContentStyle { get; set; }
        /// <summary>
        /// Whether the node is currently off-canvas
        /// </summary>
        [Parameter] public bool OffCanvas { get; set; }
        public void TriggerRender(double x, double y, double width, double height, double zoom, bool off_canvas)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Zoom = zoom;
            OffCanvas = off_canvas;
            StateHasChanged();
        }
    }
}
