using System;
using Bhik95.Grids;
using UnityEngine;
using UnityEditor;

namespace Bhik95.RegionSystem.Editor
{
    public static class GridVisualization
    {
        public static void ShowGridAsGizmosBool(IGridXZ<bool> gridXZ, GridXZCoordinateConverter coordinateConverter, Color trueColor, Color falseColor)
        {
            Vector3 cubeSize = new Vector3(coordinateConverter.CellSize.x, 0.1f, coordinateConverter.CellSize.y);
            
            for (int ix = 0; ix < gridXZ.GridSize.x; ix++)
            {
                for (int iy = 0; iy < gridXZ.GridSize.y; iy++)
                {
                    Vector2Int gridPosition = new Vector2Int(ix, iy);
                    Vector2 worldPosition = coordinateConverter.GridToWorld(gridPosition, Vector2.zero);
                    Gizmos.color = gridXZ[gridPosition]
                        ? trueColor
                        : falseColor;
                    Gizmos.DrawCube(worldPosition, cubeSize);
                }
            }
        }
        
        public static void ShowGridAsGizmosFloat(IGridXZ<float> gridXZ, GridXZCoordinateConverter coordinateConverter, Gradient gradient)
        {
            Vector3 cubeSize = new Vector3(coordinateConverter.CellSize.x, 0.1f, coordinateConverter.CellSize.y);
            
            for (int ix = 0; ix < gridXZ.GridSize.x; ix++)
            {
                for (int iy = 0; iy < gridXZ.GridSize.y; iy++)
                {
                    Vector2Int gridPosition = new Vector2Int(ix, iy);
                    Vector2 worldPosition = coordinateConverter.GridToWorld(gridPosition, Vector2.zero);
                    Gizmos.color = gradient.Evaluate(gridXZ[gridPosition]);
                    Gizmos.DrawCube(worldPosition, cubeSize);
                }
            }
        }
        
        public static void ShowGridAsGizmosEnum<T>(IGridXZ<T> gridXZ, GridXZCoordinateConverter coordinateConverter) where T : Enum
        {
            for (int ix = 0; ix < gridXZ.GridSize.x; ix++)
            {
                for (int iy = 0; iy < gridXZ.GridSize.y; iy++)
                {
                    Vector2Int gridPosition = new Vector2Int(ix, iy);
                    Vector2 worldPosition = coordinateConverter.GridToWorld(gridPosition, Vector2.zero);
                    Gizmos.color = Color.white;
                    Handles.Label(worldPosition, gridXZ[gridPosition].ToString());
                }
            }
        }
    }
}



