using Excubo.Blazor.Diagrams.__Internal;
using Microsoft.AspNetCore.Components;
using System;

namespace Excubo.Blazor.Diagrams
{
    public abstract class ContentSizedNodeBase : NodeBase, IDisposable
    {
        protected override RenderFragment<string> GetChildContentWrapper()
        {
            return (key) => (builder) =>
            {
                builder.OpenComponent<NodeContent>(0);
                builder.AddAttribute(1, nameof(NodeContent.Node), this);
                if (ChildContent != null)
                {
                    builder.AddAttribute(2, nameof(NodeContent.ChildContent), ChildContent(this));
                }
                builder.AddComponentReferenceCapture(3, (reference) => content_reference = (NodeContent)reference);
                builder.SetKey(key);
                builder.CloseComponent();
            };
        }
        private bool has_size;
        private double width = 100;
        private double height = 100;
        public override double Width { get => width; set { if (value == width) { return; } width = Math.Max(MinWidth, value); } }
        public override double Height { get => height; set { if (value == height) { return; } height = Math.Max(MinHeight, value); } }
        internal override bool HasSize => has_size || (HintWidth != null && HintHeight != null);
        [Parameter] public double? HintWidth { get; set; }
        [Parameter] public double? HintHeight { get; set; }
        internal override double GetWidth()
        {
            return HasSize ? Width : (HintWidth ?? Width);
        }
        internal override double GetHeight()
        {
            return HasSize ? Height : (HintHeight ?? Height);
        }
        protected bool Hidden { get; set; } = true;
        protected internal void GetSize((double Width, double Height) result)
        {
            has_size = true;
            (Width, Height) = result;
            ReRenderIfOffCanvasChanged();
            if (NodeLibrary != null)
            {
                OffCanvas = false;
                (X, Y, _, _) = NodeLibrary.GetPosition(this);
                TriggerPositionChanged();
            }
            Hidden = false;
            Diagram.UpdateOverview();
            TriggerSizeChanged();
            node_border_reference?.TriggerStateHasChanged();
            StateHasChanged();
            Diagram?.AutoLayoutSettings?.RunIfRequested();
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