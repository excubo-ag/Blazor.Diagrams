using Microsoft.AspNetCore.Components.Web;
using System;

namespace Excubo.Blazor.Diagrams
{
    public partial class Diagram
    {
        private sealed class Panning : InteractionState
        {
            private readonly Point referencePoint;

            public Panning(InteractionState previous, Point referencePoint) : base(previous)
            {
                this.referencePoint = referencePoint;
            }
            public override InteractionState OnMouseMove(MouseEventArgs e)
            {
                Pan(e);
                return this;
            }
            public override InteractionState OnMouseUp(MouseEventArgs e)
            {
                return new Default(this);
            }
            private void Pan(MouseEventArgs e)
            {
                MoveOrigin(e.ClientX - referencePoint.X, e.ClientY - referencePoint.Y);
                (referencePoint.X, referencePoint.Y) = (e.ClientX, e.ClientY);
                diagram.Overview?.TriggerUpdate(just_pan_or_zoom: true);
            }
            private void MoveOrigin(double offset_x, double offset_y)
            {
                diagram.NavigationSettings.Pan(offset_x, offset_y);
                diagram.Nodes.render_not_necessary = true;
                diagram.Nodes.ReRenderIfOffCanvasChanged();
                diagram.Links.render_not_necessary = true;
                diagram.render_necessary = true;
            }
        }
    }
}