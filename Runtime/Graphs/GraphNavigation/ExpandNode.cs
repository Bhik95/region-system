using System;

namespace Bhik95.Graph
{
    /// <summary>
    /// Helper class storing the relevant information for node expansion of the function GraphOperations.Expand
    /// </summary>
    /// <typeparam name="TNode">The type of the node data</typeparam>
    public class ExpandNode<TNode>
    {
        public readonly TNode Node;
        public readonly float CumulativeCost;
        public readonly bool ToYield;

        /// <summary>
        /// Construct an ExpandNode given the node it should wrap, the cumulative cost to reach from a source position,
        /// and whether it should be yielded by the expansion algorithm.
        /// </summary>
        /// <param name="node">The node to wrap</param>
        /// <param name="cumulativeCost">The cumulative cost to reach the node</param>
        /// <param name="toYield">True, if the node should be yielded by the expansion algorithm</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public ExpandNode(TNode node, float cumulativeCost, bool toYield)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));
            if (cumulativeCost < 0)
                throw new ArgumentOutOfRangeException(nameof(cumulativeCost), cumulativeCost, "Cumulative cost is expected to be non-negative.");
            
            Node = node;
            CumulativeCost = cumulativeCost;
            ToYield = toYield;
        }
    }
}


