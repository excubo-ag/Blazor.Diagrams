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
    }
}
