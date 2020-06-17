using Excubo.Blazor.Diagrams.__Internal;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;

namespace Excubo.Blazor.Diagrams
{
    public abstract class NodeBase : ComponentBase
    {
        private double x;
        private double y;
        private double width = 100;
        private double height = 100;
        /// <summary>
        /// Unique Id of the node
        /// </summary>
        [Parameter] public string Id { get; set; }
        /// <summary>
        /// Horizontal position of the node
        /// </summary>
        [Parameter] public double X { get => x; set { x = value; XChanged?.Invoke(x); } }
        [Parameter] public Action<double> XChanged { get; set; }
        /// <summary>
        /// Vertical position of the node
        /// </summary>
        [Parameter] public double Y { get => y; set { y = value; YChanged?.Invoke(y); } }
        [Parameter] public Action<double> YChanged { get; set; }
        /// <summary>
        /// The fill color of the node
        /// </summary>
        [Parameter] public string Fill { get; set; } = "#f5f5f5";
        /// <summary>
        /// The stroke color of the node
        /// </summary>
        [Parameter] public string Stroke { get; set; } = "#e8e8e8";
        /// <summary>
        /// The node's content.
        /// </summary>
        [Parameter] public RenderFragment<NodeBase> ChildContent { get; set; }
        /// <summary>
        /// NOT INTENDED FOR USE BY USERS.
        /// Callback for when the node has been created. This is only invoked for nodes that are created during interactive usage of the diagram, not for nodes that are provided declaratively.
        /// </summary>
        [Parameter] public Action<NodeBase> OnCreate { get; set; }
        [CascadingParameter] public Nodes Nodes { get; set; }
        [CascadingParameter] public NodeLibrary NodeLibrary { get; set; }
        private Diagram Diagram => Nodes?.Diagram ?? NodeLibrary?.Diagram;
        [CascadingParameter(Name = nameof(IsInternallyGenerated))] public bool IsInternallyGenerated { get; set; }
        public double Width { get => width; set { if (value == width) { return; } width = value; } }
        public double Height { get => height; set { if (value == height) { return; } height = value; } }
        public bool Selected { get; private set; }
        public bool Hovered { get; private set; }
        private void ChangeHover(HoverType hover_type)
        {
            Hovered = hover_type == HoverType.Node || hover_type == HoverType.Border;
            if (Nodes != null)
            {
                Diagram.CurrentlyHoveredNode = (this, hover_type);
                StateHasChanged();
            }
            if (NodeLibrary != null)
            {
                Diagram.CurrentlyHoveredNode = (this, HoverType.NewNode);
            }
        }
        protected void OnNodeOver(MouseEventArgs _) => ChangeHover(HoverType.Node);
        protected void OnNodeOut(MouseEventArgs _) => ChangeHover(HoverType.Unknown);
        protected void OnBorderOver(MouseEventArgs _) => ChangeHover(HoverType.Border);
        protected void OnBorderOut(MouseEventArgs _) => ChangeHover(HoverType.Unknown);
        protected string NodePositionAndScale => $"translate({Zoom * X} {Zoom * Y}) scale({Zoom})";
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
                if (Nodes != null)
                {
                    Nodes.Add(this);
                }
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
            if (NodeLibrary == null)
            {
                Diagram.AddNodeContentFragment(GetChildContentWrapper());
                Diagram.AddNodeBorderFragment(node_border);
            }
            else
            {
                Diagram.AddNodeTemplateContentFragment(GetChildContentWrapper());
            }
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
                if (ChildContent != null)
                {
                    builder.AddAttribute(6, nameof(NodeContent.ChildContent), ChildContent(this));
                }
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
            if (NodeLibrary != null)
            {
                (X, Y, _, _) = NodeLibrary.GetPosition(this);
            }
            Hidden = false;
            StateHasChanged();
        }
        protected override void OnAfterRender(bool first_render)
        {
            if (first_render)
            {
                if (GetType() != typeof(Node))
                {
                    OnCreate?.Invoke(this);
                }
            }
            if (NodeLibrary != null)
            {
                content_reference?.TriggerRender(
                    X,
                    Y,
                    Width,
                    Height,
                    1);
            }
            else
            {
                content_reference?.TriggerRender(
                    Zoom * X,
                    Zoom * Y,
                    Width,
                    Height,
                    Diagram.NavigationSettings.Zoom);
                node_border_reference?.TriggerStateHasChanged();
            }
            base.OnAfterRender(first_render);
        }
        protected double Zoom => Nodes?.Diagram.NavigationSettings.Zoom ?? 1;
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
