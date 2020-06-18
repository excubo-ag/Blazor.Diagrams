using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;

namespace Excubo.Blazor.Diagrams.__Internal
{
    public class GeneratedNodes : ComponentBase
    {
        private RenderFragment CreateNodes()
        {
            return (builder) =>
            {
                foreach (var node in Nodes)
                {
                    if (references.ContainsKey(node))
                    {
                        var actual_node = references[node];
                        node.ChildContent = actual_node.ChildContent;
                        node.X = actual_node.X;
                        node.Y = actual_node.Y;
                    }
                    builder.OpenComponent(1, node.Type);
                    builder.AddAttribute(2, nameof(NodeBase.Id), node.Id);
                    builder.AddAttribute(3, nameof(NodeBase.ChildContent), node.ChildContent);
                    builder.AddAttribute(4, nameof(NodeBase.OnCreate), node.OnCreate);
                    builder.AddAttribute(5, nameof(NodeBase.X), node.X);
                    builder.AddAttribute(6, nameof(NodeBase.Y), node.Y);
                    var i = 8;
                    foreach (var (key, value) in node.Attributes)
                    {
                        if (key == nameof(NodeBase.Id)
                        || key == nameof(NodeBase.ChildContent)
                        || key == nameof(NodeBase.OnCreate)
                        || key == nameof(NodeBase.X)
                        || key == nameof(NodeBase.XChanged)
                        || key == nameof(NodeBase.Y)
                        || key == nameof(NodeBase.YChanged))
                        {
                            continue;
                        }
                        builder.AddAttribute(i, key, value);
                        ++i;
                    }
                    builder.SetKey(node.Id);
                    builder.AddComponentReferenceCapture(7, (reference) =>
                    {
                        if (references.ContainsKey(node))
                        {
                            return;
                        }
                        references.Add(node, (NodeBase)reference);
                    });
                    builder.CloseComponent();
                }
            };
        }
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenComponent<CascadingValue<bool>>(0);
            builder.AddAttribute(1, nameof(CascadingValue<bool>.Name), nameof(NodeBase.IsInternallyGenerated));
            builder.AddAttribute(2, nameof(CascadingValue<bool>.Value), true);
            builder.AddAttribute(3, nameof(CascadingValue<bool>.IsFixed), true);
            builder.AddAttribute(4, nameof(CascadingValue<bool>.ChildContent), CreateNodes());
            builder.CloseComponent();
        }
        [Parameter] public List<NodeData> Nodes { get; set; }
        private readonly Dictionary<NodeData, NodeBase> references = new Dictionary<NodeData, NodeBase>();
        internal void TriggerStateHasChanged() => StateHasChanged();
    }
}
