using Excubo.Blazor.Diagrams.Extensions;
using Microsoft.AspNetCore.Components.Web;
using System.Linq;

namespace Excubo.Blazor.Diagrams
{
    public partial class Diagram
    {
        private class ActiveElementContainer
        {
            internal NodeBase Node { get; private set; }
            internal LinkBase Link { get; private set; }
            internal ControlPoint ControlPoint { get; private set; }
            internal NodeAnchor Anchor { get; private set; }
            internal double X { get; private set; }
            internal double Y { get; private set; }
            internal Point Point { get; private set; }
            internal Point Origin { get; private set; }
            internal void Set(NodeBase node)
            {
                Node = node;
                Link = null;
                ControlPoint = null;
                Anchor = null;
                Point = null;
            }
            internal void Set(LinkBase link)
            {
                Node = null;
                Link = link;
                ControlPoint = null;
                Anchor = null;
                Point = null;
            }
            internal void Set(LinkBase link, ControlPoint control_point)
            {
                Node = null;
                Link = link;
                ControlPoint = control_point;
                Anchor = null;
                Point = null;
            }
            internal void Set(LinkBase link, NodeAnchor anchor)
            {
                Node = null;
                Link = link;
                ControlPoint = null;
                Anchor = anchor;
                Point = null;
            }
            internal void Clear()
            {
                Node = null;
                Link = null;
                ControlPoint = null;
                Anchor = null;
                Point = null;
                Origin = null;
            }
            internal void Remember(NodeBase node, double x, double y)
            {
                Node = node;
                X = x;
                Y = y;
            }
            internal void RememberOrigin(Point point)
            {
                Origin = point;
            }
            internal void SetPoint(Point point)
            {
                Point = point;
            }
            internal void ResetPoint()
            {
                Point = null;
            }
        }
        private readonly ActiveElementContainer ActiveElement = new ActiveElementContainer();
        internal void SetActiveElement(NodeBase node, HoverType hover_type)
        {
            ActiveElement.Set(node);
            ActiveElementType = hover_type;
        }
        internal void SetActiveElement(LinkBase link, HoverType hover_type)
        {
            ActiveElement.Set(link);
            ActiveElementType = hover_type;
        }
        internal void SetActiveElement(LinkBase link, ControlPoint control_point, HoverType hover_type)
        {
            ActiveElement.Set(link, control_point);
            ActiveElementType = hover_type;
        }
        internal void SetActiveElement(LinkBase link, NodeAnchor anchor, HoverType hover_type)
        {
            ActiveElement.Set(link, anchor);
            ActiveElementType = hover_type;
        }
        internal void DeactivateElement()
        {
            ActiveElement.Clear();
            ActiveElementType = HoverType.Unknown;
        }
        private HoverType ActiveElementType { get; set; }
        private readonly ActiveElementContainer ActionObject = new ActiveElementContainer();
        private ActionType ActionType { get; set; }
        private bool NewNodeAddingInProgress { get; set; }
        private void OnMouseMove(MouseEventArgs e)
        {
            switch (ActionType)
            {
                case ActionType.SelectRegion:
                    // TODO select by region
                    break;
                case ActionType.Pan when ActiveElementType == HoverType.Unknown && e.Buttons == 1:
                    Pan(e);
                    break;
                case ActionType.MoveControlPoint when e.Buttons == 1:
                    MoveControlPoint(e);
                    break;
                case ActionType.MoveAnchor when e.Buttons == 1:
                    MoveNodeAnchor(e);
                    break;
                case ActionType.Move when e.Buttons == 1:
                    MoveGroup(e);
                    break;
                case ActionType.UpdateLinkTarget:
                    FollowCursorForLinkTarget(e);
                    break;
                default:
                    // no action, reset original_cursor_position
                    ActionObject.ResetPoint();
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
            switch (ActionType)
            {
                case ActionType.Move:
                case ActionType.MoveAnchor:
                case ActionType.MoveControlPoint:
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
                case ActionType.ModifyLink when ActiveElementType == HoverType.Anchor:
                    StartMoveAnchor();
                    break;
                case ActionType.ModifyLink when ActiveElementType == HoverType.ControlPoint:
                    StartMoveControlPoint();
                    break;
                case ActionType.ModifyLink when ActionObject.Link != null:
                    StopModifyingLink(e);
                    break;
            }
        }
        private void OnMouseUp(MouseEventArgs e)
        {
            switch (ActionType)
            {
                case ActionType.MoveAnchor:
                    FixNodeAnchor(e);
                    break;
                case ActionType.MoveControlPoint:
                    GoBackToEditingLink();
                    break;
                case ActionType.Move:
                    StopMove();
                    break;
                case ActionType.Pan:
                    ActionType = ActionType.None;
                    break;
                case ActionType.None:
                    // nothing to do here
                    break;
                case ActionType.SelectRegion:
                    // TODO finish selection
                    ActionType = ActionType.None;
                    break;
                case ActionType.UpdateLinkTarget:
                    // this shouldn't do anything as link creation is simply clicking twice.
                    // therefore, the mouse up movement shouldn't change the process.
                    break;
            }
        }
        private void GoBackToEditingLink()
        {
            var cp = ActionObject.ControlPoint;
            var (new_x, new_y) = (ActionObject.ControlPoint.X, ActionObject.ControlPoint.Y);
            var (old_x, old_y) = (ActionObject.Origin.X, ActionObject.ControlPoint.Y);
            Changes.New(new ChangeAction(() => { (cp.X, cp.Y) = (new_x, new_y); }, () => { (cp.X, cp.Y) = (old_x, old_y); }));
            var link = ActionObject.Link;
            ActionObject.Set(link);
            ActionType = ActionType.ModifyLink;
        }
        private void StopMove()
        {
            if (ActionObject.Origin == null)
            {
                // no move was actually done
                ActionObject.Clear();
                ActionType = ActionType.None;
                return;
            }
            var nodes = Group.Nodes.ToList();
            var positions = nodes.Select(n => (X: n.X, Y: n.Y)).ToList();
            var (delta_x, delta_y) = (ActionObject.Node.X - ActionObject.Origin.X, ActionObject.Node.Y - ActionObject.Origin.Y);
            var old_positions = nodes.Select(n => (X: n.X - delta_x, Y: n.Y - delta_y)).ToList();
            Changes.New(new ChangeAction(() => 
            {
                foreach (var (node, position) in nodes.Zip(positions, (n, p) => (n, p)))
                {
                    (node.X, node.Y) = (position.X, position.Y);
                    node.TriggerStateHasChanged();
                }
            }, () =>
            {
                foreach (var (node, position) in nodes.Zip(old_positions, (n, p) => (n, p)))
                {
                    (node.X, node.Y) = (position.X, position.Y);
                    node.TriggerStateHasChanged();
                }
            }));
            ActionObject.Clear();
            ActionType = ActionType.None;
            if (Group.Nodes.Count == 1)
            {
                Group.Nodes[0].Deselect();
            }
            NewNodeAddingInProgress = false;
        }
        private void StopModifyingLink(MouseEventArgs e)
        {
            // another click means ending the current action and starting a new one
            var link = ActionObject.Link;
            link.Deselect();
            ActionObject.Clear();
            ActionType = ActionType.None;
            StartAction(e);
        }
        private void StartMoveAnchor()
        {
            ActionObject.Set(ActiveElement.Link, ActiveElement.Anchor);
            ActionType = ActionType.MoveAnchor;
        }
        private void StartMoveControlPoint()
        {
            ActionObject.Set(ActiveElement.Link, ActiveElement.ControlPoint);
            ActionType = ActionType.MoveControlPoint;
        }
        private void FollowCursorForLinkTarget(MouseEventArgs e)
        {
            var link = ActionObject.Link;
            link.Target.RelativeX = e.RelativeXToOrigin(this);
            link.Target.RelativeY = e.RelativeYToOrigin(this);
        }
        private void Pan(MouseEventArgs e)
        {
            if (ActionObject.Point != null)
            {
                NavigationSettings.Pan(e.ClientX - ActionObject.Point.X, e.ClientY - ActionObject.Point.Y);
                (ActionObject.Point.X, ActionObject.Point.Y) = (e.ClientX, e.ClientY);
            }
            else
            {
                ActionObject.SetPoint(new Point(e.ClientX, e.ClientY));
            }
        }
        private void MoveControlPoint(MouseEventArgs e)
        {
            if (ActionObject.Point != null)
            {
                var point = ActionObject.ControlPoint;
                var delta_x = e.RelativeXTo(ActionObject.Point) / NavigationSettings.Zoom;
                var delta_y = e.RelativeYTo(ActionObject.Point) / NavigationSettings.Zoom;
                point.X += delta_x;
                point.Y += delta_y;
                (ActionObject.Point.X, ActionObject.Point.Y) = (e.ClientX, e.ClientY);
            }
            else
            {
                ActionObject.SetPoint(new Point(e.ClientX, e.ClientY));
                ActionObject.RememberOrigin(new Point(ActionObject.ControlPoint.X, ActionObject.ControlPoint.Y));
            }
        }
        private void MoveNodeAnchor(MouseEventArgs e)
        {
            if (ActionObject.Point != null)
            {
                var anchor = ActionObject.Anchor;
                anchor.RelativeX = e.RelativeXToOrigin(this);
                anchor.RelativeY = e.RelativeYToOrigin(this);
            }
            else
            {
                ActionObject.SetPoint(new Point(e.ClientX, e.ClientY));
                var anchor = ActionObject.Anchor;
                ActionObject.Remember(anchor.Node, anchor.RelativeX, anchor.RelativeY);
                anchor.Node = null;
                anchor.NodeId = null;
                anchor.RelativeX = e.RelativeXToOrigin(this);
                anchor.RelativeY = e.RelativeYToOrigin(this);
            }
        }
        private void MoveGroup(MouseEventArgs e)
        {
            if (ActionObject.Point != null)
            {
                var delta_x = e.RelativeXTo(ActionObject.Point) / NavigationSettings.Zoom;
                var delta_y = e.RelativeYTo(ActionObject.Point) / NavigationSettings.Zoom;
                foreach (var node in Group.Nodes)
                {
                    node.UpdatePosition(node.X + delta_x, node.Y + delta_y);
                }
                (ActionObject.Point.X, ActionObject.Point.Y) = (e.ClientX, e.ClientY);
            }
            else
            {
                ActionObject.SetPoint(new Point(e.ClientX, e.ClientY));
                ActionObject.RememberOrigin(new Point(ActionObject.Node.X, ActionObject.Node.Y));
            }
        }
        private void FixNodeAnchor(MouseEventArgs e)
        {
            var (anchor, old_node, old_x, old_y) = (ActionObject.Anchor, ActionObject.Node, ActionObject.X, ActionObject.Y);
            var node = ActiveElement.Node;
            var (x, y) = (node != null) ? e.RelativeTo(node) : e.RelativeToOrigin(this);
            Changes.NewAndDo(new ChangeAction(() =>
            {
                anchor.Node = node;
                anchor.RelativeX = x;
                anchor.RelativeY = y;
            }, () =>
            {
                anchor.Node = old_node;
                anchor.RelativeX = old_x;
                anchor.RelativeY = old_y;
            }));
            ActionObject.Set(ActionObject.Link);
            ActionType = ActionType.ModifyLink;
        }
        private void EndLink(MouseEventArgs e)
        {
            var link = ActionObject.Link;
            var node = ActiveElement.Node;
            if (node != null)
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
            Changes.New(new ChangeAction(() => { Links.Add(link); }, () => { Links.Remove(link); }));
            ActionType = ActionType.None;
        }
        private void StartAction(MouseEventArgs e)
        {
            switch (ActiveElementType)
            {
                case HoverType.Unknown when !e.CtrlKey:
                    // panning starts, when you simply press the mouse down anywhere where there's nothing.
                    ActionType = ActionType.Pan;
                    break;
                case HoverType.Unknown when e.CtrlKey:
                    // this isn't really a sensible thing to do. It's probably a misplaced select
                    break;
                case HoverType.Border:
                    // we create a new link if we simply press on the border of a node
                    CreateNewLink(e);
                    break;
                case HoverType.Node when !e.CtrlKey:
                    StartMove();
                    break;
                case HoverType.Node when e.CtrlKey:
                    // this is a selection/deselection, but the action type is not yet known.
                    TriggerSelectionOfNode();
                    break;
                case HoverType.Anchor:
                    ActionObject.Set(ActiveElement.Link, ActiveElement.Anchor);
                    ActionType = ActionType.MoveAnchor;
                    break;
                case HoverType.ControlPoint:
                    ActionObject.Set(ActiveElement.Link, ActiveElement.ControlPoint);
                    ActionType = ActionType.MoveControlPoint;
                    break;
                case HoverType.Link:
                    ActionObject.Set(ActiveElement.Link);
                    ActiveElement.Link.Select();
                    ActionType = ActionType.ModifyLink;
                    break;
                case HoverType.NewNode:
                    CreateNewNode();
                    break;
            }
        }
        private void StartMove()
        {
            ActionObject.Set(ActiveElement.Node);
            var active_node = ActiveElement.Node;
            if (!Group.Contains(active_node))
            {
                Group.Clear();
                Group.Add(active_node);
                active_node.Select();
            }
            ActionType = ActionType.Move;
        }
        private void TriggerSelectionOfNode()
        {
            var node = ActiveElement.Node;
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
            var node = ActiveElement.Node;
            Nodes.AddNewNode(node, (new_node) =>
            {
                Changes.New(new ChangeAction(() => Nodes.Add(new_node), () => Nodes.Remove(new_node)));
                Group.Clear();
                Group = new Group();
                Group.Add(new_node);
                new_node.Select();
                ActionObject.Set(new_node);
                ActionType = ActionType.Move;
                NewNodeAddingInProgress = true;
            });
        }
        private void CreateNewLink(MouseEventArgs e)
        {
            var node = ActiveElement.Node;
            Links.AddNewLink(node, e, (generated_link) =>
            {
                ActionObject.Set(generated_link);
                ActionType = ActionType.UpdateLinkTarget;
                generated_link.Select();
            });
        }
    }
}
