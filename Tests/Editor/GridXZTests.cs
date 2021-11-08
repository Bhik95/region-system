using System;
using NUnit.Framework;
using UnityEngine;

namespace Bhik95.Utility.Grids.Tests
{
    public class GridXZTests
    {
        private static readonly Vector2Int GridSize = new Vector2Int(3, 4);
        
        private static readonly string DefaultValue = "A";
        
        private BasicGridXZ<string> _gridXZ;

        [SetUp]
        public void Setup()
        {
            _gridXZ = new BasicGridXZ<string>(GridSize, DefaultValue);
        }

        [TearDown]
        public void TearDown()
        {
            _gridXZ = null;
        }

        [Test]
        public void TestConstructorExceptions()
        {
            // Grid size with negative component (x)
            Assert.Throws<ArgumentException>(() =>
            {
                _gridXZ = new BasicGridXZ<string>(new Vector2Int(-3, 4), "A");
            });
            
            // Grid size with negative component (y)
            Assert.Throws<ArgumentException>(() =>
            {
                _gridXZ = new BasicGridXZ<string>(new Vector2Int(3, -4), "A");
            });
            
            // Grid size with x - component = 0
            Assert.Throws<ArgumentException>(() =>
            {
                _gridXZ = new BasicGridXZ<string>(new Vector2Int(0, 4), "A");
            });
            
            // Grid size with y-component = 0
            Assert.Throws<ArgumentException>(() =>
            {
                _gridXZ = new BasicGridXZ<string>(new Vector2Int(3, 0), "A");
            });
        }
        
        [Test]
        public void TestGridSize()
        {
            Assert.AreEqual(GridSize.x, _gridXZ.GridSize.x);
            Assert.AreEqual(GridSize.y, _gridXZ.GridSize.y);
        }

        [Test]
        public void TestDefaultValue()
        {
            Assert.AreEqual(DefaultValue, _gridXZ[Vector2Int.zero]);
        }

        

        [Test]
        public void TestGridPosWithinBounds()
        {
            Assert.IsTrue(_gridXZ.IsWithinBounds(Vector2Int.zero));
            Assert.IsTrue(_gridXZ.IsWithinBounds(new Vector2Int(2, 0)));
            Assert.IsTrue(_gridXZ.IsWithinBounds(new Vector2Int(2, 3)));
            Assert.IsTrue(_gridXZ.IsWithinBounds(new Vector2Int(0, 3)));
            
            Assert.IsFalse(_gridXZ.IsWithinBounds(new Vector2Int(-1, 0)));
            Assert.IsFalse(_gridXZ.IsWithinBounds(new Vector2Int(3, 0)));
            Assert.IsFalse(_gridXZ.IsWithinBounds(new Vector2Int(0, -1)));
            Assert.IsFalse(_gridXZ.IsWithinBounds(new Vector2Int(0, 4)));
        }

        [Test]
        public void TestSetGridPos()
        {
            for (int ix = 0; ix < _gridXZ.GridSize.x; ix++)
            {
                for (int iy = 0; iy < _gridXZ.GridSize.y; iy++)
                {
                    Assert.AreEqual("A", _gridXZ[ix,iy]);
                    
                    // Test set method (int, int)
                    _gridXZ.Set(ix,iy, "B");
                    Assert.AreEqual("B", _gridXZ[ix,iy]);
                    Assert.AreEqual("B", _gridXZ.Get(ix, iy));
                    Assert.AreEqual("B", _gridXZ.Get(new Vector2Int(ix, iy)));
                    
                    // Test set method (Vector2Int)
                    _gridXZ.Set(new Vector2Int(ix, iy), "C");
                    Assert.AreEqual("C", _gridXZ[new Vector2Int(ix,iy)]);
                    Assert.AreEqual("C", _gridXZ.Get(ix, iy));
                    Assert.AreEqual("C", _gridXZ.Get(new Vector2Int(ix, iy)));
                    
                    // Test set accessor
                    _gridXZ[ix, iy] = "D";
                    Assert.AreEqual("D", _gridXZ[ix,iy]);
                    Assert.AreEqual("D", _gridXZ.Get(ix, iy));
                    Assert.AreEqual("D", _gridXZ.Get(new Vector2Int(ix, iy)));
                }
            }
        }
    }
}

