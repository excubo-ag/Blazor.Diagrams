using System;

namespace Excubo.Blazor.Diagrams
{
    public class NodeAnchor
    {
        /// <summary>
        /// The Node that this anchor is attached to. If Node is null, the anchor is free-floating and refers to a point on the canvas.
        /// </summary>
        public NodeBase Node
        {
            get => node; set
            {
                node = value;
                if (node != null && CoordinatesChanged != null)
                {
                    node.PositionChanged += CoordinatesChanged;
                }
            }
        }
        public static implicit operator NodeAnchor((string NodeId, Position Port) value)
        {
            return new NodeAnchor { NodeId = value.NodeId, Port = value.Port };
        }
        public static implicit operator NodeAnchor(string node_id)
        {
            return new NodeAnchor { NodeId = node_id };
        }
        /// <summary>
        /// The node id that this anchor should be associated to.
        /// </summary>
        public string NodeId { get; set; }
        /// <summary>
        /// Specify a position on the node (optional. Otherwise, RelativeX and RelativeY are used).
        /// </summary>
        public Position Port
        {
            get => port;
            set
            {
                if (value == port)
                {
                    return;
                }
                port = value;
                if (Port != Position.Any && Node != null)
                {
                    (RelativeX, RelativeY) = Node.GetDefaultPort(Port);
                }
            }
        }
        private Position port;
        /// <summary>
        /// The relative horizontal position of the anchor in reference to the node. If Node is null, the anchor is free-floating and refers to a point on the canvas.
        /// Only used if Port is not used.
        /// </summary>
        public double RelativeX { get => relative_x; set { relative_x = value; CoordinatesChanged?.Invoke(); } }
        /// <summary>
        /// The relative vertical position of the anchor in reference to the node. If Node is null, the anchor is free-floating and refers to a point on the canvas.
        /// Only used if Port is not used.
        /// </summary>
        public double RelativeY { get => relative_y; set { relative_y = value; CoordinatesChanged?.Invoke(); } }
        /// <summary>
        /// The absolute horizontal position of the anchor.
        /// </summary>
        public double X => RelativeX + (Node?.X ?? 0);
        /// <summary>
        /// The absolute vertical position of the anchor.
        /// </summary>
        public double Y => RelativeY + (Node?.Y ?? 0);
        private NodeBase node;
        private double relative_x;
        private double relative_y;
        private Action coordinates_changed;
        internal Action CoordinatesChanged
        {
            get => coordinates_changed;
            set
            {
                coordinates_changed = value;
                Node = node; // try attaching this callback again.
            }
        }
        public override string ToString()
        {
            return $"{(Node?.Id != null ? "#" + Node.Id : "")}+({RelativeX},{RelativeY})";
        }
    }
}