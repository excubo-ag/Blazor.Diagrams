using Microsoft.AspNetCore.Components;
using System;

namespace Excubo.Blazor.Diagrams
{
    public partial class Diagram
    {
        /// <summary>
        /// Callback that is executed if a group is removed. Beware of double processing when using OnRemove on nodes/links as well.
        /// </summary>
        [Parameter] public Action<Group> OnRemove { get; set; }
        /// <summary>
        /// Callback that is executed whenever the selection of nodes and/or links changes.
        /// </summary>
        [Parameter] public Action<Group> SelectionChanged { get; set; }
        /// <summary>
        /// Whether to show grid lines on the diagram
        /// </summary>
        [Parameter] public bool ShowGridLines { get; set; }
        /// <summary>
        /// The distance between two adjacent grid lines (only shown when ShowGridLines is set to true). Default value is 100.
        /// </summary>
        [Parameter] public int GridLineDistance { get; set; } = 100;
    }
}
