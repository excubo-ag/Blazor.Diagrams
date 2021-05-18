using Microsoft.AspNetCore.Components.Web;

namespace Excubo.Blazor.Diagrams
{
    public partial class Diagram
    {
        private sealed class ModifyingLink : InteractionState
        {
            private readonly LinkBase link;

            public ModifyingLink(InteractionState previous, LinkBase link) : base(previous)
            {
                this.link = link;
                link.Select();
                link.TriggerStateHasChanged();
            }
            public override InteractionState OnKeyPress(KeyboardEventArgs e)
            {
                if (e.Key == "Escape")
                {
                    StopModifyingLink();
                    return new Default(this);
                }
                if (e.Key == "Delete" || e.Key == "Backspace")
                {
                    link.Deselect();
                    diagram.Changes.NewAndDo(new ChangeAction(() => diagram.Links.Remove(link), () => diagram.Links.Add(link)));
                    diagram.Overview?.TriggerUpdate();
                    return new Default(this);
                }
                return base.OnKeyPress(e);
            }
            public override InteractionState OnMouseDown(MouseEventArgs e)
            {
                if (diagram.ActiveElementType == HoverType.Anchor)
                {
                    return new MovingAnchor(this, link, diagram.ActiveElement as NodeAnchor, e);
                }
                if (diagram.ActiveElementType == HoverType.ControlPoint)
                {
                    return new MovingControlPoint(this, link, diagram.ActiveElement as ControlPoint, e);
                }
                StopModifyingLink();
                return new Default(this).OnMouseDown(e);
            }
            private void StopModifyingLink()
            {
                diagram.Overview?.TriggerUpdate();
                link.Deselect();
                link.TriggerStateHasChanged();
            }
        }
    }
}