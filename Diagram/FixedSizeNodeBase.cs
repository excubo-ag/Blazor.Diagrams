using Excubo.Blazor.Diagrams.__Internal;
using Microsoft.AspNetCore.Components;
using System;
using System.Globalization;

namespace Excubo.Blazor.Diagrams
{
    public abstract class FixedSizeNodeBase : NodeBase, IDisposable
    {
        protected override RenderFragment<string> GetChildContentWrapper()
        {
            return (key) => (builder) =>
            {
                builder.OpenElement(0, "div");
                builder.AddAttribute(1, "style", $"height: {Height.ToString(CultureInfo.InvariantCulture)}px; width: {Width.ToString(CultureInfo.InvariantCulture)}px;");
                builder.OpenComponent<NodeContent>(2);
                builder.AddAttribute(3, nameof(NodeContent.Node), this as NodeBase);
                if (ChildContent != null)
                {
                    builder.AddAttribute(4, nameof(NodeContent.ChildContent), ChildContent(this));
                }
                builder.AddComponentReferenceCapture(5, (reference) => content_reference = (NodeContent)reference);
                builder.SetKey(key);
                builder.CloseComponent();
                builder.CloseElement();
            };
        }
        internal override bool HasSize => true;
        [Parameter] public override double Width { get; set; }
        [Parameter] public override double Height { get; set; }
        internal override double GetWidth()
        {
            return Width;
        }
        internal override double GetHeight()
        {
            return Height;
        }
        protected override void OnAfterRender(bool first_render)
        {
            if (Deleted)
            {
                Diagram.RemoveNodeBorderFragment(actual_border);
                Diagram.RemoveNodeContentFragment(content);
            }
            if (NodeLibrary != null)
            {
                OffCanvas = false;
                (X, Y, _, _) = NodeLibrary.GetPosition(this);
                content_reference?.TriggerStateHasChanged();
            }
            else
            {
                content_reference?.TriggerStateHasChanged();
                node_border_reference?.TriggerStateHasChanged();
            }
            base.OnAfterRender(first_render);
        }
        public void Dispose()
        {
            Nodes?.Deregister(this);
            RemoveBorderAndContent();
        }
    }
}