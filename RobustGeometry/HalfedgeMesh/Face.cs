using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Diagnostics;

namespace RobustGeometry.HalfedgeMesh
{

    // Idea is for other classes to inherit from Mesh to add stuff like faster point-in-mesh finding.
    public partial class Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>
    {

        // Traits could give material...?
        // public class Surface {}

        public class Face
        {

            //  internal int ManifoldIndex; // < 0 for 'outside' and 'holes' (Or SurfaceIndex or Surface reference)

            public TFaceTraits Traits { get; set; }

            // One of the bounding Halfedges - there might be many.
            // We require there to be at least one - i.e. an 'isolated' face will have edges around it.
            public Halfedge Bounding { get; internal set; }

            public IEnumerable<Halfedge> Perimeter
            {
                get
                {
                    var first = Bounding;
                    var current = first;
                    do
                    {
                        yield return current;
                        current = current.Next;
                    } while (current != first);
                }
            }

            public IEnumerable<Vertex> Vertices
            {
                get
                {
                    foreach (var he in Perimeter)
                    {
                        yield return he.Target;
                    }
                }
            }

            void CheckValid()
            {
                Debug.Assert(Bounding != null);
            }

            public static implicit operator TFaceTraits(Face face)
            {
                return face.Traits;
            }

        }
    }
}
