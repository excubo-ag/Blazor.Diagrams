using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Excubo.Blazor.Diagrams
{
    public class NodeBorder : ComponentBase
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (Node.OffCanvas)
            {
                return;
            }
            builder.OpenElement(0, "g");
            builder.AddAttribute(1, "style", "pointer-events: visiblepainted");
            builder.AddContent(2, ChildContent);
            builder.CloseElement();
        }
        [Parameter] public RenderFragment ChildContent { get; set; }
        [Parameter] public NodeBase Node { get; set; }
        internal void TriggerStateHasChanged() => StateHasChanged();
    }
}