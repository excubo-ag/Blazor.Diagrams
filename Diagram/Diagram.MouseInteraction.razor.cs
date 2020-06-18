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
        UpdateLinkTarget,
        ModifyLink
    }
    public partial class Diagram
    {
        internal object ActiveElement { private get; set; } // TODO improve type
        internal HoverType ActiveElementType { private get; set; }
        private object ActionObject { get; set; } // TODO improve type
        private ActionType Action { get; set; }
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
                case ActionType.Move when ActionObject is ControlPoint && e.Buttons == 1:
                    MoveControlPoint(e);
                    break;
                case ActionType.Move when ActionObject is AssociatedAnchor && e.Buttons == 1:
                    MoveNodeAnchor(e);
                    break;
                case ActionType.Move when !(ActionObject is ControlPoint || ActionObject is AssociatedAnchor) && e.Buttons == 1:
                    MoveGroup(e);
                    break;
                case ActionType.UpdateLinkTarget:
                    FollowCursorForLinkTarget(e);
                    break;
                default:
                    // no action, reset original_cursor_position
                    original_cursor_position = null;
                    break;
            }
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
                    EndLink(e);
                    break;
                case ActionType.ModifyLink when ActiveElementType == HoverType.ControlPoint:
                    ActionObject = ActiveElement;
                    Action = ActionType.Move;
                    break;
                case ActionType.ModifyLink when ActionObject is LinkBase link:
                    // another click means ending the current action and starting a new one
                    link.Deselect();
                    ActionObject = null;
                    Action = ActionType.None;
                    StartAction(e);
                    break;

            }
        }
        private void OnMouseUp(MouseEventArgs e)
        {
            switch (Action)
            {
                case ActionType.Move:
                case ActionType.Pan:
                    if (ActionObject is AssociatedAnchor)
                    {
                        FixNodeAnchor(e);
                    }
                    else if (ActionObject is ControlPoint point)
                    {
                        // go back to edit mode of the link
                        ActionObject = point.Link;
                        Action = ActionType.ModifyLink;
                    }
                    else
                    {
                        // active stop the action
                        ActionObject = null;
                        Action = ActionType.None;
                        if (Group.Nodes.Count == 1)
                        {
                            Group.Nodes[0].Deselect();
                        }
                        NewNodeAddingInProgress = false;
                    }
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
        private void FollowCursorForLinkTarget(MouseEventArgs e)
        {
            var link = ActionObject as LinkBase;
            link.Target.RelativeX = e.RelativeXToOrigin(this);
            link.Target.RelativeY = e.RelativeYToOrigin(this);
        }
        private void Pan(MouseEventArgs e)
        {
            if (original_cursor_position != null)
            {
                NavigationSettings.Origin.X += (NavigationSettings.InversedPanning ? 1 : -1) * (e.ClientX - original_cursor_position.X) / NavigationSettings.Zoom;
                NavigationSettings.Origin.Y += (NavigationSettings.InversedPanning ? 1 : -1) * (e.ClientY - original_cursor_position.Y) / NavigationSettings.Zoom;
                (original_cursor_position.X, original_cursor_position.Y) = (e.ClientX, e.ClientY);
            }
            else
            {
                original_cursor_position = new Point(e.ClientX, e.ClientY);
            }
        }
        private void MoveControlPoint(MouseEventArgs e)
        {
            if (original_cursor_position != null)
            {
                var delta_x = e.RelativeXTo(original_cursor_position) / NavigationSettings.Zoom;
                var delta_y = e.RelativeYTo(original_cursor_position) / NavigationSettings.Zoom;
                (ActionObject as ControlPoint).X += delta_x;
                (ActionObject as ControlPoint).Y += delta_y;
                (original_cursor_position.X, original_cursor_position.Y) = (e.ClientX, e.ClientY);
            }
            else
            {
                original_cursor_position = new Point(e.ClientX, e.ClientY);
            }
        }
        private void MoveNodeAnchor(MouseEventArgs e)
        {
            if (original_cursor_position != null)
            {
                var (_, anchor) = ActionObject as AssociatedAnchor;
                anchor.RelativeX = e.RelativeXToOrigin(this);
                anchor.RelativeY = e.RelativeYToOrigin(this);
            }
            else
            {
                original_cursor_position = new Point(e.ClientX, e.ClientY);
                var (_, anchor) = ActionObject as AssociatedAnchor;
                anchor.Node = null;
                anchor.RelativeX = e.RelativeXToOrigin(this);
                anchor.RelativeY = e.RelativeYToOrigin(this);
            }
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
                (original_cursor_position.X, original_cursor_position.Y) = (e.ClientX, e.ClientY);
            }
            else
            {
                original_cursor_position = new Point(e.ClientX, e.ClientY);
            }
        }
        private void FixNodeAnchor(MouseEventArgs e)
        {
            var (link, anchor) = ActionObject as AssociatedAnchor;
            if (ActiveElement is NodeBase node)
            {
                anchor.Node = node;
                anchor.RelativeX = e.RelativeXTo(node);
                anchor.RelativeY = e.RelativeYTo(node);
            }
            else
            {
                anchor.RelativeX = e.RelativeXToOrigin(this);
                anchor.RelativeY = e.RelativeYToOrigin(this);
            }
            ActionObject = link;
            Action = ActionType.ModifyLink;
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
                    ActionObject = ActiveElement;
                    Action = ActionType.Move;
                    break;
                case HoverType.Link:
                    ActionObject = ActiveElement;
                    (ActiveElement as LinkBase).Select();
                    Action = ActionType.ModifyLink;
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
                Group.Clear();
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
    }
}
