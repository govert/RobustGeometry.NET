using System;
using System.Linq;
using System.Diagnostics;

namespace RobustGeometry.HalfedgeMesh
{
    public partial class Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>
    {
        public class Edge
        {
            public TEdgeTraits Traits { get; set; }

            // One of the halfedges - must be non-null
            public Halfedge Half1 { get; internal set; }
            public Halfedge Half2 { get { return Half1.Opposite; } }

            internal bool IsBoundary
            {
                get
                {
                    return Half1.IsBoundary || Half2.IsBoundary;
                }
            }

            void CheckValid()
            {
                Debug.Assert(Half1 != null);
                Debug.Assert(Half1.Edge == this);
            }

            public static implicit operator TEdgeTraits(Edge edge)
            {
                return edge.Traits;
            }

        }
    }
}
