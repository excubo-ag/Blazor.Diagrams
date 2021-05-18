using Excubo.Blazor.Diagrams.Extensions;
using Microsoft.AspNetCore.Components.Web;
using System.Linq;

namespace Excubo.Blazor.Diagrams
{
    public partial class Diagram
    {
        private sealed class MovingNodeOrNodeGroup : InteractionState
        {
            private readonly Point referencePoint;
            private readonly Point originalCoordinates;
            private readonly NodeBase grabbedNode;

            public MovingNodeOrNodeGroup(InteractionState previous, NodeBase node, MouseEventArgs e) : base(previous)
            {
                referencePoint = new Point(e.ClientX, e.ClientY);
                originalCoordinates = new Point(node.X, node.Y);
                grabbedNode = node;
            }
            public override InteractionState OnMouseMove(MouseEventArgs e)
            {
                MoveGroup(diagram, e);
                return this;
            }
            public override InteractionState OnMouseUp(MouseEventArgs e)
            {
                StopMove(diagram);
                return new Default(this);
            }
            private void MoveGroup(Diagram diagram, MouseEventArgs e)
            {
                var delta_x = e.RelativeXTo(referencePoint) / diagram.NavigationSettings.Zoom;
                var delta_y = e.RelativeYTo(referencePoint) / diagram.NavigationSettings.Zoom;
                foreach (var node in diagram.Group.Nodes)
                {
                    node.MoveTo(node.X + delta_x, node.Y + delta_y);
                }
                (referencePoint.X, referencePoint.Y) = (e.ClientX, e.ClientY);
                diagram.Overview?.TriggerUpdate();
            }
            private void StopMove(Diagram diagram)
            {
                var (delta_x, delta_y) = (grabbedNode.X - originalCoordinates.X, grabbedNode.Y - originalCoordinates.Y);
                if (delta_x != 0 || delta_y != 0)
                {
                    var nodes = diagram.Group.Nodes.ToList();
                    var positions = nodes.Select(n => (X: n.X, Y: n.Y)).ToList();
                    var old_positions = nodes.Select(n => (X: n.X - delta_x, Y: n.Y - delta_y)).ToList();
                    diagram.Changes.New(new ChangeAction(() =>
                    {
                        foreach (var (node, position) in nodes.Zip(positions, (n, p) => (n, p)))
                        {
                            node.MoveTo(position.X, position.Y);
                            node.TriggerStateHasChanged();
                        }
                    }, () =>
                    {
                        foreach (var (node, position) in nodes.Zip(old_positions, (n, p) => (n, p)))
                        {
                            node.MoveTo(position.X, position.Y);
                            node.TriggerStateHasChanged();
                        }
                    }));
                    foreach (var node in nodes)
                    {
                        diagram.Nodes.OnModified?.Invoke(node);
                    }
                }
                if (diagram.Group.Nodes.Count == 1)
                {
                    grabbedNode.Deselect();
                    diagram.Group.Clear();
                }
                diagram.node_library_wrapper.NewNodeAddingInProgress = false;
                diagram.Overview?.TriggerUpdate();
            }
        }
    }
}