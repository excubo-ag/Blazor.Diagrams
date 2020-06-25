using Excubo.Blazor.LazyStyleSheet;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace Excubo.Blazor.Diagrams
{
    public partial class Diagram
    {
#pragma warning disable S2376, IDE0051
        [Inject] private IStyleSheetService StyleSheetService { set { if (value != null) { value.Add("_content/Excubo.Blazor.Diagrams/style.min.css"); } } }
#pragma warning restore S2376, IDE0051
        protected override async Task OnAfterRenderAsync(bool first_render)
        {
            if (Links != null && Nodes != null && render_cycles < 2)
            {
                ++render_cycles;
                await GetPositionAsync();
                await InvokeAsync(ReRender);
            }
            await base.OnAfterRenderAsync(first_render);
        }
        private int render_cycles = 0;
        private void ReRender()
        {
            Nodes.TriggerStateHasChanged();
            Links.TriggerStateHasChanged();
            Links.Redraw();
            StateHasChanged();
        }
    }
}
