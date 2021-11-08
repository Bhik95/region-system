using UnityEngine;

namespace Bhik95.Utility.Grids
{
    /// <summary>
    /// This class represents a Union-Find data structure to store a collection of disjoints (non-overlapping) regions
    /// made of 2D tiles. See https://en.wikipedia.org/wiki/Disjoint-set_data_structure for information about Union-Fine.
    /// UnionFind2D provides operations for adding new tiles, merging tile regions (replacing them by their union), and
    /// finding a representative member of a region (regionKey). The last operation allows to find out efficiently if
    /// any two elements are in the same or different regions.
    /// </summary>
    public class UnionFind2D
    {
        // Value for a key that has not been set
        private static readonly Vector2Int NoKey = new Vector2Int(-1, -1);
        
        public Vector2Int GridSize => _keys.GridSize;
        
        private readonly BasicGridXZ<Vector2Int> _keys;
        
        public UnionFind2D(Vector2Int gridSize)
        {
            _keys = new BasicGridXZ<Vector2Int>(gridSize, NoKey);
        }
        
        /// <summary>
        /// Finds the key associated to the connected region.
        /// </summary>
        /// <param name="position">The position of a tile in the region.</param>
        /// <returns>The region key</returns>
        public Vector2Int FindKey(Vector2Int position)
        {
            if (position.x < 0 || position.y < 0 || position.x >= _keys.GridSize.x || position.y >= _keys.GridSize.y)
                return NoKey;
            
            var p = _keys[position];
            if (p.x == position.x && p.y == position.y)
                return p;
            
            var q = FindKey(p);
            _keys[position] = q; // path compression
            
            return q;
        }

        /// <summary>
        /// Given a position for each region, unite two regions together. For improved performance, set positionOfSmaller
        /// to the position of the smaller region and positionOfBigger to the position of the bigger region.
        /// </summary>
        /// <param name="positionOfSmaller">A position of the smaller region</param>
        /// <param name="positionOfBigger">A position of the bigger region</param>
        public void UniteKeys(Vector2Int positionOfSmaller, Vector2Int positionOfBigger)
        {
            Vector2Int p = FindKey(positionOfSmaller);
            Vector2Int q = FindKey(positionOfBigger);

            if (p.x == q.x && p.y == q.y)
                return;

            _keys[p.x, p.y] = q;
        }

        /// <summary>
        /// Find whether the given position is associated to a region key.
        /// </summary>
        /// <param name="position">The position</param>
        /// <returns>True, if the position is associated to a region key</returns>
        public bool HasKey(Vector2Int position)
        {
            return FindKey(position) != NoKey;
        }

        /// <summary>
        /// Set the given position as not part of any region.
        /// </summary>
        /// <param name="position">The position</param>
        public void ClearKey(Vector2Int position)
        {
            _keys[position] = NoKey;
        }

        /// <summary>
        /// Set the given position to be part of the region with key keyPosition.
        /// </summary>
        /// <param name="position">The position</param>
        /// <param name="keyPosition">The key associated to the region you want the input position to be in</param>
        public void SetKey(Vector2Int position, Vector2Int keyPosition)
        {
            _keys[position] = keyPosition;
        }
    }
}

