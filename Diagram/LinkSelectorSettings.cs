using Microsoft.AspNetCore.Components;
using System.Diagnostics;

namespace Excubo.Blazor.Diagrams
{
    public class LinkSelectorSettings : ComponentBase
    {
        [Parameter] public Position Position { get; set; } = Position.TopRight;
        [CascadingParameter] public Diagram Diagram { get; set; }
        protected override void OnParametersSet()
        {
            Debug.Assert(Diagram != null, $"{nameof(LinkSelectorSettings)} are not meant to be used outside a {nameof(Diagram)} component");
            Diagram.LinkSelectorSettings = this;
            base.OnParametersSet();
        }
    }
}