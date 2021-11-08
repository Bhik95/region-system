using System;
using System.Collections.Generic;
using Bhik95.Utility.Grids;
using UnityEngine;

namespace Bhik95.Utility.Graph
{
    public enum ExpansionType
    {
        FourDirections,
        EightDirections
    }
    
    /// <summary>
    /// This class represents the graph representation of a regular quad grid.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GridGraphXZ<T> : IDirectedGraph<Vector2Int, float>
    {

        private readonly BasicGridXZ<T> _gridXZ;
        private readonly ExpansionType _expansionType;

        public GridGraphXZ(BasicGridXZ<T> gridXZ, ExpansionType expansionType)
        {
            _gridXZ = gridXZ ?? throw new ArgumentNullException(nameof(gridXZ));
            _expansionType = expansionType;
        }

        public bool TryGetEdge(Vector2Int regionKeySource, Vector2Int regionKeyDestination, out float value)
        {
            if (ContainsEdge(regionKeySource, regionKeyDestination))
            {
                value = 1;
                return true;
            }
            value = float.NaN;
            return false;
        }

        public bool ContainsNode(Vector2Int regionKey) => _gridXZ.IsWithinBounds(regionKey);

        public bool ContainsEdge(Vector2Int source, Vector2Int destination)
        {
            Vector2Int delta = destination - source;
            switch (_expansionType)
            {
                case ExpansionType.EightDirections:
                    if (Mathf.Abs(delta.x) <= 1 && Mathf.Abs(delta.y) <= 1)
                        return true;
                    break;
                case ExpansionType.FourDirections:
                    if (Mathf.Abs(delta.x) + Mathf.Abs(delta.y) <= 1)
                        return true;
                    break;
            }

            return false;
        }

        public IEnumerable<Vector2Int> Nodes
        {
            get
            {
                for (int ix = 0; ix < _gridXZ.GridSize.x; ix++)
                {
                    for (int iz = 0; iz < _gridXZ.GridSize.y; iz++)
                    {
                        yield return new Vector2Int(ix, iz);
                    }
                }
            }
        }

        public IEnumerable<Edge<Vector2Int, float>> OutgoingEdges(Vector2Int source)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dz = -1; dz <= 1; dz++)
                {
                    if(dx == 0 && dz == 0)
                        continue;

                    switch (_expansionType)
                    {
                        case ExpansionType.EightDirections:
                            break;
                        // 4 Directions -> Discard the appropriate directions
                        case ExpansionType.FourDirections:
                            if(dx != 0 && dz != 0)
                                continue;
                            break;
                    }

                    Vector2Int destination = new Vector2Int(source.x + dx, source.y + dz);
                    if (_gridXZ.IsWithinBounds(destination))
                        yield return new Edge<Vector2Int, float>(source, destination, 1.0f);
                }
            }
        }
    }
}

