namespace Excubo.Blazor.Diagrams
{
    public partial class Diagram
    {
        private sealed class CreatingNewNode : InteractionState
        {
            public CreatingNewNode(InteractionState previous, NodeBase templateNode) : base(previous)
            {
                diagram.Nodes.AddNewNode(templateNode, (new_node) =>
                {
                    diagram.State = new NewNode(diagram.State, new_node);
                });
            }
        }
    }
}