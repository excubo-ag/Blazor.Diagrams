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
        public static async Task<double[]> GetPositionAsync(this IJSRuntime js, ElementReference element)
        {
            return await js.InvokeAsync<double[]>($"{@namespace}.GetPosition", element);
        }
        public static async Task<double[]> GetDimensionsAsync(this IJSRuntime js_runtime, ElementReference element)
        {
            return await js_runtime.InvokeAsync<double[]>($"{@namespace}.GetDimensions", element);
        }
    }
}
