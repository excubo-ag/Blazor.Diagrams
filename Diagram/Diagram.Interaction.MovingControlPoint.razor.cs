using Excubo.Blazor.Diagrams.Extensions;
using Microsoft.AspNetCore.Components.Web;

namespace Excubo.Blazor.Diagrams
{
    public partial class Diagram
    {
        private sealed class MovingControlPoint : InteractionState
        {
            private readonly ModifyingLink previous;
            private readonly LinkBase link;
            private readonly ControlPoint controlPoint;
            private readonly Point originalCoordinates;
            private readonly Point referencePoint;

            public MovingControlPoint(ModifyingLink previous, LinkBase link, ControlPoint controlPoint, MouseEventArgs e) : base(previous)
            {
                this.previous = previous;
                this.link = link;
                this.controlPoint = controlPoint;
                originalCoordinates = new Point(controlPoint.X, controlPoint.Y);
                referencePoint = new Point(e.ClientX, e.ClientY);
            }
            public override InteractionState OnMouseMove(MouseEventArgs e)
            {
                MoveControlPoint(e);
                return this;
            }
            public override InteractionState OnMouseUp(MouseEventArgs e)
            {
                CommitChanges();
                return previous;
            }
            private void MoveControlPoint(MouseEventArgs e)
            {
                var delta_x = e.RelativeXTo(referencePoint) / diagram.NavigationSettings.Zoom;
                var delta_y = e.RelativeYTo(referencePoint) / diagram.NavigationSettings.Zoom;
                controlPoint.X += delta_x;
                controlPoint.Y += delta_y;
                (referencePoint.X, referencePoint.Y) = (e.ClientX, e.ClientY);
                diagram.Overview?.TriggerUpdate();
                link.TriggerStateHasChanged();
            }
            private void CommitChanges()
            {
                var (new_x, new_y) = (controlPoint.X, controlPoint.Y);
                var (old_x, old_y) = (originalCoordinates.X, originalCoordinates.Y);
                diagram.Changes.New(new ChangeAction(() => { (controlPoint.X, controlPoint.Y) = (new_x, new_y); }, () => { (controlPoint.X, controlPoint.Y) = (old_x, old_y); }));
            }
        }
    }
}