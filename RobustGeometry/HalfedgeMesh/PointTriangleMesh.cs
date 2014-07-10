using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using RobustGeometry.Predicates;

using EA = RobustGeometry.Predicates.ExactArithmetic;
using GP = RobustGeometry.Predicates.GeometricPredicates;

namespace RobustGeometry.HalfedgeMesh
{

    // This ensures vertices have X and Y positions, which should be enough for 2D Delaunay triangulation.

    // CONSIDER: Maybe this should not be a TriangleMesh but a ConvexPolygonMesh
    // CONSIDER: Make space for a BSP index of the faces ?

    public class PointTriangleMesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>
        : TriangleMesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits> where TVertexTraits : IPoint
    {
        private object _pointIndex; // Some indexing structure to allow us to do point-locations fast.

        // Find the Face, Edge or Vertex at a point, or null if the point is outside the mesh,
        // We are assuming the faces are well-formed triangles, the mesh is convex and has only a single component.
        // TODO: Check convexity - BSP?
        // TODO: Deal with multiple components - BSP?
        public object LocatePoint(IPoint point)
        {
            if ( IsEmpty() ) { return null; }
            
            Face currentFace = Faces[0];  // Might be per component?
            Halfedge startHalfedge = currentFace.Bounding;
            Halfedge currentHalfedge = startHalfedge;
            do
            {
                var loc = LocatePoint(currentHalfedge, point);
                switch (loc)
                {
                    case HalfedgePointLocation.Inside:
                        return currentHalfedge;
                    case HalfedgePointLocation.Source:
                        return currentHalfedge.Source;
                    case HalfedgePointLocation.Target:
                        return currentHalfedge.Target;
                    case HalfedgePointLocation.Left:
                    case HalfedgePointLocation.Behind:
                        currentHalfedge = currentHalfedge.Next;
                        if (currentHalfedge == startHalfedge) return currentFace;
                        break;
                    case HalfedgePointLocation.Right:
                    case HalfedgePointLocation.Infront:
                        if (currentHalfedge.Opposite.IsBoundary)
                        {
                            // We assume convex mesh, so this means we're outside.
                            return null;
                        }
                        break;
                }
            } while (true);
            return null;
        }

        // This overloaded method allows us to use the implicit conversion from a Point to a PointVertexTraits.
        public object LocatePoint(TVertexTraits point)
        {
            return LocatePoint((IPoint)point);
        }

        public double LongestEdge()
        {
            return Edges.Max(e => e.Length());
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

        // CONSIDER: Maybe make public?
        internal enum HalfedgePointLocation
        {            
                     // Point is...
            Source,  // the Source vertex of the Halfedge
            Target,  // the Target vertex of the Halfedge
            Inside,  // an internal point of the Halfedge
            Left,    // to the left of the Halfedge 
            Behind,  // in line with Halfedge and behind Source
            Right,   // to the right of the Halfedge
            Infront  // in line with Halfedge and in front of Target.
        }

        internal static HalfedgePointLocation LocatePoint(Halfedge halfedge, IPoint point)
        {
            // CONSIDER: More the PointMesh validity assertion somewhere else.
            // CONSIDER: Formal valid range for points where Predicates are valid?

            Debug.Assert(halfedge.Source.Traits.X != halfedge.Target.Traits.X ||
                         halfedge.Source.Traits.Y != halfedge.Target.Traits.Y);

            double[] source = new[] { halfedge.Source.Traits.X, halfedge.Source.Traits.Y };
            double[] target = new[] { halfedge.Target.Traits.X, halfedge.Target.Traits.Y };
            double[] test   = new[] { point.X, point.Y };

            double orient = GP.Orient2D(source, target, test);
            
            if (orient > 0.0) return HalfedgePointLocation.Left;
            if (orient < 0.0) return HalfedgePointLocation.Right;
            
            // In line with halfedge - classify where exactly
            if (test[0] == source[0] && test[1] == source[1]) return HalfedgePointLocation.Source;
            if (test[0] == target[0] && test[1] == target[1]) return HalfedgePointLocation.Target;
            if (target[0] > source[0])
            {
                if (test[0] > target[0]) return HalfedgePointLocation.Infront;
                if (test[0] < source[0]) return HalfedgePointLocation.Behind;
                return HalfedgePointLocation.Inside;
            }
            if (target[0] < source[0])
            {
                if (test[0] < target[0]) return HalfedgePointLocation.Infront;
                if (test[0] > source[0]) return HalfedgePointLocation.Behind;
                return HalfedgePointLocation.Inside;
            }
            // thus target[0] == source[0], classify according to Y
            if (target[1] > source[1])
            {
                if (test[1] > target[1]) return HalfedgePointLocation.Infront;
                if (test[1] < source[1]) return HalfedgePointLocation.Behind;
                return HalfedgePointLocation.Inside;
            }
            // thus target[1] < source[1]
            Debug.Assert(target[1] < source[1], "Inconsistent PointLocation information");
            if (test[1] < target[1]) return HalfedgePointLocation.Infront;
            if (test[1] > source[1]) return HalfedgePointLocation.Behind;
            return HalfedgePointLocation.Inside;
        }

        internal static HalfedgePointLocation LocatePoint(Halfedge halfedge, TVertexTraits point)
        {
            return LocatePoint(halfedge, (IPoint)point);
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


    // This ensures vertices have X, Y and Z positions, which should be enough for 3D Delaunay tetrahedralization.

    // CONSIDER: Maybe this should not be a TriangleMesh but a ConvexPolygonMesh
    // CONSIDER: Make space for a BSP index of the faces ?

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
