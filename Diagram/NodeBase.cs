using Excubo.Blazor.Diagrams.__Internal;
using Excubo.Blazor.Diagrams.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.ComponentModel;

namespace Excubo.Blazor.Diagrams
{
    public abstract class NodeBase : ComponentBase, INotifyPropertyChanged
    {
        private double x;
        private double y;
        private double width = 100;
        private double height = 100;
        public event PropertyChangedEventHandler PropertyChanged;
        private void TriggerPropertyChanged(string property_name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property_name));
        }
        /// <summary>
        /// Unique Id of the node
        /// </summary>
        [Parameter] public string Id { get; set; }
        /// <summary>
        /// Horizontal position of the node
        /// </summary>
        [Parameter] public double X { get => x; set { if (value == x) { return; } x = value; TriggerPropertyChanged(nameof(X)); } }
        /// <summary>
        /// Vertical position of the node
        /// </summary>
        [Parameter] public double Y { get => y; set { if (value == y) { return; } y = value; TriggerPropertyChanged(nameof(Y)); } }
        /// <summary>
        /// The fill color of the node
        /// </summary>
        [Parameter] public string Fill { get; set; } = "#f5f5f5";
        /// <summary>
        /// The stroke color of the node
        /// </summary>
        [Parameter] public string Stroke { get; set; } = "#e8e8e8";
        internal double CanvasX => Nodes.Diagram.NavigationSettings.Zoom * X;
        internal double CanvasY => Nodes.Diagram.NavigationSettings.Zoom * Y;
        /// <summary>
        /// The node's content.
        /// </summary>
        [Parameter] public RenderFragment<NodeBase> ChildContent { get; set; }
        [CascadingParameter] public Nodes Nodes { get; set; }
        public double Width { get => width; set { if (value == width) { return; } width = value; TriggerPropertyChanged(nameof(Width)); } }
        public double Height { get => height; set { if (value == height) { return; } height = value; TriggerPropertyChanged(nameof(Height)); } }
        public bool Selected { get; private set; }
        public bool Hovered { get; private set; }
        protected void OnNodeOver(MouseEventArgs _) { Hovered = true;  Nodes.Diagram.CurrentlyHoveredNode = (this, HoverType.Node); StateHasChanged(); }
        protected void OnNodeOut(MouseEventArgs _) { Hovered = false; Nodes.Diagram.CurrentlyHoveredNode = (this, HoverType.Unknown); StateHasChanged(); }
        protected void OnBorderOver(MouseEventArgs _) { Hovered = true; Nodes.Diagram.CurrentlyHoveredNode = (this, HoverType.Border); StateHasChanged(); }
        protected void OnBorderOut(MouseEventArgs _) { Hovered = false; Nodes.Diagram.CurrentlyHoveredNode = (this, HoverType.Unknown); StateHasChanged(); }
        protected string NodePositionAndScale => $"translate({CanvasX} {CanvasY}) scale({Zoom})";
        public void UpdatePosition(double x, double y)
        {
            X = x;
            Y = y;
            StateHasChanged();
        }
        internal void TriggerStateHasChanged() => StateHasChanged();

        protected override void OnParametersSet()
        {
            if (GetType() != typeof(Node)) // Node type is just a wrapper for the actual node, so adding this would prevent the actual node from being recognised.
            {
                AddNodeContent();
                Nodes.Add(this);
            }
            base.OnParametersSet();
        }
        #region node content

        private bool content_added;
        private void AddNodeContent()
        {
            if (content_added)
            {
                return;
            }
            content_added = true;
            Nodes.Diagram.AddNodeContentFragment(GetChildContentWrapper());
            Nodes.Diagram.AddNodeBorderFragment(node_border);
        }
        protected RenderFragment GetChildContentWrapper()
        {
            return (builder) =>
            {
                builder.OpenComponent<NodeContent>(0);
                builder.AddAttribute(1, nameof(NodeContent.X), X);
                builder.AddAttribute(2, nameof(NodeContent.Y), Y);
                builder.AddAttribute(3, nameof(NodeContent.Width), Width);
                builder.AddAttribute(4, nameof(NodeContent.Height), Height);
                builder.AddAttribute(5, nameof(NodeContent.Zoom), Zoom);
                builder.AddAttribute(6, nameof(NodeContent.ChildContent), ChildContent(this));
                builder.AddAttribute(7, nameof(NodeContent.SizeCallback), (Action<double[]>)GetSize);
                builder.AddComponentReferenceCapture(8, (reference) => content_reference = (NodeContent)reference);
                builder.CloseComponent();
            };
        }
        internal void Select()
        {
            Selected = true;
            StateHasChanged();
        }
        internal void Deselect()
        {
            Selected = false;
            StateHasChanged();
        }
        protected bool Hidden { get; set; } = true;
        protected void GetSize(double[] result)
        {
            Width = result[0];
            Height = result[1];
            Hidden = false;
            StateHasChanged();
        }
        protected override void OnAfterRender(bool first_render)
        {
            content_reference?.TriggerRender(
                CanvasX,
                CanvasY,
                Width,
                Height,
                Nodes.Diagram.NavigationSettings.Zoom);
            node_border_reference?.TriggerStateHasChanged();
            base.OnAfterRender(first_render);
        }
        protected double Zoom => Nodes.Diagram.NavigationSettings.Zoom;
        #endregion
        public virtual (double RelativeX, double RelativeY) GetDefaultPort()
        {
            return (0, 0);
        }
        public abstract RenderFragment node_border { get; }
        protected NodeContent content_reference;
        protected NodeBorder node_border_reference;
    }
}
