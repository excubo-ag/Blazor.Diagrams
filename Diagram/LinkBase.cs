using Microsoft.AspNetCore.Components;
using System;

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
        protected double SourceX => Source.GetX(Links.Diagram);
        protected double SourceY => Source.GetY(Links.Diagram);
        protected double TargetX => Target.GetX(Links.Diagram);
        protected double TargetY => Target.GetY(Links.Diagram);
    }
}
