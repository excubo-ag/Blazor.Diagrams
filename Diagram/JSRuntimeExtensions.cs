using Excubo.Blazor.ScriptInjection;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace Excubo.Blazor.Diagrams
{
    internal static class JSRuntimeExtensions
    {
        private static readonly string @namespace = "Excubo.Diagrams"; // Blazor removed from namespaces, as all JS would be for Blazor anyway.
#if DEBUG
        public static readonly string JsSource = "_content/Excubo.Blazor.Diagrams/script.js";
#else
        public static readonly string JsSource = "_content/Excubo.Blazor.Diagrams/script.min.js";
#endif
        public static Task DiagramJsSourceLoadedAsync(this IScriptInjectionTracker script_injection_tracker)
        {
            return script_injection_tracker.LoadedAsync(JsSource);
        }
        public static async Task<(double Left, double Top)> GetPositionAsync(this IJSRuntime js, ElementReference element)
        {
            var values = await js.InvokeAsync<double[]>($"{@namespace}.GetPosition", element);
            return (Left: values[0], Top: values[1]);
        }
        public static async Task<(double Width, double Height)> GetDimensionsAsync(this IJSRuntime js_runtime, ElementReference element)
        {
            var values = await js_runtime.InvokeAsync<double[]>($"{@namespace}.GetDimensions", element);
            return (Width: values[0], Height: values[1]);
        }
    }
}
