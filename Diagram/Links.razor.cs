using Excubo.Blazor.Diagrams.__Internal;
using Excubo.Blazor.Diagrams.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Excubo.Blazor.Diagrams
{
    public partial class Links
    {
        /// <summary>
        /// Default link type for links created as <Link />
        /// </summary>
        [Parameter] public LinkType DefaultType { get; set; }
        /// <summary>
        /// Callback for when a link is removed. The user can return false, if the action should be cancelled.
        /// </summary>
        [Parameter] public Func<LinkBase, bool> BeforeRemoveLink { get; set; }
        /// <summary>
        /// Callback for when a link is added.
        /// </summary>
        [Parameter] public Action<LinkBase> OnAddLink { get; set; }
        /// <summary>
        /// Callback that is executed if the remove action wasn't cancelled.
        /// </summary>
        [Parameter] public Action<LinkBase> OnRemoveLink { get; set; }
        /// <summary>
        /// Callback for when a link source or target is changed.
        /// </summary>
        [Parameter] public Action<LinkBase> OnLinkModified { get; set; }
        [CascadingParameter] public Diagram Diagram { get; set; }
        protected override void OnParametersSet()
        {
            System.Diagnostics.Debug.Assert(Diagram != null);
            Diagram.Links = this;
            base.OnParametersSet();
        }
        private readonly List<LinkBase> all_links = new List<LinkBase>();
        private readonly List<LinkData> internally_generated_links = new List<LinkData>();
        public void Add(LinkBase link)
        {
            if (!all_links.Contains(link))
            {
                all_links.Add(link);
                OnAddLink?.Invoke(link);
            }
        }
        internal void AddLink(NodeBase node, MouseEventArgs e, Action<LinkBase> on_link_create)
        {
            var source_point = new NodeAnchor
            {
                Node = node,
                RelativeX = e.RelativeXTo(node),
                RelativeY = e.RelativeYTo(node)
            };
            var target_point = new NodeAnchor
            {
                RelativeX = e.RelativeXToOrigin(Diagram),
                RelativeY = e.RelativeYToOrigin(Diagram)
            };
            internally_generated_links.Add(new LinkData { Source = source_point, Target = target_point, OnCreate = on_link_create });
            generated_links_ref.TriggerStateHasChanged();
        }
        internal void Remove(LinkBase link)
        {
            all_links.Remove(link);
            var match = internally_generated_links.FirstOrDefault(l => l.Source == link.Source && l.Target == link.Target);
            if (match != null)
            {
                internally_generated_links.Remove(match);
                generated_links_ref.TriggerStateHasChanged();
            }
            OnRemoveLink?.Invoke(link);
        }
        internal void Redraw()
        {
            foreach (var link in all_links)
            {
                link.TriggerStateHasChanged();
            }
        }
    }
}
