using Excubo.Blazor.Diagrams;
using Microsoft.AspNetCore.Components;

namespace TestProject_Components
{
    public class RectangleNodeNoLinks : RectangleNode
    {
        public override RenderFragment border => builder => { };
    }
}
