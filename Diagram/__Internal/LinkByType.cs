using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Excubo.Blazor.Diagrams.__Internal
{
    public class LinkByType : ComponentBase
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenComponent(0, Data.Type);
            builder.SetKey((Data.Source, Data.Target));
            builder.AddAttribute(1, nameof(LinkBase.Source), Data.Source);
            builder.AddAttribute(2, nameof(LinkBase.Target), Data.Target);
            builder.AddAttribute(3, nameof(LinkBase.OnCreate), Data.OnCreate);
            builder.AddAttribute(4, nameof(LinkBase.Arrow), Data.Arrow);
            builder.CloseComponent();
        }
        [Parameter] public LinkData Data { get; set; }
    }
}