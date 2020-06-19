using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Excubo.Blazor.Diagrams
{
    public class LinkBase : ComponentBase
    {
        private AssociatedAnchor source;
        private AssociatedAnchor target;
        /// <summary>
        /// The source anchor for the link.
        /// </summary>
        [Parameter] public NodeAnchor Source { get => source?.Anchor; set { source = new AssociatedAnchor(this, value); } }
        /// <summary>
        /// The target anchor for the link.
        /// </summary>
        [Parameter] public NodeAnchor Target { get => target?.Anchor; set { target = new AssociatedAnchor(this, value); } }
        /// <summary>
        /// NOT INTENDED FOR USE BY USERS.
        /// Callback for when the link has been created. This is only invoked for links that are created during interactive usage of the diagram, not for links that are provided declaratively.
        /// </summary>
        [Parameter] public Action<LinkBase> OnCreate { get; set; }
        /// <summary>
        /// Arrow settings for the link. For an arrow at the target, set Arrow.Target, for arrows on both ends, set Arrow.Both. Defaults to Arrow.None.
        /// </summary>
        [Parameter] public Arrow Arrow { get; set; }
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
            if (GetType() != typeof(Link) && Source != null && Target != null && ControlPointMethods != null)
            {
                if (Source.NodeId != null)
                {
                    Source.Node ??= Diagram.Nodes.Find(Source.NodeId);
                    if (Source.Port != Position.Any && Source.Node != null)
                    {
                        (Source.RelativeX, Source.RelativeY) = Source.Node.GetDefaultPort(Source.Port);
                    }
                }
                if (Target.NodeId != null)
                {
                    Target.Node ??= Diagram.Nodes.Find(Target.NodeId);
                    if (Target.Port != Position.Any && Target.Node != null)
                    {
                        (Target.RelativeX, Target.RelativeY) = Target.Node.GetDefaultPort(Target.Port);
                    }
                }
                if (!ControlPoints.Any())
                {
                    InitializeControlPoints();
                }
                else
                {
                    UpdateControlPoints();
                }
            }
            base.OnParametersSet();
        }
        protected override void OnAfterRender(bool first_render)
        {
            if (GetType() != typeof(Link))
            {
                if (first_render)
                {
                    if (IsInternallyGenerated)
                    {
                        Links.Register(this);
                    }
                    OnCreate?.Invoke(this);
                }
            }
            base.OnAfterRender(first_render);
        }
        internal void TriggerStateHasChanged() => StateHasChanged();
        protected double Zoom => Diagram.NavigationSettings.Zoom;
        private void ChangeHover(HoverType hover_type, object @object)
        {
            if (Links != null)
            {
                Diagram.ActiveElement = @object;
                Diagram.ActiveElementType = hover_type;
                StateHasChanged();
            }
        }
        #region control points
        protected void OnLinkOver(MouseEventArgs _) => ChangeHover(HoverType.Link, this);
        protected void OnLinkOut(MouseEventArgs _) => ChangeHover(HoverType.Unknown, null);
        private void OnControlPointOver(ControlPoint control_point) => ChangeHover(HoverType.ControlPoint, control_point);
        private void OnControlPointOut() => ChangeHover(HoverType.Unknown, null);
        private void OnAnchorOver(AssociatedAnchor anchor) => ChangeHover(HoverType.ControlPoint, anchor);
        private void OnAnchorOut() => ChangeHover(HoverType.Unknown, null);
        private void OnSourceOver(ControlPoint _) => OnAnchorOver(source);
        private void OnTargetOver(ControlPoint _) => OnAnchorOver(target);
        private void OnSourceOut() => OnAnchorOut();
        private void OnTargetOut() => OnAnchorOut();
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
                    : Source.X + (ControlPoints[i + 1].X - old_sx) / (old_tx - old_sx) * (Target.X - Source.X);
                var new_y = old_sy == old_ty 
                    ? method().Y
                    : Source.Y + (ControlPoints[i + 1].Y - old_sy) / (old_ty - old_sy) * (Target.Y - Source.Y);
                cp.X = new_x;
                cp.Y = new_y;
            }
            (ControlPoints.Last().X, ControlPoints.Last().Y) = (Target.X, Target.Y);
        }

        private void InitializeControlPoints()
        {
            ControlPoints.Add(new ControlPoint(this, OnSourceOver, OnSourceOut) { X = Source.X, Y = Source.Y });
            foreach (var (x, y) in ControlPointMethods.Select(m => m()))
            {
                ControlPoints.Add(new ControlPoint(this, OnControlPointOver, OnControlPointOut) { X = x, Y = y });
            }
            ControlPoints.Add(new ControlPoint(this, OnTargetOver, OnTargetOut) { X = Target.X, Y = Target.Y });
        }
        #endregion
    }
}
