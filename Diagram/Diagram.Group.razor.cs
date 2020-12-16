using System;

namespace Excubo.Blazor.Diagrams
{
    public partial class Diagram
    {
        internal Group Group { get; private set; } = new Group();
        public Diagram()
        {
            Group.ContentChanged += Group_ContentChanged;
        }
        private void Group_ContentChanged(object _, EventArgs __) => SelectionChanged?.Invoke(Group);
    }
}