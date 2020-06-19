﻿using Microsoft.AspNetCore.Components.Web;
using System.Runtime.CompilerServices;

namespace Excubo.Blazor.Diagrams.Extensions
{
    internal static class MouseEventArgsExtension
    {
        public static (double X, double Y) RelativeTo(this MouseEventArgs e, Point point) => (e.RelativeXTo(point), e.RelativeYTo(point));
        public static (double X, double Y) RelativeTo(this MouseEventArgs e, NodeBase node) => (e.RelativeXTo(node), e.RelativeYTo(node));
        public static (double X, double Y) RelativeTo(this MouseEventArgs e, Diagram diagram) => (e.RelativeXTo(diagram), e.RelativeYTo(diagram));
        public static (double X, double Y) RelativeToOrigin(this MouseEventArgs e, Diagram diagram) => (e.RelativeXToOrigin(diagram), e.RelativeYToOrigin(diagram));
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
