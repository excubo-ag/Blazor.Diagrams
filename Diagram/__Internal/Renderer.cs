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
                builder.AddContent(0, fragment(key));
            }
        }
        private readonly Dictionary<RenderFragment<string>, string> fragments = new Dictionary<RenderFragment<string>, string>();
        public void Add(RenderFragment<string> render_fragment)
        {
            fragments.Add(render_fragment, Guid.NewGuid().ToString());
            StateHasChanged();
        }
        internal void Remove(RenderFragment<string> render_fragment)
        {
            _ = fragments.Remove(render_fragment);
            StateHasChanged();
        }
    }
}
