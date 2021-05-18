using Excubo.Blazor.Canvas.Contexts;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Excubo.Blazor.Diagrams
{
    public class LinkBase : ComponentBase, IDisposable
    {
        /// <summary>
        /// The source anchor for the link.
        /// </summary>
        [Parameter] public NodeAnchor Source { get; set; }
        /// <summary>
        /// The target anchor for the link.
        /// </summary>
        [Parameter] public NodeAnchor Target { get; set; }
        /// <summary>
        /// NOT INTENDED FOR USE BY USERS.
        /// Callback for when the link has been created. This is only invoked for links that are created during interactive usage of the diagram, not for links that are provided declaratively.
        /// </summary>
        [Parameter] public Action<LinkBase> OnCreate { get; set; }
        /// <summary>
        /// Arrow settings for the link. For an arrow at the target, set Arrow.Target, for arrows on both ends, set Arrow.Both. Defaults to Arrow.None.
        /// </summary>
        [Parameter] public Arrow Arrow { get; set; }
        /// <summary>
        /// The color of link and arrows.
        /// </summary>
        [Parameter] public string Color { get; set; } = "black";
        /// <summary>
        /// The width of link.
        /// </summary>
        [Parameter] public double Width { get; set; } = 3;
        /// <summary>
        /// The size of the arrows. If null, then the size is determined in relation to the width of the link.
        /// </summary>
        [Parameter] public double? ArrowSize { get; set; }
        [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object> AdditionalAttributes { get; set; }
        internal void AttachAnchorsTo(NodeBase node)
        {
            if (Source.NodeId == node.Id)
            {
                Source.Node = node;
                node.SizeChanged += SizeChanged;
                FixAnchor(Source);
            }
            if (Target.NodeId == node.Id)
            {
                Target.Node = node;
                node.SizeChanged += SizeChanged;
                FixAnchor(Target);
            }
        }
        protected object Class => AdditionalAttributes?.GetValueOrDefault("class");
        protected object Style => AdditionalAttributes?.GetValueOrDefault("style");
        protected IEnumerable<KeyValuePair<string, object>> other_attributes => AdditionalAttributes == null ? null : AdditionalAttributes.Where(kv => kv.Key != "class" && kv.Key != "style");
        [CascadingParameter] public Links Links { get; set; }
        [CascadingParameter] public Diagram Diagram { get; set; }
        [CascadingParameter(Name = nameof(IsInternallyGenerated))] public bool IsInternallyGenerated { get; set; }
        public bool Selected { get; private set; }
        internal void Select()
        {
            Selected = true;
        }
        internal void Deselect()
        {
            Selected = false;
        }
        internal bool Deleted { get; private set; }
        internal void MarkDeleted() => Deleted = true;
        internal void MarkUndeleted() => Deleted = false;
        protected override void OnParametersSet()
        {
            if (Source != null && Target != null && ControlPointMethods != null)
            {
                if (Source.NodeId != null)
                {
                    Source.Node = Diagram.Nodes.Find(Source.NodeId);
                    if (Source.Node != null)
                    {
                        Source.Node.SizeChanged += SizeChanged;
                    }
                    FixAnchor(Source);
                }
                if (Target.NodeId != null)
                {
                    Target.Node = Diagram.Nodes.Find(Target.NodeId);
                    if (Target.Node != null)
                    {
                        Target.Node.SizeChanged += SizeChanged;
                    }
                    FixAnchor(Target);
                }
                Source.CoordinatesChanged = UpdateControlPoints;
                Target.CoordinatesChanged = UpdateControlPoints;
                if (!ControlPoints.Any())
                {
                    InitializeControlPoints();
                }
                else
                {
                    UpdateControlPoints();
                }
            }
            if (Links.Register(this))
            {
                OnCreate?.Invoke(this);
            }
            base.OnParametersSet();
        }
        internal void TriggerStateHasChanged() => StateHasChanged();
        protected double Zoom => Diagram.NavigationSettings.Zoom;
        #region control points
        protected void OnLinkOver(MouseEventArgs _) => Diagram.SetActiveElement(this);
        protected void OnLinkOut(MouseEventArgs _) => Diagram.DeactivateElement();
        private void OnControlPointOver(ControlPoint control_point) => Diagram.SetActiveElement(control_point);
        private void OnControlPointOut() => Diagram.DeactivateElement();
        private void OnAnchorOver(NodeAnchor anchor) => Diagram.SetActiveElement(anchor);
        private void OnSourceOver(ControlPoint _) => OnAnchorOver(Source);
        private void OnTargetOver(ControlPoint _) => OnAnchorOver(Target);
        private void OnSourceOut() => Diagram.DeactivateElement();
        private void OnTargetOut() => Diagram.DeactivateElement();
        protected List<ControlPoint> ControlPoints = new List<ControlPoint>();
        protected List<Func<(double X, double Y)>> ControlPointMethods;
        private void UpdateControlPoints()
        {
            if (Source.X == ControlPoints.First().X && Source.Y == ControlPoints.First().Y
             && Target.X == ControlPoints.Last().X && Target.Y == ControlPoints.Last().Y)
            {
                return;
            }
            var old_sx = ControlPoints.First().X;
            var old_sy = ControlPoints.First().Y;
            var old_tx = ControlPoints.Last().X;
            var old_ty = ControlPoints.Last().Y;
            (ControlPoints.First().X, ControlPoints.First().Y) = (Source.X, Source.Y);
            for (var i = 0; i < ControlPointMethods.Count; ++i)
            {
                var method = ControlPointMethods[i];
                var cp = ControlPoints[i + 1];
                // If old_sx == old_tx or old_sy == old_ty, we can't infer the position as a ratio the two points, as there is no distance (division by 0).
                // In that case we default back to the default point returned by the appropriate method.
                var new_x = old_sx == old_tx
                    ? method().X
                    : Source.X + (cp.X - old_sx) / (old_tx - old_sx) * (Target.X - Source.X);
                var new_y = old_sy == old_ty
                    ? method().Y
                    : Source.Y + (cp.Y - old_sy) / (old_ty - old_sy) * (Target.Y - Source.Y);
                cp.X = new_x;
                cp.Y = new_y;
            }
            (ControlPoints.Last().X, ControlPoints.Last().Y) = (Target.X, Target.Y);
            StateHasChanged();
        }

        private void InitializeControlPoints()
        {
            ControlPoints.Add(new ControlPoint(OnSourceOver, OnSourceOut) { X = Source.X, Y = Source.Y });
            foreach (var (x, y) in ControlPointMethods.Select(m => m()))
            {
                ControlPoints.Add(new ControlPoint(OnControlPointOver, OnControlPointOut) { X = x, Y = y });
            }
            ControlPoints.Add(new ControlPoint(OnTargetOver, OnTargetOut) { X = Target.X, Y = Target.Y });
        }
        #endregion
        protected internal virtual async Task DrawPathAsync(IContext2DWithoutGetters ctx)
        {
            await ctx.Paths.BeginPathAsync();
            await ctx.Paths.MoveToAsync(ControlPoints.First().X, ControlPoints.First().Y);
            await ctx.Paths.LineToAsync(ControlPoints.Last().X, ControlPoints.Last().Y);
            await ctx.DrawingPaths.StrokeAsync();
        }
        private void SizeChanged(NodeBase node)
        {
            var anchor = Source.Node == node ? Source : Target;
            FixAnchor(anchor);
        }
        private static void FixAnchor(NodeAnchor anchor)
        {
            if (anchor.Port != Position.Any && anchor.Node != null)
            {
                (anchor.RelativeX, anchor.RelativeY) = anchor.Node.GetDefaultPort(anchor.Port);
            }
        }
        public void Dispose()
        {
            if (Source.Node != null)
            {
                Source.Node.SizeChanged -= SizeChanged;
            }
            if (Target.Node != null)
            {
                Target.Node.SizeChanged -= SizeChanged;
            }
            Links.Deregister(this);
        }
    }
}