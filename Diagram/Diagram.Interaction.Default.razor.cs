using Microsoft.AspNetCore.Components.Web;
using System;
using System.Linq;

namespace Excubo.Blazor.Diagrams
{
    public partial class Diagram
    {
        private sealed class Default : InteractionState
        {
            public Default(Diagram diagram) : base(diagram) { }
            public Default(InteractionState previous) : base(previous) { }
            public override InteractionState OnKeyPress(KeyboardEventArgs e)
            {
                if (e.Key == "Escape")
                {
                    foreach (var node in diagram.Group.Nodes)
                    {
                        node.Deselect();
                    }
                    diagram.Group.Clear();
                    return this;
                }
                if (e.Key == "Delete" || e.Key == "Backspace")
                {
                    if (diagram.Group.Nodes.Any())
                    {
                        var nodes = diagram.Group.Nodes.ToList();
                        diagram.Changes.NewAndDo(new ChangeAction(() =>
                        {
                            foreach (var node in nodes)
                            {
                                diagram.Nodes.Remove(node);
                            }
                        }, () =>
                        {
                            foreach (var node in nodes)
                            {
                                diagram.Nodes.Add(node);
                            }
                        }));
                        diagram.OnRemove?.Invoke(diagram.Group);
                        diagram.Group.Clear();
                    }
                    diagram.Overview?.TriggerUpdate();
                    return this;
                }
                return base.OnKeyPress(e);
            }
            public override InteractionState OnMouseDown(MouseEventArgs e)
            {
                switch (diagram.ActiveElementType)
                {
                    case HoverType.Unknown when !e.CtrlKey && !e.ShiftKey:
                        return new MousePressedOnCanvas(this);
                    case HoverType.Unknown when e.ShiftKey:
                        return new DragSelecting(this, e);
                    case HoverType.Unknown when e.CtrlKey:
                        // this isn't really a sensible thing to do. It's probably a misplaced select. We therefore do nothing at all.
                        return this;
                    case HoverType.Border:
                        // we create a new link if we simply press on the border of a node
                        return new CreatingLink(this, diagram.ActiveElement as NodeBase, e);
                    case HoverType.Node:
                        return new SelectingNode(this, diagram.ActiveElement as NodeBase, e);
                    case HoverType.Link:
                        return new ModifyingLink(this, diagram.ActiveElement as LinkBase);
                    case HoverType.NewNode:
                        return new CreatingNewNode(this, diagram.ActiveElement as NodeBase);
                    default:
                        throw new InvalidOperationException("The mouse interaction state machine wen't into a weird state. Please file this as a BUG and describe how you interacted with the diagram");
                }
            }
        }
    }
}