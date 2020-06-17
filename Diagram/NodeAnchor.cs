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
        internal double GetX(Diagram diagram) => diagram.NavigationSettings.Zoom * (RelativeX + (Node?.X ?? 0));
        /// <summary>
        /// The absolute vertical position of the anchor.
        /// </summary>
        internal double GetY(Diagram diagram) => diagram.NavigationSettings.Zoom * (RelativeY + (Node?.Y ?? 0));
        public override string ToString()
        {
            return $"{(Node?.Id != null ? "#" + Node.Id : "")}+({RelativeX},{RelativeY})";
        }
    }
}
