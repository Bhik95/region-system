using System.Collections.Generic;
using UnityEngine;

namespace Bhik95.Utility.RegionSystem
{
    /// <summary>
    /// This class represents a Region of the RegionSystem. A region is an area of the world grid where each tile is
    /// reachable by other tiles in the region. Each region can contain custom data that might depend on the game.
    /// </summary>
    public class Region<TData>
    {
        /// <summary>
        /// The key tile that identifies the region
        /// </summary>
        public Vector2Int Key { get; }
        
        /// <summary>
        /// The number of cells in the region
        /// </summary>
        public int CellCount { get; }

        /// <summary>
        /// The data associated with the region
        /// </summary>
        public TData Data => _data;

        /// <summary>
        /// The list of links of the region
        /// </summary>
        public List<RegionLink> Links { get; }

        private TData _data;
        
        public Region(Vector2Int key, int cellCount, IRegionDataGenerator<TData> dataGenerator) : this(key, cellCount)
        {
            _data = dataGenerator.GenerateNew(this);
        }

        private Region(Vector2Int key, int cellCount)
        {
            Key = key;
            CellCount = cellCount;
            Links = new List<RegionLink>();
        }

        public static Region<TData> GetMergedRegion(
            Vector2Int regionKeyUnion,
            Region<TData> regionA,
            Region<TData> regionB,
            IRegionDataGenerator<TData> dataGenerator)
        {
            Region<TData> regionUnion = new Region<TData>(
                regionKeyUnion, 
                regionA.CellCount + regionB.CellCount
            );
            regionUnion._data = dataGenerator.GenerateOnMerge(regionA, regionB);
            
            return regionUnion;
        }

        public override string ToString()
        {
            return $"Region(Key: {Key}, Count: {CellCount}, nLinks: {Links.Count})\nData: {_data}";
        }
    }
}
