using System.Collections.Generic;

namespace Bhik95.Utility.Graph
{
    public interface IDirectedGraph<TNode, TEdgeData>
    {
        bool TryGetEdge(TNode regionKeySource, TNode regionKeyDestination, out TEdgeData value);

        bool ContainsNode(TNode regionKey);
        bool ContainsEdge(TNode source, TNode destination);
        
        IEnumerable<TNode> Nodes { get; }
        IEnumerable<Edge<TNode, TEdgeData>> OutgoingEdges(TNode source);
    }

}