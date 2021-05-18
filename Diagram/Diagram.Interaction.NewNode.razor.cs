using Microsoft.AspNetCore.Components.Web;

namespace Excubo.Blazor.Diagrams
{
    public partial class Diagram
    {
        private sealed class NewNode : InteractionState
        {
            private readonly NodeBase node;

            public NewNode(InteractionState previous, NodeBase node) : base(previous)
            {
                this.node = node;
                diagram.Changes.New(new ChangeAction(() => diagram.Nodes.Add(node), () => diagram.Nodes.Remove(node)));
                diagram.Group.Clear();
                diagram.Group.Add(node);
                node.Select();
                diagram.node_library_wrapper.NewNodeAddingInProgress = true;
                diagram.Overview?.TriggerUpdate();
            }
            public override InteractionState OnMouseMove(MouseEventArgs e)
            {
                return new MovingNodeOrNodeGroup(this, node, e);
            }
            public override InteractionState OnMouseUp(MouseEventArgs e)
            {
                diagram.Changes.Undo();
                diagram.node_library_wrapper.NewNodeAddingInProgress = false;
                diagram.Overview?.TriggerUpdate();
                return new Default(this);
            }
        }
    }
}