using Microsoft.AspNetCore.Components.Web;

namespace Excubo.Blazor.Diagrams
{
    public partial class Diagram
    {
        private class InteractionState
        {
            protected readonly Diagram diagram;
            public InteractionState(Diagram diagram)
            {
                this.diagram = diagram;
            }
            public InteractionState(InteractionState previous)
            {
                diagram = previous.diagram;
            }

            public virtual InteractionState OnKeyPress(KeyboardEventArgs e)
            {
                if (e.Key == "z" && e.CtrlKey && !e.ShiftKey)
                {
                    diagram.Changes.Undo();
                    diagram.Overview?.TriggerUpdate();
                }
                else if ((e.Key == "Z" && e.CtrlKey && e.ShiftKey)
                      || (e.Key == "y" && e.CtrlKey && !e.ShiftKey))
                {
                    diagram.Changes.Redo();
                    diagram.Overview?.TriggerUpdate();
                }
                return this;
            }

            public virtual InteractionState OnMouseMove(MouseEventArgs e) => this;
            public virtual InteractionState OnMouseUp(MouseEventArgs e) => this;
            public virtual InteractionState OnMouseDown(MouseEventArgs e) => this;
        }
    }
}
