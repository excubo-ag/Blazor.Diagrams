using System.ComponentModel;

namespace Excubo.Blazor.Diagrams
{
    public class NodeAnchor
    {
        public NodeAnchor()
        {
        }
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

        private NodeBase node;
        private double old_node_width;
        private double old_node_height;
        /// <summary>
        /// The Node that this anchor is attached to. If Node is null, the anchor is free-floating and refers to a point on the canvas.
        /// </summary>
        public NodeBase Node 
        {
            get => node;
            set 
            {
                if (node != null)
                {
                    node.PropertyChanged -= Node_PropertyChanged;
                }
                node = value;
                if (node != null)
                {
                    old_node_width = node.Width;
                    old_node_height = node.Height;
                    node.PropertyChanged += Node_PropertyChanged;
                }
            } 
        }
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
        private void Node_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(NodeBase.Width) && e.PropertyName != nameof(NodeBase.Height))
            {
                return;
            }
            var new_width = Node.Width;
            var new_height = Node.Height;
            if (old_node_width != 0)
            {
                RelativeX *= new_width / old_node_width;
            }
            if (old_node_height != 0)
            {
                RelativeY *= new_height / old_node_height;
            }
            old_node_width = new_width;
            old_node_height = new_height;
        }
    }
}
