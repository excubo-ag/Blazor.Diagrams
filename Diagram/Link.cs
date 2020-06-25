using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Excubo.Blazor.Diagrams
{
    public class Link : LinkBase
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
            builder.CloseComponent();
        }
        [Parameter] public LinkType Type { get; set; }
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
