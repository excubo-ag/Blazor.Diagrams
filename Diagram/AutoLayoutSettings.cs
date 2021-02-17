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
        public void Run()
        {
            if (Diagram != null && Diagram.Nodes != null && Diagram.Nodes.all_nodes.Any() && Diagram.Nodes.all_nodes.All(n => n.HasSize) && Diagram.Links != null)
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
            // 1. Establish hierarchy.
            var layers = (Algorithm == Algorithm.TreeHorizontal || Algorithm == Algorithm.TreeVertical) ? GetLayersTopDown(all_nodes, all_links) : GetLayersBottomUp(all_nodes, all_links);
            // 2. Now that everything is in layers, we should arrange the items per layer such that there is minimal crossing of links.
            ArrangeNodesWithinLayers(all_links, layers);
            // 3. Layout time!
            ArrangeNodes(all_links, layers);
            // 4. fix link positions. This is easy, because we work top down, except when we have upwards arrows (cycles) or ltr/rtl arrows.
            ArrangeLinks(all_links, layers);
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
        private void ArrangeNodes(List<LinkBase> all_links, List<List<NodeBase>> layers)
        {
            const double vertical_separation = 50;
            const double horizontal_separation = 50;
            if (Algorithm == Algorithm.TreeVerticalTopDown || Algorithm == Algorithm.TreeVerticalBottomUp)
            {
                Func<NodeBase, double> left_or_top_margin = (node) => node.GetDrawingMargins().Top;
                Func<NodeBase, double> top_or_left_margin = (node) => node.GetDrawingMargins().Left;
                Func<NodeBase, double> position = (node) => node.X;
                Func<NodeBase, double> space = (node) => node.GetWidth();
                Func<NodeBase, double> bottom_or_right_margin = (node) => node.GetDrawingMargins().Right;
                Func<NodeBase, double> orthogonal_space_with_margins = (node) => node.GetHeight() + node.GetDrawingMargins().Top + node.GetDrawingMargins().Bottom;
                Func<double, Action<NodeBase, double, double>> move_generator = (y) => (node, x, y_offset) => node.MoveTo(x, y + y_offset);
                ArrangeNodes(all_links, layers, vertical_separation, horizontal_separation,
                    move_generator, orthogonal_space_with_margins, left_or_top_margin, position,
                    top_or_left_margin, space, bottom_or_right_margin);
            }
            else
            {
                Func<NodeBase, double> left_or_top_margin = (node) => node.GetDrawingMargins().Left;
                Func<NodeBase, double> top_or_left_margin = (node) => node.GetDrawingMargins().Top;
                Func<NodeBase, double> position = (node) => node.Y;
                Func<NodeBase, double> space = (node) => node.GetHeight();
                Func<NodeBase, double> bottom_or_right_margin = (node) => node.GetDrawingMargins().Bottom;
                Func<NodeBase, double> orthogonal_space_with_margins = (node) => node.GetWidth() + node.GetDrawingMargins().Left + node.GetDrawingMargins().Right;
                Func<double, Action<NodeBase, double, double>> move_generator = (x) => (node, y, x_offset) => node.MoveTo(x + x_offset, y);
                ArrangeNodes(all_links, layers, horizontal_separation, vertical_separation,
                    move_generator, orthogonal_space_with_margins, left_or_top_margin, position,
                    top_or_left_margin, space, bottom_or_right_margin);
            }
        }
        private static void ArrangeNodes(
            List<LinkBase> all_links, 
            List<List<NodeBase>> layers, 
            double layer_separation, 
            double in_layer_separation,
            Func<double, Action<NodeBase, double, double>> move_generator,
            Func<NodeBase, double> orthogonal_space_with_margins,
            Func<NodeBase, double> left_or_top_margin,
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
                var y = layer_heights.Take(widest_layer_index).Sum() + layer_separation * widest_layer_index;
                var move = move_generator(y);
                foreach (var node in widest_layer)
                {
                    move(node, x + top_or_left_margin(node), left_or_top_margin(node));
                    x += space(node) + top_or_left_margin(node) + bottom_or_right_margin(node) + in_layer_separation;
                }
            }
            OptimizeBelowWidestLayer(all_links, layers, layer_separation, in_layer_separation, layer_heights, widest_layer_index, move_generator, left_or_top_margin, position, top_or_left_margin, space, bottom_or_right_margin);
            OptimizeAboveWidestLayer(all_links, layers, layer_separation, in_layer_separation, layer_heights, widest_layer_index, move_generator, left_or_top_margin, position, top_or_left_margin, space, bottom_or_right_margin);
            if (widest_layer_index > 0)
            {
                // optimize the widest layer by the layers above
                var active_layer = layers[widest_layer_index];
                var wish_positions = active_layer.Zip(active_layer.Select(n =>
                {
                    var relevant_links = all_links.Where(l => l.Source.Node != null && l.Target.Node == n).ToList();
                    return relevant_links.Any() ? relevant_links.Average(l => position(l.Source.Node) + space(l.Source.Node) / 2) : double.MaxValue;
                }), (Node, Position) => (Node, Position)).OrderBy(e => e.Position).ToList();

                wish_positions = EnsureSeparation(in_layer_separation, wish_positions, top_or_left_margin, space, bottom_or_right_margin);

                var y = layer_heights.Take(widest_layer_index).Sum() + layer_separation * widest_layer_index;
                var move = move_generator(y);
                PlaceNodesInLayer(in_layer_separation, wish_positions, move, left_or_top_margin, position, top_or_left_margin, space, bottom_or_right_margin);
            }
            else
            {
                // optimize the widest layer by the layers below
                var active_layer = layers[widest_layer_index];
                var wish_positions = active_layer.Zip(active_layer.Select(n =>
                {
                    var relevant_links = all_links.Where(l => l.Source.Node == n && l.Target.Node != null).ToList();
                    return relevant_links.Any() ? relevant_links.Average(l => position(l.Target.Node) + space(l.Target.Node) / 2) : double.MaxValue;
                }), (Node, Position) => (Node, Position)).OrderBy(e => e.Position).ToList();

                wish_positions = EnsureSeparation(in_layer_separation, wish_positions, top_or_left_margin, space, bottom_or_right_margin);

                var y = layer_heights.Take(widest_layer_index).Sum() + layer_separation * widest_layer_index;
                var move = move_generator(y);
                PlaceNodesInLayer(in_layer_separation, wish_positions, move, left_or_top_margin, position, top_or_left_margin, space, bottom_or_right_margin);
            }
            OptimizeBelowWidestLayer(all_links, layers, layer_separation, in_layer_separation, layer_heights, widest_layer_index, move_generator, left_or_top_margin, position, top_or_left_margin, space, bottom_or_right_margin);
            OptimizeAboveWidestLayer(all_links, layers, layer_separation, in_layer_separation, layer_heights, widest_layer_index, move_generator, left_or_top_margin, position, top_or_left_margin, space, bottom_or_right_margin);

        }

        private static void OptimizeAboveWidestLayer(List<LinkBase> all_links, List<List<NodeBase>> layers, double layer_separation, double in_layer_separation, List<double> layer_heights, int widest_layer_index,
            Func<double, Action<NodeBase, double, double>> move_generator,
            Func<NodeBase, double> left_or_top_margin,
            Func<NodeBase, double> position,
            Func<NodeBase, double> top_or_left_margin,
            Func<NodeBase, double> space,
            Func<NodeBase, double> bottom_or_right_margin)
        {
            for (int i = widest_layer_index; i > 0; --i)
            {
                // going through the layers _above_ the widest layer
                var active_layer = layers[i - 1];
                var wish_positions = active_layer.Zip(active_layer.Select(n =>
                {
                    var relevant_links = all_links.Where(l => l.Source.Node == n && l.Target.Node != null).ToList();
                    return relevant_links.Any() ? relevant_links.Average(l => position(l.Target.Node) + space(l.Target.Node) / 2) : double.MaxValue;
                }), (Node, Position) => (Node, Position)).OrderBy(e => e.Position).ToList();

                wish_positions = EnsureSeparation(in_layer_separation, wish_positions, top_or_left_margin, space, bottom_or_right_margin);

                var y = layer_heights.Take(i - 1).Sum() + layer_separation * (i - 1);
                var move = move_generator(y);
                PlaceNodesInLayer(in_layer_separation, wish_positions, move, left_or_top_margin, position, top_or_left_margin, space, bottom_or_right_margin);
            }
        }

        private static void OptimizeBelowWidestLayer(List<LinkBase> all_links, List<List<NodeBase>> layers, double layer_separation, double in_layer_separation, List<double> layer_heights, int widest_layer_index,
            Func<double, Action<NodeBase, double, double>> move_generator,
            Func<NodeBase, double> left_or_top_margin,
            Func<NodeBase, double> position,
            Func<NodeBase, double> top_or_left_margin,
            Func<NodeBase, double> space,
            Func<NodeBase, double> bottom_or_right_margin)
        {
            for (int i = widest_layer_index; i + 1 < layers.Count; ++i)
            {
                // going through the layers _below_ the widest layer
                var active_layer = layers[i + 1];
                var wish_positions = active_layer.Zip(active_layer.Select(n =>
                {
                    var relevant_links = all_links.Where(l => l.Source.Node != null && l.Target.Node == n).ToList();
                    return relevant_links.Any() ? relevant_links.Average(l => position(l.Source.Node) + space(l.Source.Node) / 2) : double.MaxValue;
                }), (Node, Position) => (Node, Position)).OrderBy(e => e.Position).ToList();

                wish_positions = EnsureSeparation(in_layer_separation, wish_positions, top_or_left_margin, space, bottom_or_right_margin);

                var y = layer_heights.Take(i + 1).Sum() + layer_separation * (i + 1);
                var move = move_generator(y);
                PlaceNodesInLayer(in_layer_separation, wish_positions, move, left_or_top_margin, position, top_or_left_margin, space, bottom_or_right_margin);
            }
        }
        private static List<(NodeBase Node, double Position)> EnsureSeparation(
            double in_layer_separation, 
            List<(NodeBase Node, double Position)> wish_positions,
            Func<NodeBase, double> top_or_left_margin,
            Func<NodeBase, double> space,
            Func<NodeBase, double> bottom_or_right_margin)
        {
            var ignored = wish_positions.Where(e => e.Position == double.MaxValue).ToList();
            var to_adjust = wish_positions.Where(e => e.Position != double.MaxValue).OrderBy(e => e.Position).ToList();
            // the adjustment of positions is done from "left to right", separating two nodes by moving the left more to the left and the right more to the right.
            for (var i = 0; i + 1 < to_adjust.Count; ++i)
            {
                var (left_node, left_position) = to_adjust[i];
                var (right_node, right_position) = to_adjust[i + 1];
                var left_side_of_right = right_position - space(right_node) / 2 - top_or_left_margin(right_node);
                var right_side_of_left = left_position + space(left_node) / 2 + bottom_or_right_margin(left_node);
                var current_separation = left_side_of_right - right_side_of_left;
                if (in_layer_separation - current_separation > 1e-6)
                {
                    var difference = Math.Abs(in_layer_separation - current_separation);
                    left_position -= difference / 2;
                    right_position += difference / 2;
                    to_adjust[i] = (left_node, left_position);
                    to_adjust[i + 1] = (right_node, right_position);
                    i = -1;
                    to_adjust = to_adjust.OrderBy(e => e.Position).ToList();
                }
            }
            return to_adjust.Concat(ignored).ToList();
        }

        private static void PlaceNodesInLayer(
            double in_layer_separation, 
            List<(NodeBase Node, double Position)> nodes_and_positions, 
            Action<NodeBase, double, double> move,
            Func<NodeBase, double> left_or_top_margin,
            Func<NodeBase, double> position,
            Func<NodeBase, double> top_or_left_margin,
            Func<NodeBase, double> space,
            Func<NodeBase, double> bottom_or_right_margin)
        {
            foreach (var (node, in_layer_position) in nodes_and_positions.Where(kv => kv.Position != double.MaxValue))
            {
                move(node, in_layer_position - space(node) / 2, left_or_top_margin(node));
            }
            var right_most = nodes_and_positions.Where(kv => kv.Position != double.MaxValue).Select(kv => kv.Node).LastOrDefault();
            foreach (var (node, _) in nodes_and_positions.Where(kv => kv.Position == double.MaxValue))
            {
                if (right_most == null)
                {
                    move(node, 0, left_or_top_margin(node));
                }
                else
                {
                    move(node, position(right_most) + space(right_most) + bottom_or_right_margin(right_most) + in_layer_separation + top_or_left_margin(node), left_or_top_margin(node));
                }
                right_most = node;
            }
        }

        private static void ArrangeNodesWithinLayers(List<LinkBase> all_links, List<List<NodeBase>> layers)
        {
            // 2.1. sort out second and first layer
            if (layers.Count > 1)
            {
                layers[1] = layers[1].OrderBy(node =>
                {
                    // calculate average position based on connected nodes in top layer
                    var connected_nodes = all_links.Where(l => l.Target?.Node == node && layers[0].Contains(l.Source?.Node)).Select(l => l.Source?.Node).ToList();
                    if (!connected_nodes.Any())
                    {
                        return 0;
                    }
                    var average_index = connected_nodes.Select(cn => { var i = layers[0].IndexOf(cn); return i; }).Average();
                    return average_index;
                }).ToList();
                layers[0] = layers[0].OrderBy(node =>
                {
                    // calculate average position based on connected nodes in top layer
                    var connected_nodes = all_links.Where(l => l.Source?.Node == node && layers[1].Contains(l.Target?.Node)).Select(l => l.Target?.Node).ToList();
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
                    // calculate average position based on connected nodes in top layer
                    var connected_nodes = all_links.Where(l => l.Target?.Node == node && top_layer.Contains(l.Source?.Node)).Select(l => l.Source?.Node).ToList();
                    if (!connected_nodes.Any())
                    {
                        return 0;
                    }
                    var average_index = connected_nodes.Select(cn => { var i = top_layer.IndexOf(cn); return i; }).Average();
                    return average_index;
                }).ToList();
            }
        }

        private static List<List<NodeBase>> GetLayersTopDown(List<NodeBase> all_nodes, List<LinkBase> all_links)
        {
            // 1.0 if we don't have nodes, we have no layers
            if (!all_nodes.Any())
            {
                return new List<List<NodeBase>>();
            }
            // 1.1. We identify the nodes that have no incoming link
            var roots = all_nodes.Where(n => !all_links.Any(l => l.Target?.Node == n)).ToList();
            // 1.2. If we do not have any such node, we exclusively have cycles. We now randomly pick a root.
            // That random choice is the first node. Chosen by a fair die, as legend has it.
            roots = roots.Any() ? roots : all_nodes.Take(1).ToList();

            var targets_by_source = all_links
                .Where(l => l.Source?.Node != null && l.Target?.Node != null)
                .Select(l => (Source: l.Source.Node, Target: l.Target.Node))
                .GroupBy(e => e.Source, e => e.Target)
                .ToNullAllowingDictionary();
            var (layer_assignments, max_layer) = PushNodesDown(roots, targets_by_source);
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
            var rest_in_layers = GetLayersTopDown(all_nodes.Except(layers.SelectMany(n => n)).ToList(), all_links);
            layers.Merge(rest_in_layers);

            OptimizeNodePositions(all_nodes, all_links, layers);

            return layers;
        }

        private static void OptimizeNodePositions(List<NodeBase> all_nodes, List<LinkBase> all_links, List<List<NodeBase>> layers)
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
                foreach (var node in all_nodes.Where(nd => !all_links.Any(x => x.Target.Node == nd))) // look at all nodes without incoming links 
                {
                    var nodes_to_check = all_links
                        .Where(x => x.Source.Node == node) // get links leaving node 
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
                foreach (var node in all_nodes.Where(nd => !all_links.Any(x => x.Source.Node == nd))) // look at all nodes without outgoing links 
                {
                    var nodes_to_check = all_links
                        .Where(x => x.Target.Node == node) // get links targeting node
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

        private static List<List<NodeBase>> GetLayersBottomUp(List<NodeBase> all_nodes, List<LinkBase> all_links)
        {
            // 1.0 if we don't have nodes, we have no layers
            if (!all_nodes.Any())
            {
                return new List<List<NodeBase>>();
            }
            // 1.1. We identify the nodes that have no outgoing link
            var leaves = all_nodes.Where(n => !all_links.Any(l => l.Source?.Node == n)).ToList();
            // 1.2. If we do not have any such node, we exclusively have cycles. We now randomly pick a leaf.
            // That random choice is the first node. Chosen by a fair die, as legend has it.
            leaves = leaves.Any() ? leaves : all_nodes.Take(1).ToList();

            var sources_by_target = all_links
                .Where(l => l.Source?.Node != null && l.Target?.Node != null)
                .Select(l => (Source: l.Source.Node, Target: l.Target.Node))
                .GroupBy(e => e.Target, e => e.Source)
                .ToNullAllowingDictionary();
            var (layer_assignments, max_layer) = PushNodesUp(leaves, sources_by_target);
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
            var rest_in_layers = GetLayersBottomUp(all_nodes.Except(layers.SelectMany(n => n)).ToList(), all_links);
            layers.Merge(rest_in_layers);

            return layers;
        }

        private static (Dictionary<NodeBase, int> LayerAssignments, int HighestLayer) PushNodesUp(List<NodeBase> leaves, NullAllowingDictionary<NodeBase, List<NodeBase>> sources_by_target)
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
                    if (!sources_by_target.ContainsKey(target))
                    {
                        continue;
                    }
                    var sources = sources_by_target[target];
                    foreach (var source in sources)
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
        private static (Dictionary<NodeBase, int> LayerAssignments, int HighestLayer) PushNodesDown(List<NodeBase> roots, NullAllowingDictionary<NodeBase, List<NodeBase>> targets_by_source)
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
                    if (!targets_by_source.ContainsKey(source))
                    {
                        continue;
                    }
                    var targets = targets_by_source[source];
                    foreach (var target in targets)
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