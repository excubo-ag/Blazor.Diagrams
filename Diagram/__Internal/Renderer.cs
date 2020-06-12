using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Excubo.Blazor.Diagrams.__Internal
{
    public class Renderer : ComponentBase
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            foreach (var (key, fragment) in fragments)
            {
                builder.OpenElement(0, Region);
                builder.SetKey(key);
                builder.AddContent(1, fragment);
                builder.CloseElement();
            }
            base.BuildRenderTree(builder);
        }
        private readonly Dictionary<string, RenderFragment> fragments = new Dictionary<string, RenderFragment>();
        public void Add(RenderFragment render_fragment)
        {
            fragments.Add("_" + Guid.NewGuid().ToString(), render_fragment);
            StateHasChanged();
        }
        [Parameter] public string Region { get; set; } = "g";

        internal void Remove(RenderFragment render_fragment)
        {
            if (render_fragment == null)
            {
                return;
            }
            fragments.Remove(fragments.First(f => f.Value == render_fragment).Key);
            StateHasChanged();
        }
    }
}
