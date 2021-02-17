using Excubo.Blazor.Canvas.Contexts;
using Excubo.Blazor.Diagrams.__Internal;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Excubo.Blazor.Diagrams
{
    public abstract class NodeBase : ComponentBase
    {
        [Parameter] public Action<double> XChanged { get; set; }
        [Parameter] public Action<double> YChanged { get; set; }
        [CascadingParameter] public Diagram Diagram { get; set; }
        [CascadingParameter] public NodeLibrary NodeLibrary { get; set; }
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
        [Parameter] public double MinHeight { get; set; }
        /// <summary>
        /// The minimum width the node should have. (Default: 0).
        /// </summary>
        [Parameter] public double MinWidth { get; set; }
        /// <summary>
        /// The node's content.
        /// </summary>
        [Parameter] public RenderFragment<NodeBase> ChildContent { get; set; }
        [CascadingParameter] public Nodes Nodes { get; set; }
        [CascadingParameter(Name = nameof(IsInternallyGenerated))] public bool IsInternallyGenerated { get; set; }
        /// <summary>
        /// Horizontal position of the node
        /// </summary>
        [Parameter] public double X { get; set; }
        /// <summary>
        /// Vertical position of the node
        /// </summary>
        [Parameter] public double Y { get; set; }
        public abstract RenderFragment border { get; }
        public virtual (double RelativeX, double RelativeY) GetDefaultPort(Position position = Position.Any)
        {
            return (0, 0);
        }
        protected internal double Zoom => (NodeLibrary == null) ? Diagram.NavigationSettings.Zoom : 1; // if we are in the node library, we do not want the nodes to be zoomed.
        protected internal bool OffCanvas { get; set; } = true;
        protected internal virtual (double Left, double Top, double Right, double Bottom) GetDrawingMargins()
        {
            return (0, 0, 0, 0);
        }
        internal abstract double GetWidth();
        internal abstract double GetHeight();
        protected string PositionAndScale => $"translate({(Zoom * X).ToString(CultureInfo.InvariantCulture)} {(Zoom * Y).ToString(CultureInfo.InvariantCulture)}) scale({Zoom.ToString(CultureInfo.InvariantCulture)})";
        public abstract double Width { get; set; }
        public abstract double Height { get; set; }
        internal event Action PositionChanged;
        internal event Action<NodeBase> SizeChanged;
        internal void TriggerPositionChanged() => PositionChanged?.Invoke();
        internal void TriggerSizeChanged() => SizeChanged?.Invoke(this);
        internal void TriggerStateHasChanged() => StateHasChanged();
        internal void ReRenderIfOffCanvasChanged()
        {
            var (LeftMargin, TopMargin, RightMargin, BottomMargin) = GetDrawingMargins();
            var left = X - LeftMargin;
            var right = X + Width + RightMargin;
            var top = Y - TopMargin;
            var bottom = Y + Height + BottomMargin;
            var value = right < Diagram.NavigationSettings.Origin.X
                || bottom < Diagram.NavigationSettings.Origin.Y
                || left > Diagram.NavigationSettings.Origin.X + Diagram.CanvasWidth / Diagram.NavigationSettings.Zoom
                || top > Diagram.NavigationSettings.Origin.Y + Diagram.CanvasHeight / Diagram.NavigationSettings.Zoom;
            if (value != OffCanvas)
            {
                OffCanvas = value;
                StateHasChanged();
                node_border_reference?.TriggerStateHasChanged();
                content_reference?.TriggerStateHasChanged();
            }
        }
        protected RenderFragment<string> actual_border;
        protected RenderFragment<string> content;
        protected NodeContent content_reference;
        protected NodeBorder node_border_reference;
        internal void RemoveBorderAndContent()
        {
            if (actual_border != null)
            {
                Diagram.RemoveNodeBorderFragment(actual_border);
            }
            if (content != null)
            {
                Diagram.RemoveNodeContentFragment(content);
            }
        }
        internal void AddBorderAndContent()
        {
            Diagram.AddNodeContentFragment(content);
            Diagram.AddNodeBorderFragment(actual_border);
        }
        private bool content_added;
        protected void AddNodeContent()
        {
            if (content_added)
            {
                return;
            }
            content_added = true;
            content = GetChildContentWrapper();
            actual_border = GetBorderWrapper();
            if (NodeLibrary == null)
            {
                Diagram.AddNodeContentFragment(content);
                Diagram.AddNodeBorderFragment(actual_border);
            }
            else
            {
                Diagram.AddNodeTemplateContentFragment(content);
            }
        }
        private RenderFragment<string> GetBorderWrapper()
        {
            return (key) => (builder) =>
            {
                builder.OpenComponent<NodeBorder>(0);
                builder.AddAttribute(1, nameof(NodeBorder.ChildContent), border);
                builder.AddAttribute(2, nameof(NodeBorder.Node), this);
                builder.AddComponentReferenceCapture(3, (reference) => node_border_reference = (NodeBorder)reference);
                builder.SetKey(key);
                builder.CloseComponent();
            };
        }
        internal virtual void MoveTo(double x, double y)
        {
            if (!Movable)
            {
                return;
            }
            X = x;
            Y = y;
            TriggerPositionChanged();
            XChanged?.Invoke(X);
            YChanged?.Invoke(Y);
            ReRenderIfOffCanvasChanged();
            StateHasChanged();
        }
        internal virtual void MoveToWithoutUIUpdate(double x, double y)
        {
            if (!Movable)
            {
                return;
            }
            X = x;
            Y = y;
        }
        internal virtual void ApplyMoveTo()
        {
            if (!Movable)
            {
                return;
            }
            TriggerPositionChanged();
            XChanged?.Invoke(X);
            YChanged?.Invoke(Y);
            ReRenderIfOffCanvasChanged();
            StateHasChanged();
        }
        protected bool Selected { get; private set; }
        protected bool Hovered { get; private set; }
        public bool Deleted { get; private set; }
        internal void MarkDeleted() { Deleted = true; }
        internal void MarkUndeleted() { Deleted = false; }
        protected void OnNodeOver(MouseEventArgs _) { Hovered = true; Diagram.SetActiveElement(this, (NodeLibrary == null) ? HoverType.Node : HoverType.NewNode); }
        protected void OnNodeOut(MouseEventArgs _) { Hovered = false; Diagram.DeactivateElement(); }
        protected void OnBorderOver(MouseEventArgs _) { Hovered = true; Diagram.SetActiveElement(this, HoverType.Border); }
        protected void OnBorderOut(MouseEventArgs _) { Hovered = false; Diagram.DeactivateElement(); }
        protected override void OnParametersSet()
        {
            if (!Deleted)
            {
                AddNodeContent();
            }
            if (Nodes != null)
            {
                Nodes.Register(this);
            }
            base.OnParametersSet();
        }
        internal void Select() { Selected = true; StateHasChanged(); }
        internal void Deselect() { Selected = false; StateHasChanged(); }
        internal abstract bool HasSize { get; }
        protected internal virtual async Task DrawShapeAsync(IContext2DWithoutGetters context)
        {
            await context.DrawingRectangles.FillRectAsync(X, Y, GetWidth(), GetHeight());
        }
        protected abstract RenderFragment<string> GetChildContentWrapper();
    }
}