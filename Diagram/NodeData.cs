using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;

namespace Excubo.Blazor.Diagrams
{
    public class NodeData
    {
        public string Id { get; internal set; } = Guid.NewGuid().ToString();
        public Type Type { get; set; }
        public RenderFragment<NodeBase> ChildContent { get; set; }
        public Dictionary<string, object> Attributes { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public Action<NodeBase> OnCreate { get; internal set; }
    }
}