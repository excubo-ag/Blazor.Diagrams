using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;

namespace Excubo.Blazor.Diagrams
{
    public class Link : ComponentBase
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            switch (RenderType)
            {
                case LinkType.Angled:
                    builder.OpenComponent<AngledLink>(0);
                    break;
                case LinkType.Curved:
                    builder.OpenComponent<CurvedLink>(0);
                    break;
                case LinkType.Straight:
                    builder.OpenComponent<StraightLink>(0);
                    break;
                case LinkType.Default:
                    System.Diagnostics.Debug.Assert(false, "RenderType is guaranteed to be non-default.");
                    break;
            }
            builder.AddAttribute(1, nameof(Source), Source);
            builder.AddAttribute(2, nameof(Target), Target);
            builder.AddAttribute(3, nameof(Arrow), RenderArrow);
            if (OnCreate != null)
            {
                builder.AddAttribute(4, nameof(OnCreate), OnCreate);
            }
            builder.AddAttribute(5, nameof(Color), Color);
            builder.AddAttribute(6, nameof(Width), Width);
            builder.AddAttribute(7, nameof(ArrowSize), ArrowSize);
            builder.AddAttribute(8, nameof(AdditionalAttributes), AdditionalAttributes);
            builder.CloseComponent();
        }
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
        [Parameter] public LinkType Type { get; set; }
        [CascadingParameter] public Links Links { get; set; }
        [CascadingParameter] public Diagram Diagram { get; set; }
        private LinkType render_type;
        private LinkType RenderType
        {
            get
            {
                if (render_type == LinkType.Default)
                {
                    if (Type != LinkType.Default)
                    {
                        render_type = Type;
                    }
                    else if (Links.DefaultType != LinkType.Default)
                    {
                        render_type = Links.DefaultType;
                    }
                    else
                    {
                        render_type = LinkType.Straight;
                    }
                }
                return render_type;
            }
        }
        private Arrow render_arrow;
        private Arrow RenderArrow
        {
            get
            {
                if (render_arrow == Arrow.Default)
                {
                    if (Arrow != Arrow.Default)
                    {
                        render_arrow = Arrow;
                    }
                    else if (Links.DefaultArrow != Arrow.Default)
                    {
                        render_arrow = Links.DefaultArrow;
                    }
                    else
                    {
                        render_arrow = Arrow.None;
                    }
                }
                return render_arrow;
            }
        }
    }
}