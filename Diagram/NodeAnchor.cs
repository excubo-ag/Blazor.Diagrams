namespace Excubo.Blazor.Diagrams
{
    public class NodeAnchor
    {
        /// <summary>
        /// The Node that this anchor is attached to. If Node is null, the anchor is free-floating and refers to a point on the canvas.
        /// </summary>
        public NodeBase Node { get; set; }
        /// <summary>
        /// The node id that this anchor should be associated to.
        /// </summary>
        public string NodeId { get; set; }
        /// <summary>
        /// Specify a position on the node (optional. Otherwise, RelativeX and RelativeY are used).
        /// </summary>
        public Position Port { get; set; }
        /// <summary>
        /// The relative horizontal position of the anchor in reference to the node. If Node is null, the anchor is free-floating and refers to a point on the canvas.
        /// Only used if Port is not used.
        /// </summary>
        public double RelativeX { private get; set; }
        /// <summary>
        /// The relative vertical position of the anchor in reference to the node. If Node is null, the anchor is free-floating and refers to a point on the canvas.
        /// Only used if Port is not used.
        /// </summary>
        public double RelativeY { private get; set; }
        /// <summary>
        /// The absolute horizontal position of the anchor.
        /// </summary>
        internal double X => RelativeX + (Node?.X ?? 0);
        /// <summary>
        /// The absolute vertical position of the anchor.
        /// </summary>
        internal double Y => RelativeY + (Node?.Y ?? 0);
        public override string ToString()
        {
            return $"{(Node?.Id != null ? "#" + Node.Id : "")}+({RelativeX},{RelativeY})";
        }
    }
}
