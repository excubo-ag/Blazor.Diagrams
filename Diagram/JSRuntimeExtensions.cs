using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace Excubo.Blazor.Diagrams
{
    internal static class JSRuntimeExtensions
    {
        private static readonly string @namespace = typeof(JSRuntimeExtensions).Namespace;
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
