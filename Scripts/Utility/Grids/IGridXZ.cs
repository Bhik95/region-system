using UnityEngine;

namespace Bhik95.Utility.Grids
{
    public interface IGridXZ<T>
    {
        public Vector2Int GridSize { get; }
        public bool IsWithinBounds(int gridPositionX, int gridPositionZ);
        public bool IsWithinBounds(Vector2Int gridPosition);
        
        public T Get(Vector2Int gridPosition);
        public T Get(int gridPositionX, int gridPositionZ);
        public void Set(Vector2Int gridPosition, T value);
        public void Set(int gridPositionX, int gridPositionZ, T value);
        public T this[Vector2Int key] { get; set; }
    }
}

