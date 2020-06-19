using Excubo.Blazor.LazyStyleSheet;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace Excubo.Blazor.Diagrams
{
    //TODO#05 background
    //TODO#04 auto-layout
    //TODO#03 virtualization
    //TODO#02 overview screen
    //TODO#01 common shape nodes (e.g. db, ...)
    //TODO#00 image node
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
