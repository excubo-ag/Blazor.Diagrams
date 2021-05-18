using Excubo.Blazor.Diagrams.Extensions;
using Microsoft.AspNetCore.Components.Web;
using System;

namespace Excubo.Blazor.Diagrams
{
    public partial class Diagram
    {
        private sealed class DragSelecting : InteractionState
        {
            public DragSelecting(InteractionState previous, MouseEventArgs e) : base(previous)
            {
                StartSelectionByRegion(e);
            }
            public override InteractionState OnMouseMove(MouseEventArgs e)
            {
                UpdateSelectionByRegion(e);
                return this;
            }
            public override InteractionState OnMouseDown(MouseEventArgs e)
            {
                throw new InvalidOperationException($"The mouse cannot be pressed while the state is {nameof(DragSelecting)}");
            }
            public override InteractionState OnMouseUp(MouseEventArgs e)
            {
                StopSelectionByRegion();
                return new Default(this);
            }
            private void UpdateSelectionByRegion(MouseEventArgs e)
            {
                (diagram.select_region.Point.X, diagram.select_region.Point.Y) = e.RelativeToOrigin(diagram);
                diagram.select_region.TriggerStateHasChanged();
                var p1 = diagram.select_region.Point;
                var p2 = diagram.select_region.Origin;
                var (min_x, max_x) = (Math.Min(p1.X, p2.X), Math.Max(p1.X, p2.X));
                var (min_y, max_y) = (Math.Min(p1.Y, p2.Y), Math.Max(p1.Y, p2.Y));
                foreach (var node in diagram.Nodes.all_nodes)
                {
                    if (min_x <= node.X && node.X <= max_x
                     && min_y <= node.Y && node.Y <= max_y
                     && !diagram.Group.Contains(node))
                    {
                        node.Select();
                        node.TriggerStateHasChanged();
                        diagram.Group.Add(node);
                    }
                }
            }
            private void StartSelectionByRegion(MouseEventArgs e)
            {
                diagram.select_region.Origin = new Point(e.RelativeToOrigin(diagram));
                diagram.select_region.Point = new Point(e.RelativeToOrigin(diagram));
            }
            private void StopSelectionByRegion()
            {
                diagram.select_region.Origin = null;
                diagram.select_region.Point = null;
            }
        }
    }
}