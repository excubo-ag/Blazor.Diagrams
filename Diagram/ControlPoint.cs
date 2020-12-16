using Microsoft.AspNetCore.Components.Web;
using System;

namespace Excubo.Blazor.Diagrams
{
    public class ControlPoint : Point
    {
        public Action<MouseEventArgs> OnMouseOver { get; set; }
        public Action<MouseEventArgs> OnMouseOut { get; set; }
        public ControlPoint(double x, double y, Action<ControlPoint> over_action, Action out_action) : base(x, y)
        {
            OnMouseOver = (_) => over_action(this);
            OnMouseOut = (_) => out_action();
        }
        public ControlPoint(Action<ControlPoint> over_action, Action out_action) : base()
        {
            OnMouseOver = (_) => over_action(this);
            OnMouseOut = (_) => out_action();
        }
    }
}