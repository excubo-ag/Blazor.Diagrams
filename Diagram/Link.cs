using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;

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
                case LinkType.Custom:
                    throw new NotSupportedException("Cannot instantiate a custom link type like this. Write a link component that inherits from LinkBase instead.");
            }
            builder.AddAttribute(1, nameof(Source), Source);
            builder.AddAttribute(2, nameof(Target), Target);
            builder.CloseComponent();
        }
        [Parameter] public LinkType Type { get; set; }
        private LinkType RenderType
        {
            get
            {
                if (Type != LinkType.Default)
                {
                    return Type;
                }
                if (Links.DefaultType != LinkType.Default)
                {
                    return Links.DefaultType;
                }
                return LinkType.Straight;
            }
        }
    }
}
