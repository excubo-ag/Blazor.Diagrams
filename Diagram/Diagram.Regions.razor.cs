using System;
using System.Collections.Generic;
using System.Text;

namespace Excubo.Blazor.Diagrams
{
    public partial class Diagram
    {
        internal Links Links { get; set; }
        internal Nodes Nodes { get; set; }
        internal NavigationSettings NavigationSettings { get; set; }
        protected override void OnParametersSet()
        {
            NavigationSettings ??= new NavigationSettings { Diagram = this };
            base.OnParametersSet();
        }
    }
}
