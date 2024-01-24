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
				Assert.That(1, Is.EqualTo(layer_1.Select(n => n.Y).Distinct().Count()));
				Assert.That(1, Is.EqualTo(layer_2.Select(n => n.Y).Distinct().Count()));
				Assert.That(1, Is.EqualTo(layer_3.Select(n => n.Y).Distinct().Count()));
				Assert.That(1, Is.EqualTo(layer_4.Select(n => n.Y).Distinct().Count()));
				Assert.That(1, Is.EqualTo(layer_5.Select(n => n.Y).Distinct().Count()));
				Assert.That(layer_1.Select(n => n.Y).First(), Is.LessThan(layer_2.Select(n => n.Y).First()));
				Assert.That(layer_2.Select(n => n.Y).First(), Is.LessThan(layer_3.Select(n => n.Y).First()));
				Assert.That(layer_3.Select(n => n.Y).First(), Is.LessThan(layer_4.Select(n => n.Y).First()));
				Assert.That(layer_4.Select(n => n.Y).First(), Is.LessThan(layer_5.Select(n => n.Y).First()));
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
				Assert.That(1, Is.EqualTo(layer_0.Select(n => n.Y).Distinct().Count()));
				Assert.That(1, Is.EqualTo(layer_1.Select(n => n.Y).Distinct().Count()));
				Assert.That(1, Is.EqualTo(layer_2.Select(n => n.Y).Distinct().Count()));
				Assert.That(layer_0.Select(n => n.Y).First(), Is.LessThan(layer_1.Select(n => n.Y).First()));
				Assert.That(layer_1.Select(n => n.Y).First(), Is.LessThan(layer_2.Select(n => n.Y).First()));
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
				Assert.That(1, Is.EqualTo(layer_0.Select(n => n.Y).Distinct().Count()));
				Assert.That(1, Is.EqualTo(layer_1.Select(n => n.Y).Distinct().Count()));
				Assert.That(1, Is.EqualTo(layer_2.Select(n => n.Y).Distinct().Count()));
				Assert.That(layer_0.Select(n => n.Y).First(), Is.LessThan(layer_1.Select(n => n.Y).First()));
				Assert.That(layer_1.Select(n => n.Y).First(), Is.LessThan(layer_2.Select(n => n.Y).First()));
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
				Assert.That(1, Is.EqualTo(layer_0.Select(n => n.Y).Distinct().Count()));
				Assert.That(1, Is.EqualTo(layer_1.Select(n => n.Y).Distinct().Count()));
				Assert.That(layer_0.Select(n => n.Y).First(), Is.LessThan(layer_1.Select(n => n.Y).First()));
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
					Assert.That(1, Is.EqualTo(layer.Select(n => n.Y).Distinct().Count()));
				}
				for (int li = 0; li + 1 < layers.Count; ++li)
				{
					Assert.That(layers[li].Select(n => n.Y).First(), Is.LessThan(layers[li + 1].Select(n => n.Y).First()));
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
				Assert.That(1, Is.EqualTo(layer_0.Select(n => n.Y).Distinct().Count()));
				Assert.That(1, Is.EqualTo(layer_1.Select(n => n.Y).Distinct().Count()));
				Assert.That(1, Is.EqualTo(layer_2.Select(n => n.Y).Distinct().Count()));
				Assert.That(1, Is.EqualTo(layer_3.Select(n => n.Y).Distinct().Count()));
				Assert.That(1, Is.EqualTo(layer_4.Select(n => n.Y).Distinct().Count()));
				Assert.That(layer_0.Select(n => n.Y).First(), Is.LessThan(layer_1.Select(n => n.Y).First()));
				Assert.That(layer_1.Select(n => n.Y).First(), Is.LessThan(layer_2.Select(n => n.Y).First()));
				Assert.That(layer_2.Select(n => n.Y).First(), Is.LessThan(layer_3.Select(n => n.Y).First()));
				Assert.That(layer_3.Select(n => n.Y).First(), Is.LessThan(layer_4.Select(n => n.Y).First()));
			}
		}
		[Test]
		public void AutoLayout_Real()
		{
			var diagram = new Diagram();
			var auto_layout = new AutoLayoutSettings();
			auto_layout.Algorithm = Algorithm.TreeVerticalTopDown;
			var rnd = new Random();
			for (int i = 0; i < 10; ++i)
			{
				var nodes = new List<NodeBase>
				{
					new TestNode(diagram, "01"),
					new TestNode(diagram, "02"),
					new TestNode(diagram, "03"),
					new TestNode(diagram, "04"),
					new TestNode(diagram, "05"),
					new TestNode(diagram, "06"),
					new TestNode(diagram, "07"),
					new TestNode(diagram, "11"),
					new TestNode(diagram, "12"),
					new TestNode(diagram, "21"),
					new TestNode(diagram, "22"),
				}
				.ToList();
				var links = new List<LinkBase>
				{
					new TestLink(nodes, source: "01", target: "11"),
					new TestLink(nodes, source: "02", target: "11"),
					new TestLink(nodes, source: "03", target: "11"),
					new TestLink(nodes, source: "04", target: "11"),
					new TestLink(nodes, source: "05", target: "11"),
					new TestLink(nodes, source: "05", target: "12"),
					new TestLink(nodes, source: "06", target: "12"),
					new TestLink(nodes, source: "07", target: "12"),
					new TestLink(nodes, source: "11", target: "21"),
					new TestLink(nodes, source: "12", target: "22"),
				}
				.ToList();
				;
				auto_layout.Layout(nodes, links);
				var layers = Enumerable.Empty<List<NodeBase>>()
					.Append(nodes.Where(n => n.Id.StartsWith("0")).ToList())
					.Append(nodes.Where(n => n.Id.StartsWith("1")).ToList())
					.Append(nodes.Where(n => n.Id.StartsWith("2")).ToList())
					.ToList();
				foreach (var layer in layers)
				{
					Assert.That(1, Is.EqualTo(layer.Select(n => n.Y).Distinct().Count()));
				}
				for (int li = 0; li + 1 < layers.Count; ++li)
				{
					Assert.That(layers[li].Select(n => n.Y).First(), Is.LessThan(layers[li + 1].Select(n => n.Y).First()));
				}
			}
		}
		[Test]
		public void AutoLayout_Real2()
		{
			var diagram = new Diagram();
			var auto_layout = new AutoLayoutSettings();
			auto_layout.Algorithm = Algorithm.TreeVerticalTopDown;
			var rnd = new Random();
			for (int i = 0; i < 10; ++i)
			{
				var nodes = new List<NodeBase>
				{
					new TestNode(diagram, "01"),
					new TestNode(diagram, "02"),
					new TestNode(diagram, "03"),
					new TestNode(diagram, "11"),
					new TestNode(diagram, "12"),
					new TestNode(diagram, "13"),
					new TestNode(diagram, "14"),
					new TestNode(diagram, "15"),
					new TestNode(diagram, "16"),
					new TestNode(diagram, "21"),
					new TestNode(diagram, "22"),
				}
				.ToList();
				var links = new List<LinkBase>
				{
					new TestLink(nodes, source: "01", target: "11"),
					new TestLink(nodes, source: "01", target: "12"),
					new TestLink(nodes, source: "01", target: "13"),
					new TestLink(nodes, source: "01", target: "22"),

					new TestLink(nodes, source: "02", target: "21"),
					new TestLink(nodes, source: "02", target: "14"),

					new TestLink(nodes, source: "03", target: "12"),
					new TestLink(nodes, source: "03", target: "15"),

					new TestLink(nodes, source: "11", target: "21"),
					new TestLink(nodes, source: "12", target: "21"),
					new TestLink(nodes, source: "13", target: "22"),
					new TestLink(nodes, source: "16", target: "22"),
				}
				.ToList();
				;
				auto_layout.Layout(nodes, links);
				var layers = Enumerable.Empty<List<NodeBase>>()
					.Append(nodes.Where(n => n.Id.StartsWith("0")).ToList())
					.Append(nodes.Where(n => n.Id.StartsWith("1")).ToList())
					.Append(nodes.Where(n => n.Id.StartsWith("2")).ToList())
					.ToList();
				foreach (var layer in layers)
				{
					Assert.That(1, Is.EqualTo(layer.Select(n => n.Y).Distinct().Count()));
				}
				for (int li = 0; li + 1 < layers.Count; ++li)
				{
					Assert.That(layers[li].Select(n => n.Y).First(), Is.LessThan(layers[li + 1].Select(n => n.Y).First()));
				}
			}
		}
		[Test]
		public void AutoLayout_Real3()
		{
			var diagram = new Diagram();
			var auto_layout = new AutoLayoutSettings();
			auto_layout.Algorithm = Algorithm.TreeVerticalTopDown;
			var rnd = new Random();
			for (int i = 0; i < 10; ++i)
			{
				var nodes = new List<NodeBase>
				{
					new TestNode(diagram, "01"),
					new TestNode(diagram, "02"),

					new TestNode(diagram, "03"),
					new TestNode(diagram, "04"),
					new TestNode(diagram, "05"),
					new TestNode(diagram, "06"),

					new TestNode(diagram, "11"),
					new TestNode(diagram, "12")
				}
				.ToList();
				var links = new List<LinkBase>
				{
					new TestLink(nodes, source: "01", target: "11"),
					new TestLink(nodes, source: "02", target: "11"),
					new TestLink(nodes, source: "02", target: "12"),
					new TestLink(nodes, source: "03", target: "12"),
					new TestLink(nodes, source: "04", target: "12"),
					new TestLink(nodes, source: "05", target: "12"),
					new TestLink(nodes, source: "06", target: "12"),
				}
				.ToList();
				;
				auto_layout.Layout(nodes, links);
				var layers = Enumerable.Empty<List<NodeBase>>()
					.Append(nodes.Where(n => n.Id.StartsWith("0")).ToList())
					.Append(nodes.Where(n => n.Id.StartsWith("1")).ToList())
					.ToList();
				foreach (var layer in layers)
				{
					Assert.That(1, Is.EqualTo(layer.Select(n => n.Y).Distinct().Count()));
				}
				for (int li = 0; li + 1 < layers.Count; ++li)
				{
					Assert.That(layers[li].Select(n => n.Y).First(), Is.LessThan(layers[li + 1].Select(n => n.Y).First()));
				}
				foreach (var layer in layers)
				{
					var positions = layer.Select(n => n.X).OrderBy(v => v).ToList();
					Assert.That(positions.SkipLast(1).Zip(positions.Skip(1), (first, second) => second - first).Any(v => v < 150), Is.False, "Some nodes were too close to each other");
				}
			}
		}
	}
}
