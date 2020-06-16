using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Excubo.Blazor.Diagrams
{
    public partial class Nodes
    {
        /// <summary>
        /// Default node type for nodes created as <Node />
        /// </summary>
        [Parameter] public NodeType DefaultType { get; set; }
        /// <summary>
        /// Callback for when a node is removed. The user can return false, if the action should be cancelled.
        /// </summary>
        [Parameter] public Func<NodeBase, bool> BeforeRemove { get; set; }
        /// <summary>
        /// Callback that is executed if the remove action wasn't cancelled.
        /// </summary>
        [Parameter] public Action<NodeBase> OnRemove { get; set; }
        /// <summary>
        /// Callback that is executed when a node is added.
        /// </summary>
        [Parameter] public Action<NodeBase> OnAdd { get; set; }
        [CascadingParameter] public Diagram Diagram { get; set; }
        protected override void OnParametersSet()
        {
            System.Diagnostics.Debug.Assert(Diagram != null);
            Diagram.Nodes = this;
            base.OnParametersSet();
        }
        protected override bool ShouldRender() => false;
        private readonly List<NodeBase> all_nodes = new List<NodeBase>();
        public void Add(NodeBase node)
        {
            if (!all_nodes.Contains(node))
            {
                all_nodes.Add(node);
                OnAdd?.Invoke(node);
            }
        }
        public NodeBase Find(string id)
        {
            return all_nodes.FirstOrDefault(n => n.Id == id);
        }

        internal void Redraw()
        {
            foreach (var node in all_nodes)
            {
                node.TriggerStateHasChanged();
            }
        }
    }
}
