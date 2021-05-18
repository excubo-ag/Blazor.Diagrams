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
        /// <summary>
        /// Callback that is executed after a node was moved.
        /// </summary>
        [Parameter] public Action<NodeBase> OnModified { get; set; }
        [CascadingParameter] public Diagram Diagram { get; set; }
        internal bool render_not_necessary;
        protected override bool ShouldRender()
        {
            if (render_not_necessary)
            {
                render_not_necessary = false;
                return false;
            }
            return base.ShouldRender();
        }
        protected override void OnParametersSet()
        {
            System.Diagnostics.Debug.Assert(Diagram != null);
            Diagram.Nodes = this;
            base.OnParametersSet();
        }
        internal readonly List<NodeBase> all_nodes = new List<NodeBase>();
        private readonly List<NodeData> internally_generated_nodes = new List<NodeData>();
        internal void Register(NodeBase node)
        {
            if (!all_nodes.Contains(node))
            {
                all_nodes.Add(node);
                Diagram.Links.AttachAnchorsTo(node);
                Diagram.UpdateOverview();
                if (node.IsInternallyGenerated)
                {
                    OnAdd?.Invoke(node);
                }
            }
        }
        internal void Deregister(NodeBase node)
        {
            if (all_nodes.Contains(node))
            {
                all_nodes.Remove(node);
                Diagram.UpdateOverview();
                if (node.IsInternallyGenerated)
                {
                    OnRemove?.Invoke(node);
                }
            }
        }
        internal void Add(NodeBase node)
        {
            all_nodes.Add(node);
            if (node.Deleted)
            {
                node.MarkUndeleted();
                node.AddBorderAndContent();
            }
            else
            {
                internally_generated_nodes.Add(new NodeData { Id = node.Id, X = node.X, Y = node.Y, ChildContent = node.ChildContent, Type = node.GetType() });
                generated_nodes_ref.TriggerStateHasChanged();
            }
            OnAdd?.Invoke(node);
        }
        internal void Remove(NodeBase node)
        {
            _ = all_nodes.Remove(node);
            var match = internally_generated_nodes.FirstOrDefault(n => n.Id == node.Id);
            if (match != null)
            {
                _ = internally_generated_nodes.Remove(match);
                generated_nodes_ref.TriggerStateHasChanged();
            }
            else
            {
                node.MarkDeleted();
            }
            node.RemoveBorderAndContent();
            OnRemove?.Invoke(node);
        }
        public NodeBase Find(string id)
        {
            return all_nodes.FirstOrDefault(n => n.Id == id);
        }
        internal void TriggerStateHasChanged() => StateHasChanged();
        internal void Redraw()
        {
            foreach (var node in all_nodes)
            {
                node.TriggerStateHasChanged();
            }
        }
        internal void ReRenderIfOffCanvasChanged()
        {
            foreach (var node in all_nodes)
            {
                node.ReRenderIfOffCanvasChanged();
            }
        }
        internal void AddNewNode(NodeBase node, Action<NodeBase> on_create)
        {
            internally_generated_nodes.Add(new NodeData
            {
                Type = node.GetType(),
                ChildContent = node.ChildContent,
                X = Diagram.NavigationSettings.Origin.X + node.X / Diagram.NavigationSettings.Zoom,
                Y = Diagram.NavigationSettings.Origin.Y + node.Y / Diagram.NavigationSettings.Zoom,
                Attributes = node.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.GetCustomAttribute<ParameterAttribute>() != null)
                .Where(p => p.Name != nameof(NodeBase.ChildContent))
                .Where(p => p.Name != nameof(NodeBase.Id))
                .Where(p => p.Name != nameof(NodeBase.X))
                .Where(p => p.Name != nameof(NodeBase.Y))
                .ToDictionary(p => p.Name, p => p.GetValue(node)),
                OnCreate = on_create
            });
            generated_nodes_ref.TriggerStateHasChanged();
        }
    }
}