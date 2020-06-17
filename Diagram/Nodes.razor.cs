using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Excubo.Blazor.Diagrams
{
    public partial class Nodes
    {
        /// <summary>
        /// Default node type for nodes created as <Node />
        /// </summary>
        [Parameter] public NodeType DefaultType { get; set; }
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
        private readonly List<NodeData> internally_generated_nodes = new List<NodeData>();
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
        internal void AddNewNode(NodeBase node, Action<NodeBase> on_create)
        {
            internally_generated_nodes.Add(new NodeData
            {
                Type = (node.GetType() == typeof(Node)) ? ((node as Node).GetImplicitType()) : node.GetType(),
                ChildContent = node.ChildContent,
                OnCreate = on_create,
                X = Diagram.NavigationSettings.Origin.X + node.X / Diagram.NavigationSettings.Zoom,
                Y = Diagram.NavigationSettings.Origin.Y + node.Y / Diagram.NavigationSettings.Zoom,
                Attributes = node.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.GetCustomAttribute<ParameterAttribute>() != null)
                .Where(p => p.Name != nameof(NodeBase.ChildContent)) // TODO ignore other properties too, those that are manually added.
                .Where(p => p.Name != nameof(NodeBase.Id))
                .Where(p => p.Name != nameof(NodeBase.OnCreate))
                .Where(p => p.Name != nameof(NodeBase.X))
                .Where(p => p.Name != nameof(NodeBase.Y))
                .ToDictionary(p => p.Name, p => p.GetValue(node))
            });
            generated_nodes_ref.TriggerStateHasChanged();
        }
        internal void Remove(NodeBase node)
        {
            all_nodes.Remove(node);
            var match = internally_generated_nodes.FirstOrDefault(n => n.Id == node.Id);
            if (match != null)
            {
                internally_generated_nodes.Remove(match);
                generated_nodes_ref.TriggerStateHasChanged();
            }
            OnRemove?.Invoke(node);
        }
    }
}
