using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RobustGeometry.HalfedgeMesh
{

    // This ensures vertices have X and Y positions, which should be enough for 2D Delaunay triangulation.
    public class PointTriangleMesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>
        : TriangleMesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits> where TVertexTraits : IPoint
    {
        private object _pointIndex; // Some indexing structure to allow us to do point-locations fast.

        public object LocatePoint(IPoint point)
        {
            // Find the Face, Edge or Vertex at a point, or null if the point is outside the mesh,
            // using _pointIndex.
            return null;
        }

        public double LongestEdge()
        {
            return Edges.Max(e => e.Length());
            //            var maxLength = (from e in Edges orderby e.Length() select e, e.Length()).Take(1);
            //            return maxLength.Len;
            throw new NotImplementedException();
        }

        // Remove triangles that have a very large angle (close to 180 degrees)
        // Flip the long edge, and check things then...?
        // Or just collapse away the triangle.
        public void RemoveCaps(double angleThresholdDegrees)
        {
        }

        // Remove triangles that have a very short edge, by collapsing that edge
        // Keep the vertex that is the most 'complicated' (remove vertex that changes geometry the least).
        public void RemoveNeedles(double lengthThreshold)
        {
        }
    }

    public static class PositionTriangleMeshExtensions
    {
        private static double Distance(IPoint point1, IPoint point2)
        {
            double dx = point2.X - point1.X;
            double dy = point2.Y - point1.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        public static double Length<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>(this PointTriangleMesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>.Edge edge) where TVertexTraits : IPoint
        {
            return Distance(edge.Half1.Target.Traits, edge.Half2.Target.Traits);
        }

        //public static TrianglePointMesh<TVertexTraits>.Edge LongestEdge<TVertexTraits>(this TrianglePointMesh<TVertexTraits>.Face face)
        //{
        //    return from e in face.Ed
        //}
    }


    // This ensures vertices have X and Y positions, which should be enough for 3D Delaunay tetrahedralization.
    public class Point3DTriangleMesh<TEdgeTraits, TFaceTraits, THalfedgeBase, TVertexTraits>
        : PointTriangleMesh<TEdgeTraits, TFaceTraits, THalfedgeBase, TVertexTraits>
        where TVertexTraits : IPoint3D
    {

    }



    // Allow us to say Vertex.Traits.X
    // Can add an extension method to allow us to say Vertex.X();

    public interface IPoint
    {
        double X { get; }
        double Y { get; }
    }

    public interface IPointUpdate
    {
        void UpdatePosition(double x, double y);
    }

    public interface IPoint3D : IPoint
    {
        double Z { get; }
    }

    public interface IPoint3DUpdate : IPointUpdate
    {
        void UpdatePosition(double x, double y, double z);
    }

    //public struct PositionVertexTraits : IPoint, IDynamicMetaObjectProvider
    //{
    //    public double X
    //    {
    //        get
    //        {
    //            throw new NotImplementedException();
    //        }
    //        set
    //        {
    //            throw new NotImplementedException();
    //        }
    //    }

    //    public double Y
    //    {
    //        get
    //        {
    //            throw new NotImplementedException();
    //        }
    //        set
    //        {
    //            throw new NotImplementedException();
    //        }
    //    }

    //    private ExpandoObject Other; // = new ExpandoObject();

    //    public DynamicMetaObject GetMetaObject(System.Linq.Expressions.Expression parameter)
    //    {
    //        if (Other == null) Other = new ExpandoObject();
    //        return (Other as IDynamicMetaObjectProvider).GetMetaObject(parameter);
    //    }
    //}

    //public class DynamicBase : dynamic
    //{
    //}
}
