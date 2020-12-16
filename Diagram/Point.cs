namespace Excubo.Blazor.Diagrams
{
    public class Point
    {
        public double X { get; set; }
        public double Y { get; set; }
        public Point(double x, double y) => (X, Y) = (x, y);
        public Point((double X, double Y) p) => (X, Y) = p;
        public Point() { }
    }
}