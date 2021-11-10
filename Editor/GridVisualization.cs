#if UNITY_EDITOR

using System;
using Bhik95.Grids;
using UnityEngine;

using UnityEditor;

namespace Bhik95.RegionSystem.Editor
{
    public static class GridVisualization
    {
        public static void ShowGridAsGizmosBool(IGridXZ<bool> gridXZ, GridXZCoordinateConverter coordinateConverter, Color trueColor, Color falseColor, float innerAlpha = 0.5f)
        {
            Vector3 cubeSize = new Vector3(coordinateConverter.CellSize.x, coordinateConverter.CellSize.y, 0.1f);
            
            for (int ix = 0; ix < gridXZ.GridSize.x; ix++)
            {
                for (int iy = 0; iy < gridXZ.GridSize.y; iy++)
                {
                    Vector2Int gridPosition = new Vector2Int(ix, iy);
                    Vector2 worldPosition = coordinateConverter.GridToWorld(gridPosition, Vector2.zero);
                    Gizmos.color = gridXZ[gridPosition]
                        ? trueColor
                        : falseColor;
                    Gizmos.DrawWireCube(worldPosition, cubeSize - 0.02f * Vector3.one);
                    Gizmos.color = Gizmos.color * innerAlpha;
                    Gizmos.DrawCube(worldPosition, cubeSize);
                }
            }
        }
        
        public static void ShowGridAsGizmosFloat(IGridXZ<float> gridXZ, GridXZCoordinateConverter coordinateConverter, Gradient gradient, float innerAlpha = 0.5f)
        {
            Vector3 cubeSize = new Vector3(coordinateConverter.CellSize.x, coordinateConverter.CellSize.y, 0.1f);
            
            for (int ix = 0; ix < gridXZ.GridSize.x; ix++)
            {
                for (int iy = 0; iy < gridXZ.GridSize.y; iy++)
                {
                    Vector2Int gridPosition = new Vector2Int(ix, iy);
                    Vector2 worldPosition = coordinateConverter.GridToWorld(gridPosition, Vector2.zero);
                    Gizmos.color = gradient.Evaluate(gridXZ[gridPosition]);
                    Gizmos.DrawWireCube(worldPosition, cubeSize - 0.02f * Vector3.one);
                    Gizmos.color = Gizmos.color * innerAlpha;
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

#endif