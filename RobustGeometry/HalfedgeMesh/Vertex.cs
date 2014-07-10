using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace RobustGeometry.HalfedgeMesh
{
    public partial class Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>
    {
        public class Vertex
        {
            // This is a field rather than a property so that we have convenient member access when the traits type is a struct.
            // Better would have been for Vertex to derive from VertexTraits (called VertexBase), 
            // but I think that can't work with the nested class design, 
            // which we need in order to keep the generic type declarations under control.
            public TVertexTraits Traits { get; set; }

            // We don't allow self-loops (check by an invariant in Halfedge)

            // TODO: For boundary vertices - ensure that the Outgoing halfedge is a boundary halfedge
            //       (Or _the_ boundary halfedge, if we restrict ourselves to manifold meshes.)
            // CONSIDER: For now we don't allow isolated vertices.
            //           If we wanted to, could represent these with null Outgoing reference.
            public Halfedge Outgoing { get; internal set; }

            public IEnumerable<Vertex> Neighbors
            {
                get
                {
                    var currentHalfedge = Outgoing;
                    do
                    {
                        yield return currentHalfedge.Target;
                        currentHalfedge = currentHalfedge.NextAtSource;
                    } while (currentHalfedge != Outgoing);
                }
            }

            public IEnumerable<Halfedge> Incomings
            {
                get
                {
                    Halfedge start = Outgoing.Opposite;
                    Halfedge current = start;
                    do
                    {
                        yield return current;
                        current = current.NextAtTarget;
                    } while (current != start);
                }
            }

            public bool IsBoundary
            {
                get
                {
                    return Incomings.Any(he => he.Edge.IsBoundary);
                }
            }

            /// <summary>
            /// Finds the Halfedge going from this vertex to the given vertex.
            /// </summary>
            /// <param name="v"></param>
            /// <returns>Either null, if v is not a neighbour, or the halfedge going to v</returns>
            public Halfedge HalfedgeTo(Vertex v)
            {
                return v.Incomings.FirstOrDefault(inHe => inHe.Source == this);
            }

            [Conditional("DEBUG")]
            internal void AssertValid()
            {
                Debug.Assert(Outgoing != null); // We don't allow isolated vertices (but might later)
                Debug.Assert(Outgoing.Source == this);  // Checks some of the halfedge hookup

                if (IsBoundary)
                {
                    // Condition ensuring Outgoing of a Boundary Vertex is the Boundary Halfedge
                    Debug.Assert(Outgoing.IsBoundary);
                    // Condition ensuring Manifold property for vertices
                    Debug.Assert(Incomings.Count(he => he.Edge.IsBoundary) <= 2);
                }
            }

            public override string ToString()
            {
                if (!object.Equals(Traits, null))
                    return Traits.ToString();
                return base.ToString();
            }

            public static implicit operator TVertexTraits(Vertex vertex)
            {
                return vertex.Traits;
            }
        }

    }
}
