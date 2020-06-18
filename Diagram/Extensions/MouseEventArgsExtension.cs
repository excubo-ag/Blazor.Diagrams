using Microsoft.AspNetCore.Components.Web;

namespace Excubo.Blazor.Diagrams.Extensions
{
    internal static class MouseEventArgsExtension
    {
        public static double RelativeXTo(this MouseEventArgs e, Point point) => e.ClientX - point.X;
        public static double RelativeXTo(this MouseEventArgs e, NodeBase node) => e.RelativeXToOrigin(node.Diagram) - node.X;
        public static double RelativeXTo(this MouseEventArgs e, Diagram diagram) => (e.ClientX - diagram.CanvasLeft) / diagram.NavigationSettings.Zoom;
        public static double RelativeXToOrigin(this MouseEventArgs e, Diagram diagram) => e.RelativeXTo(diagram) + diagram.NavigationSettings.Origin.X;
        public static double RelativeYTo(this MouseEventArgs e, Point point) => e.ClientY - point.Y;
        public static double RelativeYTo(this MouseEventArgs e, NodeBase node) => e.RelativeYToOrigin(node.Diagram) - node.Y;
        public static double RelativeYTo(this MouseEventArgs e, Diagram diagram) => (e.ClientY - diagram.CanvasTop) / diagram.NavigationSettings.Zoom;
        public static double RelativeYToOrigin(this MouseEventArgs e, Diagram diagram) => e.RelativeYTo(diagram) + diagram.NavigationSettings.Origin.Y;
    }
}
