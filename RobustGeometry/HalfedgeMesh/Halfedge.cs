using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace RobustGeometry.HalfedgeMesh
{
    // TODO: Deal with multiple Components

    public partial class Mesh<TEdgeTraits, TFaceTraits, THalfedgeTraits, TVertexTraits>
    {
        public class Halfedge
        {
            /*                      \\ <--- NextAtTarget (incoming)
             *                       \\     //
             *             Next ----> \\   //  <--- PreviousAtTarget (incoming)
             *            (in face)    \v /v
             *                           o <-------- Target vertex
             *                           ^|
             *                           ||
             *     'this' halfedge ----->|| 
             *                           || <----- Opposite halfedge
             *                           ||
             *       Face                |v
             *                           o <--------- Source vertex
             *                          ^/ ^\   
             *           Previous ---> //   \\  <-- NextAtSource (outgoing)
             *           (in face)    //<<|  \\ 
             *                       /v   |   \v               
             *                            |
             *                            |--PreviousAtSource  (outgoing)
             */

            public THalfedgeTraits Traits { get; set; }


            public Vertex Target { get; internal set; }
            public Face Face { get; internal set; }  // Can be null!
            public Halfedge Opposite { get; internal set; }
            public Halfedge Next { get; internal set; }  // along Face
            public Halfedge Previous { get; internal set; }  // along Face (optional to store, can recover by going around, esp. for triangle?)
            public Edge Edge { get; internal set; }  // (optional concept, but convenient for hanging traits?)

            // Derived relations
            public Vertex Source { get { return Opposite.Target; } }  // Or Previous.Target, keeping us on the same face
            public Halfedge PreviousAtSource { get { return Previous.Opposite; } }
            public Halfedge NextAtSource { get { return Opposite.Next; } }
            public Halfedge PreviousAtTarget { get { return Opposite.Previous; } }
            public Halfedge NextAtTarget { get { return Next.Opposite; } }

            // Are we on the left boundary (i.e. we have no face to the left).
            // We are not a halfedge on the boundary if our left has a face, but right does not.
            public bool IsBoundary { get { return Face == null; } }

            internal Halfedge()
            {
            }

            [Conditional("DEBUG")]
            internal void AssertValid()
            {
                Debug.Assert(Opposite != null);
                Debug.Assert(Opposite.Opposite == this);
                Debug.Assert(Edge != null);
                Debug.Assert(Next != null);
                Debug.Assert(Previous != null);

                // No isolated edges - an edge is always associated with two distinct vertices.
                Debug.Assert(Target != null);

                // No self-loops
                Debug.Assert(Source != Target);
            }

            /// <summary>
            /// Ring of Halfedges around this Halfedge's Face, trversed CCW.
            /// </summary>
            public IEnumerable<Halfedge> Ring
            {
                get
                {
                    Halfedge start = this;
                    Halfedge current = start;
                    do
                    {
                        yield return current;
                        current = current.Next;
                    } while (current != start);
                }
            }

            public override string ToString()
            {
                return Source.ToString() + " | " + Target.ToString();
            }

            public static implicit operator THalfedgeTraits(Halfedge halfedge)
            {
                return halfedge.Traits;
            }

        }
    }
}