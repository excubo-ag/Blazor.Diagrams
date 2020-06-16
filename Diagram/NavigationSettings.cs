using Excubo.Blazor.Diagrams.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

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
        [Parameter] public double Zoom { get; set; } = 1.0;
        [Parameter] public EventCallback<double> ZoomChanged { get; set; }
        private double min_zoom = double.Epsilon;
        private double max_zoom = double.PositiveInfinity;
        /// <summary>
        /// The minimum zoom level. Must be smaller than MaxZoom and greater than zero.
        /// </summary>
        [Parameter] public double MinZoom
        {
            get => min_zoom; 
            set
            {
                min_zoom = value;
                if (max_zoom < min_zoom)
                {
                    (min_zoom, max_zoom) = (max_zoom, min_zoom);
                }
            }
        }
        /// <summary>
        /// the maximum zoom level.Must be larger than MinZoom and less than infinity.
        /// </summary>
        [Parameter] public double MaxZoom 
        {
            get => max_zoom;
            set
            {
                max_zoom = value;
                if (max_zoom < min_zoom)
                {
                    (min_zoom, max_zoom) = (max_zoom, min_zoom);
                }
            }
        }
        /// <summary>
        /// Whether panning feels like dragging the canvas or dragging all elements.
        /// </summary>
        [Parameter] public bool InversedPanning { get; set; }
        /// <summary>
        /// The coordinates displayed in the top left corner of the diagram. Use @bind-Origin="YourOriginVariable".
        /// </summary>
        [Parameter] public Point Origin { get; set; } = new Point();
        [Parameter] public EventCallback<Point> OriginChanged { get; set; }

        internal void OnMouseWheel(WheelEventArgs e)
        {
            // this point should remain where it is right now (the cursor is the stable zoom point), hence if we adjust zoom, we need to adjust origin as well.
            var canvas_x = Origin.X + e.RelativeXTo(Diagram);
            var canvas_y = Origin.Y + e.RelativeYTo(Diagram);
            if (e.DeltaY > 0)
            {
                Zoom *= 1.05;
                if (Zoom > MaxZoom)
                {
                    Zoom = MaxZoom;
                }
            }
            else
            {
                Zoom /= 1.05;
                if (Zoom < MinZoom)
                {
                    Zoom = MinZoom;
                }
            }
            Origin.X = canvas_x - e.RelativeXTo(Diagram);
            Origin.Y = canvas_y - e.RelativeYTo(Diagram);
        }
    }
}
