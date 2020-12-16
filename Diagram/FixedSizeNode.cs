using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;

namespace Excubo.Blazor.Diagrams
{
    public partial class FixedSizeNode : ComponentBase
    {
        #region properties
        /// <summary>
        /// Horizontal position of the node
        /// </summary>
        [Parameter] public double X { get; set; }
        [Parameter] public Action<double> XChanged { get; set; }
        /// <summary>
        /// Vertical position of the node
        /// </summary>
        [Parameter] public double Y { get; set; }
        [Parameter] public Action<double> YChanged { get; set; }
        [Parameter] public bool Movable { get; set; } = true;
        /// <summary>
        /// Unique Id of the node
        /// </summary>
        [Parameter] public string Id { get; set; }
        /// <summary>
        /// The fill color of the node
        /// </summary>
        [Parameter] public string Fill { get; set; } = "#f5f5f5";
        /// <summary>
        /// The stroke color of the node
        /// </summary>
        [Parameter] public string Stroke { get; set; } = "#e8e8e8";
        /// <summary>
        /// Any child content of the node is placed inside a div. To apply CSS classes to that div, use this property.
        /// </summary>
        [Parameter] public string ContentClasses { get; set; }
        /// <summary>
        /// Any child content of the node is placed inside a div. To apply CSS style to that div, use this property.
        /// </summary>
        [Parameter] public string ContentStyle { get; set; }
        /// <summary>
        /// The minimum height the node should have. (Default: 0).
        /// </summary>
        [Parameter] public double Height { get; set; }
        /// <summary>
        /// The minimum width the node should have. (Default: 0).
        /// </summary>
        [Parameter] public double Width { get; set; }
        /// <summary>
        /// The node's content.
        /// </summary>
        [Parameter] public RenderFragment<NodeBase> ChildContent { get; set; }
        [CascadingParameter] public Nodes Nodes { get; set; }
        #endregion
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            switch (RenderType)
            {
                case NodeType.Diamond:
                    builder.OpenComponent<FixedSizeDiamondNode>(0);
                    break;
                case NodeType.Rectangle:
                    builder.OpenComponent<FixedSizeRectangleNode>(0);
                    break;
                case NodeType.Ellipse:
                    builder.OpenComponent<FixedSizeEllipseNode>(0);
                    break;
                case NodeType.Default:
                    System.Diagnostics.Debug.Assert(false, "RenderType is guaranteed to be non-default.");
                    break;
            }
            builder.AddAttribute(1, nameof(NodeBase.X), X);
            builder.AddAttribute(2, nameof(NodeBase.XChanged), XChanged);
            builder.AddAttribute(3, nameof(NodeBase.Y), Y);
            builder.AddAttribute(4, nameof(NodeBase.YChanged), YChanged);
            builder.AddAttribute(5, nameof(NodeBase.Id), Id);
            builder.AddAttribute(6, nameof(NodeBase.Fill), Fill);
            builder.AddAttribute(7, nameof(NodeBase.Stroke), Stroke);
            builder.AddAttribute(8, nameof(FixedSizeNodeBase.Width), Width);
            builder.AddAttribute(9, nameof(FixedSizeNodeBase.Height), Height);
            builder.AddAttribute(10, nameof(NodeBase.ChildContent), ChildContent);
            builder.AddAttribute(11, nameof(NodeBase.ContentClasses), ContentClasses);
            builder.AddAttribute(12, nameof(NodeBase.ContentStyle), ContentStyle);
            builder.AddAttribute(13, nameof(NodeBase.Movable), Movable);
            builder.CloseComponent();
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
    }
}