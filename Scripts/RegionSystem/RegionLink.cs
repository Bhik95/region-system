using UnityEngine;

namespace Bhik95.RegionSystem
{
    /// <summary>
    /// This class represents a link between two adjacent regions of the world.
    /// </summary>
    public class RegionLink
    {
        public readonly Vector2Int Root;
        public readonly RegionLinkDirection Direction;
        public readonly int Length;

        public RegionLink(Vector2Int root, RegionLinkDirection direction, int length)
        {
            Root = root;
            Direction = direction;
            Length = length;
        }

        public override string ToString()
        {
            return $"RegionLink(Root: {Root}, Direction: {Direction}, L: {Length})";
        }

        public override int GetHashCode()
        {
            // [direction - 8 bit, length - 8 bit, root x - 8 bit, root y - 8bit]
            int hash = 0;
            hash = (int)Direction << 24;
            hash |= (Length & 255) << 16;
            hash |= (Root.x & 255) << 8;
            hash |= (Root.y & 255);

            return hash;
        }
    }
}
