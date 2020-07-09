using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace Excubo.Blazor.Diagrams
{
    public partial class Diagram
    {
        [Inject] private IJSRuntime js { get; set; }
        internal double CanvasLeft { get; private set; }
        internal double CanvasTop { get; private set; }
        internal double CanvasWidth { get; private set; }
        internal double CanvasHeight { get; private set; }
        private async Task GetPositionAsync()
        {
            (CanvasLeft, CanvasTop) = await js.GetPositionAsync(canvas);
            (CanvasWidth, CanvasHeight) = await js.GetDimensionsAsync(canvas);
        }
    }
}
