using Excubo.Blazor.Diagrams.__Internal;
using Excubo.Blazor.Diagrams.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;

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
        private Renderer Renderer { get; set; }
        private readonly List<LinkBase> all_links = new List<LinkBase>();
        public void Add(LinkBase link)
        {
            if (!all_links.Contains(link))
            {
                all_links.Add(link);
                OnAddLink(link);
            }
        }
        internal LinkBase NewLink { get; set; }
        private RenderFragment NewLinkFragment { get; set; }
        internal void AddLink(NodeBase node, MouseEventArgs e)
        {
            var source_point = new NodeAnchor
            {
                Node = node,
                RelativeX = e.RelativeXTo(node) / Diagram.NavigationSettings.Zoom,
                RelativeY = e.RelativeYTo(node) / Diagram.NavigationSettings.Zoom
            };
            var target_point = new NodeAnchor
            {
                RelativeX = e.RelativeXTo(Diagram),
                RelativeY = e.RelativeYTo(Diagram)
            };
            NewLinkFragment = GetTemplateLink(source_point, target_point, (generated_link) =>
            {
                NewLink = generated_link;
                NewLink.Select();
            });
            Renderer.Add(NewLinkFragment);
        }
        private RenderFragment GetTemplateLink(NodeAnchor source, NodeAnchor target, Action<LinkBase> register)
        {
            return (builder) =>
            {
                builder.OpenComponent<CascadingValue<Links>>(0);
                builder.AddAttribute(1, nameof(CascadingValue<Links>.Value), this);
                builder.AddAttribute(2, nameof(CascadingValue<Links>.IsFixed), true);
                builder.AddAttribute(3, nameof(CascadingValue<Links>.ChildContent), (RenderFragment)((builder2) =>
                {
                    builder2.OpenComponent<Link>(0);
                    builder2.AddAttribute(1, nameof(StraightLink.Source), source);
                    builder2.AddAttribute(2, nameof(StraightLink.Target), target);
                    builder2.AddAttribute(3, nameof(StraightLink.OnCreate), register);
                    builder2.CloseComponent();
                }));
                builder.CloseComponent();
            };
        }
        internal void OnMouseMove(double x, double y)
        {
            if (NewLink != null)
            {
                NewLink.UpdateTarget(x, y);
            }
        }
        internal void CancelNewLink()
        {
            Renderer.Remove(NewLinkFragment);
            all_links.Remove(NewLink);
            NewLink = null;
            NewLinkFragment = null;
        }
        internal void ResetNewLink()
        {
            NewLinkFragment = null;
            NewLink = null;
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
