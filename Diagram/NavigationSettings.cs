using Excubo.Blazor.Diagrams.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Diagnostics;

namespace Excubo.Blazor.Diagrams
{
    public class NavigationSettings : ComponentBase
    {
        [CascadingParameter] public Diagram Diagram { get; set; }
        protected override void OnParametersSet()
        {
            Debug.Assert(Diagram != null, $"{nameof(NavigationSettings)} are not meant to be used outside a {nameof(Diagram)} component");
            Diagram.NavigationSettings = this;
            base.OnParametersSet();
        }
        /// <summary>
        /// Whether panning feels like dragging the canvas or dragging all elements.
        /// </summary>
        [Parameter] public bool InversedPanning { get; set; }
        /// <summary>
        /// The coordinates displayed in the top left corner of the diagram.
        /// </summary>
        [Parameter] public Point Origin { get; set; } = new Point();
        [Parameter] public EventCallback<Point> OriginChanged { get; set; }
        /// <summary>
        /// The current zoom level
        /// </summary>
        [Parameter] public double Zoom
        {
            get => zoom;
            set => zoom = value <= 0 ? 1 : value;
        }
        [Parameter] public EventCallback<double> ZoomChanged { get; set; }
        /// <summary>
        /// The minimum zoom level. Must be smaller than MaxZoom and greater than zero.
        /// </summary>
        [Parameter]
        public double MinZoom
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
        [Parameter]
        public double MaxZoom
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
        private double zoom = 1;
        private double min_zoom = double.Epsilon;
        private double max_zoom = double.PositiveInfinity;

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
            _ = OriginChanged.InvokeAsync(Origin);
            _ = ZoomChanged.InvokeAsync(Zoom);
        }
        protected override bool ShouldRender() => false;

        internal void Pan(double offset_x, double offset_y)
        {
            Origin.X += (InversedPanning ? 1 : -1) * offset_x / Zoom;
            Origin.Y += (InversedPanning ? 1 : -1) * offset_y / Zoom;
            _ = OriginChanged.InvokeAsync(Origin);
        }
    }
}
