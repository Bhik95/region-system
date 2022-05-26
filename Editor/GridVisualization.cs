#if UNITY_EDITOR

using System;
using Bhik95.Grids;
using UnityEngine;

using UnityEditor;
using Random = UnityEngine.Random;

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
                    Vector2 worldPosition = coordinateConverter.GridCenterToWorld(gridPosition);
                    Gizmos.color = gridXZ[gridPosition]
                        ? trueColor
                        : falseColor;
                    Gizmos.DrawWireCube(worldPosition, cubeSize - 0.02f * Vector3.one);
                    Gizmos.color *= innerAlpha;
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
                    Vector2 worldPosition = coordinateConverter.GridCenterToWorld(gridPosition);
                    Gizmos.color = gradient.Evaluate(gridXZ[gridPosition]);
                    Gizmos.DrawWireCube(worldPosition, cubeSize - 0.02f * Vector3.one);
                    Gizmos.color *= innerAlpha;
                    Gizmos.DrawCube(worldPosition, cubeSize);
                }
            }
        }
        
        public static void ShowGridAsGizmosEnum<T>(IGridXZ<T> gridXZ, GridXZCoordinateConverter coordinateConverter, Color[] colors, float innerAlpha = 0.5f) where T : Enum
        {
            Vector3 cubeSize = new Vector3(coordinateConverter.CellSize.x, coordinateConverter.CellSize.y, 0.1f);
            
            for (int ix = 0; ix < gridXZ.GridSize.x; ix++)
            {
                for (int iy = 0; iy < gridXZ.GridSize.y; iy++)
                {
                    Vector2Int gridPosition = new Vector2Int(ix, iy);
                    Vector2 worldPosition = coordinateConverter.GridCenterToWorld(gridPosition);
                    Gizmos.color = colors[(int)(object)gridXZ[gridPosition]];// Get the Enum Index
                    Gizmos.DrawWireCube(worldPosition, cubeSize - 0.02f * Vector3.one);
                    Gizmos.color *= innerAlpha;
                    Gizmos.DrawCube(worldPosition, cubeSize);
                }
            }
        }

        public static void ShowRegionSystemOcclusion<T>(RegionSystem<T> regionSystem, GridXZCoordinateConverter coordinateConverter, Color trueColor, Color falseColor, float innerAlpha = 0.5f)
        {
            Vector3 cubeSize = new Vector3(coordinateConverter.CellSize.x, coordinateConverter.CellSize.y, 0.1f);
            
            for (int ix = 0; ix < regionSystem.GridSize.x; ix++)
            {
                for (int iy = 0; iy < regionSystem.GridSize.y; iy++)
                {
                    Vector2Int gridPosition = new Vector2Int(ix, iy);
                    Vector2 worldPosition = coordinateConverter.GridCenterToWorld(gridPosition);
                    Gizmos.color = regionSystem.GetOcclusionValue(gridPosition)
                        ? trueColor
                        : falseColor;
                    Gizmos.DrawWireCube(worldPosition, cubeSize - 0.02f * Vector3.one);
                    Gizmos.color *= innerAlpha;
                    Gizmos.DrawCube(worldPosition, cubeSize);
                }
            }
        }
        
        public static void ShowRegions<T>(RegionSystem<T> regionSystem, GridXZCoordinateConverter coordinateConverter, float innerAlpha = 0.5f)
        {
            Random.State initialRandomState = Random.state;
            
            Vector3 cubeSize = new Vector3(coordinateConverter.CellSize.x, coordinateConverter.CellSize.y, 0.1f);
            
            for (int ix = 0; ix < regionSystem.GridSize.x; ix++)
            {
                for (int iy = 0; iy < regionSystem.GridSize.y; iy++)
                {
                    Vector2Int gridPosition = new Vector2Int(ix, iy);
                    Vector2 worldPosition = coordinateConverter.GridCenterToWorld(gridPosition);
                    if (regionSystem.RegionKeys.HasKey(gridPosition))
                    {
                        Vector2Int regionKey = regionSystem.RegionKeys.FindKey(gridPosition);
                        Random.InitState(regionKey.GetHashCode());
                        Gizmos.color = Color.HSVToRGB(Random.value, 0.8f, 1.0f);
                    }
                    else
                    {
                        Gizmos.color = Color.black;
                    }
                    Gizmos.DrawWireCube(worldPosition, cubeSize - 0.02f * Vector3.one);
                    Gizmos.color *= innerAlpha;
                    Gizmos.DrawCube(worldPosition, cubeSize);
                }
            }

            Random.state = initialRandomState;
        }
    }
}

#endif