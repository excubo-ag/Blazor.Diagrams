using Excubo.Blazor.Diagrams.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Diagnostics;
using System.Linq;

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
        [Parameter] public Action<Point> OriginChanged { get; set; }
        /// <summary>
        /// The current zoom level
        /// </summary>
        [Parameter]
        public double Zoom
        {
            get => zoom;
            set => zoom = value <= 0 ? 1 : value;
        }
        [Parameter] public Action<double> ZoomChanged { get; set; }
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
        /// <summary>
        /// When true, zooming by user interaction is disabled (default: false). Note that changing the zoom parameter still affects zoom.
        /// </summary>
        [Parameter]
        public bool DisableZooming { get; set; }
        /// <summary>
        /// Invert zoom direction.
        /// </summary>
        [Parameter] public bool InversedZoom { get; set; }
        /// <summary>
        /// When true, panning by user interaction is disabled (default: false). Note that changing the origin parameter still affects the diagram area.
        /// </summary>
        [Parameter]
        public bool DisablePanning { get; set; }
        internal void OnMouseWheel(WheelEventArgs e)
        {
            if (DisableZooming)
            {
                return;
            }
            // this point should remain where it is right now (the cursor is the stable zoom point), hence if we adjust zoom, we need to adjust origin as well.
            var canvas_x = Origin.X + e.RelativeXTo(Diagram);
            var canvas_y = Origin.Y + e.RelativeYTo(Diagram);
            if (!InversedZoom && e.DeltaY > 0 || InversedZoom && e.DeltaY < 0)
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
            OriginChanged?.Invoke(Origin);
            ZoomChanged?.Invoke(Zoom);
        }
        protected override bool ShouldRender() => false;

        internal void Pan(double offset_x, double offset_y)
        {
            if (DisablePanning)
            {
                return;
            }
            Origin.X += (InversedPanning ? 1 : -1) * offset_x / Zoom;
            Origin.Y += (InversedPanning ? 1 : -1) * offset_y / Zoom;
            OriginChanged?.Invoke(Origin);
        }

        internal void ZoomToFit(bool pan_to_center=false)
        {
            var non_deleted_nodes = Diagram.Nodes.all_nodes.Where(n => !n.Deleted).ToList();
            if (non_deleted_nodes.Any())
            {
                var min_x = non_deleted_nodes.Min(n => n.X);
                var max_x = non_deleted_nodes.Max(n => n.X);
                var max_x_node = non_deleted_nodes.FirstOrDefault(n => n.X == max_x);
                if (max_x_node == null)
                {
                    // this is not a normal situation, abort!
                    return;
                }
                max_x += max_x_node.GetWidth();
                var min_y = non_deleted_nodes.Min(n => n.Y);
                var max_y = non_deleted_nodes.Max(n => n.Y);
                var max_y_node = non_deleted_nodes.FirstOrDefault(n => n.Y == max_y);
                if (max_y_node == null)
                {
                    // this is not a normal situation, abort!
                    return;
                }
                max_y += max_y_node.GetHeight();
                var zoom_x = (min_x == max_x) ? double.MaxValue : .9 * Diagram.CanvasWidth / (max_x - min_x);
                var zoom_y = (min_y == max_y) ? double.MaxValue : .9 * Diagram.CanvasHeight / (max_y - min_y);
                var zoom = Math.Min(zoom_x, zoom_y);
                zoom = Math.Max(MinZoom, Math.Min(zoom, MaxZoom));
                if (zoom == double.MaxValue)
                {
                    // this is not a normal situation, abort!
                    return;
                }
                if (pan_to_center)
                {
                    (Origin.X, Origin.Y) = (min_x - Math.Abs((Diagram.CanvasWidth / 2) / zoom - (max_x - min_x) / 2), min_y - Math.Abs((Diagram.CanvasHeight / 2) / zoom - (max_y - min_y) / 2));
                }
                else
                {
                    (Origin.X, Origin.Y) = (min_x - 0.05 * (max_x - min_x), min_y - 0.05 * (max_y - min_y));
                }
                OriginChanged?.Invoke(Origin);
                Zoom = zoom;
                ZoomChanged?.Invoke(Zoom);
            }
        }
    }
}