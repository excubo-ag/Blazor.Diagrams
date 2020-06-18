using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;

namespace Excubo.Blazor.Diagrams.__Internal
{
    public class Renderer : ComponentBase
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            foreach (var (fragment, key) in fragments)
            {
                builder.OpenElement(0, Region);
                builder.SetKey(key);
                builder.AddContent(1, fragment);
                builder.CloseElement();
            }
        }
        private readonly Dictionary<RenderFragment, string> fragments = new Dictionary<RenderFragment, string>();
        public void Add(RenderFragment render_fragment)
        {
            fragments.Add(render_fragment, Guid.NewGuid().ToString());
            StateHasChanged();
        }
        [Parameter] public string Region { get; set; } = "g";

        internal void Remove(RenderFragment render_fragment)
        {
            _ = fragments.Remove(render_fragment);
            StateHasChanged();
        }
    }
}
