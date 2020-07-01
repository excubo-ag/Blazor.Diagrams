using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Excubo.Blazor.Diagrams.__Internal
{
    public partial class Overview
    {
        protected override void OnParametersSet()
        {
            Height = Width * Diagram.CanvasHeight / Diagram.CanvasWidth;
            base.OnParametersSet();
        }
        [CascadingParameter] public Diagram Diagram { get; set; }
        [Parameter] public double Width { get; set; } = 300;
        private double Height { get; set; } = 1;
        private double ViewLeft { get; set; }
        private double ViewTop { get; set; }
        private double ViewWidth { get; set; } = 4;
        private double ViewHeight { get; set; } = 4;
        private double Scale { get; set; } = 1;
        private long last_trigger;
        private bool render_requested;
        private Task update_task;
        internal void TriggerUpdate()
        {
            render_requested = true;
            var current = DateTime.UtcNow.Ticks;
            if (current < last_trigger + 25 * 100 * 1000) // 25 milliseconds in between refreshes make it 40Hz if refreshes happen often.
            {
                return;
            }
            last_trigger = current;
            if (update_task != null && !update_task.IsCompleted)
            {
                return;
            }
            RenderAsync();
        }
        private async void RenderAsync()
        {
            while (render_requested)
            {
                render_requested = false;
                update_task = CreateImgAsync();
                try
                {
                    await update_task;
                }
                catch
                {
                }
            }
        }
        private Point last_point; 
        private void OnMouseMove(MouseEventArgs e)
        {
            if (e.Buttons != 1) // only modify with mouse pressed.
            {
                last_point = null;
                return;
            }
            if (last_point != null)
            {
                Diagram.NavigationSettings.Pan(Diagram.NavigationSettings.Zoom * (last_point.X - e.ClientX) / Scale, Diagram.NavigationSettings.Zoom * (last_point.Y - e.ClientY) / Scale);
                TriggerUpdate();
                last_point.X = e.ClientX;
                last_point.Y = e.ClientY;
            }
            else
            {
                last_point = new Point(e.ClientX, e.ClientY);
            }
        }
        private async Task CreateImgAsync()
        {
            if (Height == 0 || double.IsNaN(Height))
            {
                Height = Width * Diagram.CanvasHeight / Diagram.CanvasWidth;
            }
            var min_x = Diagram.NavigationSettings.Origin.X;
            var max_x = Diagram.NavigationSettings.Origin.X + Diagram.CanvasWidth / Diagram.NavigationSettings.Zoom;
            var min_y = Diagram.NavigationSettings.Origin.Y;
            var max_y = Diagram.NavigationSettings.Origin.Y + Diagram.CanvasHeight / Diagram.NavigationSettings.Zoom;
            foreach (var node in Diagram.Nodes.all_nodes)
            {
                var margins = node.GetDrawingMargins();
                var x = node.X - margins.Left;
                var y = node.Y - margins.Top;
                var r = x + node.Width + margins.Left + margins.Right;
                var b = y + node.Height + margins.Top + margins.Bottom;
                if (x < min_x)
                { min_x = x; }
                if (y < min_y)
                { min_y = y; }
                if (r > max_x)
                { max_x = r; }
                if (b > max_y)
                { max_y = b; }
            }
            var width = min_x == double.MaxValue ? 1 : (max_x - min_x);
            var height = min_y == double.MaxValue ? 1 : (max_y - min_y);

            var h_scale = Width / width;
            var v_scale = Height / height;
            Scale = Math.Min(h_scale, v_scale);
            ViewLeft = (Diagram.NavigationSettings.Origin.X - min_x) * Scale;
            ViewTop = (Diagram.NavigationSettings.Origin.Y - min_y) * Scale;
            ViewWidth = (Diagram.CanvasWidth / Diagram.NavigationSettings.Zoom) * Scale;
            ViewHeight = (Diagram.CanvasHeight / Diagram.NavigationSettings.Zoom) * Scale;

            var hidden_canvas = canvas_2_visible ? canvas1 : canvas2;
            await using var ctx = await hidden_canvas.GetContext2DAsync(alpha: false);
            await ctx.SetTransformAsync(1, 0, 0, 1, 0, 0);
            await ctx.FillStyleAsync("white");
            await ctx.FillRectAsync(0, 0, Width, Height);
            await ctx.FillStyleAsync("#222222");
            await ctx.ScaleAsync(Scale, Scale);
            await ctx.TranslateAsync(-min_x, -min_y);
            foreach (var node in Diagram.Nodes.all_nodes.Where(n => !n.Deleted))
            {
                await node.DrawShapeAsync(ctx);
            }
            await ctx.LineWidthAsync(4);
            await ctx.StrokeStyleAsync("black");
            foreach (var link in Diagram.Links.all_links.Where(n => !n.Deleted))
            {
                await link.DrawPathAsync(ctx);
            }
            canvas_2_visible = (hidden_canvas == canvas2);
            await InvokeAsync(StateHasChanged);
        }
    }
}
