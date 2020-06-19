﻿using Excubo.Blazor.LazyStyleSheet;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace Excubo.Blazor.Diagrams
{
    //TODO#06 undo/redo
    //TODO#05 background
    //TODO#04 auto-layout
    //TODO#03 virtualization
    //TODO#02 overview screen
    //TODO#01 common shape nodes (e.g. db, ...)
    //TODO#00 image node
    public partial class Diagram
    {
        #region diagram position
        #endregion
#pragma warning disable S2376, IDE0051
        [Inject] private IStyleSheetService StyleSheetService { set { if (value != null) { value.Add("_content/Excubo.Blazor.Diagrams/style.min.css"); } } }
#pragma warning restore S2376, IDE0051
        protected override async Task OnAfterRenderAsync(bool first_render)
        {
            await GetPositionAsync();
            await base.OnAfterRenderAsync(first_render);
        }
        private bool done_rendering;
        protected override void OnParametersSet()
        {
            NavigationSettings ??= new NavigationSettings { Diagram = this };
            if (Links != null && Nodes != null)
            {
                done_rendering = true;
            }
            base.OnParametersSet();
        }
        protected override bool ShouldRender() => !done_rendering;
    }
}
