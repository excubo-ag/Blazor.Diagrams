using Microsoft.AspNetCore.Components.Web;
using System.Linq;

namespace Excubo.Blazor.Diagrams
{
    public partial class Diagram
    {
        internal Changes Changes = new Changes();
    }
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
            else if (e.Key == "z" && e.CtrlKey && !e.ShiftKey)
            {
                Changes.Undo();
            }
            else if (e.Key == "Z" && e.CtrlKey && e.ShiftKey)
            {
                Changes.Redo();
            }
            else if (e.Key == "Delete" || e.Key == "Backspace")
            {
                if (ActionObject is LinkBase link)
                {
                    Changes.NewAndDo(new ChangeAction(() => Links.Remove(link), () => Links.Add(link)));
                }
                else if (Group.Nodes.Any())
                {
                    var nodes = Group.Nodes.ToList();
                    Changes.NewAndDo(new ChangeAction(() =>
                    {
                        foreach (var node in nodes)
                        {
                            Nodes.Remove(node);
                        }
                    }, () =>
                    {
                        foreach (var node in nodes)
                        {
                            Nodes.Add(node);
                        }
                    }));
                    OnRemove?.Invoke(Group);
                    Group = new Group();
                }
            }
        }
    }
}
