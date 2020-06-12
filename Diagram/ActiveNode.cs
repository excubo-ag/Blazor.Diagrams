using Microsoft.AspNetCore.Components.Web;

namespace Excubo.Blazor.Diagrams
{
    internal class ActiveNode
    {
        public ActiveNode(NodeBase node, MouseEventArgs e, double canvas_left, double canvas_top)
        {
            Node = node;
            RelativeX = (e.ClientX - canvas_left) - node.X;
            RelativeY = (e.ClientY - canvas_top) - node.Y;
        }

        public NodeBase Node { get; set; }
        public double RelativeX { get; set; }
        public double RelativeY { get; set; }
    }
}
