using Microsoft.AspNetCore.Components.Web;

namespace Excubo.Blazor.Diagrams.Extensions
{
    internal static class MouseEventArgsExtension
    {
        public static double RelativeXTo(this MouseEventArgs e, NodeBase node) => e.RelativeXTo(node.Nodes.Diagram) - node.CanvasX;
        public static double RelativeXTo(this MouseEventArgs e, Diagram diagram) => e.ClientX - diagram.CanvasLeft;
        public static double RelativeYTo(this MouseEventArgs e, NodeBase node) => e.RelativeYTo(node.Nodes.Diagram) - node.CanvasY;
        public static double RelativeYTo(this MouseEventArgs e, Diagram diagram) => e.ClientY - diagram.CanvasTop;
    }
}
