namespace Bhik95.Graph
{
    /// <summary>
    /// This struct represents an edge between two nodes of a graph. Each node is of type TNode and each edge stores
    /// data of type TEdgeData
    /// </summary>
    /// <typeparam name="TNode"></typeparam>
    /// <typeparam name="TEdgeData"></typeparam>
    public struct Edge<TNode, TEdgeData>
    {
        public TNode Source;
        public TNode Destination;
        public TEdgeData Data;

        public Edge(TNode source, TNode destination, TEdgeData data)
        {
            Source = source;
            Destination = destination;
            Data = data;
        }
    }
}


