using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bhik95.Grids
{
    public class BasicGridXZ<T> : IGridXZ<T>
    {
        private readonly T[,] _grid;
        public Vector2Int GridSize => new Vector2Int(_grid.GetLength(0), _grid.GetLength(1));

        public BasicGridXZ(Vector2Int gridSize, T defaultValue = default)
        {
            if (gridSize.x <= 0 || gridSize.y <= 0)
                throw new ArgumentException($"{nameof(gridSize)} is expected to have both non-negative components. Actual value: {gridSize}");
            
            _grid = new T[gridSize.x, gridSize.y];

            if (!EqualityComparer<T>.Default.Equals(defaultValue, default(T)))
            {
                for (int ix = 0; ix < _grid.GetLength(0); ix++)
                {
                    for (int iy = 0; iy < _grid.GetLength(1); iy++)
                    {
                        _grid[ix, iy] = defaultValue;
                    }
                }
            }
        }

        public bool IsWithinBounds(Vector2Int gridPosition)
            => IsWithinBounds(gridPosition.x, gridPosition.y);

        public bool IsWithinBounds(int gridPositionX, int gridPositionZ)
        {
            return gridPositionX >= 0
                   && gridPositionZ >= 0
                   && gridPositionX < _grid.GetLength(0)
                   && gridPositionZ < _grid.GetLength(1);
        }

        public void Set(Vector2Int gridPosition, T value)
        {
            _grid[gridPosition.x, gridPosition.y] = value;
        }
        
        public void Set(int gridPositionX, int gridPositionZ, T value)
        {
            _grid[gridPositionX, gridPositionZ] = value;
        }

        public T Get(Vector2Int gridPosition)
        {
            return _grid[gridPosition.x, gridPosition.y];
        }
        
        public T Get(int gridPositionX, int gridPositionZ)
        {
            return _grid[gridPositionX, gridPositionZ];
        }

        public T this[Vector2Int key]
        {
            get => _grid[key.x, key.y];
            set => _grid[key.x, key.y] = value;
        }
        
        public T this[int keyX, int keyZ]
        {
            get => _grid[keyX, keyZ];
            set => _grid[keyX, keyZ] = value;
        }
        
    }
}
