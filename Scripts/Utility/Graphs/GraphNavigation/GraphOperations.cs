using System;
using System.Collections.Generic;
using System.Linq;
using Priority_Queue;

namespace Bhik95.Utility.Graph
{
    /// <summary>
    /// Class to be used to perform common operations on graphs such as search, expansion (BFS), and more (TO UPDATE)
    /// </summary>
    public static class GraphOperations
    {
        /// <summary>
        /// Given a goalFunction, a costFunction and a heuristic function, it performs an A* search on the specified
        /// graph starting from the given source node.
        /// </summary>
        /// <param name="graph">The graph to search on</param>
        /// <param name="source">The source node to start the search from</param>
        /// /// <param name="goalFunction">A function whose result defines which are the possible goal nodes (result = true)</param>
        /// <param name="costFunction">A function that returns the cost associated with an edge</param>
        /// <param name="heuristicFunction">A function that, given an edge, returns a pessimist estimate of the cost to reach the closest goal node</param>
        /// /// <param name="edgeFilter">A function whose boolean return value determines whether an edge should be
        /// considered (true) or ignored (false)</param>
        /// <param name="searchResult">A search result</param>
        /// <returns>True, if the search is successful</returns>
        public static bool AStarSearch<TNode, TEdgeData>(
            IDirectedGraph<TNode, TEdgeData> graph,
            TNode source,
            Func<TNode, bool> goalFunction,
            Func<Edge<TNode, TEdgeData>, float> costFunction,
            Func<TNode, float> heuristicFunction,
            Func<Edge<TNode, TEdgeData>, bool> edgeFilter,
            out SearchResult<TNode, TEdgeData> searchResult)
        {
            SearchResult<TNode, TEdgeData>[] results = AStarMultiSearch(graph, source, goalFunction, costFunction, heuristicFunction, edgeFilter, 1).ToArray();
            if (results.Length == 0)
            {
                searchResult = null;
                return false;
            }

            searchResult = results[0];
            return true;
        }

        /// <summary>
        /// Given a goalFunction, a costFunction and a heuristic function, it performs an A* search on the specified
        /// graph starting from the given source node. The search can yield multiple results.
        /// </summary>
        /// <param name="graph">The graph to search on</param>
        /// <param name="source">The source node to start the search from</param>
        /// <param name="maxCount">The maximum number of results to yield.</param>
        /// <param name="goalFunction">A function whose result defines which are the possible goal nodes (result = true)</param>
        /// <param name="costFunction">A function that returns the cost associated with an edge</param>
        /// <param name="heuristicFunction">A function that, given an edge, returns a pessimist estimate of the cost to reach the closest goal node</param>
        /// /// <param name="edgeFilter">A function whose boolean return value determines whether an edge should be
        /// considered (true) or ignored (false)</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static IEnumerable<SearchResult<TNode, TEdgeData>> AStarMultiSearch<TNode, TEdgeData>(
            IDirectedGraph<TNode, TEdgeData> graph,
            TNode source,
            Func<TNode, bool> goalFunction,
            Func<Edge<TNode, TEdgeData>, float> costFunction,
            Func<TNode, float> heuristicFunction,
            Func<Edge<TNode, TEdgeData>, bool> edgeFilter,
            int maxCount = -1)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (goalFunction == null)
                throw new ArgumentNullException(nameof(goalFunction));
            if (costFunction == null)
                throw new ArgumentNullException(nameof(costFunction));
            if (heuristicFunction == null)
                throw new ArgumentNullException(nameof(heuristicFunction));
            if (edgeFilter == null)
                throw new ArgumentNullException(nameof(edgeFilter));

            if (!graph.ContainsNode(source))
                throw new ArgumentException("The graph does not contain the specified source node.");

            if (maxCount < -1)
                throw new ArgumentException(nameof(maxCount) + " is expected to be non-negative or -1.");

            int totalExploredNodes = 0;
            DateTime startTime = DateTime.Now;
            
            SimplePriorityQueue<SearchNode<TNode, TEdgeData>> priorityQueue = new SimplePriorityQueue<SearchNode<TNode, TEdgeData>>();
            HashSet<TNode> visitedSet = new HashSet<TNode>();

            Dictionary<TNode, float> bestFValues = new Dictionary<TNode, float>();
            Dictionary<TNode, SearchNode<TNode, TEdgeData>> bestSearchNodes = new Dictionary<TNode, SearchNode<TNode, TEdgeData>>();

            int countResults = 0;

            SearchNode<TNode, TEdgeData> currentSearchNode = new SearchNode<TNode, TEdgeData>(source, 0, null, null);
            priorityQueue.Enqueue(currentSearchNode, 0);
            bestSearchNodes.Add(source, currentSearchNode);
            bestFValues[source] = heuristicFunction(source);

            while (priorityQueue.Count > 0)
            {
                currentSearchNode = priorityQueue.Dequeue();
                visitedSet.Add(currentSearchNode.Node);
                totalExploredNodes++;

                if (goalFunction(currentSearchNode.Node))
                {
                    countResults++;
                    double elapsedMillis = (DateTime.Now - startTime).TotalMilliseconds;
                    yield return SearchResult<TNode, TEdgeData>.ReconstructPath(currentSearchNode, totalExploredNodes, elapsedMillis);
                }
                if (maxCount >= 0 && countResults == maxCount)
                    break;

                foreach (var edge in graph.OutgoingEdges(currentSearchNode.Node))
                {
                    if(visitedSet.Contains(edge.Destination))
                        continue;
                    if(!edgeFilter(edge))
                        continue;

                    float cumulativeCost = currentSearchNode.CumulativeCost + costFunction(edge);
                    float fValue = cumulativeCost + heuristicFunction(edge.Destination);

                    if (!bestFValues.ContainsKey(edge.Destination) || fValue < bestFValues[edge.Destination])
                    {
                        if (bestSearchNodes.ContainsKey(edge.Destination) && fValue < bestFValues[edge.Destination])
                        {
                            // Remove non-optimal search node from the queue, if present
                            priorityQueue.TryRemove(bestSearchNodes[edge.Destination]);
                        }

                        SearchNode<TNode, TEdgeData> destinationSearchNode = new SearchNode<TNode, TEdgeData>(
                            edge.Destination,
                            cumulativeCost,
                            edge,
                            currentSearchNode);
                    
                        priorityQueue.Enqueue(destinationSearchNode, fValue);
                        bestSearchNodes[edge.Destination] = destinationSearchNode;
                        bestFValues[edge.Destination] = fValue;
                    }

                }
            }
        }

        /// <summary>
        /// Given a graph and a source node, it returns the nodes that are reachable from the source node
        /// </summary>
        /// <param name="graph">The graph</param>
        /// <param name="source">The source node to start from</param>
        /// <param name="includeSource">Whether the source node should be included in the final result</param>
        /// <param name="edgeFilter">A function whose boolean return value determines whether an edge should be
        /// considered (true) or ignored (false)</param>
        /// <returns>The reachable nodes from the source node whose total cost to reach does not exceed maxCost</returns>
        public static IEnumerable<TNode> Expand<TNode, TEdgeData>(
            IDirectedGraph<TNode, TEdgeData> graph,
            TNode source,
            Func<Edge<TNode, TEdgeData>, bool> edgeFilter,
            bool includeSource = true)
        {
            return Expand(graph, source, edge => 0f, float.PositiveInfinity, edgeFilter, includeSource);
        }

        /// <summary>
        /// Given a graph and a source node, the method returns the nodes that are reachable from the source node
        /// and whose total cost to reach from the source node do not exceed maxCost.
        /// </summary>
        /// <param name="graph">The graph</param>
        /// <param name="source">The source node to start from</param>
        /// <param name="costFunction">A function that returns the cost associated with an edge</param>
        /// <param name="maxCost">The maximum cost</param>
        /// <param name="includeSource">Whether the source node should be included in the final result</param>
        /// <param name="edgeFilter">A function whose boolean return value determines whether an edge should be
        /// considered (true) or ignored (false)</param>
        /// <returns>The reachable nodes from the source node whose total cost to reach does not exceed maxCost</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static IEnumerable<TNode> Expand<TNode, TEdgeData>(
            IDirectedGraph<TNode, TEdgeData> graph,
            TNode source,
            Func<Edge<TNode, TEdgeData>, float> costFunction,
            float maxCost,
            Func<Edge<TNode, TEdgeData>, bool> edgeFilter,
            bool includeSource = true)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (!graph.ContainsNode(source))
                throw new ArgumentException($"Source node was not found in the graph: {source}");
            if (costFunction == null)
                throw new ArgumentNullException(nameof(costFunction));
            if (maxCost < 0)
                throw new ArgumentException($"Parameter {nameof(maxCost)} is expected to be non-negative.");
            if (edgeFilter == null)
                throw new ArgumentNullException(nameof(edgeFilter));
            
            Queue<ExpandNode<TNode>> queue = new Queue<ExpandNode<TNode>>();
            HashSet<TNode> visitedSet = new HashSet<TNode>();

            queue.Enqueue(new ExpandNode<TNode>(source, 0, includeSource));
            visitedSet.Add(source);
            
            while (queue.Count > 0)
            {
                ExpandNode<TNode> currentExpandNode = queue.Dequeue();

                if(currentExpandNode.ToYield)
                    yield return currentExpandNode.Node;

                foreach (Edge<TNode,TEdgeData> edge in graph.OutgoingEdges(currentExpandNode.Node))
                {
                    if(visitedSet.Contains(edge.Destination))
                        continue;
                    
                    if(!edgeFilter(edge))
                        continue;

                    float cost = costFunction(edge);

                    float totalCost = currentExpandNode.CumulativeCost + cost;
                    if(totalCost > maxCost)
                        continue;
                    
                    queue.Enqueue(new ExpandNode<TNode>(edge.Destination, totalCost, true));
                    visitedSet.Add(edge.Destination);
                }
            }
        }
    }
}


