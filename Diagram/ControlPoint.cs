using Microsoft.AspNetCore.Components.Web;
using System;

namespace Excubo.Blazor.Diagrams
{
    public class ControlPoint : Point
    {
        public LinkBase Link { get; set; }
        public Action<MouseEventArgs> OnMouseOver { get; set; }
        public Action<MouseEventArgs> OnMouseOut { get; set; }
        public ControlPoint(double x, double y, LinkBase link, Action<ControlPoint> over_action, Action<ControlPoint> out_action) : base(x, y)
        {
            Link = link;
            OnMouseOver = (_) => over_action(this);
            OnMouseOut = (_) => out_action(this);
        }
        public ControlPoint(LinkBase link, Action<ControlPoint> over_action, Action<ControlPoint> out_action) : base()
        {
            Link = link;
            OnMouseOver = (_) => over_action(this);
            OnMouseOut = (_) => out_action(this);
        }
    }
}
