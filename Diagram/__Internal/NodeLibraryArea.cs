using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Collections.Generic;

namespace Excubo.Blazor.Diagrams.__Internal
{
    public class NodeLibraryArea : ComponentBase
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "svg");
            var default_style = Orientation == Orientation.Horizontal
                ? "position: absolute; top: 0; left: 0; width: 100%"
                : "position: absolute; top: 0; left: 0; height: 100%";
            if (AdditionalAttributes != null && AdditionalAttributes.ContainsKey("style"))
            {
                builder.AddAttribute(1, "style", $"{default_style}; {AdditionalAttributes["style"]}");
            }
            else
            {
                builder.AddAttribute(1, "style", default_style);
            }
            if (AdditionalAttributes != null)
            {
                var i = 2;
                foreach (var (key, value) in AdditionalAttributes)
                {
                    if (key == "class"
                     || key == "style")
                    {
                        continue;
                    }
                    builder.AddAttribute(i, key, value);
                    i++;
                }
            }
            builder.AddContent(999, ChildContent);
            builder.CloseElement();
        }
        [Parameter] public RenderFragment ChildContent { get; set; }
        /// <summary>
        /// Orientation of the node library (defaults to Horizontal).
        /// </summary>
        [Parameter] public Orientation Orientation { get; set; }
        [Parameter] public Dictionary<string, object> AdditionalAttributes { get; set; }
    }
}