using Excubo.Blazor.Diagrams.Extensions;
using Microsoft.AspNetCore.Components.Web;

namespace Excubo.Blazor.Diagrams
{
    public partial class Diagram
    {
        private sealed class MovingAnchor : InteractionState
        {
            private readonly LinkBase link;
            private readonly NodeAnchor anchor;
            private readonly NodeBase originalNode;
            private readonly Point originalCoordinates;

            public MovingAnchor(InteractionState previous, LinkBase link, NodeAnchor anchor, MouseEventArgs e) : base(previous)
            {
                this.link = link;
                this.anchor = anchor;
                originalNode = anchor.Node;
                originalCoordinates = new Point(anchor.RelativeX, anchor.RelativeY);
                anchor.Node = null;
                anchor.NodeId = null;
                anchor.RelativeX = e.RelativeXToOrigin(diagram);
                anchor.RelativeY = e.RelativeYToOrigin(diagram);
            }
            public override InteractionState OnMouseMove(MouseEventArgs e)
            {
                MoveNodeAnchor(e);
                return this;
            }
            public override InteractionState OnMouseUp(MouseEventArgs e)
            {
                var node = diagram.ActiveElement as NodeBase;
                if (node == null && !diagram.Links.AllowFreeFloatingLinks)
                {
                    // free-floating links are not allowed and this action would not attach the link to a node
                    return this;
                }
                FixNodeAnchor(e, node);
                return new Default(this);
            }
            private void MoveNodeAnchor(MouseEventArgs e)
            {
                anchor.RelativeX = e.RelativeXToOrigin(diagram);
                anchor.RelativeY = e.RelativeYToOrigin(diagram);
                diagram.Overview?.TriggerUpdate();
            }
            private void FixNodeAnchor(MouseEventArgs e, NodeBase node)
            {
                var (x, y) = (node != null) ? e.RelativeTo(node) : e.RelativeToOrigin(diagram);
                diagram.Changes.NewAndDo(new ChangeAction(() =>
                {
                    anchor.Node = node;
                    anchor.RelativeX = x;
                    anchor.RelativeY = y;
                }, () =>
                {
                    anchor.Node = originalNode;
                    anchor.RelativeX = originalCoordinates.X;
                    anchor.RelativeY = originalCoordinates.Y;
                }));
                diagram.Overview?.TriggerUpdate();
                link.Deselect();
                link.Links.OnModified?.Invoke(link);
            }
        }
    }
}