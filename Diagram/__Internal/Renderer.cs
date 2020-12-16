using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Excubo.Blazor.Diagrams.__Internal
{
    public class Renderer : ComponentBase, IDisposable
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (disposed)
            {
                return;
            }
            rendering = true;
            foreach (var (fragment, key) in fragments)
            {
                builder.AddContent(0, fragment(key));
            }
            rendering = false;
            bool rendering_required = false;
            if (old_values.Any())
            {
                foreach (var fragment in old_values)
                {
                    fragments.Remove(fragment);
                }
                old_values.Clear();
                rendering_required = true;
            }
            if (new_values.Any())
            {
                foreach (var (fragment, key) in new_values)
                {
                    fragments.Add(fragment, key);
                }
                new_values.Clear();
                rendering_required = true;
            }
            if (rendering_required)
            {
                StateHasChanged();
            }
        }
        private bool rendering;
        private readonly Dictionary<RenderFragment<string>, string> fragments = new Dictionary<RenderFragment<string>, string>();
        private readonly Dictionary<RenderFragment<string>, string> new_values = new Dictionary<RenderFragment<string>, string>();
        private readonly List<RenderFragment<string>> old_values = new List<RenderFragment<string>>();
        public void Add(RenderFragment<string> render_fragment)
        {
            if (disposed)
            {
                return;
            }
            if (rendering)
            {
                new_values.Add(render_fragment, Guid.NewGuid().ToString());
            }
            else
            {
                fragments.Add(render_fragment, Guid.NewGuid().ToString());
                TryStateHasChanged();
            }
        }
        internal void Remove(RenderFragment<string> render_fragment)
        {
            if (disposed)
            {
                return;
            }
            if (rendering)
            {
                old_values.Add(render_fragment);
            }
            else
            {
                _ = fragments.Remove(render_fragment);
                TryStateHasChanged();
            }
        }
        private void TryStateHasChanged()
        {
            try
            {
                StateHasChanged();
            }
            catch
            {
            }
        }
        private bool disposed;
        public void Dispose()
        {
            disposed = true;
        }
    }
}