using Excubo.Blazor.Diagrams.Extensions;
using Microsoft.AspNetCore.Components.Web;

namespace Excubo.Blazor.Diagrams
{
    public partial class Diagram
    {
        private sealed class UpdatingLinkTarget : InteractionState
        {
            private readonly LinkBase link;

            public UpdatingLinkTarget(InteractionState previous, LinkBase link) : base(previous)
            {
                this.link = link;
            }
            public override InteractionState OnMouseMove(MouseEventArgs e)
            {
                FollowCursorForLinkTarget(e);
                return this;
            }
            public override InteractionState OnMouseUp(MouseEventArgs e)
            {
                EndLink(e);
                return new Default(this);
            }

            private void EndLink(MouseEventArgs e)
            {
                var node = diagram.ActiveElement as NodeBase;
                if (node == null && !link.Links.AllowFreeFloatingLinks)
                {
                    // free-floating links are not allowed and this action would not attach the link to a node
                    return;
                }
                if (node != null)
                {
                    link.Target.Node = node;
                    link.Target.RelativeX = e.RelativeXTo(node);
                    link.Target.RelativeY = e.RelativeYTo(node);
                }
                else
                {
                    link.Target.RelativeX = e.RelativeXToOrigin(diagram);
                    link.Target.RelativeY = e.RelativeYToOrigin(diagram);
                }
                link.Deselect();
                link.Links.OnModified?.Invoke(link);
                diagram.Overview?.TriggerUpdate();
                diagram.Changes.New(new ChangeAction(() => { diagram.Links.Add(link); }, () => { diagram.Links.Remove(link); }));
            }

            private void FollowCursorForLinkTarget(MouseEventArgs e)
            {
                link.Target.RelativeX = e.RelativeXToOrigin(diagram);
                link.Target.RelativeY = e.RelativeYToOrigin(diagram);
                diagram.Overview?.TriggerUpdate();
            }
        }
    }
}