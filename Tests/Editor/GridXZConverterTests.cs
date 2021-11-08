using System;
using NUnit.Framework;
using UnityEngine;

namespace Bhik95.Utility.Grids.Tests
{
    public class GridXZConverterTests
    {
        private static readonly Vector2 Origin = new Vector2(1.0f, 2.0f);
        private static readonly Vector2 CellSize = new Vector2(5.0f, 6.0f);
        
        private static readonly Vector2Int GridSize = new Vector2Int(3, 4);
        private static readonly string DefaultValue = "A";
        
        private BasicGridXZ<string> _gridXZ;
        private GridXZCoordinateConverter _converter;

        [SetUp]
        public void Setup()
        {
            _gridXZ = new BasicGridXZ<string>(GridSize, DefaultValue);
            _converter = new GridXZCoordinateConverter(Origin, GridSize, CellSize);
        }

        [TearDown]
        public void TearDown()
        {
            _gridXZ = null;
            _converter = null;
        }
        
        [Test]
        public void TestOrigin()
        {
            Assert.AreEqual(Origin.x, _converter.LowerLeftCorner.x);
            Assert.AreEqual(Origin.y, _converter.LowerLeftCorner.y);
        }
        
        [Test]
        public void TestCellSize()
        {
            Assert.AreEqual(CellSize.x, _converter.CellSize.x);
            Assert.AreEqual(CellSize.y, _converter.CellSize.y);
        }
        
        [Test]
        public void TextWorldPosWithinBounds()
        {
            float epsilon = 0.01f;
            
            Vector2 delta = GridSize * CellSize;

            Vector2 mid = Origin + delta / 2;
            Vector2 bound = Origin + delta;
            
            Assert.IsTrue(_converter.IsWorldPosWithinBounds(Origin)); // Lower left corner
            Assert.IsTrue(_converter.IsWorldPosWithinBounds(new Vector2(bound.x, Origin.y))); // Lower right corner
            Assert.IsTrue(_converter.IsWorldPosWithinBounds(new Vector2(bound.x, bound.y))); // Top right corner
            Assert.IsTrue(_converter.IsWorldPosWithinBounds(new Vector2(Origin.x, bound.y))); // Top left corner
            Assert.IsTrue(_converter.IsWorldPosWithinBounds(mid)); // A point inside
            
            Assert.IsFalse(_converter.IsWorldPosWithinBounds(new Vector2(Origin.x - epsilon, mid.y))); // Too left
            Assert.IsFalse(_converter.IsWorldPosWithinBounds(new Vector2(bound.x + epsilon, mid.y))); // Too right
            Assert.IsFalse(_converter.IsWorldPosWithinBounds(new Vector2(mid.x, bound.y + epsilon))); // Too high
            Assert.IsFalse(_converter.IsWorldPosWithinBounds(new Vector2(mid.x, Origin.y - epsilon))); // Too low
        }
        
        [Test]
        public void TestSetInWorldPos()
        {
            Vector2 epsilon2 = new Vector2(0.001f, 0.001f);
            string newValue = "New value";
            
            Vector2 delta = GridSize * CellSize;
            
            _converter.SetInWorldPos(_gridXZ, Origin + epsilon2, newValue);
            Assert.AreEqual(newValue, _gridXZ[0, 0]);
            
            _converter.SetInWorldPos(_gridXZ, Origin + delta - epsilon2, newValue);
            Assert.AreEqual(newValue, _gridXZ[2, 3]);
        }
        
        [Test]
        public void TestGetInWorldPos()
        {
            Vector2 epsilon2 = new Vector2(0.001f, 0.001f);
            string newValue = "New value";
            
            Vector2 delta = GridSize * CellSize;
            
            _converter.SetInWorldPos(_gridXZ, Origin + delta - epsilon2, newValue);
            Assert.AreEqual(newValue, _converter.GetInWorldPos(_gridXZ, Origin + delta - epsilon2));

            _converter.SetInWorldPos(_gridXZ, Origin, newValue);
            Assert.AreEqual(newValue, _converter.GetInWorldPos(_gridXZ, Origin));

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                _converter.GetInWorldPos(_gridXZ, Origin - epsilon2);
            });
            
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                _converter.GetInWorldPos(_gridXZ, Origin + delta + epsilon2);
            });
        }
        
        [Test]
        public void TestConversions()
        {
            for (int ix = 0; ix < _gridXZ.GridSize.x; ix++)
            {
                for (int iz = 0; iz < _gridXZ.GridSize.y; iz++)
                {
                    Vector2Int gridPos = new Vector2Int(ix, iz);
                    Assert.AreEqual(gridPos, _converter.WorldToGrid(_converter.GridCenterToWorld(gridPos)));
                    Assert.AreEqual(gridPos, _converter.WorldToGrid(_converter.GridCenterToWorld(ix, iz)));
                }
            }
            
            Assert.AreEqual(Origin + (new Vector2(0.5f, 0.5f) * CellSize), _converter.GridCenterToWorld(0, 0));
            Assert.AreEqual(Vector2Int.zero, _converter.WorldToGrid(Origin));
        }

        [Test]
        public void TestGridToWorldDifferentCellPositions()
        {
            Vector2Int gridPosition = new Vector2Int(0, 0);
            Vector2 cellPosition = new Vector2(0.5f, 0.5f);
            
            Assert.AreEqual(
                _converter.GridCenterToWorld(gridPosition),
                _converter.GridToWorld(gridPosition, cellPosition)
                );
            
            Assert.AreEqual(
                Origin,
                _converter.GridToWorld(gridPosition, Vector2.zero)
            );

            float epsilon = 0.000001f;
            Assert.AreEqual(
                Origin.x + CellSize.x,
                _converter.GridToWorld(gridPosition, Vector2.one * ( 1 - epsilon)).x,
                0.0001
            );
            Assert.AreEqual(
                Origin.y + CellSize.y,
                _converter.GridToWorld(gridPosition, Vector2.one * ( 1 - epsilon)).y,
                0.001
            );
            
            cellPosition = new Vector2(0.25f, 0.53f);
            Assert.AreEqual(
                Origin + (cellPosition * CellSize),
                _converter.GridToWorld(gridPosition, cellPosition)
            );

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                cellPosition = -epsilon * Vector2.one;
                _converter.GridToWorld(gridPosition, cellPosition);
            });
            
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                cellPosition = (1 + epsilon) * Vector2.one;
                _converter.GridToWorld(gridPosition, cellPosition);
            });
        }
    }
}

