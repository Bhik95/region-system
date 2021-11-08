using System.Collections.Generic;
using UnityEngine;

namespace Bhik95.Utility.RegionSystem
{
    /// <summary>
    /// This class keeps track of all the links between regions and provides utility methods to navigate
    /// </summary>
    public class LinkToRegionsDatabase<TRegionData>
    {
        /// <summary>
        /// For each link, this dictionary keeps track of set of regions connected by that link
        /// </summary>
        private readonly Dictionary<int, HashSet<Vector2Int>> _linkToRegionKeys;

        public LinkToRegionsDatabase()
        {
            _linkToRegionKeys = new Dictionary<int, HashSet<Vector2Int>>();
        }
        
        /// <summary>
        /// Add an association between a region link and a region to the database.
        /// </summary>
        /// <param name="link">The link</param>
        /// <param name="regionKey">The key of the region</param>
        public void AddLinkAssociatedWithRegion(RegionLink link, Vector2Int regionKey)
        {
            int linkHash = link.GetHashCode();
            if (!_linkToRegionKeys.ContainsKey(linkHash))
            {
                _linkToRegionKeys[linkHash] = new HashSet<Vector2Int>();
            }
            _linkToRegionKeys[linkHash].Add(regionKey);
        }

        /// <summary>
        /// Remove all the links associated with a region from the database 
        /// </summary>
        /// <param name="regionKey">The key of the region</param>
        /// <param name="regionSystem">The region system</param>
        public void RemoveLinksAssociatedWithRegion(Vector2Int regionKey, RegionSystem<TRegionData> regionSystem)
        {
            foreach (RegionLink link in regionSystem.RegionDict[regionKey].Links)
            {
                int linkHash = link.GetHashCode();
                if (_linkToRegionKeys.ContainsKey(linkHash))
                {
                    _linkToRegionKeys[linkHash].Remove(regionKey);
                }

                if (_linkToRegionKeys[linkHash].Count == 0)
                    _linkToRegionKeys.Remove(linkHash);
            }
        }
        
        /// <summary>
        /// Given the key of a source region, return the keys of the adjacent regions
        /// </summary>
        /// <param name="sourceRegionKey">The source region</param>
        /// <param name="regionSystem">The region system</param>
        /// <returns></returns>
        public IEnumerable<Vector2Int> GetAdjacentRegionKeys(Vector2Int sourceRegionKey, RegionSystem<TRegionData> regionSystem)
        {
            HashSet<Vector2Int> adjacentRegions = new HashSet<Vector2Int>();
            foreach (RegionLink link in regionSystem.RegionDict[sourceRegionKey].Links)
            {
                if (TryGetOtherRegionFromLink(link, sourceRegionKey, out Vector2Int otherRegion))
                    adjacentRegions.Add(otherRegion);
            }
            return adjacentRegions;
        }
        
        /// <summary>
        /// Given a link and a source region, try to get the other region associated with the link.
        /// </summary>
        /// <param name="link">The link between two regions</param>
        /// <param name="sourceRegionKey">The source region</param>
        /// <param name="otherRegion">The other region</param>
        /// <returns></returns>
        public bool TryGetOtherRegionFromLink(RegionLink link, Vector2Int sourceRegionKey, out Vector2Int otherRegion)
        {
            foreach (Vector2Int regionKey in _linkToRegionKeys[link.GetHashCode()])
            {
                if (sourceRegionKey != regionKey)
                {
                    otherRegion = regionKey;
                    return true;
                }
                    
            }
            
            otherRegion = new Vector2Int(-1, -1);
            return false;
        }
    }
}

