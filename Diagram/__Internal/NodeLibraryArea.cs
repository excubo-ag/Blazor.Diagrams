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
            if (AdditionalAttributes != null && AdditionalAttributes.ContainsKey("class"))
            {
                builder.AddAttribute(2, "class", $"diagram-node-library {AdditionalAttributes["class"]}");
            }
            else
            {
                builder.AddAttribute(2, "class", "diagram-node-library");
            }
            var default_style = Orientation == Orientation.Horizontal ? "width: 100%" : "height: 100%";
            if (AdditionalAttributes != null && AdditionalAttributes.ContainsKey("style"))
            {
                builder.AddAttribute(3, "style", $"{default_style}; {AdditionalAttributes["style"]}");
            }
            else
            {
                builder.AddAttribute(3, "style", default_style);
            }
            if (AdditionalAttributes != null)
            {
                var i = 4;
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
            builder.AddContent(1, ChildContent);
            builder.CloseElement();
        }
        [Parameter] public RenderFragment ChildContent { get; set; }
        [Parameter] public Orientation Orientation { get; set; }
        [Parameter] public Dictionary<string, object> AdditionalAttributes { get; set; }
    }
}
