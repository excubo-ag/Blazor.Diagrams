using Excubo.Blazor.ScriptInjection;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace Excubo.Blazor.Diagrams
{
    public partial class Diagram
    {
        [Inject] private IJSRuntime js { get; set; }
        [Inject] private IScriptInjectionTracker script_injection_tracker { get; set; }
        internal double CanvasLeft { get; private set; }
        internal double CanvasTop { get; private set; }
        internal double CanvasWidth { get; private set; }
        internal double CanvasHeight { get; private set; }
        private async Task GetPositionAsync()
        {
            await script_injection_tracker.DiagramJsSourceLoadedAsync();
            var values = await js.GetPositionAsync(canvas);
            CanvasLeft = values[0];
            CanvasTop = values[1];
            values = await js.GetDimensionsAsync(canvas);
            CanvasWidth = values[0];
            CanvasHeight = values[1];
        }
    }
}
