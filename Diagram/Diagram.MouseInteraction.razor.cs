using Excubo.Blazor.Diagrams.Extensions;
using Microsoft.AspNetCore.Components.Web;
using System;

namespace Excubo.Blazor.Diagrams
{
    internal enum ActionType
    {
        None,
        Pan,
        SelectRegion,
        Move,
        UpdateLinkTarget
    }
    public partial class Diagram
    {
        internal object ActiveElement { private get; set; } // TODO improve type
        private HoverType ht;
        internal HoverType ActiveElementType { private get => ht;
            set {
                ht = value;
            } }
        private object ActionObject { get; set; } // TODO improve type
        private ActionType a;
        private ActionType Action
        {
            get => a;
            set
            {
                a = value;
            }
        }
        private bool NewNodeAddingInProgress { get; set; }
        private Point original_cursor_position;
        private void OnMouseMove(MouseEventArgs e)
        {
            switch (Action)
            {
                case ActionType.SelectRegion:
                    // select by region
                    // TODO
                    break;
                case ActionType.Pan when ActiveElementType == HoverType.Unknown && e.Buttons == 1:
                    Pan(e);
                    break;
                case ActionType.Move when e.Buttons == 1:
                    MoveGroup(e);
                    break;
                // TODO handle this:
                //case ActionType.Move when ActiveElementType == HoverType.ControlPoint && e.Buttons == 1:
                    // TODO move control point
                //    break;
                case ActionType.UpdateLinkTarget:
                    FollowCursorForLinkTarget(e);
                    break;
                default:
                    // no action, reset original_cursor_position
                    original_cursor_position = null;
                    break;
            }
        }
        private void FollowCursorForLinkTarget(MouseEventArgs e)
        {
            var link = ActionObject as LinkBase;
            // the shenanigans of placing the target slightly off from where the cursor actually is, are absolutely crucial:
            // we want to identify whenever the cursor is over the border of a node, hence the cursor must be over the border, not over the currently drawn link!
            // by placing the link slightly off, we make sure that what we see underneath the cursor is not the link, but the border.
            var x = e.RelativeXToOrigin(this);
            var y = e.RelativeYToOrigin(this);
            var source_x = link.Source.GetX(this);
            var source_y = link.Source.GetY(this);
            link.Target.RelativeX = source_x < x ? x - 1 : x + 1;
            link.Target.RelativeY = source_y < y ? y - 1 : y + 1;
        }
        private void Pan(MouseEventArgs e)
        {
            if (original_cursor_position != null)
            {
                NavigationSettings.Origin.X += (NavigationSettings.InversedPanning ? 1 : -1) * (e.ClientX - original_cursor_position.X) / NavigationSettings.Zoom;
                NavigationSettings.Origin.Y += (NavigationSettings.InversedPanning ? 1 : -1) * (e.ClientY - original_cursor_position.Y) / NavigationSettings.Zoom;
            }
            original_cursor_position = new Point(e.ClientX, e.ClientY);
        }
        private void MoveGroup(MouseEventArgs e)
        {
            if (original_cursor_position != null)
            {
                var delta_x = e.RelativeXTo(original_cursor_position) / NavigationSettings.Zoom;
                var delta_y = e.RelativeYTo(original_cursor_position) / NavigationSettings.Zoom;
                foreach (var node in Group.Nodes)
                {
                    node.UpdatePosition(node.X + delta_x, node.Y + delta_y);
                }
            }
            original_cursor_position = new Point(e.ClientX, e.ClientY);
        }
        private void OnMouseWheel(WheelEventArgs e)
        {
            NavigationSettings.OnMouseWheel(e);
            Nodes.Redraw();

        }
        private void OnMouseDown(MouseEventArgs e)
        {
            switch (Action)
            {
                case ActionType.Move:
                    // nothing to be done here
                    break;
                case ActionType.None:
                    StartAction(e);
                    break;
                case ActionType.SelectRegion:
                    // nothing to be done here
                    break;
                case ActionType.UpdateLinkTarget:
                    // TODO end the action of updating the link target. fix to wherever we are.
                    EndLink(e);
                    break;
            }
            if (ActiveElement == null)
            {
                //probably deselect everything unless ctrl is held?
                return;
            }
            //switch (ActiveElementType)
            //{
            //    case HoverType.Border:
            //        TryHandleBorder(e);
            //        break;
            //    case HoverType.NewNode:
            //        TryHandleNewNode(e);
            //        break;
            //    case HoverType.Node:
            //        TryHandleNode(e);
            //        break;
            //    case HoverType.ControlPoint:
            //        TryHandleControlPoint(e);
            //        break;
            //}
        }
        private void EndLink(MouseEventArgs e)
        {
            var link = ActionObject as LinkBase;
            if (ActiveElement is NodeBase node)
            {
                link.Target.Node = node;
                link.Target.RelativeX = e.RelativeXTo(node);
                link.Target.RelativeY = e.RelativeYTo(node);
                link.Deselect();
            }
            else
            {
                link.Target.RelativeX = e.RelativeXToOrigin(this);
                link.Target.RelativeY = e.RelativeYToOrigin(this);
            }
            Action = ActionType.None;
        }
        private void StartAction(MouseEventArgs e)
        {
            switch (ActiveElementType)
            {
                case HoverType.Unknown when !e.CtrlKey:
                    // panning starts, when you simply press the mouse down anywhere where there's nothing.
                    Action = ActionType.Pan;
                    break;
                case HoverType.Unknown when e.CtrlKey:
                    // this isn't really a sensible thing to do. It's probably a misplaced select
                    break;
                case HoverType.Border:
                    // we create a new link if we simply press on the border of a node
                    CreateNewLink(e);
                    break;
                case HoverType.Node when !e.CtrlKey:
                    var active_node = ActiveElement as NodeBase;
                    if (!Group.Contains(active_node))
                    {
                        Group.Clear();
                        Group.Add(active_node);
                        active_node.Select();
                    }
                    // we want to move now
                    Action = ActionType.Move;
                    break;
                case HoverType.Node when e.CtrlKey:
                    // this is a selection/deselection, but the action type is not yet known.
                    TriggerSelectionOfNode();
                    break;
                case HoverType.ControlPoint:
                    // TODO select control point
                    break;
                case HoverType.Link:
                    // TODO select link
                    break;
                case HoverType.NewNode:
                    CreateNewNode();
                    break;
            }     
        }
        private void TriggerSelectionOfNode()
        {
            var node = ActiveElement as NodeBase;
            if (Group.Contains(node))
            {
                Group.Remove(node);
                node.Deselect();
            }
            else
            {
                Group.Add(node);
                node.Select();
            }
        }
        private void CreateNewNode()
        {
            var node = ActiveElement as NodeBase;
            Nodes.AddNewNode(node, (new_node) =>
            {
                Group = new Group();
                Group.Add(new_node);
                new_node.Select();
                ActionObject = new_node;
                Action = ActionType.Move;
                NewNodeAddingInProgress = true;
            });
        }
        private void CreateNewLink(MouseEventArgs e)
        {
            var node = ActiveElement as NodeBase;
            Links.AddLink(node, e, (generated_link) =>
            {
                ActionObject = generated_link;
                Action = ActionType.UpdateLinkTarget;
                generated_link.Select();
            });
        }
        [Obsolete("Needs to be rethought")]
        private void OnMouseUp(MouseEventArgs e)
        {
            switch (Action)
            {
                case ActionType.Move:
                case ActionType.Pan:
                    // active stop the action
                    Action = ActionType.None;
                    if (Group.Nodes.Count == 1)
                    {
                        Group.Nodes[0].Deselect();
                    }
                    NewNodeAddingInProgress = false;
                    break;
                case ActionType.None:
                    // nothing to do here
                    break;
                case ActionType.SelectRegion:
                    // TODO finish selection
                    Action = ActionType.None;
                    break;
                case ActionType.UpdateLinkTarget:
                    // this shouldn't do anything as link creation is simply clicking twice.
                    // therefore, the mouse up movement shouldn't change the process.
                    break;
            }
        }
    }
}
