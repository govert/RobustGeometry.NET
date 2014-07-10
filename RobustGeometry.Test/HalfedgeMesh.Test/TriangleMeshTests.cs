using System;
using System.Windows;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RobustGeometry.HalfedgeMesh;
using RobustGeometry.Wpf.HalfedgeMesh;

namespace RobustGeometry.Test.HalfedgeMesh.Test
{
    // Assist IntelliSense a bit
    using Mesh     = PointTriangleMesh<NullTraits, NullTraits, NullTraits, PointVertexTraits>;
    using Face     = PointTriangleMesh<NullTraits, NullTraits, NullTraits, PointVertexTraits>.Face;
    using Edge     = PointTriangleMesh<NullTraits, NullTraits, NullTraits, PointVertexTraits>.Edge;
    using Halfedge = PointTriangleMesh<NullTraits, NullTraits, NullTraits, PointVertexTraits>.Halfedge;
    using Vertex   = PointTriangleMesh<NullTraits, NullTraits, NullTraits, PointVertexTraits>.Vertex;

    [TestClass]
    public class TriangleMeshTests
    {
        [TestMethod]
        public void Mesh_Halfedge_LocatePoint()
        {
            Mesh mesh = new Mesh();
            mesh.CreateTriangle( new Point(0,0),
                                 new Point(1,0),
                                 new Point(1,1) );
            var he = mesh.Halfedges[0];
            Assert.AreEqual(new Point(0,0), he.Source.Traits.Position);
            Assert.AreEqual(new Point(1,0), he.Target.Traits.Position);

            Assert.AreEqual(Mesh.HalfedgePointLocation.Source, Mesh.LocatePoint(he, new Point(0, 0)));
            Assert.AreEqual(Mesh.HalfedgePointLocation.Target, Mesh.LocatePoint(he, new Point(1, 0)));
            Assert.AreEqual(Mesh.HalfedgePointLocation.Inside, Mesh.LocatePoint(he, new Point(0.99999, 0)));
            Assert.AreEqual(Mesh.HalfedgePointLocation.Inside, Mesh.LocatePoint(he, new Point(0.00001, 0)));
            Assert.AreEqual(Mesh.HalfedgePointLocation.Infront, Mesh.LocatePoint(he, new Point(1.0000000000001, 0)));
            Assert.AreEqual(Mesh.HalfedgePointLocation.Behind, Mesh.LocatePoint(he, new Point(-0.0000000000001, 0)));
            Assert.AreEqual(Mesh.HalfedgePointLocation.Behind, Mesh.LocatePoint(he, new Point(-1.0, 0)));
            Assert.AreEqual(Mesh.HalfedgePointLocation.Left, Mesh.LocatePoint(he, new Point(0.5, 0.0001)));
            Assert.AreEqual(Mesh.HalfedgePointLocation.Right, Mesh.LocatePoint(he, new Point(0.5, -0.0001)));

        }

        [TestMethod]
        public void Mesh_Halfedge_Locate_CheckRobust()
        {
            Mesh mesh = new Mesh();
            mesh.CreateTriangle(new Point(-10000000000.0, 0),
                                 new Point(0.00000000001, 0),
                                 new Point(1, 1));
            var he = mesh.Halfedges[0];

            Assert.AreEqual(Mesh.HalfedgePointLocation.Infront, Mesh.LocatePoint(he, new Point(0.00000000002, 0)));
        }
    }
}
