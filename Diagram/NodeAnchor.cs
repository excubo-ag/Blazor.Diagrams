namespace Excubo.Blazor.Diagrams
{
    public class NodeAnchor
    {
        public static NodeAnchor WithDefaultNodePort(NodeBase node)
        {
            var (RelativeX, RelativeY) = node.GetDefaultPort();
            return new NodeAnchor
            {
                Node = node,
                RelativeX = RelativeX,
                RelativeY = RelativeY
            };
        }
        /// <summary>
        /// The Node that this anchor is attached to. If Node is null, the anchor is free-floating and refers to a point on the canvas.
        /// </summary>
        public NodeBase Node { get; set; }
        /// <summary>
        /// The relative horizontal position of the anchor in reference to the node. If Node is null, the anchor is free-floating and refers to a point on the canvas.
        /// </summary>
        public double RelativeX { private get; set; }
        /// <summary>
        /// The relative vertical position of the anchor in reference to the node. If Node is null, the anchor is free-floating and refers to a point on the canvas.
        /// </summary>
        public double RelativeY { private get; set; }
        /// <summary>
        /// The absolute horizontal position of the anchor.
        /// </summary>
        public double X => Node == null ? RelativeX : Node.CanvasX + Node.Nodes.Diagram.NavigationSettings.Zoom * RelativeX;
        /// <summary>
        /// The absolute vertical position of the anchor.
        /// </summary>
        public double Y => Node == null ? RelativeY : Node.CanvasY + Node.Nodes.Diagram.NavigationSettings.Zoom * RelativeY;
        public override string ToString()
        {
            return $"{(Node?.Id != null ? "#" + Node.Id : "")}({X},{Y})";
        }
    }
}
