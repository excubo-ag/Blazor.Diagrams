using Microsoft.AspNetCore.Components.Web;
using System;

namespace Excubo.Blazor.Diagrams
{
    public partial class Diagram
    {
        private class ActiveElementContainer
        {
            internal NodeBase Node { get; private set; }
            internal LinkBase Link { get; private set; }
            internal ControlPoint ControlPoint { get; private set; }
            internal NodeAnchor Anchor { get; private set; }
            internal Point Point { get; private set; }
            internal Point Origin { get; private set; }
            internal void Set(NodeBase node)
            {
                Node = node;
                Link = null;
                ControlPoint = null;
                Anchor = null;
                Point = null;
            }
            internal void Set(LinkBase link)
            {
                Node = null;
                Link = link;
                ControlPoint = null;
                Anchor = null;
                Point = null;
            }
            internal void Set(LinkBase link, ControlPoint control_point)
            {
                Node = null;
                Link = link;
                ControlPoint = control_point;
                Anchor = null;
                Point = null;
            }
            internal void Set(LinkBase link, NodeAnchor anchor)
            {
                Node = null;
                Link = link;
                ControlPoint = null;
                Anchor = anchor;
                Point = null;
            }
            internal void Clear()
            {
                Node = null;
                Link = null;
                ControlPoint = null;
                Anchor = null;
                Point = null;
                Origin = null;
            }
        }
        private ActiveElementContainer ActiveElement { get; } = new ActiveElementContainer();
        internal void SetActiveElement(NodeBase node, HoverType hover_type)
        {
            ActiveElement.Set(node);
            ActiveElementType = hover_type;
        }
        internal void SetActiveElement(LinkBase link, HoverType hover_type)
        {
            ActiveElement.Set(link);
            ActiveElementType = hover_type;
        }
        internal void SetActiveElement(LinkBase link, ControlPoint control_point, HoverType hover_type)
        {
            ActiveElement.Set(link, control_point);
            ActiveElementType = hover_type;
        }
        internal void SetActiveElement(LinkBase link, NodeAnchor anchor, HoverType hover_type)
        {
            ActiveElement.Set(link, anchor);
            ActiveElementType = hover_type;
        }
        internal void DeactivateElement()
        {
            ActiveElement.Clear();
            ActiveElementType = HoverType.Unknown;
        }
        internal HoverType ActiveElementType;
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