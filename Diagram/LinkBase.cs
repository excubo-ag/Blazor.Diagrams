using Excubo.Blazor.Diagrams.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.ComponentModel;

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
        /// Callback for when the link has been created. This is only invoked for links that are created during interactive usage of the diagram, not for links that are provided declaratively.
        /// </summary>
        [Parameter] public Action<LinkBase> OnCreate { get; set; }
        /// <summary>
        /// Arrow settings for the link. For an arrow at the target, set Arrow.Target, for arrows on both ends, set Arrow.Both. Defaults to Arrow.None.
        /// </summary>
        [Parameter] public Arrow Arrow { get; set; }
        [CascadingParameter] public Links Links { get; set; }
        [CascadingParameter(Name = nameof(IsInternallyGenerated))] public bool IsInternallyGenerated { get; set; }
        public bool Selected { get; private set; }
        protected override void OnParametersSet()
        {
            if (Source?.Node != null)
            {
                Source.Node.PropertyChanged += Node_PropertyChanged;
            }
            if (Target?.Node != null)
            {
                Target.Node.PropertyChanged += Node_PropertyChanged;
            }
            base.OnParametersSet();
        }
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
        private void Node_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            StateHasChanged();
        }
        internal void FixTo(NodeBase node, MouseEventArgs e)
        {
            Target.Node = node;
            Target.Node.PropertyChanged += Node_PropertyChanged;
            Target.RelativeX = e.RelativeXTo(node) / Links.Diagram.NavigationSettings.Zoom;
            Target.RelativeY = e.RelativeYTo(node) / Links.Diagram.NavigationSettings.Zoom;
            Selected = false;
            StateHasChanged();
        }
        internal void UpdateTarget(double x, double y)
        { 
            // the shenanigans of placing the target slightly off from where the cursor actually is, are absolutely crucial:
            // we want to identify whenever the cursor is over the border of a node, hence the cursor must be over the border, not over the currently drawn link!
            // by placing the link slightly off, we make sure that what we see underneath the cursor is not the link, but the border.
            Target.RelativeX = Source.X < x ? x - 1 : x + 1;
            Target.RelativeY = Source.Y < y ? y - 1 : y + 1;
            StateHasChanged();
        }
        internal void TriggerStateHasChanged() => StateHasChanged();
    }
}
