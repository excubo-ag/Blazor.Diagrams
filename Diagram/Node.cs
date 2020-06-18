using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;

namespace Excubo.Blazor.Diagrams
{
    public partial class Node : NodeBase
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            switch (RenderType)
            {
                case NodeType.Diamond:
                    builder.OpenComponent<DiamondNode>(0);
                    break;
                case NodeType.Rectangle:
                    builder.OpenComponent<RectangleNode>(0);
                    break;
                case NodeType.Ellipse:
                    builder.OpenComponent<EllipseNode>(0);
                    break;
                case NodeType.Custom:
                    throw new NotSupportedException("Cannot instantiate a custom node type like this. Write a node component that inherits from NodeBase instead.");
                case NodeType.Default:
                    System.Diagnostics.Debug.Assert(false, "RenderType is guaranteed to be non-default.");
                    break;
            }
            builder.AddAttribute(1, nameof(X), X);
            builder.AddAttribute(2, nameof(XChanged), (Action<double>)((v) => X = v));
            builder.AddAttribute(3, nameof(Y), Y);
            builder.AddAttribute(4, nameof(YChanged), (Action<double>)((v) => Y = v));
            builder.AddAttribute(5, nameof(Id), Id);
            builder.AddAttribute(6, nameof(Fill), Fill);
            builder.AddAttribute(7, nameof(Stroke), Stroke);
            builder.AddAttribute(8, nameof(ChildContent), ChildContent);
            builder.AddComponentReferenceCapture(9, (r) => actual_node = (NodeBase)r);
            builder.CloseComponent();
        }
        internal Type GetImplicitType()
        {
            return RenderType switch
            {
                NodeType.Diamond => typeof(DiamondNode),
                NodeType.Ellipse => typeof(EllipseNode),
                NodeType.Rectangle => typeof(RectangleNode),
                _ => null,
            };
        }
        [Parameter] public NodeType Type { get; set; }
        // the default type is configurable in the diagram's node collection. If the diagram doesn't specify a default type, it's defaulting to rectangle
        private NodeType RenderType
        {
            get
            {
                if (Type != NodeType.Default)
                {
                    return Type;
                }
                if (Nodes.DefaultType != NodeType.Default)
                {
                    return Nodes.DefaultType;
                }
                return NodeType.Rectangle;
            }
        }
        private NodeBase actual_node;
        public override (double RelativeX, double RelativeY) GetDefaultPort()
        {
            if (actual_node == null)
            {
                return base.GetDefaultPort();
            }
            return actual_node.GetDefaultPort();
        }
        public override RenderFragment border => actual_node?.border;
    }
}
