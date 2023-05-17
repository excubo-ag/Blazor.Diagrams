using Excubo.Blazor.Diagrams.__Internal;
using Microsoft.AspNetCore.Components;
using Microsoft.Msagl.Core;
using Microsoft.Msagl.Core.Geometry;
using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Layout.Incremental;
using Microsoft.Msagl.Layout.Layered;
using Microsoft.Msagl.Layout.MDS;
using Microsoft.Msagl.Miscellaneous;
using System;
using System.Collections.Generic;
using System.Linq;
using MNode = Microsoft.Msagl.Core.Layout.Node;

namespace Excubo.Blazor.Diagrams
{
    public enum Algorithm
    {
        Ranking,
        FastIncremental,
        MultiDimensionalScaling,
        Sugiyama,
        TreeVerticalTopDown,
        TreeHorizontalTopDown,
        TreeVertical = TreeVerticalTopDown,
        TreeHorizontal = TreeHorizontalTopDown,
        TreeVerticalBottomUp,
        TreeHorizontalBottomUp,
    }
    public static class EnumerableExtensions
    {
        public static IEnumerable<double> Cummulative(this IEnumerable<double> values)
        {
            double running_total = 0;
            foreach (var element in values)
            {
                running_total += element;
                yield return running_total;
            }
        }
    }
    internal class FloatEqualityComparer : IEqualityComparer<double>
    {
        public bool Equals(double x, double y)
        {
            return Math.Abs(x - y) < 1e-6;
        }

        public int GetHashCode(double obj)
        {
            return 0;
        }
    }
    public class AutoLayoutSettings : ComponentBase
    {
        [CascadingParameter] public Diagram Diagram { get; set; }
        protected override void OnParametersSet()
        {
            System.Diagnostics.Debug.Assert(Diagram != null);
            Diagram.AutoLayoutSettings = this;
            base.OnParametersSet();
        }
        private Algorithm algorithm;
        [Parameter]
        public Algorithm Algorithm
        {
            get => algorithm;
            set
            {
                if (value == algorithm)
                {
                    return;
                }
                algorithm = value;
                Run();
            }
        }
        [Parameter]
        public bool PreservePorts { get; set; }
        private bool runIsRequested;
        internal void RunIfRequested()
        {
            if (!runIsRequested)
            {
                return;
            }
            Run();
        }
        public void Run()
        {
            runIsRequested = true;
            if (Diagram != null && Diagram.IsInitialized && Diagram.Nodes != null && Diagram.Nodes.all_nodes.Any() && Diagram.Nodes.all_nodes.All(n => n.HasSize) && Diagram.Links != null)
            {
                Diagram.DisableRendering();
                try
                {
                    Layout(Diagram.Nodes.all_nodes, Diagram.Links.all_links);
                    Diagram.UpdateOverview();
                }
                finally
                {
                    Diagram.EnableRendering();
                    runIsRequested = false;
                }
            }
        }
        internal void Layout(List<NodeBase> all_nodes, List<LinkBase> all_links)
        {
            switch (Algorithm)
            {
                case Algorithm.TreeVertical:
                case Algorithm.TreeHorizontal:
                case Algorithm.TreeVerticalBottomUp:
                case Algorithm.TreeHorizontalBottomUp:
                    TreeAlgorithm(all_nodes, all_links);
                    break;
                case Algorithm.Sugiyama:
                case Algorithm.Ranking:
                case Algorithm.MultiDimensionalScaling:
                case Algorithm.FastIncremental:
                    MSAGL(all_nodes, all_links);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
        #region tree
        private void TreeAlgorithm(List<NodeBase> all_nodes, List<LinkBase> all_links)
        {
            var links_by_source = all_links
                .Where(l => l.Source?.Node != null && l.Target?.Node != null)
                .GroupBy(e => e.Source.Node, e => e)
                .ToDictionary(e => e.Key, e => e.ToList());
            var links_by_target = all_links
                .Where(l => l.Source?.Node != null && l.Target?.Node != null)
                .GroupBy(e => e.Target.Node, e => e)
                .ToDictionary(e => e.Key, e => e.ToList());
            var connected_groups = FindConnectedGroups(all_nodes, links_by_source, links_by_target);
            const double separation_between_groups = 50;
            double offset = 0;
            // 1. Establish hierarchy.
            foreach (var group in connected_groups)
            {
                var links_by_source_this_group = links_by_source.Where(kv => group.Contains(kv.Key)).ToDictionary(kv => kv.Key, kv => kv.Value);
                var links_by_target_this_group = links_by_target.Where(kv => group.Contains(kv.Key)).ToDictionary(kv => kv.Key, kv => kv.Value);
                var all_links_this_group = links_by_source_this_group.SelectMany(kv => kv.Value).ToList();
                var layers = (Algorithm == Algorithm.TreeHorizontal || Algorithm == Algorithm.TreeVertical)
                    ? GetLayersTopDown(group, links_by_source_this_group, links_by_target_this_group)
                    : GetLayersBottomUp(group, links_by_source_this_group, links_by_target_this_group);
                ArrangeNodesWithinLayers(links_by_source_this_group, links_by_target_this_group, layers);
                layers = layers.Where(layer => layer.Any()).ToList();
                // 3. Layout time!
                ArrangeNodes(links_by_source_this_group, links_by_target_this_group, layers, offset);
                // 4. fix link positions. This is easy, because we work top down, except when we have upwards arrows (cycles) or ltr/rtl arrows.
                ArrangeLinks(all_links_this_group, layers);
                var is_vertical = Algorithm is Algorithm.TreeVertical or Algorithm.TreeVerticalBottomUp or Algorithm.TreeVerticalTopDown;
                offset = is_vertical
                    ? layers.Last().Max(n => n.Y + n.Height + n.GetDrawingMargins().Bottom)
                    : layers.Last().Max(n => n.X + n.Width + n.GetDrawingMargins().Right);
                offset += separation_between_groups;
            }
            foreach (var node in all_nodes)
            {
                node.ApplyMoveTo();
            }
        }

        private List<List<NodeBase>> FindConnectedGroups(List<NodeBase> all_nodes, Dictionary<NodeBase, List<LinkBase>> links_by_source, Dictionary<NodeBase, List<LinkBase>> links_by_target)
        {
            var groups = new List<List<NodeBase>>();
            var copy = new List<NodeBase>(all_nodes);
            while (copy.Any())
            {
                var node = copy.Last();
                copy.RemoveAt(copy.Count - 1);
                var group = new List<NodeBase> { node };
                var unvisited = group.ToList();
                while (unvisited.Any())
                {
                    var current = unvisited.Last();
                    unvisited.RemoveAt(unvisited.Count - 1);
                    if (links_by_source.TryGetValue(current, out var sLinks))
                    {
                        foreach (var link in sLinks)
                        {
                            if (copy.Contains(link.Target.Node))
                            {
                                copy.Remove(link.Target.Node);
                                unvisited.Add(link.Target.Node);
                                group.Add(link.Target.Node);
                            }
                        }
                    }
                    if (links_by_target.TryGetValue(current, out var tLinks))
                    {
                        foreach (var link in tLinks)
                        {
                            if (copy.Contains(link.Source.Node))
                            {
                                copy.Remove(link.Source.Node);
                                unvisited.Add(link.Source.Node);
                                group.Add(link.Source.Node);
                            }
                        }
                    }
                }
                groups.Add(group);
            }
            return groups;
        }

        private void ArrangeLinks(List<LinkBase> all_links, List<List<NodeBase>> layers)
        {
            if (Algorithm == Algorithm.TreeVerticalBottomUp || Algorithm == Algorithm.TreeVerticalTopDown)
            {
                ArrangeLinksForVerticalTree(all_links, layers);
            }
            else
            {
                ArrangeLinksForHorizontalTree(all_links, layers);
            }
        }
        private static void ArrangeLinksForHorizontalTree(List<LinkBase> all_links, List<List<NodeBase>> layers)
        {
            foreach (var link in all_links)
            {
                if (link.Source != null && link.Target != null)
                {
                    var source_layer_index = layers.IndexOf(layers.First(l => l.Contains(link.Source.Node)));
                    var target_layer_index = layers.IndexOf(layers.First(l => l.Contains(link.Target.Node)));
                    if (source_layer_index < target_layer_index)
                    {
                        link.Source.Port = Position.Right;
                        link.Target.Port = Position.Left;
                    }
                    else if (source_layer_index > target_layer_index)
                    {
                        link.Source.Port = Position.Left;
                        link.Target.Port = Position.Right;
                    }
                    else
                    {
                        var ltr = link.Source.Node.Y < link.Target.Node.Y;
                        link.Source.Port = ltr ? Position.Bottom : Position.Top;
                        link.Target.Port = ltr ? Position.Top : Position.Bottom;
                    }
                }
            }
        }
        private static void ArrangeLinksForVerticalTree(List<LinkBase> all_links, List<List<NodeBase>> layers)
        {
            foreach (var link in all_links)
            {
                if (link.Source == null || link.Target == null)
                {
                    continue;
                }
                var source_layer = layers.FirstOrDefault(l => l.Contains(link.Source.Node));
                var target_layer = layers.FirstOrDefault(l => l.Contains(link.Target.Node));
                if (source_layer == null || target_layer == null)
                {
                    continue;
                }
                var source_layer_index = layers.IndexOf(source_layer);
                var target_layer_index = layers.IndexOf(target_layer);
                if (source_layer_index < target_layer_index)
                {
                    link.Source.Port = Position.Bottom;
                    link.Target.Port = Position.Top;
                }
                else if (source_layer_index > target_layer_index)
                {
                    link.Source.Port = Position.Top;
                    link.Target.Port = Position.Bottom;
                }
                else
                {
                    var ltr = link.Source.Node.X < link.Target.Node.X;
                    link.Source.Port = ltr ? Position.Right : Position.Left;
                    link.Target.Port = ltr ? Position.Left : Position.Right;
                }
            }
        }
        private void ArrangeNodes(Dictionary<NodeBase, List<LinkBase>> links_by_source, Dictionary<NodeBase, List<LinkBase>> links_by_target, List<List<NodeBase>> layers, double initial_offset)
        {
            foreach (var node in layers.SelectMany(l => l))
            {
                node.MoveToWithoutUIUpdate(0, 0);
            }
            const double vertical_separation = 50;
            const double horizontal_separation = 50;
            if (Algorithm == Algorithm.TreeVerticalTopDown || Algorithm == Algorithm.TreeVerticalBottomUp)
            {
                Func<NodeBase, double> between_layer_start_margin = (node) => node.GetHeight() / 2;
                Func<NodeBase, double> position = (node) => node.X;
                Action<NodeBase, double> adjust_position = (node, x) => node.MoveToWithoutUIUpdate(x, node.Y);
                Func<NodeBase, double> top_or_left_margin = (node) => node.GetDrawingMargins().Left;
                Func<NodeBase, double> space = (node) => node.GetWidth();
                Func<NodeBase, double> bottom_or_right_margin = (node) => node.GetDrawingMargins().Right;
                Func<NodeBase, double> orthogonal_space_with_margins = (node) => node.GetHeight() + node.GetDrawingMargins().Top + node.GetDrawingMargins().Bottom;
                Func<double, Action<NodeBase, double, double>> move_generator = (y) => (node, x, y_offset) => node.MoveToWithoutUIUpdate(x, initial_offset + y - y_offset);
                ArrangeNodes(links_by_source, links_by_target, layers, vertical_separation, horizontal_separation,
                    move_generator, adjust_position, orthogonal_space_with_margins, between_layer_start_margin, position,
                    top_or_left_margin, space, bottom_or_right_margin);
            }
            else
            {
                Func<NodeBase, double> between_layer_start_margin = (node) => node.GetWidth() / 2;
                Func<NodeBase, double> position = (node) => node.Y;
                Action<NodeBase, double> adjust_position = (node, y) => node.MoveToWithoutUIUpdate(node.X, y);
                Func<NodeBase, double> top_or_left_margin = (node) => node.GetDrawingMargins().Top;
                Func<NodeBase, double> space = (node) => node.GetHeight();
                Func<NodeBase, double> bottom_or_right_margin = (node) => node.GetDrawingMargins().Bottom;
                Func<NodeBase, double> orthogonal_space_with_margins = (node) => node.GetWidth() + node.GetDrawingMargins().Left + node.GetDrawingMargins().Right;
                Func<double, Action<NodeBase, double, double>> move_generator = (x) => (node, y, x_offset) => node.MoveToWithoutUIUpdate(initial_offset + x - x_offset, y);
                ArrangeNodes(links_by_source, links_by_target, layers, horizontal_separation, vertical_separation,
                    move_generator, adjust_position, orthogonal_space_with_margins, between_layer_start_margin, position,
                    top_or_left_margin, space, bottom_or_right_margin);
            }
        }
        private static void ArrangeNodes(Dictionary<NodeBase, List<LinkBase>> links_by_source, Dictionary<NodeBase, List<LinkBase>> links_by_target,
            List<List<NodeBase>> layers,
            double layer_separation,
            double in_layer_separation,
            Func<double, Action<NodeBase, double, double>> move_generator,
            Action<NodeBase, double> adjust_position,
            Func<NodeBase, double> orthogonal_space_with_margins,
            Func<NodeBase, double> between_layer_start_margin,
            Func<NodeBase, double> position,
            Func<NodeBase, double> top_or_left_margin,
            Func<NodeBase, double> space,
            Func<NodeBase, double> bottom_or_right_margin)
        {
            var widths = layers.Select(layer =>
                layer.Select(n => space(n) + top_or_left_margin(n) + bottom_or_right_margin(n)).ToList())
                .ToList();
            var layer_heights = layers.Select(layer => layer.Max(n => orthogonal_space_with_margins(n))).ToList();
            var layer_widths = widths.Select(layer => in_layer_separation * (layer.Count - 1) + layer.Sum()).ToList();
            var widest_layer_width = layer_widths.Max();
            var widest_layer_index = layer_widths.IndexOf(widest_layer_width);
            var widest_layer = layers[widest_layer_index];

            // Provisionally arrange the widest layer
            {
                double x = 0;
                var y = layer_heights.Take(widest_layer_index).Sum() + layer_separation * widest_layer_index + layer_heights[widest_layer_index] / 2;
                var move = move_generator(y);
                foreach (var node in widest_layer)
                {
                    move(node, x + top_or_left_margin(node), between_layer_start_margin(node));
                    x += space(node) + top_or_left_margin(node) + bottom_or_right_margin(node) + in_layer_separation;
                }
            }
            OptimizeBelowWidestLayer(links_by_target, layers, layer_separation, in_layer_separation, layer_heights, widest_layer_index, move_generator, between_layer_start_margin, position, top_or_left_margin, space, bottom_or_right_margin);
            OptimizeAboveWidestLayer(links_by_source, layers, layer_separation, in_layer_separation, layer_heights, widest_layer_index, move_generator, between_layer_start_margin, position, top_or_left_margin, space, bottom_or_right_margin);
            if (widest_layer_index > 0)
            {
                // optimize the widest layer by the layers above
                var active_layer = layers[widest_layer_index];
                var wish_positions = active_layer.Zip(active_layer.Select(n =>
                {
                    if (!links_by_target.ContainsKey(n))
                    {
                        return double.MaxValue;
                    }
                    return links_by_target[n].Average(l => position(l.Source.Node) + space(l.Source.Node) / 2);
                }), (Node, Position) => (Node, Position)).OrderBy(e => e.Position).ToList();

                wish_positions = Implementation.EnsureSeparation(in_layer_separation, wish_positions, top_or_left_margin, space, bottom_or_right_margin);

                var y = layer_heights.Take(widest_layer_index).Sum() + layer_separation * widest_layer_index + layer_heights[widest_layer_index] / 2;
                var move = move_generator(y);
                PlaceNodesInLayer(in_layer_separation, wish_positions, move, between_layer_start_margin, position, top_or_left_margin, space, bottom_or_right_margin);
            }
            else
            {
                // optimize the widest layer by the layers below
                var active_layer = layers[widest_layer_index];
                var wish_positions = active_layer.Zip(active_layer.Select(n =>
                {
                    if (!links_by_source.ContainsKey(n))
                    {
                        return double.MaxValue;
                    }
                    return links_by_source[n].Average(l => position(l.Target.Node) + space(l.Target.Node) / 2);
                }), (Node, Position) => (Node, Position)).OrderBy(e => e.Position).ToList();

                wish_positions = Implementation.EnsureSeparation(in_layer_separation, wish_positions, top_or_left_margin, space, bottom_or_right_margin);

                var y = layer_heights.Take(widest_layer_index).Sum() + layer_separation * widest_layer_index + layer_heights[widest_layer_index] / 2;
                var move = move_generator(y);
                PlaceNodesInLayer(in_layer_separation, wish_positions, move, between_layer_start_margin, position, top_or_left_margin, space, bottom_or_right_margin);
            }
            OptimizeBelowWidestLayer(links_by_target, layers, layer_separation, in_layer_separation, layer_heights, widest_layer_index, move_generator, between_layer_start_margin, position, top_or_left_margin, space, bottom_or_right_margin);
            OptimizeAboveWidestLayer(links_by_source, layers, layer_separation, in_layer_separation, layer_heights, widest_layer_index, move_generator, between_layer_start_margin, position, top_or_left_margin, space, bottom_or_right_margin);

            IEnumerable<(NodeBase Node, double Position, bool IsLeftMargin)> NodeWithLeftOrRightMargin(NodeBase n)
            {
                yield return (n, position(n) - space(n) / 2 - top_or_left_margin(n), true);
                yield return (n, position(n) + space(n) / 2 + bottom_or_right_margin(n), false);
            }

            var nodes_ordered_by_position = layers.SelectMany(layer => layer)
                .SelectMany(NodeWithLeftOrRightMargin)
                .OrderBy(n => n.Position) // order the nodes by their position
                .ToList();
            for (int i = 0; i + 1 < nodes_ordered_by_position.Count; ++i)
            {
                var left = nodes_ordered_by_position[i];
                var right = nodes_ordered_by_position[i + 1];
                if (left.IsLeftMargin || !right.IsLeftMargin)
                {
                    continue;
                }
                // we are now looking at the right-most position of the left node and the left-most position of the right node
                var separation = right.Position - left.Position;
                if (separation > in_layer_separation)
                {
                    for (int j = i + 1; j < nodes_ordered_by_position.Count; ++j)
                    {
                        if (!nodes_ordered_by_position[j].IsLeftMargin)
                        {
                            // only nodes that have their left margin further right than the left node should be moved
                            continue;
                        }
                        adjust_position(nodes_ordered_by_position[j].Node, position(nodes_ordered_by_position[j].Node) - (separation - in_layer_separation));
                    }
                }
            }
        }
        private static void OptimizeAboveWidestLayer(Dictionary<NodeBase, List<LinkBase>> links_by_source,
            List<List<NodeBase>> layers, double layer_separation, double in_layer_separation, List<double> layer_heights, int widest_layer_index,
            Func<double, Action<NodeBase, double, double>> move_generator,
            Func<NodeBase, double> between_layer_start_margin,
            Func<NodeBase, double> position,
            Func<NodeBase, double> top_or_left_margin,
            Func<NodeBase, double> space,
            Func<NodeBase, double> bottom_or_right_margin)
        {
            for (int i = widest_layer_index; i > 0;)
            {
                --i;
                // going through the layers _above_ the widest layer
                var active_layer = layers[i];
                var wish_positions = active_layer.Zip(active_layer.Select(n =>
                {
                    if (!links_by_source.ContainsKey(n))
                    {
                        return double.MaxValue;
                    }
                    return links_by_source[n].Average(l => position(l.Target.Node) + space(l.Target.Node) / 2);
                }), (Node, Position) => (Node, Position)).OrderBy(e => e.Position).ToList();

                wish_positions = Implementation.EnsureSeparation(in_layer_separation, wish_positions, top_or_left_margin, space, bottom_or_right_margin);

                var y = layer_heights.Take(i).Sum() + layer_separation * i + layer_heights[i] / 2;
                var move = move_generator(y);
                PlaceNodesInLayer(in_layer_separation, wish_positions, move, between_layer_start_margin, position, top_or_left_margin, space, bottom_or_right_margin);
            }
        }

        private static void OptimizeBelowWidestLayer(Dictionary<NodeBase, List<LinkBase>> links_by_target,
            List<List<NodeBase>> layers, double layer_separation, double in_layer_separation, List<double> layer_heights, int widest_layer_index,
            Func<double, Action<NodeBase, double, double>> move_generator,
            Func<NodeBase, double> between_layer_start_margin,
            Func<NodeBase, double> position,
            Func<NodeBase, double> top_or_left_margin,
            Func<NodeBase, double> space,
            Func<NodeBase, double> bottom_or_right_margin)
        {
            for (int i = widest_layer_index + 1; i < layers.Count; ++i)
            {
                // going through the layers _below_ the widest layer
                var active_layer = layers[i];
                var wish_positions = active_layer.Zip(active_layer.Select(n =>
                {
                    if (!links_by_target.ContainsKey(n))
                    {
                        return double.MaxValue;
                    }
                    return links_by_target[n].Average(l => position(l.Source.Node) + space(l.Source.Node) / 2);
                }), (Node, Position) => (Node, Position)).OrderBy(e => e.Position).ToList();

                wish_positions = Implementation.EnsureSeparation(in_layer_separation, wish_positions, top_or_left_margin, space, bottom_or_right_margin);

                var y = layer_heights.Take(i).Sum() + layer_separation * i + layer_heights[i] / 2;
                var move = move_generator(y);
                PlaceNodesInLayer(in_layer_separation, wish_positions, move, between_layer_start_margin, position, top_or_left_margin, space, bottom_or_right_margin);
            }
        }
        internal static class Implementation
        {
            internal static List<(NodeBase Node, double Position)> EnsureSeparation(
                double in_layer_separation,
                List<(NodeBase Node, double Position)> wish_positions,
                Func<NodeBase, double> top_or_left_margin,
                Func<NodeBase, double> space,
                Func<NodeBase, double> bottom_or_right_margin)
            {
                var ignored = wish_positions.Where(e => e.Position == double.MaxValue).ToList();
                var to_adjust = wish_positions.Where(e => e.Position != double.MaxValue).OrderBy(e => e.Position).ToList();
                if (to_adjust.Count <= 1)
                {
                    return wish_positions;
                }
                var original_wishes = to_adjust.ToList();
                to_adjust = ApplyForceBasedApproach(in_layer_separation, to_adjust, top_or_left_margin, space, bottom_or_right_margin);
                to_adjust = ApplyWishBasedApproach(in_layer_separation, original_wishes, to_adjust, top_or_left_margin, space, bottom_or_right_margin);
                to_adjust = ApplyForceBasedApproach(in_layer_separation, to_adjust, top_or_left_margin, space, bottom_or_right_margin);


                return to_adjust.Concat(ignored).ToList();
            }

            private static List<(NodeBase Node, double Position)> ApplyForceBasedApproach(
                double in_layer_separation,
                List<(NodeBase Node, double Position)> to_adjust,
                Func<NodeBase, double> top_or_left_margin,
                Func<NodeBase, double> space,
                Func<NodeBase, double> bottom_or_right_margin)
            {
                var forces = Enumerable.Range(0, to_adjust.Count - 1).Select(i =>
                {
                    var (left_node, left_position) = to_adjust[i];
                    var (right_node, right_position) = to_adjust[i + 1];
                    var left_side_of_right = right_position - space(right_node) / 2 - top_or_left_margin(right_node);
                    var right_side_of_left = left_position + space(left_node) / 2 + bottom_or_right_margin(left_node);
                    var current_separation = left_side_of_right - right_side_of_left;
                    return (in_layer_separation - current_separation) / 2;
                }).ToList();

                IEnumerable<TCummulative> Cummulative<TCummulative, TValue>(IEnumerable<TValue> values, TCummulative seed, Func<TCummulative, TValue, TCummulative> func)
                {
                    yield return seed;
                    foreach (var value in values)
                    {
                        seed = func(seed, value);
                        yield return seed;
                    }
                }
                Func<double, double, double> forcePropagation = (existing, additional) => Math.Max(0, existing + additional);

                var forces_to_right_cummulative = Cummulative(forces, 0, forcePropagation).ToList();
                forces.Reverse();
                var forces_to_left_cummulative = Cummulative(forces, 0, forcePropagation).Reverse().ToList();

                // step one: fix the separation issues
                for (int i = 0; i < to_adjust.Count; ++i)
                {
                    var force_to_left = forces_to_left_cummulative[i];
                    var force_to_right = forces_to_right_cummulative[i];
                    to_adjust[i] = (to_adjust[i].Node, to_adjust[i].Position - force_to_left + force_to_right);
                }
                return to_adjust;
            }
            private static List<(NodeBase Node, double Position)> ApplyWishBasedApproach(
                double in_layer_separation,
                List<(NodeBase Node, double Position)> original_wishes,
                List<(NodeBase Node, double Position)> to_adjust,
                Func<NodeBase, double> top_or_left_margin,
                Func<NodeBase, double> space,
                Func<NodeBase, double> bottom_or_right_margin)
            {
                // step two: find the "rigid" ranges.
                // A rigid range is a range of consecutive nodes, where there is to freedom to move the elements within this range closer to each other without violating the separation constraint.
                IEnumerable<List<(NodeBase Node, double CurrentPosition, double Wish)>> InGroups()
                {
                    List<(NodeBase Node, double CurrentPosition, double Wish)> group = new();
                    group.Add((to_adjust[0].Node, to_adjust[0].Position, original_wishes[0].Position));
                    for (int i = 0; i + 1 < to_adjust.Count; ++i)
                    {
                        var (left_node, left_position) = to_adjust[i];
                        var (right_node, right_position) = to_adjust[i + 1];
                        var left_side_of_right = right_position - space(right_node) / 2 - top_or_left_margin(right_node);
                        var right_side_of_left = left_position + space(left_node) / 2 + bottom_or_right_margin(left_node);
                        var current_separation = left_side_of_right - right_side_of_left;
                        if (current_separation <= in_layer_separation + 1e-6)
                        {
                            // node i + 1 is still in the same group as node i
                            group.Add((to_adjust[i + 1].Node, to_adjust[i + 1].Position, original_wishes[i + 1].Position));
                        }
                        else
                        {
                            // the group is full. Yield group, and create a new one for i + 1.
                            yield return group;
                            group = new();
                            group.Add((to_adjust[i + 1].Node, to_adjust[i + 1].Position, original_wishes[i + 1].Position));
                        }
                    }
                    if (group.Any())
                    {
                        yield return group;
                    }
                }
                IEnumerable<(NodeBase Node, double Position)> ApplyAverageForce(List<(NodeBase Node, double CurrentPosition, double Wish)> group)
                {
                    var average_force = group.Average(ncw => ncw.Wish - ncw.CurrentPosition);
                    foreach (var element in group)
                    {
                        yield return (Node: element.Node, element.CurrentPosition + average_force);
                    }
                }
                return InGroups().SelectMany(ApplyAverageForce).ToList();
            }
        }

        private static void PlaceNodesInLayer(
            double in_layer_separation,
            List<(NodeBase Node, double Position)> nodes_and_positions,
            Action<NodeBase, double, double> move,
            Func<NodeBase, double> between_layer_start_margin,
            Func<NodeBase, double> position,
            Func<NodeBase, double> top_or_left_margin,
            Func<NodeBase, double> space,
            Func<NodeBase, double> bottom_or_right_margin)
        {
            foreach (var (node, in_layer_position) in nodes_and_positions.Where(kv => kv.Position != double.MaxValue))
            {
                move(node, in_layer_position - space(node) / 2, between_layer_start_margin(node));
            }
            var right_most = nodes_and_positions.Where(kv => kv.Position != double.MaxValue).Select(kv => kv.Node).LastOrDefault();
            foreach (var (node, _) in nodes_and_positions.Where(kv => kv.Position == double.MaxValue))
            {
                if (right_most == null)
                {
                    move(node, 0, between_layer_start_margin(node));
                }
                else
                {
                    move(node, position(right_most) + space(right_most) + bottom_or_right_margin(right_most) + in_layer_separation + top_or_left_margin(node), between_layer_start_margin(node));
                }
                right_most = node;
            }
        }

        private static void ArrangeNodesWithinLayers(Dictionary<NodeBase, List<LinkBase>> links_by_source, Dictionary<NodeBase, List<LinkBase>> links_by_target, List<List<NodeBase>> layers)
        {
            // 2.1. sort out second and first layer
            if (layers.Count > 1)
            {
                layers[1] = layers[1].OrderBy(node =>
                {
                    if (!links_by_target.ContainsKey(node)) // node is not connected from anywhere, so also not from layer 0.
                    {
                        return 0;
                    }
                    // calculate average position based on connected nodes in top layer
                    var connected_nodes = links_by_target[node].Where(l => layers[0].Contains(l.Source.Node)).Select(l => l.Source.Node).ToList();
                    if (!connected_nodes.Any())
                    {
                        return 0;
                    }
                    var average_index = connected_nodes.Select(cn => { var i = layers[0].IndexOf(cn); return i; }).Average();
                    return average_index;
                }).ToList();
                layers[0] = layers[0].OrderBy(node =>
                {
                    if (!links_by_source.ContainsKey(node)) // node is not connected to anywhere, so also not from layer 1.
                    {
                        return 0;
                    }
                    // calculate average position based on connected nodes in top layer
                    var connected_nodes = links_by_source[node].Where(l => layers[1].Contains(l.Target.Node)).Select(l => l.Target.Node).ToList();
                    if (!connected_nodes.Any())
                    {
                        return 0;
                    }
                    var average_index = connected_nodes.Select(cn => { var i = layers[1].IndexOf(cn); return i; }).Average();
                    return average_index;
                }).ToList();
            }
            // 2.2. sort out each layer by looking at where it connects from
            for (var i = 0; i + 1 < layers.Count; ++i)
            {
                var top_layer = layers[i];
                layers[i + 1] = layers[i + 1].OrderBy(node =>
                {
                    if (!links_by_target.ContainsKey(node)) // node is not connected from anywhere, so also not from top_layer.
                    {
                        return 0;
                    }
                    // calculate average position based on connected nodes in top layer
                    var connected_nodes = links_by_target[node].Where(l => top_layer.Contains(l.Source.Node)).Select(l => l.Source.Node).ToList();
                    if (!connected_nodes.Any())
                    {
                        return 0;
                    }
                    var average_index = connected_nodes.Select(cn => { var i = top_layer.IndexOf(cn); return i; }).Average();
                    return average_index;
                }).ToList();
            }
        }

        private static List<List<NodeBase>> GetLayersTopDown(List<NodeBase> all_nodes, Dictionary<NodeBase, List<LinkBase>> links_by_source, Dictionary<NodeBase, List<LinkBase>> links_by_target)
        {
            // 1.0 if we don't have nodes, we have no layers
            if (!all_nodes.Any())
            {
                return new List<List<NodeBase>>();
            }
            // 1.1. We identify the nodes that have no incoming link
            var roots = all_nodes.Where(n => !links_by_target.ContainsKey(n)).ToList();
            // 1.2. If we do not have any such node, we exclusively have cycles. We now randomly pick a root.
            // That random choice is the first node. Chosen by a fair die, as legend has it.
            roots = roots.Any() ? roots : all_nodes.Take(1).ToList();

            var (layer_assignments, max_layer) = PushNodesDown(roots, links_by_source);
            var layers = new List<List<NodeBase>>(max_layer + 1);
            for (int i = 0; i <= max_layer; ++i)
            {
                layers.Add(new List<NodeBase>());
            }
            foreach (var kv in layer_assignments)
            {
                var node = kv.Key;
                var layer = kv.Value;
                layers[layer].Add(node);
            }

            // 1.5. we might have remaining remaining nodes. We calculate the layout for those too, and merge it afterwards
            var rest_in_layers = GetLayersTopDown(all_nodes.Except(layers.SelectMany(n => n)).ToList(), links_by_source, links_by_target);
            layers.Merge(rest_in_layers);

            OptimizeNodePositions(all_nodes, links_by_source, links_by_target, layers);

            return layers;
        }

        private static void OptimizeNodePositions(List<NodeBase> all_nodes, Dictionary<NodeBase, List<LinkBase>> links_by_source, Dictionary<NodeBase, List<LinkBase>> links_by_target, List<List<NodeBase>> layers)
        {
            while (true)
            {
                // helper method to find the lowest-index layer containing any of the candidate nodes
                static int GetLowestLevel(List<List<NodeBase>> layers, List<NodeBase> candidates)
                {
                    foreach (var i in Enumerable.Range(0, layers.Count))
                    {
                        foreach (var ntc in candidates)
                        {
                            if (layers[i].Contains(ntc))
                            {
                                return i;
                            }
                        }
                    }
                    return layers.Count;
                }
                // helper method to find the highest-index layer containing any of the candidate nodes
                static int GetHighestLevel(List<List<NodeBase>> layers, List<NodeBase> candidates)
                {
                    foreach (var i in Enumerable.Range(0, layers.Count).Reverse())
                    {
                        foreach (var ntc in candidates)
                        {
                            if (layers[i].Contains(ntc))
                            {
                                return i;
                            }
                        }
                    }
                    return 0;
                }

                var another_round_required = false;
                // SINK I 
                // For all nodes 
                foreach (var node in all_nodes.Where(nd => !links_by_target.ContainsKey(nd))) // look at all nodes without incoming links 
                {
                    if (!links_by_source.ContainsKey(node))
                    {
                        continue;
                    }
                    var nodes_to_check = links_by_source[node]
                        .Select(x => x.Target.Node)        // get target nodes from links
                        .ToList();

                    var lowest_target_level = GetLowestLevel(layers, nodes_to_check);
                    var current_layer = layers.IndexOf(layers.First(layer => layer.Contains(node)));
                    // if own level < lowest_target_level - 1 -> move own to lowest_target_level - 1 
                    if (lowest_target_level - 1 > current_layer)
                    {
                        // reposition node 
                        layers[lowest_target_level - 1].Add(node);
                        layers[current_layer].Remove(node);
                        another_round_required = true;
                    }
                }
                // SINK II 
                foreach (var node in all_nodes.Where(nd => !links_by_source.ContainsKey(nd))) // look at all nodes without outgoing links 
                {
                    if (!links_by_target.ContainsKey(node))
                    {
                        continue;
                    }
                    var nodes_to_check = links_by_target[node]
                        .Select(x => x.Source.Node)        // get source nodes from links
                        .ToList();

                    var highest_target_level = GetHighestLevel(layers, nodes_to_check);
                    var current_layer = layers.IndexOf(layers.First(layer => layer.Contains(node)));
                    // if own level < highest + 1 -> move own to highest + 1 
                    if (highest_target_level + 1 > current_layer)
                    {
                        // check if layer needs adding 
                        if (highest_target_level + 2 > layers.Count)
                        {
                            layers.Add(new List<NodeBase>());
                        }
                        // reposition node
                        layers[highest_target_level + 1].Add(node);
                        layers[current_layer].Remove(node);
                        another_round_required = true;
                    }
                }
                if (!another_round_required)
                {
                    break;
                }
            }
        }

        private static List<List<NodeBase>> GetLayersBottomUp(List<NodeBase> all_nodes, Dictionary<NodeBase, List<LinkBase>> links_by_source, Dictionary<NodeBase, List<LinkBase>> links_by_target)
        {
            // 1.0 if we don't have nodes, we have no layers
            if (!all_nodes.Any())
            {
                return new List<List<NodeBase>>();
            }
            // 1.1. We identify the nodes that have no outgoing link
            var leaves = all_nodes.Where(n => !links_by_source.ContainsKey(n)).ToList();
            // 1.2. If we do not have any such node, we exclusively have cycles. We now randomly pick a leaf.
            // That random choice is the first node. Chosen by a fair die, as legend has it.
            leaves = leaves.Any() ? leaves : all_nodes.Take(1).ToList();

            var (layer_assignments, max_layer) = PushNodesUp(leaves, links_by_target);
            var layers = new List<List<NodeBase>>(max_layer + 1);
            for (int i = 0; i <= max_layer; ++i)
            {
                layers.Add(new List<NodeBase>());
            }
            foreach (var kv in layer_assignments)
            {
                var node = kv.Key;
                var layer = kv.Value;
                layers[layer].Add(node);
            }
            layers.Reverse();

            // 1.5. we might have remaining remaining nodes. We calculate the layout for those too, and merge it afterwards
            var rest_in_layers = GetLayersBottomUp(all_nodes.Except(layers.SelectMany(n => n)).ToList(), links_by_source, links_by_target);
            layers.Merge(rest_in_layers);

            return layers;
        }

        private static (Dictionary<NodeBase, int> LayerAssignments, int HighestLayer) PushNodesUp(List<NodeBase> leaves, Dictionary<NodeBase, List<LinkBase>> links_by_target)
        {
            var layer_assignments = leaves.ToDictionary(leaf => leaf, _ => 0);
            var nodes_pushing_up = leaves.ToDictionary(leaf => leaf, _ => new List<NodeBase>());

            // 1.3. Now we identify all nodes that have an incoming link from this node and put those into the second layer.
            // We continue this until all nodes have been assigned to layers.

            var look_at = leaves.ToList();
            var max_layer = 0;

            // 1.4. we use the notion of nodes pushing "up" other nodes. Ignoring cycles, a node is pushed just above the highest node below it.
            while (look_at.Any())
            {
                var new_look_at = new List<NodeBase>();
                foreach (var target in look_at)
                {
                    if (!links_by_target.ContainsKey(target))
                    {
                        continue;
                    }
                    var links = links_by_target[target];
                    foreach (var source in links.Select(l => l.Source.Node))
                    {
                        if (nodes_pushing_up.ContainsKey(target) && nodes_pushing_up[target].Contains(source))
                        {
                            continue; // cycle detected
                        }
                        var assigned_layer = layer_assignments.ContainsKey(source)
                            ? Math.Max(layer_assignments[source], layer_assignments[target] + 1)
                            : layer_assignments[target] + 1;
                        layer_assignments[source] = assigned_layer;
                        max_layer = Math.Max(assigned_layer, max_layer);
                        if (nodes_pushing_up.ContainsKey(source))
                        {
                            nodes_pushing_up[source].Add(target);
                        }
                        else
                        {
                            nodes_pushing_up.Add(source, nodes_pushing_up[target].Append(target).ToList());
                        }
                        if (!new_look_at.Contains(source))
                        {
                            new_look_at.Add(source);
                        }
                    }
                }
                look_at = new_look_at;
            }

            return (layer_assignments, max_layer);
        }
        private static (Dictionary<NodeBase, int> LayerAssignments, int HighestLayer) PushNodesDown(List<NodeBase> roots, Dictionary<NodeBase, List<LinkBase>> links_by_source)
        {
            var layer_assignments = roots.ToDictionary(root => root, _ => 0);
            var nodes_pushing_down = roots.ToDictionary(root => root, _ => new List<NodeBase>());

            // 1.3. Now we identify all nodes that have an incoming link from this node and put those into the second layer.
            // We continue this until all nodes have been assigned to layers.

            var look_at = roots.ToList();
            var max_layer = 0;

            // 1.4. we use the notion of nodes pushing "up" other nodes. Ignoring cycles, a node is pushed just above the highest node below it.
            while (look_at.Any())
            {
                var new_look_at = new List<NodeBase>();
                foreach (var source in look_at)
                {
                    if (!links_by_source.ContainsKey(source))
                    {
                        continue;
                    }
                    var links = links_by_source[source];
                    foreach (var target in links.Select(l => l.Target.Node))
                    {
                        if (nodes_pushing_down.ContainsKey(source) && nodes_pushing_down[source].Contains(target))
                        {
                            continue; // cycle detected
                        }
                        var assigned_layer = layer_assignments.ContainsKey(target)
                            ? Math.Max(layer_assignments[target], layer_assignments[source] + 1)
                            : layer_assignments[source] + 1;
                        layer_assignments[target] = assigned_layer;
                        max_layer = Math.Max(assigned_layer, max_layer);
                        if (nodes_pushing_down.ContainsKey(target))
                        {
                            nodes_pushing_down[target].Add(source);
                        }
                        else
                        {
                            nodes_pushing_down.Add(target, nodes_pushing_down[source].Append(source).ToList());
                        }
                        if (!new_look_at.Contains(target))
                        {
                            new_look_at.Add(target);
                        }
                    }
                }
                look_at = new_look_at;
            }

            return (layer_assignments, max_layer);
        }
        #endregion
        #region msagl
        private void MSAGL(List<NodeBase> all_nodes, List<LinkBase> all_links)
        {

            var graph = new GeometryGraph();
            foreach (var node in all_nodes)
            {
                var n = new MNode(CurveFactory.CreateRectangle(new Rectangle(node.X, node.Y, node.X + node.GetWidth(), node.Y + node.GetHeight())));
                graph.Nodes.Add(n);
            }
            foreach (var link in all_links)
            {
                if (link.Source.Node != null && link.Target.Node != null)
                {
                    var e = new Edge(graph.Nodes[all_nodes.IndexOf(link.Source.Node)], graph.Nodes[all_nodes.IndexOf(link.Target.Node)]);
                    graph.Edges.Add(e);
                }
            }
            LayoutAlgorithmSettings settings = Algorithm switch
            {
                Algorithm.Ranking => new Microsoft.Msagl.Prototype.Ranking.RankingLayoutSettings(),
                Algorithm.FastIncremental => new FastIncrementalLayoutSettings(),
                Algorithm.MultiDimensionalScaling => new MdsLayoutSettings(),
                Algorithm.Sugiyama => new SugiyamaLayoutSettings(),
                _ => new SugiyamaLayoutSettings()
            };
            settings.Reporting = false;
            LayoutHelpers.CalculateLayout(graph, settings, new CancelToken());
            foreach (var (node, gnode) in all_nodes.Zip(graph.Nodes, (a, b) => (a, b)))
            {
                node.MoveTo(gnode.Center.X - gnode.Width / 2 - graph.Left, gnode.Center.Y - gnode.Height / 2 - graph.Bottom);
            }
            if (PreservePorts)
                return;
            foreach (var (link, glink) in all_links.Zip(graph.Edges, (a, b) => (a, b)))
            {
                if (link.Source.Node != null && link.Target.Node != null)
                {
                    var raw_angle = Math.Atan2(glink.Target.Center.Y - glink.Source.Center.Y, glink.Target.Center.X - glink.Source.Center.X) * 180 / Math.PI + 450;
                    var iangle = (int)Math.Floor(raw_angle) % 360;
                    var angle = iangle / 45;
                    link.Source.Port = ToPort(angle);
                    link.Target.Port = ToPort((angle + 4) % 8);
                    link.TriggerStateHasChanged();
                }
            }
        }
        private static Position ToPort(int angle)
        {
            return angle switch
            {
                0 => Position.Top,
                1 => Position.TopRight,
                2 => Position.Right,
                3 => Position.BottomRight,
                4 => Position.Bottom,
                5 => Position.BottomLeft,
                6 => Position.Left,
                7 => Position.TopLeft,
                _ => Position.Top
            };
        }
        #endregion
    }
}