using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RobustGeometry.HalfedgeMesh;

namespace RobustGeometry.Test.HalfedgeMesh.Test
{
    public class BasicMesh : Mesh<NullTraits, NullTraits, NullTraits, NullTraits> { }
    public class StringMesh : Mesh<String, String, String, String> { }


    [TestClass]
    public class MeshPrimitivesTests
    {
        [TestMethod]
        public void Mesh_CreateTriangle()
        {
            string v1Traits = "v1";
            string v2Traits = "v2";
            string v3Traits = "v3";
            string faceTraits = "face1";

            var mesh = new StringMesh();
            var face = mesh.CreateTriangle(v1Traits, v2Traits, v3Traits, faceTraits);

            // Recover the vertices and edges
            var v1 = face.Bounding.Source;
            var v2 = face.Bounding.Target;
            var v3 = face.Bounding.Next.Target;

            var e12 = v1.HalfedgeTo(v2);
            var e23 = v2.HalfedgeTo(v3);
            var e31 = v3.HalfedgeTo(v1);

            var e21 = e12.Opposite;
            var e32 = e23.Opposite;
            var e13 = e31.Opposite;

            // Check that the vertices and face is build as expected
            Assert.AreEqual(v1Traits, v1.Traits);
            Assert.AreEqual(v2Traits, v2.Traits);
            Assert.AreEqual(v3Traits, v3.Traits);
            Assert.AreEqual(faceTraits, face.Traits);

            // Check that the Halfedges are built correctly
            // Vertices
            Assert.AreEqual(v1, e12.Source);
            Assert.AreEqual(v2, e12.Target);
            Assert.AreEqual(v2, e23.Source);
            Assert.AreEqual(v3, e23.Target);
            Assert.AreEqual(v3, e31.Source);
            Assert.AreEqual(v1, e31.Target);

            // Edges
            Assert.AreEqual(e12.Edge, e21.Edge);
            Assert.AreEqual(e23.Edge, e32.Edge);
            Assert.AreEqual(e31.Edge, e13.Edge);

            // forward cycle
            Assert.AreEqual(e23, e12.Next);
            Assert.AreEqual(e31, e23.Next);
            Assert.AreEqual(e12, e31.Next);

            // reverse cycle
            Assert.AreEqual(e12, e23.Previous);
            Assert.AreEqual(e23, e31.Previous);
            Assert.AreEqual(e31, e12.Previous);

            Assert.AreEqual(1, mesh.Faces.Count);

        }

        [TestMethod]
        public void Mesh_AdjoinTriangle()
        {
            string v1Traits = "v1";
            string v2Traits = "v2";
            string v3Traits = "v3";
            string v4Traits = "v4";
            string faceTraits = "face1";

            var mesh = new StringMesh();
            var face = mesh.CreateTriangle(v2Traits, v1Traits, v4Traits, faceTraits);

            var face2Traits = "face2";
            var face3Traits = "face3";
            var face4Traits = "face4";

            // Recover the first vertex
            var v2 = face.Bounding.Source;
            var v1 = face.Bounding.Target;
            var v4 = face.Bounding.Next.Target;

            // Use it to make next triangle - careful with sequence V2 -> v1
            var face2 = mesh.CreateTriangle(v1, v2, v3Traits, face2Traits);
            var v3 = face2.Bounding.Next.Target;

            // Recover halfedges
            var e21 = v2.HalfedgeTo(v1);
            var e14 = v1.HalfedgeTo(v4);
            var e42 = v4.HalfedgeTo(v2);
            var e31 = v3.HalfedgeTo(v1);
            var e32 = v3.HalfedgeTo(v2);

            var e12 = e21.Opposite;
            var e41 = e14.Opposite;
            var e24 = e42.Opposite;
            var e13 = e31.Opposite;
            var e23 = e32.Opposite;

            // Check new outside boundary loop - forward
            Assert.AreEqual(e13, e41.Next);
            Assert.AreEqual(e32, e13.Next);
            Assert.AreEqual(e24, e32.Next);
            Assert.AreEqual(e41, e24.Next);


            // Check new outside boundary loop - reverse
            Assert.AreEqual(e13, e32.Previous);
            Assert.AreEqual(e41, e13.Previous);
            Assert.AreEqual(e24, e41.Previous);
            Assert.AreEqual(e32, e24.Previous);

            Assert.AreEqual(2, mesh.Faces.Count);

        }

        [TestMethod]
        public void Mesh_CreateTetrahedron()
        {
            string v1Traits = "v1";
            string v2Traits = "v2";
            string v3Traits = "v3";
            string faceTraits = "face1";

            var mesh = new StringMesh();
            var face = mesh.CreateTriangle(v1Traits, v2Traits, v3Traits, faceTraits);

            var v4Traits = "v4";
            var face2Traits = "face2";
            var face3Traits = "face3";
            var face4Traits = "face4";

            // Recover the first vertex
            var v1 = face.Bounding.Source;
            var v2 = face.Bounding.Target;
            var v3 = face.Bounding.Next.Target;
            
            // Use it to make next triangle - careful with sequence V2 -> v1
            var face2 = mesh.CreateTriangle(v2, v1, v4Traits, face2Traits);
            var v4 = face2.Bounding.Next.Target;

            // Recover halfedges
            var e21 = v2.HalfedgeTo(v1);
            var e14 = v1.HalfedgeTo(v4);
            var e42 = v4.HalfedgeTo(v2);

            var e12 = e21.Opposite;
            var e41 = e14.Opposite;
            var e24 = e42.Opposite;

            var face3 = mesh.CreateTriangle(v3, v4, v1, face3Traits);
            var face4 = mesh.CreateFace(e24, face4Traits);

            foreach (var he in mesh.Halfedges)
            {
                Assert.IsNotNull(he.Face);
            }

            Assert.AreEqual(4, mesh.Faces.Count);
            Assert.AreEqual(4, mesh.Vertices.Count);
        }

        [TestMethod]
        public void Mesh_CreateCube()
        {
            var vt1 = "v1";
            var vt2 = "v2";
            var vt3 = "v3";
            var vt4 = "v4";
            var vt5 = "v5";
            var vt6 = "v6";
            var vt7 = "v7";
            var vt8 = "v8";

            var mesh = new StringMesh();
        }

        [TestMethod]
        public void Mesh_TestSplitVertex()
        {
            // We make two triangles, and split the middle edge:
            //
            
        }

        [TestMethod]
        public void Mesh_TestJoinFaces()
        {
            // We make two triangles, and join the middle edge
            //
            //   v1 o--------o v4
            //      | \      |
            //      |  \ f2  |
            //      |   \    |
            //      |    \   |
            //      | f1  \  |
            //      |      \ |
            //      |       \|
            //   v2 o--------o v3

            var vt1 = "v1";
            var vt2 = "v2";
            var vt3 = "v3";
            var vt4 = "v4";
            var vt5 = "v5";
            var vt6 = "v6";
            var vt7 = "v7";
            var vt8 = "v8";

            var mesh = new StringMesh();
            StringMesh.Face f1 = mesh.CreateTriangle(vt1, vt2, vt3);
            StringMesh.Vertex v1 = f1.Vertices.First(v => v.Traits == vt1);
            StringMesh.Vertex v3 = f1.Vertices.First(v => v.Traits == vt3);
            StringMesh.Face f2 = mesh.CreateTriangle(v1, v3, vt4);

            Assert.AreEqual(2, mesh.Faces.Count);
            Assert.AreEqual(5, mesh.Edges.Count);
            Assert.AreEqual(4, mesh.Vertices.Count);

            mesh.JoinFaces(v3.HalfedgeTo(v1));
            
            Assert.AreEqual(1, mesh.Faces.Count);
            Assert.AreEqual(4, mesh.Edges.Count);
            Assert.AreEqual(4, mesh.Vertices.Count);
        }
    }
}
