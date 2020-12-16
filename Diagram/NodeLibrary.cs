using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;

namespace Excubo.Blazor.Diagrams
{
    public class NodeLibrary : ComponentBase
    {
        [CascadingParameter] public Diagram Diagram { get; set; }
        /// <summary>
        /// Influences whether the nodes in the node library are displayed horizontally or vertically stacked.
        /// </summary>
        [Parameter] public Orientation Orientation { get; set; } = Orientation.Vertical;
        /// <summary>
        /// Padding in pixels. Defaults to 10.
        /// </summary>
        [Parameter] public double Padding { get; set; } = 10;
        /// <summary>
        /// Separation between nodes in pixels. Defaults to 24.
        /// </summary>
        [Parameter] public double Separation { get; set; } = 24;
        [Parameter] public RenderFragment ChildContent { get; set; }
        [Parameter(CaptureUnmatchedValues = true)]
        public Dictionary<string, object> AdditionalAttributes { get; set; }
        protected override void OnParametersSet()
        {
            System.Diagnostics.Debug.Assert(Diagram != null, $"{nameof(NodeLibrary)} is not meant to be used outside a {nameof(Diagram)} component");
            Diagram.NodeLibrary = this;
            base.OnParametersSet();
        }
        internal (double X, double Y, double Width, double Height) GetPosition(NodeBase node)
        {
            var margins = node.GetDrawingMargins();
            if (!positions.ContainsKey(node))
            {
                if (!positions.Any())
                {
                    positions.Add(node, (X: Padding + margins.Left, Y: Padding + margins.Top, Width: node.GetWidth() + margins.Right, Height: node.GetHeight() + margins.Bottom));
                }
                else
                {
                    var outermost_value = positions.Values.Max(p => Orientation == Orientation.Horizontal ? p.X : p.Y);
                    var (pX, pY, pWidth, pHeight) = positions.Values.First(p => (Orientation == Orientation.Horizontal ? p.X : p.Y) == outermost_value);
                    var x = Orientation == Orientation.Horizontal ? (pX + pWidth + Separation) : Padding;
                    var y = Orientation == Orientation.Horizontal ? Padding : (pY + pHeight + Separation);
                    positions.Add(node, (X: x + margins.Left, Y: y + margins.Top, Width: node.GetWidth() + margins.Right, Height: node.GetHeight() + margins.Bottom));
                }
            }
            return positions[node];

        }
        private readonly Dictionary<NodeBase, (double X, double Y, double Width, double Height)> positions = new Dictionary<NodeBase, (double X, double Y, double Width, double Height)>();
    }

}