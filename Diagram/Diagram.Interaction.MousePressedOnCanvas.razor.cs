using Microsoft.AspNetCore.Components.Web;
using System;

namespace Excubo.Blazor.Diagrams
{
    public partial class Diagram
    {
        private sealed class MousePressedOnCanvas : InteractionState
        {
            public MousePressedOnCanvas(InteractionState previous) : base(previous) { }
            public override InteractionState OnMouseMove(MouseEventArgs e)
            {
                return new Panning(this, new Point(e.ClientX, e.ClientY));
            }
            public override InteractionState OnMouseUp(MouseEventArgs e)
            {
                diagram.Group.Clear();
                return new Default(this);
            }
        }
    }
}