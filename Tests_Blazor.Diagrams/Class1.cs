using Excubo.Blazor.Diagrams;
using Microsoft.AspNetCore.Components;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests_Blazor.Diagrams
{
    public class TestNode : NodeBase
    {
        public TestNode(Diagram diagram, string id)
        {
            Id = id;
        }
        public override RenderFragment border { get; }
        public override double Width { get => 100; set { } }
        public override double Height { get => 100; set { } }
        internal override bool HasSize { get; }

        protected override RenderFragment<string> GetChildContentWrapper()
        {
            throw new NotImplementedException();
        }

        internal override double GetHeight()
        {
            return 100;
        }

        internal override double GetWidth()
        {
            return 100;
        }
        internal override void MoveTo(double x, double y)
        {
            X = x;
            Y = y;
        }
        internal override void MoveToWithoutUIUpdate(double x, double y) => MoveTo(x, y);
        internal override void ApplyMoveTo()
        {
        }
    }
    public class TestLink : LinkBase
    {

        public TestLink(List<NodeBase> nodes, string source, string target)
        {
            Source = new NodeAnchor { Node = nodes.FirstOrDefault(n => n.Id == source) };
            Target = new NodeAnchor { Node = nodes.FirstOrDefault(n => n.Id == target) };
        }
    }
    public class Class1
    {
        [Test]
        public void AutoLayout_TreeVerticalTopDown()
        {
            var diagram = new Diagram();
            var auto_layout = new AutoLayoutSettings();
            auto_layout.Algorithm = Algorithm.TreeVerticalTopDown;
            var rnd = new Random();
            for (int i = 0; i < 10; ++i)
            {
                var nodes = new List<NodeBase>
                {
                    new TestNode(diagram, "L1_N0"),
                    new TestNode(diagram, "L2_N0"), new TestNode(diagram, "L2_N1"), new TestNode(diagram, "L2_N2"),
                    new TestNode(diagram, "L3_N0"), new TestNode(diagram, "L3_N1"),new TestNode(diagram, "L3_N2"),
                        new TestNode(diagram, "L4_N0"), new TestNode(diagram, "L4_N1"), new TestNode(diagram, "L4_N2"),
                    new TestNode(diagram, "L5_N0"),
                }
                /*
L1_N0,L2_N1,L2_N2
L2_N0
L3_N0,L3_N1,L3_N2
L4_N0,L4_N1,L4_N2
L5_N0
                 */
                .OrderBy(_ => rnd.NextDouble())
                .ToList();
                var links = new List<LinkBase>
                {
                    new TestLink(nodes, source: "L1_N0", target: "L2_N0"),
                    new TestLink(nodes, source: "L2_N0", target: "L3_N0"),
                    new TestLink(nodes, source: "L2_N0", target: "L3_N1"),
                    new TestLink(nodes, source: "L2_N0", target: "L3_N2"),
                    new TestLink(nodes, source: "L2_N1", target: "L3_N1"),
                    new TestLink(nodes, source: "L2_N2", target: "L3_N2"),
                    new TestLink(nodes, source: "L3_N0", target: "L4_N0"),
                    new TestLink(nodes, source: "L3_N1", target: "L4_N1"),
                    new TestLink(nodes, source: "L3_N2", target: "L4_N2"),
                    new TestLink(nodes, source: "L4_N1", target: "L5_N0"),
                }
                .OrderBy(_ => rnd.NextDouble())
                .ToList();
                ;
                auto_layout.Layout(nodes, links);
                var layering = string.Join("\n", nodes.GroupBy(n => n.Y).OrderBy(n => n.Key).Select(group => $"{group.Key}: {string.Join(",", group.Select(n => n.Id))}"));
                var layer_1 = nodes.Where(n => n.Id is "L1_N0").ToList();
                var layer_2 = nodes.Where(n => n.Id is "L2_N0" or "L2_N1" or "L2_N2").ToList();
                var layer_3 = nodes.Where(n => n.Id is "L3_N0" or "L3_N1" or "L3_N2").ToList();
                var layer_4 = nodes.Where(n => n.Id is "L4_N0" or "L4_N1" or "L4_N2" or "L4_N3").ToList();
                var layer_5 = nodes.Where(n => n.Id is "L5_N0").ToList();
                Assert.AreEqual(1, layer_1.Select(n => n.Y).Distinct().Count());
                Assert.AreEqual(1, layer_2.Select(n => n.Y).Distinct().Count());
                Assert.AreEqual(1, layer_3.Select(n => n.Y).Distinct().Count());
                Assert.AreEqual(1, layer_4.Select(n => n.Y).Distinct().Count(), $"{i}");
                Assert.AreEqual(1, layer_5.Select(n => n.Y).Distinct().Count());
                Assert.Less(layer_1.Select(n => n.Y).First(), layer_2.Select(n => n.Y).First());
                Assert.Less(layer_2.Select(n => n.Y).First(), layer_3.Select(n => n.Y).First());
                Assert.Less(layer_3.Select(n => n.Y).First(), layer_4.Select(n => n.Y).First());
                Assert.Less(layer_4.Select(n => n.Y).First(), layer_5.Select(n => n.Y).First());
            }
        }
        [Test]
        public void AutoLayout_TreeVerticalTopDown_1()
        {
            var diagram = new Diagram();
            var auto_layout = new AutoLayoutSettings();
            auto_layout.Algorithm = Algorithm.TreeVerticalTopDown;
            var rnd = new Random();
            for (int i = 0; i < 10; ++i)
            {
                var nodes = new List<NodeBase>
                {
                    new TestNode(diagram, "1"),
                    new TestNode(diagram, "2"),
                    new TestNode(diagram, "3"),
                }
                .ToList();
                var links = new List<LinkBase>
                {
                    new TestLink(nodes, source: "1", target: "2"),
                    new TestLink(nodes, source: "2", target: "3"),
                }
                .OrderBy(_ => rnd.NextDouble())
                .ToList();
                ;
                auto_layout.Layout(nodes, links);
                var layer_0 = nodes.Where(n => n.Id is "1").ToList();
                var layer_1 = nodes.Where(n => n.Id is "2").ToList();
                var layer_2 = nodes.Where(n => n.Id is "3").ToList();
                Assert.AreEqual(1, layer_0.Select(n => n.Y).Distinct().Count());
                Assert.AreEqual(1, layer_1.Select(n => n.Y).Distinct().Count());
                Assert.AreEqual(1, layer_2.Select(n => n.Y).Distinct().Count());
                Assert.Less(layer_0.Select(n => n.Y).First(), layer_1.Select(n => n.Y).First());
                Assert.Less(layer_1.Select(n => n.Y).First(), layer_2.Select(n => n.Y).First());
            }
        }
        [Test]
        public void AutoLayout_TreeVerticalTopDown_Cycle()
        {
            var diagram = new Diagram();
            var auto_layout = new AutoLayoutSettings();
            auto_layout.Algorithm = Algorithm.TreeVerticalTopDown;
            var rnd = new Random();
            for (int i = 0; i < 10; ++i)
            {
                var nodes = new List<NodeBase>
                {
                    new TestNode(diagram, "1"),
                    new TestNode(diagram, "2"),
                    new TestNode(diagram, "3"),
                }
                .ToList();
                var links = new List<LinkBase>
                {
                    new TestLink(nodes, source: "1", target: "2"),
                    new TestLink(nodes, source: "2", target: "3"),
                    new TestLink(nodes, source: "3", target: "1"),
                }
                .ToList();
                ;
                auto_layout.Layout(nodes, links);
                var layer_0 = nodes.Where(n => n.Id is "3").ToList();
                var layer_1 = nodes.Where(n => n.Id is "1").ToList();
                var layer_2 = nodes.Where(n => n.Id is "2").ToList();
                Assert.AreEqual(1, layer_0.Select(n => n.Y).Distinct().Count());
                Assert.AreEqual(1, layer_1.Select(n => n.Y).Distinct().Count());
                Assert.AreEqual(1, layer_2.Select(n => n.Y).Distinct().Count());
                Assert.Less(layer_0.Select(n => n.Y).First(), layer_1.Select(n => n.Y).First());
                Assert.Less(layer_1.Select(n => n.Y).First(), layer_2.Select(n => n.Y).First());
            }
        }
        [Test]
        public void AutoLayout_TreeVerticalTopDown_2()
        {
            var diagram = new Diagram();
            var auto_layout = new AutoLayoutSettings();
            auto_layout.Algorithm = Algorithm.TreeVerticalTopDown;
            var rnd = new Random();
            for (int i = 0; i < 10; ++i)
            {
                var nodes = new List<NodeBase>
                {
                    new TestNode(diagram, "1"),
                    new TestNode(diagram, "2"),
                    new TestNode(diagram, "3"),
                }
                .ToList();
                var links = new List<LinkBase>
                {
                    new TestLink(nodes, source: "1", target: "3"),
                    new TestLink(nodes, source: "2", target: "3"),
                }
                .OrderBy(_ => rnd.NextDouble())
                .ToList();
                ;
                auto_layout.Layout(nodes, links);
                var layer_0 = nodes.Where(n => n.Id is "1" or "2").ToList();
                var layer_1 = nodes.Where(n => n.Id is "3").ToList();
                Assert.AreEqual(1, layer_0.Select(n => n.Y).Distinct().Count());
                Assert.AreEqual(1, layer_1.Select(n => n.Y).Distinct().Count());
                Assert.Less(layer_0.Select(n => n.Y).First(), layer_1.Select(n => n.Y).First());
            }
        }
        [Test]
        public void AutoLayout_TreeVerticalTopDown_3()
        {
            var diagram = new Diagram();
            var auto_layout = new AutoLayoutSettings();
            auto_layout.Algorithm = Algorithm.TreeVerticalTopDown;
            var rnd = new Random();
            for (int i = 0; i < 10; ++i)
            {
                var nodes = new List<NodeBase>
                {
                    new TestNode(diagram, "1"),
                    new TestNode(diagram, "2"),
                    new TestNode(diagram, "3"),
                    new TestNode(diagram, "4"),
                }
                .ToList();
                var links = new List<LinkBase>
                {
                    new TestLink(nodes, source: "1", target: "2"),
                    new TestLink(nodes, source: "3", target: "4"),
                }
                .ToList();
                ;
                auto_layout.Layout(nodes, links);
                var layers = Enumerable.Empty<List<NodeBase>>()
                    .Append(nodes.Where(n => n.Id is "3").ToList())
                    .Append(nodes.Where(n => n.Id is "4").ToList())
                    .Append(nodes.Where(n => n.Id is "1").ToList())
                    .Append(nodes.Where(n => n.Id is "2").ToList())
                    .ToList();
                foreach (var layer in layers)
                {
                    Assert.AreEqual(1, layer.Select(n => n.Y).Distinct().Count());
                }
                for (int li = 0; li + 1 < layers.Count; ++li)
                {
                    Assert.Less(layers[li].Select(n => n.Y).First(), layers[li + 1].Select(n => n.Y).First());
                }
            }
        }
        [Test]
        public void AutoLayout_TreeVerticalTopDown_4()
        {
            var diagram = new Diagram();
            var auto_layout = new AutoLayoutSettings();
            auto_layout.Algorithm = Algorithm.TreeVerticalTopDown;
            var rnd = new Random();
            for (int i = 0; i < 10; ++i)
            {
                var nodes = new List<NodeBase>
                {
                    new TestNode(diagram, "1"),
                    new TestNode(diagram, "2"),
                    new TestNode(diagram, "3"),
                    new TestNode(diagram, "4"),
                    new TestNode(diagram, "5"),
                    new TestNode(diagram, "6"),
                }
                .ToList();
                var links = new List<LinkBase>
                {
                    new TestLink(nodes, source: "1", target: "2"),
                    new TestLink(nodes, source: "2", target: "3"),
                    new TestLink(nodes, source: "4", target: "5"),
                    new TestLink(nodes, source: "5", target: "6"),
                    new TestLink(nodes, source: "3", target: "5"),
                }
                .OrderBy(_ => rnd.NextDouble())
                .ToList();
                ;
                auto_layout.Layout(nodes, links);
                var layer_0 = nodes.Where(n => n.Id is "1").ToList();
                var layer_1 = nodes.Where(n => n.Id is "2").ToList();
                var layer_2 = nodes.Where(n => n.Id is "3" or "4").ToList();
                var layer_3 = nodes.Where(n => n.Id is "5").ToList();
                var layer_4 = nodes.Where(n => n.Id is "6").ToList();
                Assert.AreEqual(1, layer_0.Select(n => n.Y).Distinct().Count());
                Assert.AreEqual(1, layer_1.Select(n => n.Y).Distinct().Count());
                Assert.AreEqual(1, layer_2.Select(n => n.Y).Distinct().Count());
                Assert.AreEqual(1, layer_3.Select(n => n.Y).Distinct().Count());
                Assert.AreEqual(1, layer_4.Select(n => n.Y).Distinct().Count());
                Assert.Less(layer_0.Select(n => n.Y).First(), layer_1.Select(n => n.Y).First());
                Assert.Less(layer_1.Select(n => n.Y).First(), layer_2.Select(n => n.Y).First());
                Assert.Less(layer_2.Select(n => n.Y).First(), layer_3.Select(n => n.Y).First());
                Assert.Less(layer_3.Select(n => n.Y).First(), layer_4.Select(n => n.Y).First());
            }
        }
    }
}
