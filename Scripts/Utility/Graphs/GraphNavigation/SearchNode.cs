using System;

namespace Bhik95.Utility.Graph
{
    /// <summary>
    /// Helper class storing the relevant information for graph search algorithms in the GraphOperations class
    /// </summary>
    /// <typeparam name="TNode">The type of the node data</typeparam>
    /// <typeparam name="TEdgeData">The type of the edge data</typeparam>
    public class SearchNode<TNode, TEdgeData>
    {
        public readonly TNode Node;
        public readonly float CumulativeCost;

        private Edge<TNode, TEdgeData> _edgeFrom;
        public Edge<TNode, TEdgeData> EdgeFrom {
            get
            {
                if (IsStartingNode)
                    throw new InvalidOperationException("Can't get the previous edge of a starting search node.");
                return _edgeFrom;
            }
        }
        
        private SearchNode<TNode, TEdgeData> _searchNodeFrom;

        public SearchNode<TNode, TEdgeData> SearchNodeFrom
        {
            get
            {
                if (IsStartingNode)
                    throw new InvalidOperationException("Can't get the antecedent search node of a starting search node.");
                return _searchNodeFrom;
            }
        }

        public readonly bool IsStartingNode;
        
        public SearchNode(TNode node, float cumulativeCost, Edge<TNode, TEdgeData>? edgeFrom, SearchNode<TNode, TEdgeData> searchNodeFrom)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));
            if (cumulativeCost < 0)
                throw new ArgumentOutOfRangeException(nameof(cumulativeCost), cumulativeCost, "Cumulative cost is expected to be non-negative.");
            
            if (edgeFrom.HasValue != (searchNodeFrom != null))
                throw new ArgumentException("edgeFrom and searchNodeFrom should either both be null or not null.");
            
            Node = node;
            CumulativeCost = cumulativeCost;
            IsStartingNode = !edgeFrom.HasValue;
            _edgeFrom = edgeFrom.GetValueOrDefault();
            _searchNodeFrom = searchNodeFrom;
        }
    }
}

