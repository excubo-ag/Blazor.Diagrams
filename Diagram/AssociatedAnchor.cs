namespace Excubo.Blazor.Diagrams
{
    public class AssociatedAnchor
    {
        public LinkBase Link { get; set; }
        public NodeAnchor Anchor { get; set; }
        public NodeBase OldNode { get; set; }
        public double OldRelativeX { get; set; }
        public double OldRelativeY { get; set; }
        public AssociatedAnchor(LinkBase link, NodeAnchor anchor)
        {
            Link = link;
            Anchor = anchor;
        }
        public void Deconstruct(out LinkBase Link, out NodeAnchor Anchor)
        {
            (Link, Anchor) = (this.Link, this.Anchor);
        }
    }
}
