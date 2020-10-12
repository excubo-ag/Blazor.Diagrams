using System.Threading.Tasks;

namespace Excubo.Blazor.Diagrams
{
    public partial class Diagram
    {
        protected override async Task OnAfterRenderAsync(bool first_render)
        {
            if (first_render)
            {
                await js.InitializeJsAsync();
                if (AutoLayoutSettings != null)
                {
                    AutoLayoutSettings.Run();
                }
            }
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
            Overview?.TriggerUpdate();
            StateHasChanged();
        }
    }
}
