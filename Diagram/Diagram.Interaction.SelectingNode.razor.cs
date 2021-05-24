using Microsoft.AspNetCore.Components.Web;
using System;

namespace Excubo.Blazor.Diagrams
{
    public partial class Diagram
    {
        private sealed class SelectingNode : InteractionState
        {
            private enum AnticipatedAction
            {
                GroupNodeRemoval,
                GroupNodeAddition,
                Move,
                NewGroup
            }
            private readonly AnticipatedAction anticipatedAction;
            private readonly NodeBase node;

            public SelectingNode(InteractionState previous, NodeBase node, MouseEventArgs e) : base(previous)
            {
                if (e.CtrlKey)
                {
                    if (diagram.Group.Nodes.Contains(node))
                    {
                        // the action being prepared here is removal of the node from the group.
                        anticipatedAction = AnticipatedAction.GroupNodeRemoval;
                    }
                    else
                    {
                        // the action being prepared here is addition of the node to the group.
                        anticipatedAction = AnticipatedAction.GroupNodeAddition;
                    }
                }
                else
                {
                    // this might be the start of a node move or abandoning the group and creating a new selection
                    if (diagram.Group.Nodes.Contains(node))
                    {
                        // when we continue to move from here, it's going to move the group
                        anticipatedAction = AnticipatedAction.Move;
                    }
                    else
                    {
                        // abandon group on any next interaction.
                        anticipatedAction = AnticipatedAction.NewGroup;
                    }
                }

                this.node = node;
            }
            public override InteractionState OnMouseMove(MouseEventArgs e)
            {
                if (e.Buttons != 1)
                {
                    // this is not a move, because the mouse button isn't pressed.
                    return this;
                }
                switch (anticipatedAction)
                {
                    case AnticipatedAction.GroupNodeRemoval:
                        return this; // we consider this just an accidental movement of the mouse
                    case AnticipatedAction.GroupNodeAddition:
                        diagram.Group.Add(node);
                        node.Select();
                        return new MovingNodeOrNodeGroup(this, node, e);
                    case AnticipatedAction.Move:
                        return new MovingNodeOrNodeGroup(this, node, e);
                    case AnticipatedAction.NewGroup:
                        diagram.Group.Clear();
                        diagram.Group.Add(node);
                        node.Select();
                        return new MovingNodeOrNodeGroup(this, node, e);
                    default:
                        throw new ArgumentException("This state should never be reached and points to a bug. Please raise an issue");
                }
            }
            public override InteractionState OnMouseUp(MouseEventArgs e)
            {
                switch (anticipatedAction)
                {
                    case AnticipatedAction.GroupNodeRemoval:
                        diagram.Group.Remove(node);
                        node.Deselect();
                        break;
                    case AnticipatedAction.GroupNodeAddition:
                        diagram.Group.Add(node);
                        node.Select();
                        break;
                    case AnticipatedAction.Move:
                    case AnticipatedAction.NewGroup:
                        diagram.Group.Clear();
                        diagram.Group.Add(node);
                        node.Select();
                        break;
                    default:
                        throw new ArgumentException("This state should never be reached and points to a bug. Please raise an issue");
                }
                return new Default(this);
            }
        }
    }
}