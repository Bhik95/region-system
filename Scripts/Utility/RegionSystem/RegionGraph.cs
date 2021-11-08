using System;
using System.Collections.Generic;
using Bhik95.Utility.Graph;
using UnityEngine;

namespace Bhik95.Utility.RegionSystem
{
    /// <summary>
    /// Graph representing the connectivity between different regions of the world. Each node is a Vector2Int representing
    /// the key of the region. There is an edge with a weight of 1 between each adjacent regions.
    /// </summary>
    public class RegionGraph<TRegionData> : IDirectedGraph<Vector2Int, float>
    {
        private readonly Dictionary<Vector2Int, Region<TRegionData>> _regionDict;
        private readonly LinkToRegionsDatabase<TRegionData> _linkToRegionsDatabase;
        private readonly RegionSystem<TRegionData> _regionSystem;

        /// <summary>
        /// The connectivity graph between different regions of the world. Each key of the region dictionary is a node
        /// of the graph. The edges are calculated from the LinkToRegionsDatabase.
        /// </summary>
        /// <param name="regionDict">The dictionary whose keys represent the nodes of the graph</param>
        /// <param name="linkToRegionsDatabase">The database of links to calculate the edges from</param>
        /// <param name="regionSystem">The region system.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public RegionGraph(Dictionary<Vector2Int, Region<TRegionData>> regionDict, LinkToRegionsDatabase<TRegionData> linkToRegionsDatabase, RegionSystem<TRegionData> regionSystem)
        {
            _regionDict = regionDict ?? throw new ArgumentNullException(nameof(regionDict));
            _linkToRegionsDatabase = linkToRegionsDatabase ?? throw new ArgumentNullException(nameof(linkToRegionsDatabase));
            _regionSystem = regionSystem ?? throw new ArgumentNullException(nameof(regionSystem));
        }

        public bool TryGetEdge(Vector2Int regionKeySource, Vector2Int regionKeyDestination, out float value)
        {
            if (ContainsEdge(regionKeySource, regionKeyDestination))
            {
                value = 1f;
                return true;
            }

            value = float.NaN;
            return false;
        }

        public bool ContainsNode(Vector2Int regionKey) => _regionDict.ContainsKey(regionKey);

        public bool ContainsEdge(Vector2Int source, Vector2Int destination)
        {
            foreach (Vector2Int adjacentRegionKey in _linkToRegionsDatabase.GetAdjacentRegionKeys(source, _regionSystem))
            {
                if (adjacentRegionKey == destination)
                    return true;
            }

            return false;
        }

        public IEnumerable<Vector2Int> Nodes => _regionDict.Keys;
        public IEnumerable<Edge<Vector2Int, float>> OutgoingEdges(Vector2Int source)
        {
            foreach (Vector2Int adjacentRegionKey in _linkToRegionsDatabase.GetAdjacentRegionKeys(source, _regionSystem))
            {
                yield return new Edge<Vector2Int, float>(source, adjacentRegionKey, 1f);
            }
        }
    }
}

