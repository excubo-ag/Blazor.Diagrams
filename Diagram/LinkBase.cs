using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Excubo.Blazor.Diagrams
{
    public class LinkBase : ComponentBase
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
        protected override void OnAfterRender(bool first_render)
        {
            if (GetType() != typeof(Link))
            {
                if (first_render)
                {
                    if (IsInternallyGenerated)
                    {
                        Links.Add(this);
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
        protected void OnLinkOut(MouseEventArgs _) => ChangeHover(HoverType.Unknown, this);
        protected void OnControlPointOver(ControlPoint control_point) => ChangeHover(HoverType.ControlPoint, control_point);
        protected void OnControlPointOut(ControlPoint control_point) => ChangeHover(HoverType.Unknown, control_point);
        protected List<ControlPoint> ControlPoints = new List<ControlPoint>();
        protected List<Func<(double X, double Y)>> ControlPointMethods;
        protected override void OnParametersSet()
        {
            if (GetType() != typeof(Link) && Source != null && Target != null && ControlPointMethods != null)
            {
                if (!ControlPoints.Any())
                {
                    ControlPoints.Add(new ControlPoint(this, OnControlPointOver, OnControlPointOut) { X = Source.X, Y = Source.Y });
                    foreach (var (x, y) in ControlPointMethods.Select(m => m()))
                    {
                        ControlPoints.Add(new ControlPoint(this, OnControlPointOver, OnControlPointOut) { X = x, Y = y });
                    }
                    ControlPoints.Add(new ControlPoint(this, OnControlPointOver, OnControlPointOut) { X = Target.X, Y = Target.Y });
                }
                else
                {
                    if (Source.X != ControlPoints.First().X || Source.Y != ControlPoints.First().Y
                     || Target.X != ControlPoints.Last().X || Target.Y != ControlPoints.Last().Y)
                    {
                        var old_sx = ControlPoints.First().X;
                        var old_sy = ControlPoints.First().Y;
                        var old_tx = ControlPoints.Last().X;
                        var old_ty = ControlPoints.Last().Y;
                        (ControlPoints[0].X, ControlPoints[0].Y) = (Source.X, Source.Y);
                        if (old_sx == old_tx)
                        {
                            // we can't infer the position as a ratio between old_sx and old_tx as there's no distance. We have to get the default value
                            for (int i = 0; i < ControlPointMethods.Count; ++i)
                            {
                                ControlPoints[i + 1].X = ControlPointMethods[i]().X;
                            }
                        }
                        else
                        {
                            for (int i = 0; i < ControlPointMethods.Count; ++i)
                            {
                                ControlPoints[i + 1].X = Source.X + (ControlPoints[i + 1].X - old_sx) / (old_tx - old_sx) * (Target.X - Source.X);
                            }
                        }
                        if (old_sy == old_ty)
                        {
                            // we can't infer the position as a ratio between old_sy and old_ty as there's no distance. We have to get the default value
                            for (int i = 0; i < ControlPointMethods.Count; ++i)
                            {
                                ControlPoints[i + 1].Y = ControlPointMethods[i]().Y;
                            }
                        }
                        else
                        {
                            for (int i = 0; i < ControlPointMethods.Count; ++i)
                            {
                                ControlPoints[i + 1].Y = Source.Y + (ControlPoints[i + 1].Y - old_sy) / (old_ty - old_sy) * (Target.Y - Source.Y);
                            }
                        }
                        (ControlPoints.Last().X, ControlPoints.Last().Y) = (Target.X, Target.Y);
                    }
                }
            }
            base.OnParametersSet();
        }
        #endregion
    }
}
