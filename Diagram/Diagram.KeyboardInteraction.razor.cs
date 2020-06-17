using Microsoft.AspNetCore.Components.Web;
using System.Linq;

namespace Excubo.Blazor.Diagrams
{
    public partial class Diagram
    {
        private void OnKeyPress(KeyboardEventArgs e)
        {
            if (e.Key == "Escape")
            {
                foreach (var node in Group.Nodes)
                {
                    node.Deselect();
                }
                Group = new Group();
            }
            else if (e.Key == "Delete" || e.Key == "Backspace")
            {
                if (Group.Nodes.Any() || Group.Links.Any())
                {
                    foreach (var node in Group.Nodes)
                    {
                        Nodes.Remove(node);
                    }
                    foreach (var link in Group.Links)
                    {
                        Links.Remove(link);
                    }
                    OnRemove?.Invoke(Group);
                    Group = new Group();
                }
            }
        }
    }
}
