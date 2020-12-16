using Microsoft.AspNetCore.Components;
using System.Diagnostics;

namespace Excubo.Blazor.Diagrams
{
    public class OverviewSettings : ComponentBase
    {
        /// <summary>
        /// The border color for the rectangle that marks the viewable area.
        /// </summary>
        [Parameter] public string ViewableAreaBorderColor { get; set; } = "deepskyblue";
        /// <summary>
        /// The fill color for the rectangle that marks the viewable area.
        /// </summary>
        [Parameter] public string ViewableAreaFillColor { get; set; } = "white";
        /// <summary>
        /// The thickness of the border for the rectangle that marks the viewable area.
        /// </summary>
        [Parameter] public double ViewableAreaBorderWidth { get; set; } = 4;
        [Parameter] public string BackgroundColor { get; set; } = "white";
        /// <summary>
        /// Whether the overview area should have a visible black border all around it.
        /// </summary>
        [Parameter] public bool FullBorder { get; set; }
        /// <summary>
        /// The thickness of the border of the overview area.
        /// </summary>
        [Parameter] public double BorderWidth { get; set; } = 2;
        [Parameter] public Position Position { get; set; } = Position.BottomRight;
        [CascadingParameter] public Diagram Diagram { get; set; }
        protected override void OnParametersSet()
        {
            Debug.Assert(Diagram != null, $"{nameof(OverviewSettings)} are not meant to be used outside a {nameof(Diagram)} component");
            Diagram.OverviewSettings = this;
            base.OnParametersSet();
        }
    }

}