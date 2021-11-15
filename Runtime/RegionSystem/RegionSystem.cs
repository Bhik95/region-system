using System;
using System.Collections.Generic;
using System.Linq;
using Bhik95.Graph;
using Bhik95.Grids;
using UnityEngine;

namespace Bhik95.RegionSystem
{
    /// <summary>
    /// This class represents the RegionSystem. The world grid is partitioned into rectangular chunks of a given size.
    /// Each chunk might contain multiple regions. Each tile in a region is reachable from any other tile in the same
    /// region. The region system keeps track of all the regions and their links to adjacent regions).
    /// </summary>
    /// <typeparam name="TRegionData"></typeparam>
    public class RegionSystem<TRegionData>
    {
        public Vector2Int GridSize => _gridSize;
        public LinkToRegionsDatabase<TRegionData> LinkToRegionsDatabase => _linkToRegionsDatabase;
        public UnionFind2D RegionKeys => _regionKeys;
        public Dictionary<Vector2Int, Region<TRegionData>> RegionDict => _regionDict;
        public BasicGridXZ<HashSet<Vector2Int>> RegionsInChunks => _regionsInChunks;
        
        private readonly Vector2Int _gridSize;
        private readonly Vector2Int _chunkSize;
        private readonly Vector2Int _nChunks;
        
        private readonly BasicGridXZ<bool> _occlusionGrid;
        
        private readonly UnionFind2D _regionKeys;
        private readonly Dictionary<Vector2Int, Region<TRegionData>> _regionDict;
        private readonly BasicGridXZ<HashSet<Vector2Int>> _regionsInChunks;

        private readonly LinkToRegionsDatabase<TRegionData> _linkToRegionsDatabase;
        
        private readonly HashSet<Vector2Int> _regionsWithLinksToRecalculate = new HashSet<Vector2Int>();

        private readonly GridGraphXZ<bool> _gridGraph;
        private readonly RegionGraph<TRegionData> _regionGraph;

        private readonly IRegionDataGenerator<TRegionData> _regionDataGenerator;

        public RegionSystem(BasicGridXZ<bool> occlusionGrid, Vector2Int chunkSize, IRegionDataGenerator<TRegionData> regionDataGenerator)
        {
            _occlusionGrid = occlusionGrid;
            _gridSize = occlusionGrid.GridSize;
            
            if (_gridSize.x <= 0 || _gridSize.y <= 0)
                throw new ArgumentOutOfRangeException(nameof(_gridSize));
            if (chunkSize.x <= 0 || chunkSize.y <= 0)
                throw new ArgumentOutOfRangeException(nameof(chunkSize));
            if (_gridSize.x % chunkSize.x != 0 || _gridSize.y % chunkSize.y != 0)
                throw new ArgumentException($"{nameof(_gridSize)} should be multiple of {nameof(chunkSize)}");
            
            _chunkSize = chunkSize;
            _regionDataGenerator = regionDataGenerator ?? throw new ArgumentNullException(nameof(regionDataGenerator));
            _nChunks = new Vector2Int(_gridSize.x / _chunkSize.x, _gridSize.y / _chunkSize.y);
            
            _gridGraph = new GridGraphXZ<bool>(_occlusionGrid, ExpansionType.FourDirections);
            _regionKeys = new UnionFind2D(_gridSize);
            _regionDict = new Dictionary<Vector2Int, Region<TRegionData>>();
            _linkToRegionsDatabase = new LinkToRegionsDatabase<TRegionData>();
            _regionGraph = new RegionGraph<TRegionData>(_regionDict, _linkToRegionsDatabase, this);

            Vector2Int nChunks = new Vector2Int(_gridSize.x / _chunkSize.x, _gridSize.y / _chunkSize.y);
            _regionsInChunks = new BasicGridXZ<HashSet<Vector2Int>>(nChunks);

            InitializeRegions();
        }

        public RegionSystem(Vector2Int gridSize, Vector2Int chunkSize, IRegionDataGenerator<TRegionData> regionDataGenerator)          
            : this(new BasicGridXZ<bool>(gridSize), chunkSize, regionDataGenerator)
        {
            
        }
        
        public bool ArePositionsConnected(Vector2Int positionA, Vector2Int positionB)
        {
            if (ArePositionsInTheSameRegion(positionA, positionB))
                return true;

            Vector2Int regionKeyA = _regionKeys.FindKey(positionA);
            Vector2Int regionKeyB = _regionKeys.FindKey(positionB);
            
            return GraphOperations.AStarSearch(_regionGraph,
                regionKeyA,
                destination => destination == regionKeyB,
                edge => edge.Data,
                edge => 0,
                edge => true,
                out _);
        }

        public bool ArePositionsInTheSameRegion(Vector2Int gridPositionA, Vector2Int gridPositionB)
        {
            return _regionKeys.FindKey(gridPositionA) ==
                   _regionKeys.FindKey(gridPositionB);
        }

        public void ForceRecalculateLinksInRegion(Vector2Int regionKey)
        {
            Debug.Log($"Forced link recalculation in {regionKey}");
            _regionsWithLinksToRecalculate.Add(regionKey);
            RecalculateLinksInPendingRegions();
        }
        
        public void SetOcclusion(Vector2Int gridPosition, bool value)
        {
            if (_occlusionGrid[gridPosition] != value)
            {
                if (value)
                {
                    // Add block
                    _occlusionGrid[gridPosition] = true;
                    
                    Vector2Int chunkId = GridToChunkId(gridPosition);
                    RecalculateRegionsInChunk(chunkId);
                    
                    // The regions around gridPosition might be in another chunk and might connect to the new regions
                    // in this chunk, so we need to make sure their links are updated
                    _regionsWithLinksToRecalculate.Add(_regionKeys.FindKey(gridPosition + Vector2Int.up));
                    _regionsWithLinksToRecalculate.Add(_regionKeys.FindKey(gridPosition + Vector2Int.right));
                    _regionsWithLinksToRecalculate.Add(_regionKeys.FindKey(gridPosition + Vector2Int.down));
                    _regionsWithLinksToRecalculate.Add(_regionKeys.FindKey(gridPosition + Vector2Int.left));
                    
                    RecalculateLinksInPendingRegions();
                }
                else
                {
                    // Remove block
                    _occlusionGrid[gridPosition] = false;
                    JoinRegionsAroundCell(gridPosition);
                }
            }
        }

        public bool GetOcclusionValue(Vector2Int gridPosition)
        {
            return _occlusionGrid[gridPosition];
        }
        
        private void InitializeRegions()
        {
            for (int ix = 0; ix < _nChunks.x; ix++)
            {
                for (int iy = 0; iy < _nChunks.y; iy++)
                {
                    _regionsInChunks[ix, iy] = new HashSet<Vector2Int>();
                }
            }
        
            //(chX, chY) = chunk id
            for (int chX = 0; chX < _nChunks.x; chX++)
            {
                for (int chY = 0; chY < _nChunks.y; chY++)
                {
                    RecalculateRegionsInChunk(new Vector2Int(chX, chY));
                }
            }

            RecalculateLinksInPendingRegions();
        }
        
        private void RecalculateRegionsInChunk(Vector2Int chunkId)
        {
            ClearRegionsInChunk(chunkId);
            // (ciX, ciY) = chunk coordinates
            for (int ciX = 0; ciX < _chunkSize.x; ciX++)
            {
                for (int ciY = 0; ciY < _chunkSize.y; ciY++)
                {
                    //To Grid coordinates
                    int ix = chunkId.x * _chunkSize.x + ciX;
                    int iy = chunkId.y * _chunkSize.y + ciY;

                    Vector2Int regionKey = new Vector2Int(ix, iy);
                
                    if (_occlusionGrid[ix, iy] || _regionKeys.HasKey(regionKey))
                        continue;
                
                    CreateRegionFromPosition(regionKey, chunkId);
                }
            }
            
            // Update neighbouring chunks as well :)
            if (chunkId.x > 0)
                _regionsWithLinksToRecalculate.UnionWith(_regionsInChunks[chunkId + Vector2Int.left]);
            if(chunkId.x < _nChunks.x - 1)
                _regionsWithLinksToRecalculate.UnionWith(_regionsInChunks[chunkId + Vector2Int.right]);
            if(chunkId.y > 0)
                _regionsWithLinksToRecalculate.UnionWith(_regionsInChunks[chunkId + Vector2Int.down]);
            if(chunkId.y < _nChunks.y - 1)
                _regionsWithLinksToRecalculate.UnionWith(_regionsInChunks[chunkId + Vector2Int.up]);
        }
        
        private void ClearRegionsInChunk(Vector2Int chunkId)
        {
            Vector2Int[] regionsToRemove = _regionsInChunks[chunkId.x, chunkId.y].ToArray();
            foreach (Vector2Int regionKey in regionsToRemove)
            {
                RemoveRegion(regionKey);
            }
            
            //TODO: Extract
            for (int ciX = 0; ciX < _chunkSize.x; ciX++)
            {
                for (int ciY = 0; ciY < _chunkSize.y; ciY++)
                {
                    //To Grid coordinates
                    int ix = chunkId.x * _chunkSize.x + ciX;
                    int iy = chunkId.y * _chunkSize.y + ciY;
                
                    _regionKeys.ClearKey(new Vector2Int(ix, iy));
                }
            }
        }
        
        private void CreateRegionFromPosition(Vector2Int keyPosition, Vector2Int chunkId)
        {
            int count = 0;

            Vector2Int chunkLowerLeftTile = _chunkSize * chunkId;
            //Expand within the limits of the chunk
            Vector2Int[] regionPositions = GraphOperations.Expand(
                _gridGraph,
                keyPosition,
                edge =>
                    !_occlusionGrid[edge.Destination]
                    && IsVectorInRange(edge.Destination, chunkLowerLeftTile,
                        chunkLowerLeftTile + (_chunkSize - Vector2Int.one)),
                includeSource: true).ToArray();
            foreach (var position in regionPositions)
            {
                _regionKeys.SetKey(position, keyPosition);
                count++;
            }

            RegisterRegion(new Region<TRegionData>(keyPosition, count, _regionDataGenerator), chunkId);
            _regionsWithLinksToRecalculate.Add(keyPosition);
        }
        
        private void RegisterRegion(Region<TRegionData> region, Vector2Int chunkId)
        {
            _regionDict[region.Key] = region;
            _regionsInChunks[chunkId.x, chunkId.y].Add(region.Key);
        }
        
        private void RemoveRegion(Vector2Int regionKey)
        {
            _regionsWithLinksToRecalculate.UnionWith(_linkToRegionsDatabase.GetAdjacentRegionKeys(regionKey, this));
        
            _linkToRegionsDatabase.RemoveLinksAssociatedWithRegion(regionKey, this);

            _regionsInChunks[GridToChunkId(regionKey)].Remove(regionKey);
            _regionDict.Remove(regionKey);
        }

        private void JoinRegions(Vector2Int gridPositionA, Vector2Int gridPositionB)
        {
            Vector2Int regionKeyA = _regionKeys.FindKey(gridPositionA);
            Vector2Int regionKeyB = _regionKeys.FindKey(gridPositionB);
        
            if (regionKeyA == regionKeyB) // If the positions are in the same region, do nothing
                return;

            Region<TRegionData> regionA = _regionDict[regionKeyA];
            Region<TRegionData> regionB = _regionDict[regionKeyB];

            // The neighbours of regionA need to recalculate the links
            foreach (Vector2Int adjacentRegionKey in _linkToRegionsDatabase.GetAdjacentRegionKeys(regionKeyA, this))
            {
                _regionsWithLinksToRecalculate.Add(adjacentRegionKey);
            }
            // The neighbours of regionB need to recalculate the links
            foreach (Vector2Int adjacentRegionKey in _linkToRegionsDatabase.GetAdjacentRegionKeys(regionKeyB, this))
            {
                _regionsWithLinksToRecalculate.Add(adjacentRegionKey);
            }

            RemoveRegion(regionKeyA);
            RemoveRegion(regionKeyB);

            Vector2Int regionKeyUnion;
            if (regionA.CellCount <= regionB.CellCount)
            {
                regionKeyUnion = regionKeyB;
                _regionKeys.UniteKeys(regionKeyA, regionKeyB);
            }
            else
            {
                regionKeyUnion = regionKeyA;
                _regionKeys.UniteKeys(regionKeyB, regionKeyA);
            }


            Region<TRegionData> regionUnion = Region<TRegionData>.GetMergedRegion(
                regionKeyUnion,
                regionA,
                regionB,
                _regionDataGenerator);
            RegisterRegion(regionUnion, GridToChunkId(regionKeyUnion));
        
            _regionsWithLinksToRecalculate.Add(regionKeyUnion);
        }

        private void RecalculateLinksInPendingRegions()
        {
            foreach (Vector2Int regionKey in _regionsWithLinksToRecalculate)
            {
                if(_regionDict.ContainsKey(regionKey))
                    RecalculateLinksInRegion(regionKey);
            }
            _regionsWithLinksToRecalculate.Clear();
        }
        
        private void RecalculateLinksInRegion(Vector2Int regionKey)
        {
            _linkToRegionsDatabase.RemoveLinksAssociatedWithRegion(regionKey, this);
        
            List<RegionLink> links = new List<RegionLink>();
        
            Vector2Int chunkId = GridToChunkId(regionKey);
            Vector2Int chunkLowerLeftCorner = chunkId * _chunkSize;

            // Lower links
            if (chunkLowerLeftCorner.y > 0)
                links.AddRange(CalculateLink(chunkLowerLeftCorner, RegionLinkDirection.Right, regionKey, Vector2Int.down, Vector2Int.zero));
            // Upper links
            Vector2Int otherLowerLeftCorner = chunkLowerLeftCorner + (_chunkSize.y - 1) * Vector2Int.up;
            if (otherLowerLeftCorner.y < _gridSize.y - 1)
                links.AddRange(CalculateLink(otherLowerLeftCorner, RegionLinkDirection.Right, regionKey, Vector2Int.up, Vector2Int.up));
            // Left links
            if (chunkLowerLeftCorner.x > 0)
                links.AddRange(CalculateLink(chunkLowerLeftCorner, RegionLinkDirection.Up, regionKey, Vector2Int.left, Vector2Int.zero));
            // Right links
            otherLowerLeftCorner = chunkLowerLeftCorner + (_chunkSize.x - 1) * Vector2Int.right;
            if (otherLowerLeftCorner.x < _gridSize.x - 1)
                links.AddRange(CalculateLink(otherLowerLeftCorner, RegionLinkDirection.Up, regionKey, Vector2Int.right, Vector2Int.right));
        
            _regionDict[regionKey].Links.Clear();
            _regionDict[regionKey].Links.AddRange(links);
        
            for (int i = 0; i < links.Count; i++)
                _linkToRegionsDatabase.AddLinkAssociatedWithRegion(links[i], regionKey);
        }
        
        private IEnumerable<RegionLink> CalculateLink(Vector2Int startingCorner, RegionLinkDirection direction, Vector2Int regionKeyPosition, Vector2Int checkDirection, Vector2Int offset)
        {
            Vector2Int deltaDir = direction == RegionLinkDirection.Right ? Vector2Int.right : Vector2Int.up;
            int chunkSizeComponent = direction == RegionLinkDirection.Right ? _chunkSize.x : _chunkSize.y;
            for (int delta = 0; delta < chunkSizeComponent; delta++)
            {
                Vector2Int linkLowerLeftCorner = startingCorner + delta * deltaDir;

                Vector2Int currentCell = linkLowerLeftCorner;
                Vector2Int otherCell = currentCell + checkDirection;
            
                //If the current cell is obstructed or it's not in the region
                if (_occlusionGrid[linkLowerLeftCorner] || _regionKeys.FindKey(linkLowerLeftCorner) != regionKeyPosition)
                    continue;
                if (_occlusionGrid[otherCell])
                    continue;

                Vector2Int otherCellRegionKey = _regionKeys.FindKey(otherCell);

                int linkLength = 0;
            
                while (
                    otherCellRegionKey == _regionKeys.FindKey(otherCell)
                    && regionKeyPosition == _regionKeys.FindKey(currentCell))
                {
                    linkLength++;
                    currentCell = linkLowerLeftCorner + linkLength * deltaDir;
                    otherCell = currentCell + checkDirection;
                }
            
                if(linkLength > 0)
                    yield return new RegionLink(linkLowerLeftCorner + offset, direction, linkLength);
            
                delta += linkLength;
            }
        }
        
        private bool IsVectorInRange(Vector2Int pos, Vector2Int minPos, Vector2Int maxPos)
        {
            return pos.x >= minPos.x && pos.x <= maxPos.x && pos.y >= minPos.y && pos.y <= maxPos.y;
        }

        private Vector2Int GridToChunkId(Vector2Int gridPosition)
        {
            return new Vector2Int(gridPosition.x / _chunkSize.x, gridPosition.y / _chunkSize.y);
        }
    
        private Vector2Int GridToChunkPosition(Vector2Int gridPosition)
        {
            return gridPosition - _chunkSize * GridToChunkId(gridPosition);
        }
        
        private void JoinRegionsAroundCell(Vector2Int gridPosition)
        {
            // Create a temporary region in
            _regionKeys.SetKey(gridPosition, gridPosition);
            RegisterRegion(new Region<TRegionData>(gridPosition, 1, _regionDataGenerator), GridToChunkId(gridPosition));

            Vector2Int chunkPosition = GridToChunkPosition(gridPosition);

            Vector2Int targetPosition = gridPosition + Vector2Int.left;
            if(chunkPosition.x > 0 && !_occlusionGrid[targetPosition])
                JoinRegions(gridPosition, targetPosition);
            // The region of targetPosition might be in another chunk and might connect to the joined regions,
            // so we need to make sure its links are updated
            _regionsWithLinksToRecalculate.Add(_regionKeys.FindKey(targetPosition));
            
            targetPosition = gridPosition + Vector2Int.down;
            if(chunkPosition.y > 0 && !_occlusionGrid[targetPosition])
                JoinRegions(gridPosition, targetPosition);
            // The region of targetPosition might be in another chunk and might connect to the joined regions,
            // so we need to make sure its links are updated
            _regionsWithLinksToRecalculate.Add(_regionKeys.FindKey(targetPosition));
            
            targetPosition = gridPosition + Vector2Int.right;
            if(chunkPosition.x < _chunkSize.x - 1 && !_occlusionGrid[targetPosition])
                JoinRegions(gridPosition, targetPosition);
            // The region of targetPosition might be in another chunk and might connect to the joined regions,
            // so we need to make sure its links are updated
            _regionsWithLinksToRecalculate.Add(_regionKeys.FindKey(targetPosition));
            
            targetPosition = gridPosition + Vector2Int.up;
            if(chunkPosition.y < _chunkSize.y - 1 && !_occlusionGrid[targetPosition])
                JoinRegions(gridPosition, targetPosition);
            // The region of targetPosition might be in another chunk and might connect to the joined regions,
            // so we need to make sure its links are updated
            _regionsWithLinksToRecalculate.Add(_regionKeys.FindKey(targetPosition));
            
            RecalculateLinksInPendingRegions();
        }
    }
}