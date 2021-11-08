using System;
using System.Collections.Generic;

namespace Bhik95.Graph
{
    /// <summary>
    /// This class represents a directed graph where each node can be connected to another distinct node of the graph
    /// through an edge. The implementation is based on Adjacency lists.
    /// </summary>
    /// <typeparam name="TNode">The type of the node</typeparam>
    /// <typeparam name="TEdgeData">The type of of the edge between two nodes</typeparam>
    public class DirectedGraph<TNode, TEdgeData> : IDirectedGraph<TNode, TEdgeData>
    {
        
        // For each node n, store the list of its adjacent nodes
        private Dictionary<TNode, LinkedList<TNode>> _adjacencyDictionary = new Dictionary<TNode, LinkedList<TNode>>();
        
        // Store the data associated to the edge between node n1 and node n2 
        private Dictionary<(TNode, TNode), TEdgeData> _edges = new Dictionary<(TNode, TNode), TEdgeData>();

        /// <summary>
        /// Adds a node to the current graph
        /// </summary>
        /// <param name="node">The node to add to the graph</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public void AddNode(TNode node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));
            
            if (_adjacencyDictionary.ContainsKey(node))
                throw new ArgumentException("The node is already in the graph");
            
            _adjacencyDictionary.Add(node, new LinkedList<TNode>());
        }

        /// <summary>
        /// Add an edge to the current graph. If the source or destination nodes were not previously added to the graph,
        /// they are automatically added by this method.
        /// </summary>
        /// <param name="source">The source node</param>
        /// <param name="destination">The destination node</param>
        /// <param name="edgeData">The data associated with the edge</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public void AddEdge(TNode source, TNode destination, TEdgeData edgeData)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (destination == null)
                throw new ArgumentNullException(nameof(destination));
            if (edgeData == null)
                throw new ArgumentNullException(nameof(edgeData));
            if (source.Equals(destination))
                throw new ArgumentException("Source node and destination node need to be distinct.");
            
            if (!_adjacencyDictionary.ContainsKey(source))
                AddNode(source);
            
            if (!_adjacencyDictionary.ContainsKey(destination))
                AddNode(destination);

            _edges.Add((source, destination), edgeData);

            if (_adjacencyDictionary.TryGetValue(source, out LinkedList<TNode> adjacencyList))
            {
                adjacencyList.AddLast(destination);
                _adjacencyDictionary[source] = adjacencyList;
            }
        }
        
        /// <summary>
        /// Searches for the edge between two nodes and attempts to return the data associated with it.
        /// </summary>
        /// <param name="regionKeySource">The source node</param>
        /// <param name="regionKeyDestination">The destination node</param>
        /// <param name="value">The data associated with the edge between the two input nodes</param>
        /// <returns>A value indicating whether there's an edge between the two nodes</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public bool TryGetEdge(TNode regionKeySource, TNode regionKeyDestination, out TEdgeData value)
        {
            if (regionKeySource == null)
                throw new ArgumentNullException(nameof(regionKeySource));
            if (regionKeyDestination == null)
                throw new ArgumentNullException(nameof(regionKeyDestination));
            if (regionKeySource.Equals(regionKeyDestination))
                throw new ArgumentException("Source node and destination node need to be distinct.");
            
            return _edges.TryGetValue((regionKeySource, regionKeyDestination), out value);
        }

        /// <summary>
        /// The number of nodes in the graph
        /// </summary>
        public int NodeCount => _adjacencyDictionary.Count;

        /// <summary>
        /// The number of edges in the graph
        /// </summary>
        public int EdgeCount => _edges.Count;

        /// <summary>
        /// Determines whether the graph contains the specified node
        /// </summary>
        /// <param name="regionKey">The node to locate in the graph</param>
        /// <returns>True, if the graph contains the node</returns>
        public bool ContainsNode(TNode regionKey) => _adjacencyDictionary.ContainsKey(regionKey);

        /// <summary>
        /// Determines whether the graph contains the edge between the specified source and destination nodes
        /// </summary>
        /// <param name="source">The source node</param>
        /// <param name="destination">The destination node</param>
        /// <returns>True, if the graph contains the edge</returns>
        public bool ContainsEdge(TNode source, TNode destination) => _edges.ContainsKey((source, destination));

        /// <summary>
        /// The nodes in the graph
        /// </summary>
        public IEnumerable<TNode> Nodes => _adjacencyDictionary.Keys;
        
        /// <summary>
        /// It returns the outgoing edges starting from a source node
        /// </summary>
        /// <param name="source">The source node</param>
        /// <returns>The outgoing edges starting from the source node</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public IEnumerable<Edge<TNode, TEdgeData>> OutgoingEdges(TNode source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (_adjacencyDictionary.TryGetValue(source, out LinkedList<TNode> adjacencyList))
            {
                foreach (TNode destination in adjacencyList)
                {
                    TryGetEdge(source, destination, out TEdgeData value);
                    yield return new Edge<TNode, TEdgeData>(source, destination, value);
                }
            }
            else
            {
                throw new ArgumentException("The source node is not in the graph.");
            }
        }
    }
}
