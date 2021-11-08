using System;
using System.Linq;
using Bhik95.Grids;
using NUnit.Framework;
using UnityEngine;

namespace Bhik95.Graph.Tests
{
    public class GraphNavigationTests
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
        public void TestAStarMultiSearch()
        {
            AssertSearchResult("1", n => (int.Parse(n) % 2) == 0, new[] { "2", "4" }, new[] { 7.5f, 4.0f });
            AssertSearchResult("7", n => (int.Parse(n) % 2) == 0,new[] { "2", "4" }, new[] { 7.0f, 3.5f });
            AssertSearchResult("3", n => (int.Parse(n) % 2) == 0,new[] { "2", "4" }, new[] { 4.0f, 0.5f });
            AssertSearchResult("2", n => (int.Parse(n) % 2) == 0,new[] { "2", "4" }, new[] { 0.0f, 1.0f });
            AssertSearchResult("4", n => (int.Parse(n) % 2) == 0,new[] { "4" }, new[] { 0.0f });
            AssertSearchResult("5", n => (int.Parse(n) % 2) == 0,new[] { "6" }, new[] { 10.0f });
            AssertSearchResult("6", n => (int.Parse(n) % 2) == 0,new[] { "6" }, new[] { 0.0f });
            
            // Test results not found
            AssertSearchResult("1", n => n == "999",new string[]{}, new float[]{});
        }

        private void AssertSearchResult(string sourceNode, Func<string, bool> goalFunction, string[] goals, float[] totalCosts)
        {
            SearchResult<string, float>[] results = GraphOperations.AStarMultiSearch(
                _graph,
                sourceNode,
                goalFunction,
                e => e.Data,
                n => 0,
                edge => true,
                maxCount: -1).OrderBy(searchNode => searchNode.GoalNode).ToArray();

            // Check if source nodes are correct
            Assert.IsTrue(results.All(sn => sn.SourceNode == sourceNode));
            
            // Check if goals are correct
            Assert.AreEqual(
                goals,
                results.Select(sn => sn.GoalNode).ToArray()
            );
            
            // Check if total costs are correct
            Assert.AreEqual(
                totalCosts,
                results.Select(sn => sn.TotalCost).ToArray()
            );
        }
        
        [Test]
        public void TestAStarMultiSearchExceptions()
        {
            // null graph
            Assert.Throws<ArgumentNullException>(() =>
            {
                var unused = GraphOperations.AStarMultiSearch<string, float>(
                    null,
                    "1",
                    n => (int.Parse(n) % 2) == 0,
                    e => e.Data,
                    n => 0,
                    edge => true,
                    maxCount: -1).ToArray();
            });

            // null source
            Assert.Throws<ArgumentNullException>(() =>
            {
                var unused = GraphOperations.AStarMultiSearch(
                    _graph,
                    null,
                    n => (int.Parse(n) % 2) == 0,
                    e => e.Data,
                    n => 0,
                    edge => true,
                    maxCount: -1).ToArray();
            });
            
            // source not in graph
            Assert.Throws<ArgumentException>(() =>
            {
                var unused = GraphOperations.AStarMultiSearch(
                    _graph,
                    "not in graph",
                    n => (int.Parse(n) % 2) == 0,
                    e => e.Data,
                    n => 0,
                    edge => true,
                    maxCount: -1).ToArray();
            });
            
            // null goal function
            Assert.Throws<ArgumentNullException>(() =>
            {
                var unused = GraphOperations.AStarMultiSearch(
                    _graph,
                    "1",
                    null,
                    e => e.Data,
                    n => 0,
                    edge => true,
                    maxCount: -1).ToArray();
            });
            
            // null cost function
            Assert.Throws<ArgumentNullException>(() =>
            {
                var unused = GraphOperations.AStarMultiSearch(
                    _graph,
                    "1",
                    n => (int.Parse(n) % 2) == 0,
                    null,
                    n => 0,
                    edge => true,
                    maxCount: -1).ToArray();
            });
            
            // null heuristic function
            Assert.Throws<ArgumentNullException>(() =>
            {
                var unused = GraphOperations.AStarMultiSearch(
                    _graph,
                    "1",
                    n => (int.Parse(n) % 2) == 0,
                    e => e.Data,
                    null,
                    edge => true,
                    maxCount: -1).ToArray();
            });
            
            // null edge filter function
            Assert.Throws<ArgumentNullException>(() =>
            {
                var unused = GraphOperations.AStarMultiSearch(
                    _graph,
                    "1",
                    n => (int.Parse(n) % 2) == 0,
                    e => e.Data,
                    n => 0,
                    null,
                    maxCount: -1).ToArray();
            });
            
            // invalid maxCount (should be -1 or in [0, +inf[)
            Assert.Throws<ArgumentException>(() =>
            {
                var unused = GraphOperations.AStarMultiSearch(
                    _graph,
                    "1",
                    n => (int.Parse(n) % 2) == 0,
                    e => e.Data,
                    n => 0,
                    edge => true,
                    maxCount: -999).ToArray();
            });
        }

        private void AssertExpand(string sourceNode, string[] expectedResult, bool includeSource)
        {
            string[] actualResult = GraphOperations.Expand(_graph, sourceNode, edge => true, includeSource).OrderBy(n => n).ToArray();
            Assert.AreEqual(
                expectedResult,
                actualResult
            );
        }
        
        private void AssertExpand(string sourceNode, string[] expectedResult, Func<Edge<string, float>, float> costFunction, float maxCost, bool includeSource)
        {
            string[] actualResult = GraphOperations.Expand(_graph, sourceNode, costFunction, maxCost, edge => true, includeSource).OrderBy(n => n).ToArray();
            Assert.AreEqual(
                expectedResult,
                actualResult
            );
        }

        [Test]
        public void TestAStarSearchWithFilter()
        {
            string sourceNode = "1";
            string goalNode = "3";

            GraphOperations.AStarSearch(
                _graph,
                sourceNode, 
                n => n == goalNode, 
                edge => edge.Data, 
                h => 0, 
                edge => edge.Destination != "7",
                out var searchResult);
            
            Assert.AreEqual(sourceNode, searchResult.SourceNode);
            Assert.AreEqual(goalNode, searchResult.GoalNode);
            Assert.AreEqual(100.0f, searchResult.TotalCost);
        }

        [Test]
        public void TestAStarSearchHeuristics()
        {
            BasicGridXZ<string> gridXZ = new BasicGridXZ<string>(new Vector2Int(8, 8), " ");

            for (int ix = 0; ix <= 6; ix++)
                gridXZ[ix, 6] = "X";

            Vector2Int sourceNode = new Vector2Int(2, 2);
            Vector2Int goalNode = new Vector2Int(6, 7);
            
            GridGraphXZ<string> gridGraphXZ = new GridGraphXZ<string>(gridXZ, ExpansionType.FourDirections);
            
            GraphOperations.AStarSearch(
                gridGraphXZ,
                sourceNode,
                n => n == goalNode,
                edge => edge.Data,
                n => 0,
                edge => gridXZ[edge.Destination] != "X",
                out var resultNoHeuristics);

            GraphOperations.AStarSearch(
                gridGraphXZ,
                sourceNode,
                n => n == goalNode,
                edge => edge.Data,
                n => Mathf.Abs(goalNode.x - n.x) + Mathf.Abs(goalNode.y - n.y),
                edge => gridXZ[edge.Destination] != "X",
                out var resultWithHeuristics);
            
            Assert.AreEqual(11, resultWithHeuristics.TotalCost);
            Assert.AreEqual(11, resultNoHeuristics.TotalCost);
            Assert.Less(resultWithHeuristics.TotalExploredNodes, resultNoHeuristics.TotalExploredNodes);
        }
        
        [Test]
        public void TestExpandNoCost()
        {
            AssertExpand("1", new[]{"2", "3", "4", "7"}, false);
            AssertExpand("1", new[]{"1", "2", "3", "4", "7"}, true);
            
            AssertExpand("2", new[]{"4"}, false);
            AssertExpand("2", new[]{"2", "4"}, true);
            
            AssertExpand("4", new string[]{}, false);
            AssertExpand("4", new[]{"4"}, true);
        }
        
        [Test]
        public void TestExpandWithCost()
        {
            AssertExpand("1", new[]{"3", "4", "7"}, edge => edge.Data, 5, false);
            AssertExpand("1", new[]{"1", "3", "4", "7"}, edge => edge.Data, 5,true);
            
            AssertExpand("2", new[]{"4"}, edge => edge.Data, 5,false);
            AssertExpand("2", new[]{"2", "4"}, edge => edge.Data, 5,true);
            
            AssertExpand("4", new string[]{}, edge => edge.Data, 5,false);
            AssertExpand("4", new[]{"4"}, edge => edge.Data, 5,true);
        }
        
        [Test]
        public void TestExpandWithCostExceptions()
        {
            // Null graph check
            Assert.Throws<ArgumentNullException>(() =>
            {
                var unused = GraphOperations.Expand<string, float>(null, "1", edge => edge.Data, 10f, edge => true).ToArray();
            });
            
            // Null source check
            Assert.Throws<ArgumentNullException>(() =>
            {
                var unused = GraphOperations.Expand(_graph, null, edge => edge.Data, 10f, edge => true).ToArray();
            });
            
            // Source in graph check
            Assert.Throws<ArgumentException>(() =>
            {
                var unused = GraphOperations.Expand(_graph, "not in graph", edge => edge.Data, 10f, edge => true).ToArray();
            });
            
            // Null cost function
            Assert.Throws<ArgumentNullException>(() =>
            {
                var unused = GraphOperations.Expand(_graph, "1", null, 10f, edge => true).ToArray();
            });
            
            // Null cost function with infinite maxCost
            Assert.Throws<ArgumentNullException>(() =>
            {
                var unused = GraphOperations.Expand(_graph, "1", null, float.PositiveInfinity, edge => true).ToArray();
            });
            
            // Non-negative maxCost check
            Assert.Throws<ArgumentException>(() =>
            {
                var unused = GraphOperations.Expand(_graph, "1", edge => edge.Data, -1, edge => true).ToArray();
            });
            
            // Null edge filter check
            Assert.Throws<ArgumentNullException>(() =>
            {
                var unused = GraphOperations.Expand(_graph, "1", edge => edge.Data, 10.0f, null).ToArray();
            });
        }
    }

}
