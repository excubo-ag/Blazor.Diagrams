using System;
using System.Collections.Generic;

namespace Excubo.Blazor.Diagrams
{
    public class Group
    {
        public event EventHandler ContentChanged;
        public List<NodeBase> Nodes { get; } = new List<NodeBase>();
        public List<LinkBase> Links { get; } = new List<LinkBase>();

        internal void Add(NodeBase node)
        {
            Nodes.Add(node);
            ContentChanged?.Invoke(this, EventArgs.Empty);
        }
        internal void Add(LinkBase link)
        {
            Links.Add(link);
            ContentChanged?.Invoke(this, EventArgs.Empty);
        }
        internal bool Contains(NodeBase node)
        {
            return Nodes.Contains(node);
        }
        internal bool Contains(LinkBase link)
        {
            return Links.Contains(link);
        }
        internal void Remove(NodeBase node)
        {
            _ = Nodes.Remove(node);
        }
        internal void Remove(LinkBase link)
        {
            _ = Links.Remove(link);
        }
        internal void Clear()
        {
            foreach (var node in Nodes)
            {
                node.Deselect();
            }
            Nodes.Clear();
            foreach (var link in Links)
            {
                link.Deselect();
            }
            Links.Clear();
        }
    }
}
