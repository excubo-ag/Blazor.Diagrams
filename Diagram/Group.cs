using System;
using System.Collections.Generic;

namespace Excubo.Blazor.Diagrams
{
    public class Group
    {
        public event EventHandler ContentChanged;
        public List<NodeBase> Nodes { get; } = new List<NodeBase>();

        internal void Add(NodeBase node)
        {
            Nodes.Add(node);
            ContentChanged?.Invoke(this, EventArgs.Empty);
        }
        internal bool Contains(NodeBase node)
        {
            return Nodes.Contains(node);
        }
        internal void Remove(NodeBase node)
        {
            _ = Nodes.Remove(node);
        }
        internal void Clear()
        {
            foreach (var node in Nodes)
            {
                node.Deselect();
            }
            Nodes.Clear();
        }
    }
}
