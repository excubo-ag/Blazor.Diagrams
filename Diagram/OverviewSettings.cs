using Microsoft.AspNetCore.Components;
using System.Diagnostics;

namespace Excubo.Blazor.Diagrams
{
    public class OverviewSettings : ComponentBase
    {
        [Parameter] public Position Position { get; set; } = Position.BottomRight;
        [CascadingParameter] public Diagram Diagram { get; set; }
        protected override void OnParametersSet()
        {
            Debug.Assert(Diagram != null, $"{nameof(OverviewSettings)} are not meant to be used outside a {nameof(Diagram)} component");
            Diagram.OverviewSettings = this;
            base.OnParametersSet();
        }
    }

}
