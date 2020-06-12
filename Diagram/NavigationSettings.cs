using Excubo.Blazor.Diagrams.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;

namespace Excubo.Blazor.Diagrams
{
    public class NavigationSettings : ComponentBase
    {
        [CascadingParameter] public Diagram Diagram { get; set; }
        protected override void OnParametersSet()
        {
            System.Diagnostics.Debug.Assert(Diagram != null);
            Diagram.NavigationSettings = this;
            base.OnParametersSet();
        }
        /// <summary>
        /// The current zoom level
        /// </summary>
        [Parameter] public double Zoom { get; set; } = 1.0; // higher means larger objects. TODO allow @bind-Zoom?
        /// <summary>
        /// The minimum zoom level. Must be smaller than MaxZoom and greater than zero.
        /// </summary>
        [Parameter] public double MinZoom { get; set; } = double.Epsilon; // TODO disallow non-positive values. If MinZoom > MaxZoom, swap
        /// <summary>
        /// the maximum zoom level.Must be larger than MinZoom and less than infinity.
        /// </summary>
        [Parameter] public double MaxZoom { get; set; } = double.PositiveInfinity; // TODO disallow non-positive values. If MinZoom > MaxZoom, swap
        /// <summary>
        /// Whether panning feels like dragging the canvas or dragging the nodes.
        /// </summary>
        [Parameter] public bool InversedPanning { get; set; }
        /// <summary>
        /// The coordinates displayed in the top left corner of the diagram. Use @bind-Origin="YourOriginVariable".
        /// </summary>
        [Parameter] public Point Origin { get; set; } = new Point();
        [Parameter] public EventCallback<Point> OriginChanged { get; set; }

        internal void OnMouseWheel(WheelEventArgs e)
        {
            // cursor is at
            var cursor_x = e.RelativeXTo(Diagram);
            var cursor_y = e.RelativeYTo(Diagram);
            // point in the diagram world
            var canvas_x = Origin.X + cursor_x / Zoom;
            var canvas_y = Origin.Y + cursor_y / Zoom;
            // this point should remain where it is right now (the cursor is the stable zoom point), hence if we adjust zoom, we need to adjust origin as well.
            if (e.DeltaY > 0)
            {
                Zoom *= 1.05;
            }
            else
            {
                Zoom /= 1.05;
            }
            Origin.X = canvas_x - cursor_x / Zoom;
            Origin.Y = canvas_y - cursor_y / Zoom;
        }
    }
}
