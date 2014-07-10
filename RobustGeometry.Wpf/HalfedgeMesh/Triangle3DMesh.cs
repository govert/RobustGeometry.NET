using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RobustGeometry.HalfedgeMesh;
using System.Windows.Media.Media3D;

namespace RobustGeometry.Wpf.HalfedgeMesh
{

    public class Point3DVertexTraits : IPoint3D
    {
        public Point3D Position { get; private set; }

        Point3DVertexTraits(Point3D point)
        {
            Position = point;
        }

        // TODO: How should the implicit operators and the constructor interact?
        public static implicit operator Point3DVertexTraits(Point3D point)
        {
            return new Point3DVertexTraits(point);
        }

        public double  X
        {
            get { return Position.X; }
        }

        public double  Y
        {
            get { return Position.X; }
        }

        public void SetPosition(double x, double y)
        {
            throw new NotImplementedException();
        }

        public double  Z
        {
            get { return Position.X; }
        }

        public void SetPosition(double x, double y, double z)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// A simple implementation of a triangle mesh with Point3D points.
    /// </summary>
    /// <typeparam name="TEdgeTraits"></typeparam>
    /// <typeparam name="TTFaceTraits"></typeparam>
    /// <typeparam name="THalfedgeTraits"></typeparam>
    /// <typeparam name="TVertexTraits"></typeparam>
    public class Point3DTriangleMesh<TEdgeTraits, TTFaceTraits, THalfedgeTraits, TVertexTraits>
        : PointTriangleMesh<NullTraits, NullTraits, NullTraits, Point3DVertexTraits>
        where TVertexTraits : Point3DVertexTraits
    {
    }

    
    /// <summary>
    /// An implementation of a triangle mesh with a MeshGeometry3D as the data storage.
    /// TODO: Normals are maintained - 
    ///       (Also) store normal in the Halfedge to allow a vertix to have different normals associated with different faces.
    ///       Flat facets are possibly identified via Surfaces (smoothing groups) - this deals better with normals.
    
    /// </summary>
    /// <typeparam name="TEdgeTraits"></typeparam>
    /// <typeparam name="TFaceTraits"></typeparam>
    /// <typeparam name="THalfedgeTraits"></typeparam>
    /// <typeparam name="TVertexTraits"></typeparam>
    public class MeshGeometry3DTriangleMesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>
        : PointTriangleMesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>
        where THalfedgeTraits : MeshGeometry3DTriangleMesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.HalfEdgeTraits
        where TVertexTraits : MeshGeometry3DTriangleMesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.VertexTraits
    {

        MeshGeometry3D _meshGeometry;

        public class HalfEdgeTraits
        {
            MeshGeometry3D MeshGeometry3D;  // OR a reference to the parent MeshGeometry3DMesh  
            int PositionIndex;              // Position Index for the Halfedge.Target, relative to the Halfedge.Face

            public Point3D Position
            {
                get { return MeshGeometry3D.Positions[PositionIndex]; }
            }

            // CONSIDER: If we want to update lots of positions, this is very inefficient 
            //           - need to disconnect the collection from the mesh, then reconnect.
            //           Perhaps that should be done in the Mesh.BeginEdit...?
            public void UpdatePosition(Point3D point3D)
            {
                MeshGeometry3D.Positions[PositionIndex] = point3D;
            }
        }

        public class VertexTraits : IPoint3D, IPoint3DUpdate
        {
            Halfedge halfedge;

            Point3D Position
            {
                get { return halfedge.Traits.Position; }
            }

            public double X
            {
                get { return Position.X; }
            }

            public double Y
            {
                get { return Position.Y; }
            }

            public double Z
            {
                get { return Position.Z; }
            }

            public void UpdatePosition(double x, double y)
            {
                UpdatePosition(x, y, Z);
            }

            public void UpdatePosition(double x, double y, double z)
            {
                // TODO: Get all Halfedges that point to this Vertex
                foreach (var he in halfedge.Target.Incomings)
                {
                    he.Traits.UpdatePosition(new Point3D(x,y,z));
                }
            }
        }

    }
}
