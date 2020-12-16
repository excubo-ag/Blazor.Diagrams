using System;

namespace Excubo.Blazor.Diagrams
{
    public class LinkData
    {
        public NodeAnchor Source { get; set; }
        public NodeAnchor Target { get; set; }
        internal Action<LinkBase> OnCreate { get; set; }
        public Type Type { get; set; }
        public LinkType LinkType { get; set; }
        public Arrow Arrow { get; set; }
    }
}