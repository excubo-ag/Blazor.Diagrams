using Microsoft.AspNetCore.Components.Web;

namespace Excubo.Blazor.Diagrams
{
    public partial class Diagram
    {
        private object ActiveElement { get; set; }
        private HoverType ActiveElementType { get; set; }
        internal void SetActiveElement(NodeBase node, HoverType hover_type)
        {
            ActiveElement = node;
            ActiveElementType = hover_type;
        }
        internal void SetActiveElement(LinkBase link)
        {
            ActiveElement = link;
            ActiveElementType = HoverType.Link;
        }
        internal void SetActiveElement(ControlPoint control_point)
        {
            ActiveElement = control_point;
            ActiveElementType = HoverType.ControlPoint;
        }
        internal void SetActiveElement(NodeAnchor anchor)
        {
            ActiveElement = anchor;
            ActiveElementType = HoverType.Anchor;
        }
        internal void DeactivateElement()
        {
            ActiveElement = null;
            ActiveElementType = HoverType.Unknown;
        }
        private bool rendering_disabled;
        internal void DisableRendering()
        {
            rendering_disabled = true;
        }
        internal void EnableRendering()
        {
            rendering_disabled = false;
        }
        protected override bool ShouldRender()
        {
            if (rendering_disabled)
            {
                return false;
            }
            if (!render_necessary)
            {
                render_necessary = true;
                return false;
            }
            return base.ShouldRender();
        }
        private bool render_necessary = true;
        private InteractionState State { get; set; }
        private void OnMouseMove(MouseEventArgs e)
        {
            render_necessary = false;
            State ??= new Default(this);
            State = State.OnMouseMove(e);
        }
        private void OnMouseDown(MouseEventArgs e)
        {
            render_necessary = false;
            State ??= new Default(this);
            State = State.OnMouseDown(e);
        }
        private void OnMouseUp(MouseEventArgs e)
        {
            render_necessary = false;
            State ??= new Default(this);
            State = State.OnMouseUp(e);
        }
        private void OnKeyPress(KeyboardEventArgs e)
        {
            State ??= new Default(this);
            State = State.OnKeyPress(e);
        }
        private void OnMouseWheel(WheelEventArgs e)
        {
            NavigationSettings.OnMouseWheel(e);
            Nodes.ReRenderIfOffCanvasChanged();
            Links.TriggerStateHasChanged();
            Overview?.TriggerUpdate(just_pan_or_zoom: true);
        }
        internal void MoveOrigin(double offset_x, double offset_y)
        {
            NavigationSettings.Pan(offset_x, offset_y);
            Nodes.render_not_necessary = true;
            Nodes.ReRenderIfOffCanvasChanged();
            Links.render_not_necessary = true;
            render_necessary = true;
            StateHasChanged();
        }
    }
}