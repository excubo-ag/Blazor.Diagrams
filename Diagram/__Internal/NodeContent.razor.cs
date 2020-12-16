using Microsoft.AspNetCore.Components;

namespace Excubo.Blazor.Diagrams.__Internal
{
    /// <summary>
    /// INTERNAL component, do not use as a user of Excubo.Blazor.Diagrams!
    /// </summary>
    public partial class NodeContent : ComponentBase
    {
        [Parameter] public NodeBase Node { get; set; }
        /// <summary>
        /// Actual content
        /// </summary>
        [Parameter] public RenderFragment ChildContent { get; set; }
        public void TriggerStateHasChanged()
        {
            StateHasChanged();
        }
    }
}