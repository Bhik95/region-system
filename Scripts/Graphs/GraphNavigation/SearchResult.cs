using System.Collections.Generic;

namespace Bhik95.Graph
{
    /// <summary>
    /// Helper class storing the information about the result of a graph search algorithm in class GraphOperations
    /// </summary>
    /// <typeparam name="TNode">The type of the node data</typeparam>
    /// <typeparam name="TEdgeData">The type of the edge data</typeparam>
    public class SearchResult<TNode, TEdgeData>
    {
        private readonly LinkedList<Edge<TNode, TEdgeData>> _pathToSolution = new LinkedList<Edge<TNode, TEdgeData>>();
        private TNode _sourceNode;
        private TNode _goalNode;
        private float _totalCost;
        private int _totalExploredNodes;
        private double _timeElapsedMillis;

        private SearchResult(){}

        public IEnumerable<Edge<TNode, TEdgeData>> PathToSolution => _pathToSolution;
        public TNode SourceNode => _sourceNode;
        public TNode GoalNode => _goalNode;
        public float TotalCost => _totalCost;
        public int TotalExploredNodes => _totalExploredNodes;
        public double TimeElapsedMillis => _timeElapsedMillis;

        /// <summary>
        /// Given the goal search node, return the search result containing the information about the reconstructed path
        /// to the goal alongside other useful information.
        /// </summary>
        /// <param name="goalSearchNode">The search node containing the goal node</param>
        /// <param name="totalExploredNodes">The amount of nodes that have been explored by the search algorithm</param>
        /// <param name="timeElapsedMillis">The time elapsed since the start of the search algorithm</param>
        /// <returns></returns>
        public static SearchResult<TNode, TEdgeData> ReconstructPath(
            SearchNode<TNode, TEdgeData> goalSearchNode,
            int totalExploredNodes,
            double timeElapsedMillis)
        {
            SearchResult<TNode, TEdgeData> searchResult = new SearchResult<TNode, TEdgeData>();

            searchResult._goalNode = goalSearchNode.Node;
            searchResult._totalCost = goalSearchNode.CumulativeCost;
            searchResult._totalExploredNodes = totalExploredNodes;
            searchResult._timeElapsedMillis = timeElapsedMillis;
            
            SearchNode<TNode, TEdgeData> currentSearchNode = goalSearchNode;
            
            while (!currentSearchNode.IsStartingNode)
            {
                searchResult._pathToSolution.AddFirst(currentSearchNode.EdgeFrom);
                currentSearchNode = currentSearchNode.SearchNodeFrom;
            }
            searchResult._sourceNode = currentSearchNode.Node;

            return searchResult;
        }
    }
}

