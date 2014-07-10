using System;
using System.Collections.Generic;
using System.Text;

namespace RobustGeometry.HalfedgeMesh
{
    // We are not using the TriangleMesh to optimise the storage, 
    // but to add the additional constraints to the mesh, allowing algorithms to assume all faces are triangles.
    // Also, it is an example of how to create a more constrained Mesh class that might constrain some operations, 
    // or implement additional transformations when mesh operations are performed.
    // CONSIDER: Should this just be a flag in the Mesh?
    // CONSIDER: Would it be better to have a ConvexPolygonMesh instead?
    public class TriangleMesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>
        : Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>
    {
        public bool CanFlipEdge(Edge edge)
        {
            return !edge.IsBoundary;
            return false;
        }

        // Condition - must not be a boundary edge.
        public void FlipEdge(Edge edge)
        {
        }

        //public override void CollapseEdge(Halfedge h)
        //{
        //    throw new InvalidOperationException("TriangleMesh does not support arbitraty Edge collapse");
        //}

        //public virtual Vertex CollapseTriangle(Face face)
        //{
        //    return null;
        //}

        // CONSIDER: - this might create many face - how to process all of them
        //public override 

        //    Face CreateFace(Halfedge halfedge)
        //{
        //    Face polyFace = base.CreateFace(halfedge);

        //    // TODO: Triangulate the face
        //    return polyFace ;
        //}

    }
}
