using Excubo.Blazor.Diagrams.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace Excubo.Blazor.Diagrams
{
    //TODO#09 arrows
    //TODO#08 symbol palette
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
        protected override async Task OnAfterRenderAsync(bool first_render)
        {
            await GetPositionAsync();
            await base.OnAfterRenderAsync(first_render);
        }
        #region interaction
        internal ActiveNode ActiveNode { get; set; }
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
                Nodes.OnMouseMove(ActiveNode, e.RelativeXTo(this), e.RelativeYTo(this));
            }
            else if (Links.NewLink != null)
            {
                Links.OnMouseMove(e.RelativeXTo(this), e.RelativeYTo(this));
            }
            else if (e.Buttons == 1)
            {
                if (pan_point == null)
                {
                    pan_point = new Point(e.ClientX, e.ClientY);
                    original_origin = new Point(NavigationSettings.Origin.X, NavigationSettings.Origin.Y);
                }
                else
                {
                    NavigationSettings.Origin.X = original_origin.X + (NavigationSettings.InversedPanning ? 1 : -1) * (e.ClientX - pan_point.X) / NavigationSettings.Zoom;
                    NavigationSettings.Origin.Y = original_origin.Y + (NavigationSettings.InversedPanning ? 1 : -1) * (e.ClientY - pan_point.Y) / NavigationSettings.Zoom;
                    Nodes.Redraw();
                    Links.Redraw();
                }
            }
            else
            {
                pan_point = null;
            }
        }
        private void OnMouseWheel(WheelEventArgs e)
        {
            NavigationSettings.OnMouseWheel(e);
            Nodes.Redraw();
            Links.Redraw();

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
                    ActiveNode = new ActiveNode(node, e, CanvasLeft, CanvasTop);
                }
            }
            if (CurrentlyHoveredNode.Type == HoverType.Border)
            {
                if (Links.NewLink == null)
                {
                    Links.AddLink(node, e);
                }
                else
                {
                    Links.NewLink.FixTo(node, e);
                    Links.ResetNewLink();
                }
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
                if (Links.NewLink != null)
                {
                    Links.CancelNewLink();
                }
                ActiveNode = null;
            }
        }
        #endregion
        public void AddNodeContentFragment(RenderFragment content)
        {
            node_content_renderer.Add(content);
        }
        public NodeAnchor GetAnchorTo(string node_id)
        {
            var node = Nodes.Find(node_id);
            return node == null ? null : NodeAnchor.WithDefaultNodePort(node);
        }
    }
}
