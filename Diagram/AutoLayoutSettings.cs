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
            if (Diagram != null && Diagram.Nodes != null && Diagram.Nodes.all_nodes.Any() && Diagram.Links != null)
            {
                Layout(Diagram.Nodes.all_nodes, Diagram.Links.all_links);
                Diagram.UpdateOverview();
            }
        }
        private void Layout(List<NodeBase> all_nodes, List<LinkBase> all_links)
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
            ArrangeNodes(layers);
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
        private void ArrangeNodes(List<List<NodeBase>> layers)
        {
            const double vertical_separation = 50;
            const double horizontal_separation = 50;
            if (Algorithm == Algorithm.TreeVerticalTopDown || Algorithm == Algorithm.TreeVerticalBottomUp)
            {
                ArrangeNodesInRows(layers, vertical_separation, horizontal_separation);
            }
            else
            {
                ArrangeNodesInColumns(layers, vertical_separation, horizontal_separation);
            }
        }
        private static void ArrangeNodesInColumns(List<List<NodeBase>> layers, double vertical_separation, double horizontal_separation)
        {
            double x = 0;
            var heights = layers.Select(layer =>
                layer.Select(n => n.Height + n.GetDrawingMargins().Top + n.GetDrawingMargins().Bottom).ToList())
                .ToList();
            var layer_heights = heights.Select(layer => horizontal_separation * (layer.Count - 1) + layer.Sum()).ToList();
            var highest_layer_height = layer_heights.Max();
            foreach (var (layer, height) in layers.Zip(layer_heights, (l, h) => (l, h)))
            {
                if (!layer.Any())
                {
                    continue;
                }
                double y = (highest_layer_height - height) / 2;
                foreach (var node in layer)
                {
                    node.MoveTo(x, y);
                    var margins = node.GetDrawingMargins();
                    y += node.Height + margins.Top + margins.Bottom + vertical_separation;
                }
                var maximum_width = layer.Max(n => n.Width + n.GetDrawingMargins().Left + n.GetDrawingMargins().Right);
                x += horizontal_separation + maximum_width;
            }
        }

        private static void ArrangeNodesInRows(List<List<NodeBase>> layers, double vertical_separation, double horizontal_separation)
        {
            double y = 0;
            var widths = layers.Select(layer =>
                layer.Select(n => n.Width + n.GetDrawingMargins().Left + n.GetDrawingMargins().Right).ToList())
                .ToList();
            var layer_widths = widths.Select(layer => horizontal_separation * (layer.Count - 1) + layer.Sum()).ToList();
            var widest_layer_width = layer_widths.Max();
            foreach (var (layer, width) in layers.Zip(layer_widths, (l, w) => (l, w)))
            {
                if (!layer.Any())
                {
                    continue;
                }
                double x = (widest_layer_width - width) / 2;
                foreach (var node in layer)
                {
                    node.MoveTo(x, y);
                    var margins = node.GetDrawingMargins();
                    x += node.Width + margins.Left + margins.Right + horizontal_separation;
                }
                var maximum_height = layer.Max(n => n.Height + n.GetDrawingMargins().Top + n.GetDrawingMargins().Bottom);
                y += vertical_separation + maximum_height;
            }
        }
        private static void ArrangeNodesWithinLayers(List<LinkBase> all_links, List<List<NodeBase>> layers)
        {
            // 2.1. sort out each layer by looking at where it connects from
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
            // 2.2. now that all but the first layer are layed out, let's deal with the first
            if (layers.Count > 1)
            {
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
            var (layer_assignments, max_layer) = PushNodesUp(roots, targets_by_source);
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

            return layers;
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
                        var known = layer_assignments.ContainsKey(source);
                        var assigned_layer = known
                            ? Math.Max(layer_assignments[source], layer_assignments[target] + 1)
                            : layer_assignments[target] + 1;
                        layer_assignments[source] = assigned_layer;
                        max_layer = Math.Max(assigned_layer, max_layer);
                        if (!known)
                        {
                            new_look_at.Add(source);
                        }
                        if (nodes_pushing_up.ContainsKey(source))
                        {
                            nodes_pushing_up[source].Add(target);
                        }
                        else
                        {
                            nodes_pushing_up.Add(source, nodes_pushing_up[target].Append(target).ToList());
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
                        var known = layer_assignments.ContainsKey(target);
                        var assigned_layer = known
                            ? Math.Max(layer_assignments[target], layer_assignments[source] + 1)
                            : layer_assignments[source] + 1;
                        layer_assignments[target] = assigned_layer;
                        max_layer = Math.Max(assigned_layer, max_layer);
                        if (!known)
                        {
                            new_look_at.Add(target);
                        }
                        if (nodes_pushing_down.ContainsKey(target))
                        {
                            nodes_pushing_down[target].Add(source);
                        }
                        else
                        {
                            nodes_pushing_down.Add(target, nodes_pushing_down[source].Append(source).ToList());
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
                var n = new MNode(CurveFactory.CreateRectangle(new Rectangle(node.X, node.Y, node.X + node.Width, node.Y + node.Height)));
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
