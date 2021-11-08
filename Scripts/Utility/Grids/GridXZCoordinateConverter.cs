using System;
using UnityEngine;

namespace Bhik95.Utility.Grids
{
    /// <summary>
    /// Utility class used to quickly and repeatedly convert beetween an IGridXZ grid and world coordinates
    /// </summary>
    public class GridXZCoordinateConverter
    {
        private readonly Vector2 _lowerLeftCorner;
        private readonly Vector2Int _gridSize;
        private readonly Vector2 _cellSize;

        /// <summary>
        /// The world-space coordinates of the point corresponding to the lower-left corner of the grid
        /// </summary>
        public Vector2 LowerLeftCorner => _lowerLeftCorner;
        
        /// <summary>
        /// The world size of each cell of the input grids
        /// </summary>
        public Vector2 CellSize => _cellSize;
        
        public GridXZCoordinateConverter(Vector2 lowerLeftCorner, Vector2Int gridSize, Vector2 cellSize)
        {
            _lowerLeftCorner = lowerLeftCorner;
            _gridSize = gridSize;
            _cellSize = cellSize;
        }
        
        /// <summary>
        /// Returns the world-space coordinates of the point corresponding to the top-right corner of the grid
        /// </summary>
        /// <returns>The world-space coordinates of the point corresponding to the top-right corner of the grid</returns>
        public Vector2 GetTopRightCorner() => _lowerLeftCorner + _cellSize * _gridSize;
        
        /// <summary>
        /// Returns whether the given world position is within the bounds of the grid
        /// </summary>
        /// <param name="worldPosition">The world position</param>
        /// <returns>True if the given world position is within bounds</returns>
        public bool IsWorldPosWithinBounds(Vector2 worldPosition)
            => IsWorldPosWithinBounds(worldPosition.x, worldPosition.y);
            
        /// <summary>
        /// Returns whether the given world position is within the bounds of the grid
        /// </summary>
        /// <param name="worldPositionX">The x coordinate of the world position</param>
        /// <param name="worldPositionZ">The z coordinate of the world position</param>
        /// <returns>True if the given world position is within bounds</returns>
        public bool IsWorldPosWithinBounds(float worldPositionX, float worldPositionZ)
        {
            Vector2 topRightCorner = GetTopRightCorner();
            return worldPositionX >= _lowerLeftCorner.x 
                   && worldPositionZ >= _lowerLeftCorner.y
                   && worldPositionX <= topRightCorner.x
                   && worldPositionZ <= topRightCorner.y;
        }
        
        /// <summary>
        /// Converts a position in world-space into a position in grid-space
        /// </summary>
        /// <param name="worldPosition">The world position</param>
        /// <returns>The grid position</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public Vector2Int WorldToGrid(Vector2 worldPosition)
        {
            if (!IsWorldPosWithinBounds(worldPosition))
                throw new ArgumentOutOfRangeException($"Parameter {nameof(worldPosition)} is out of range: {worldPosition}");
                
            return Vector2Int.FloorToInt((worldPosition - _lowerLeftCorner) / _cellSize);
        }
            
        /// <summary>
        /// Converts a position in world-space into a position in grid-space
        /// </summary>
        /// <param name="worldPositionX">The x coordinate of the world position</param>
        /// <param name="worldPositionZ">The z coordinate of the world position</param>
        /// <returns>The grid position</returns>
        public Vector2Int WorldToGrid(float worldPositionX, float worldPositionZ)
            => WorldToGrid(new Vector2(worldPositionX, worldPositionZ));

        /// <summary>
        /// Converts a position in grid-space into a position in world-space
        /// </summary>
        /// <param name="gridPosition">The grid position</param>
        /// <param name="cellPosition">A value in [0;1[x[0;1[ representing the relative position of a point within the
        /// selected cell. For example, (0f,0f) represents the lower-left corner, (0.5f, 0.5f) represents the center of
        /// the selected cell, while (1.0f, 1.0f) represents the top-right corner.</param>
        /// <returns>The world position</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public Vector2 GridToWorld(Vector2Int gridPosition, Vector2 cellPosition)
        {
            if (gridPosition.x < 0 || gridPosition.x >= _gridSize.x || gridPosition.y < 0 || gridPosition.y >= _gridSize.y)
                throw new ArgumentOutOfRangeException(nameof(gridPosition));
            if (cellPosition.x < 0f || cellPosition.x > 1f || cellPosition.y < 0 || cellPosition.y > 1f)
                throw new ArgumentOutOfRangeException($"Parameter {nameof(cellPosition)} is expected in [0;1]x[0;1]. Actual value: {cellPosition}");
            
            return _lowerLeftCorner + (gridPosition + cellPosition) * _cellSize;
        }

        /// <summary>
        /// Converts a position in grid-space into a position in world-space
        /// </summary>
        /// <param name="gridPositionX">The x coordinate of the position in grid-space</param>
        /// <param name="gridPositionZ">The z coordinate of the position in grid-space</param>
        /// <param name="cellPositionX">A value in [0;1[ representing the relative x position of a point within the
        /// selected cell.</param>
        /// <param name="cellPositionZ">A value in [0;1[ representing the relative z position of a point within the
        /// selected cell.</param>
        /// <returns>The world position</returns>
        public Vector2 GridToWorld(int gridPositionX, int gridPositionZ, float cellPositionX, float cellPositionZ)
            => GridToWorld(
                new Vector2Int(gridPositionX, gridPositionZ),
                new Vector2(cellPositionX, cellPositionZ)
                );

        /// <summary>
        /// Returns the world position of the center of the cell at the given grid position
        /// </summary>
        /// <param name="gridPosition">The grid position</param>
        /// <returns>The world position of the center of the cell at the given grid position</returns>
        public Vector2 GridCenterToWorld(Vector2Int gridPosition)
            => GridToWorld(gridPosition, new Vector2(0.5f, 0.5f));

        /// <summary>
        /// Returns the world position of the center of the cell at the given grid position
        /// </summary>
        /// <param name="gridPositionX">The x coordinate of the grid position</param>
        /// <param name="gridPositionZ">The z coordinate of the grid position</param>
        /// <returns>The world position of the center of the cell at the given grid position</returns>
        public Vector2 GridCenterToWorld(int gridPositionX, int gridPositionZ)
            => GridCenterToWorld(new Vector2Int(gridPositionX, gridPositionZ));
        
        
        /// <summary>
        /// Set a value in the cell of the grid associated to the given world position
        /// </summary>
        /// <param name="grid">The input grid</param>
        /// <param name="worldPosition">The world position</param>
        /// <param name="value">The value to assign</param>
        /// <typeparam name="T">The type of the value</typeparam>
        public void SetInWorldPos<T>(IGridXZ<T> grid, Vector2 worldPosition, T value)
        {
            SetInWorldPos(grid, worldPosition.x, worldPosition.y, value);
        }
        
        /// <summary>
        /// Set a value in the cell of the grid associated to the given world position
        /// </summary>
        /// <param name="grid">The input grid</param>
        /// <param name="worldPositionX">The x coordinate of the world position</param>
        /// <param name="worldPositionZ">The z coordinate of the world position</param>
        /// <param name="value">The value to assign</param>
        /// <typeparam name="T">The type of the value</typeparam>
        public void SetInWorldPos<T>(IGridXZ<T> grid, float worldPositionX, float worldPositionZ, T value)
        {
            grid.Set(WorldToGrid(worldPositionX, worldPositionZ), value);
        }
        
        /// <summary>
        /// Get the value of the cell of the grid associated to the given world position
        /// </summary>
        /// <param name="grid">The input grid</param>
        /// <param name="worldPosition">The world position</param>
        /// <typeparam name="T">The type of the value</typeparam>
        /// <returns>The value</returns>
        public T GetInWorldPos<T>(IGridXZ<T> grid, Vector2 worldPosition)
        {
            return GetInWorldPos(grid, worldPosition.x, worldPosition.y);
        }
            
        /// <summary>
        /// Get the value of the cell of the grid associated to the given world position
        /// </summary>
        /// <param name="grid">The input grid</param>
        /// <param name="worldPositionX">The x coordinate of the world position</param>
        /// <param name="worldPositionZ">The z coordinate of the world position</param>
        /// <typeparam name="T">The type of the value</typeparam>
        /// <returns>The value</returns>
        public T GetInWorldPos<T>(IGridXZ<T> grid, float worldPositionX, float worldPositionZ)
        {
            return grid.Get(WorldToGrid(worldPositionX, worldPositionZ));
        }
    }
}

