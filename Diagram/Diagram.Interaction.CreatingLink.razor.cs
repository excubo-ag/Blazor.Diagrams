using Microsoft.AspNetCore.Components.Web;

namespace Excubo.Blazor.Diagrams
{
    public partial class Diagram
    {
        private sealed class CreatingLink : InteractionState
        {
            public CreatingLink(InteractionState previous, NodeBase node, MouseEventArgs e) : base(previous)
            {
                diagram.Links.AddNewLink(node, e, (generated_link) =>
                {
                    diagram.State = new UpdatingLinkTarget(diagram.State, generated_link, e);
                    generated_link.Select();
                    diagram.Overview?.TriggerUpdate();
                });
            }
        }
    }
}