using Microsoft.AspNetCore.Components.Web;
using System;

namespace Excubo.Blazor.Diagrams
{
    public class Point
    {
        public double X { get; set; }
        public double Y { get; set; }
        public Point(double x, double y) => (X, Y) = (x, y);
        public Point() { }
    }
    public class ControlPoint : Point
    {
        public Action<MouseEventArgs> OnMouseOver { get; set; }
        public ControlPoint(double x, double y, Action<ControlPoint> action) : base(x, y)
        {
            OnMouseOver = (_) => action(this);
        }
        public ControlPoint(Action<ControlPoint> action) : base()
        {
            OnMouseOver = (_) => action(this);
        }
    }
}
