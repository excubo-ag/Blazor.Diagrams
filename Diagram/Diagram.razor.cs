using Excubo.Blazor.Diagrams.Extensions;
using Excubo.Blazor.LazyStyleSheet;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace Excubo.Blazor.Diagrams
{
    //TODO#08 arrows
    //TODO#07 image node
    //TODO#06 auto-layout
    //TODO#05 undo/redo
    //TODO#04 background
    //TODO#03 virtualization
    //TODO#02 overview screen
    //TODO#01 common shape nodes (e.g. db, ...)
    public partial class Diagram
    {
        /// <summary>
        /// Callback for when a group of nodes and links is removed.
        /// The user can return false, if the action should be cancelled.
        /// If a BeforeRemoveGroup action is registered,
        /// BeforeRemoveLink/Node are ignored on the group members, otherwise those callbacks are executed as well.
        /// </summary>
        [Parameter] public Func<Group, bool> BeforeRemoveGroup { get; set; }
        /// <summary>
        /// Callback that is executed if the remove action wasn't cancelled.
        /// </summary>
        [Parameter] public Action<Group> OnRemoveGroup { get; set; }
        /// <summary>
        /// Callback that is executed whenever the selection of nodes and/or links changes.
        /// </summary>
        [Parameter] public Action<Group> SelectionChanged { get; set; }
        public Links Links { get; set; }
        public Nodes Nodes { get; set; }
        public Group Group { get; private set; } = new Group();
        private bool NewNodeAddingInProgress { get; set; }
        public Diagram()
        {
            Group.ContentChanged += Group_ContentChanged;
        }
        private void Group_ContentChanged(object _, EventArgs __)
        {
            SelectionChanged?.Invoke(Group);
        }
        #region diagram position
        [Inject]
        private IJSRuntime js { get; set; }
        internal double CanvasLeft { get; private set; }
        internal double CanvasTop { get; private set; }
        private async Task GetPositionAsync()
        {
            var values = await js.GetPositionAsync(canvas);
            CanvasLeft = values[0];
            CanvasTop = values[1];
        }
        #endregion
#pragma warning disable S2376, IDE0051
        [Inject] private IStyleSheetService StyleSheetService { set { if (value != null) { value.Add("_content/Excubo.Blazor.Diagrams/style.css"); } } }
#pragma warning restore S2376, IDE0051
        protected override async Task OnAfterRenderAsync(bool first_render)
        {
            await GetPositionAsync();
            await base.OnAfterRenderAsync(first_render);
        }
        #region interaction
        internal NodeBase ActiveNode { get; set; }
        internal LinkBase ActiveLink { get;set; }
        internal (NodeBase Node, HoverType Type) CurrentlyHoveredNode { get; set; }
        public NavigationSettings NavigationSettings { get; set; }
        protected override void OnParametersSet()
        {
            NavigationSettings ??= new NavigationSettings { Diagram = this };
            base.OnParametersSet();
        }
        private Point pan_point;
        private Point original_origin;
        private void OnMouseMove(MouseEventArgs e)
        {
            if (e.CtrlKey)
            {
                return;
            }
            if (ActiveNode != null)
            {
                if (e.Buttons == 1)
                {
                    if (pan_point == null)
                    {
                        pan_point = new Point(e.ClientX, e.ClientY);
                    }
                    else
                    {
                        var delta_x = e.RelativeXTo(pan_point) / NavigationSettings.Zoom;
                        var delta_y = e.RelativeYTo(pan_point) / NavigationSettings.Zoom;
                        pan_point.X = e.ClientX;
                        pan_point.Y = e.ClientY;
                        foreach (var node in Group.Nodes)
                        {
                            node.UpdatePosition(node.X + delta_x, node.Y + delta_y);
                        }
                    }
                }
            }
            else if (ActiveLink != null)
            {
                // the shenanigans of placing the target slightly off from where the cursor actually is, are absolutely crucial:
                // we want to identify whenever the cursor is over the border of a node, hence the cursor must be over the border, not over the currently drawn link!
                // by placing the link slightly off, we make sure that what we see underneath the cursor is not the link, but the border.
                var x = e.RelativeXToOrigin(this);
                var y = e.RelativeYToOrigin(this);
                var source_x = ActiveLink.Source.GetX(this);
                var source_y = ActiveLink.Source.GetY(this);
                ActiveLink.Target.RelativeX = source_x < x ? x - 1 : x + 1;
                ActiveLink.Target.RelativeY = source_y < y ? y - 1 : y + 1;
            }
            else if (e.Buttons == 1)
            {
                if (pan_point == null && original_origin == null)
                {
                    pan_point = new Point(e.ClientX, e.ClientY);
                    original_origin = new Point(NavigationSettings.Origin.X, NavigationSettings.Origin.Y);
                }
                else
                {
                    NavigationSettings.Origin.X = original_origin.X + (NavigationSettings.InversedPanning ? 1 : -1) * (e.ClientX - pan_point.X) / NavigationSettings.Zoom;
                    NavigationSettings.Origin.Y = original_origin.Y + (NavigationSettings.InversedPanning ? 1 : -1) * (e.ClientY - pan_point.Y) / NavigationSettings.Zoom;
                }
            }
            else
            {
                if (pan_point != null)
                {
                    pan_point = null;
                    original_origin = null;
                }
            }
        }
        private void OnMouseWheel(WheelEventArgs e)
        {
            NavigationSettings.OnMouseWheel(e);
            Nodes.Redraw();

        }
        private void OnMouseDown(MouseEventArgs e)
        {
            if (CurrentlyHoveredNode.Node == null)
            {
                //probably deselect everything unless ctrl is held?
                return;
            }
            var node = CurrentlyHoveredNode.Node;
            if (CurrentlyHoveredNode.Type == HoverType.Node)
            {
                if (!e.CtrlKey)
                {
                    Group = new Group();
                }
                if (Group.Contains(node))
                {
                    Group.Remove(node);
                    node.Deselect();
                    ActiveNode = null;
                }
                else
                {
                    Group.Add(node);
                    node.Select();
                    ActiveNode = node;
                    pan_point = new Point(e.ClientX, e.ClientY);
                }
            }
            if (CurrentlyHoveredNode.Type == HoverType.Border)
            {
                if (ActiveLink == null)
                {
                    Links.AddLink(node, e, (generated_link) =>
                    {
                        ActiveLink = generated_link;
                        ActiveLink.Select();
                    });
                }
                else
                {
                    ActiveLink.Target.Node = node;
                    ActiveLink.Target.RelativeX = e.RelativeXTo(node);
                    ActiveLink.Target.RelativeY = e.RelativeYTo(node);
                    ActiveLink.Deselect();
                    ActiveLink = null;
                }
            }
            if (CurrentlyHoveredNode.Type == HoverType.NewNode)
            {
                NewNodeAddingInProgress = true;
                Nodes.AddNewNode(node, (new_node) =>
                {
                    CurrentlyHoveredNode = (new_node, HoverType.NewNode);
                    Group = new Group();
                    Group.Add(new_node);
                    new_node.Select();
                    ActiveNode = new_node;
                    pan_point = new Point(e.ClientX, e.ClientY);
                });
            }
        }
        private void OnMouseUp(MouseEventArgs e)
        {
            if (!e.CtrlKey)
            {
                foreach (var cnode in Group.Nodes)
                {
                    cnode.Deselect();
                }
                Group = new Group();
                ActiveNode = null;
            }
            if (NewNodeAddingInProgress)
            {
                Group = new Group();
                ActiveNode?.Deselect();
                NewNodeAddingInProgress = false;
                CurrentlyHoveredNode = default;
                ActiveNode = null;
            }
        }
        private void OnKeyPress(KeyboardEventArgs e)
        {
            if (e.Key == "Escape")
            {
                foreach (var node in Group.Nodes)
                {
                    node.Deselect();
                }
                Group = new Group();
                if (ActiveLink != null)
                {
                    Links.Remove(ActiveLink);
                    ActiveLink = null;
                }
                ActiveNode = null;
            }
        }
        #endregion
        internal void AddNodeContentFragment(RenderFragment content)
        {
            node_content_renderer.Add(content);
        }
        internal void AddNodeTemplateContentFragment(RenderFragment content)
        {
            node_template_content_renderer.Add(content);
        }
        internal void AddNodeBorderFragment(RenderFragment content)
        {
            node_border_renderer.Add(content);
        }
        public NodeAnchor GetAnchorTo(string node_id)
        {
            var node = Nodes.Find(node_id);
            return node == null ? null : NodeAnchor.WithDefaultNodePort(node);
        }
    }
}
