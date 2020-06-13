using Excubo.Blazor.Diagrams.__Internal;
using Excubo.Blazor.Diagrams.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.ComponentModel;

namespace Excubo.Blazor.Diagrams
{
    public class NodeBase : ComponentBase, INotifyPropertyChanged
    {
        private double x;
        private double y;
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
        internal double CanvasX => Nodes.Diagram.NavigationSettings.Zoom * (X - Nodes.Diagram.NavigationSettings.Origin.X);
        internal double CanvasY => Nodes.Diagram.NavigationSettings.Zoom * (Y - Nodes.Diagram.NavigationSettings.Origin.Y);
        /// <summary>
        /// The node's content.
        /// </summary>
        [Parameter] public RenderFragment<NodeBase> ChildContent { get; set; }
        [CascadingParameter] public Nodes Nodes { get; set; }
        public double Width { get; set; } = 100;
        public double Height { get; set; } = 100;
        public bool Selected { get; private set; }
        protected void OnNodeOver(MouseEventArgs _) => Nodes.Diagram.CurrentlyHoveredNode = (this, HoverType.Node);
        protected void OnNodeOut(MouseEventArgs _) => Nodes.Diagram.CurrentlyHoveredNode = (this, HoverType.Unknown);
        protected void OnBorderOver(MouseEventArgs _) => Nodes.Diagram.CurrentlyHoveredNode = (this, HoverType.Border);
        protected void OnBorderOut(MouseEventArgs _) => Nodes.Diagram.CurrentlyHoveredNode = (this, HoverType.Unknown);
        protected string GetCoordinates()
        {
            return $"{CanvasX} {CanvasY}";
        }
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
        protected NodeContent content_reference;
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
            base.OnAfterRender(first_render);
        }
        protected double Zoom => Nodes.Diagram.NavigationSettings.Zoom;
        #endregion
    }
}
