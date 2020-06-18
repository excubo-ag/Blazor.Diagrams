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
                if (Group.Nodes.Any())
                {
                    foreach (var node in Group.Nodes)
                    {
                        Nodes.Remove(node);
                    }
                    OnRemove?.Invoke(Group);
                    Group = new Group();
                }
                if (ActionObject is LinkBase link)
                {
                    Links.Remove(link);
                }
            }
        }
    }
}
