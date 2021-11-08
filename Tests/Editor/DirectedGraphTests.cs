using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Bhik95.Graph.Tests
{
    public class DirectedGraphTests
    {

        private DirectedGraph<string, float> _graph;

        [SetUp]
        public void Setup()
        {
            _graph = new DirectedGraph<string, float>();
            _graph.AddEdge("1", "7", 0.5f);
            _graph.AddEdge("7", "3", 3.0f);
            _graph.AddEdge("1", "3", 100.0f);
            _graph.AddEdge("3", "1", 2.0f);
            _graph.AddEdge("3", "2", 4.0f);
            _graph.AddEdge("2", "4", 1.0f);
            _graph.AddEdge("3", "4", 0.5f);
            _graph.AddEdge("5", "6", 10f);
            _graph.AddEdge("6", "5", 0.1f);
        }
        
        [TearDown]
        public void Teardown()
        {
            _graph = null;
        }
        
        [Test]
        public void TestNodeCount()
        {
            Assert.AreEqual(7, _graph.NodeCount);
        }
        
        [Test]
        public void TestEdgeCount()
        {
            Assert.AreEqual(9, _graph.EdgeCount);
        }

        [Test]
        public void TestContainsNode()
        {
            Assert.IsTrue(_graph.ContainsNode("1"));
            Assert.IsTrue(_graph.ContainsNode("2"));
            Assert.IsTrue(_graph.ContainsNode("3"));
            Assert.IsTrue(_graph.ContainsNode("4"));
            Assert.IsTrue(_graph.ContainsNode("5"));
            Assert.IsTrue(_graph.ContainsNode("6"));
            Assert.IsTrue(_graph.ContainsNode("7"));
            
            Assert.IsFalse(_graph.ContainsNode("not in graph"));
        }
        
        [Test]
        public void TestContainsNodeExceptions()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                _graph.ContainsNode(null);
            });
        }
        
        [Test]
        public void TestContainsAddedNode()
        {
            _graph.AddNode("new node");
            Assert.IsTrue(_graph.ContainsNode("new node"));
        }
        
        [Test]
        public void TestContainsNodeAddedFromEdge()
        {
            _graph.AddEdge("new node 1", "new node 2", 1);
            Assert.IsTrue(_graph.ContainsNode("new node 1"));
            Assert.IsTrue(_graph.ContainsNode("new node 2"));
        }

        [Test]
        public void TestContainsEdge()
        {
            Assert.IsTrue(_graph.ContainsEdge("1", "7"));
                
            Assert.IsFalse(_graph.ContainsEdge("1", "6"));
        }
        
        [Test]
        public void TestAddNodeExceptions()
        {
            // Add null node
            Assert.Throws<ArgumentNullException>(() =>
            {
                _graph.AddNode(null);
            });
            
            // Add duplicate node
            Assert.Throws<ArgumentException>(() =>
            {
                _graph.AddNode("1"); // 1 should already be in the graph
            });
        }

        [Test]
        public void TestAddEdgeExceptions()
        {
            // Source null check
            Assert.Throws<ArgumentNullException>(() =>
            {
                _graph.AddEdge(null, "1", 1);
            });
            
            // Destination null check
            Assert.Throws<ArgumentNullException>(() =>
            {
                _graph.AddEdge("1", null, 1);
            });
            
            // Edge data null check
            DirectedGraph<string, string> graphWithNullableTypes = new DirectedGraph<string, string>();
            Assert.Throws<ArgumentNullException>(() =>
            {
                graphWithNullableTypes.AddEdge("4", "1", null);
            });
            
            // Source and destination should not be the same
            Assert.Throws<ArgumentException>(() =>
            {
                _graph.AddEdge("1", "1", 1);
            });
            
            // Add duplicate check
            Assert.Throws<ArgumentException>(() =>
            {
                _graph.AddEdge("1", "7", 1);
            });
        }

        [Test]
        public void TestTryGetEdge()
        {
            bool res = _graph.TryGetEdge("1", "7", out float value);
            Assert.IsTrue(res);
            Assert.AreEqual(0.5f, value);
            
            res = _graph.TryGetEdge("not in graph", "1000", out value);
            Assert.IsFalse(res);
        }
        
        [Test]
        public void TestTryGetEdgeExceptions()
        {
            // Source null check
            Assert.Throws<ArgumentNullException>(() =>
            {
                _graph.TryGetEdge(null, "7", out _);
            });
            
            // Destination null check
            Assert.Throws<ArgumentNullException>(() =>
            {
                _graph.TryGetEdge("1", null, out float _);
            });
            
            // Source and destination should not be the same
            Assert.Throws<ArgumentException>(() =>
            {
                _graph.TryGetEdge("same node", "same node", out float _);
            });
        }

        [Test]
        public void TestNodes()
        {
            Assert.AreEqual(
                new[]{"1", "2", "3", "4", "5", "6", "7"}, 
                _graph.Nodes.OrderBy(n => n).ToArray());
        }

        [Test]
        public void TestOutgoingEdges()
        {
            IEnumerable<Edge<string, float>> edges;

            edges = _graph.OutgoingEdges("1").OrderBy(e => e.Destination).ToArray();
            Assert.AreEqual(
                new[] { "3", "7" },
                edges.Select(e => e.Destination).ToArray()
            );
            Assert.AreEqual(
                new[] { 100.0f, 0.5f },
                edges.Select(e => e.Data).ToArray()
            );
        }
        
        [Test]
        public void TestOutgoingEdgesExceptions()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                var unused = _graph.OutgoingEdges(null).ToArray();
            });
        }
    }
}
