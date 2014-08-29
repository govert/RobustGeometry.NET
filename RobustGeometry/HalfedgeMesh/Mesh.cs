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
        // We store full collections - might even refactor the implementations to store indices into these instead of pointers
        // (especially good for 64-bit architecture, where index would be smaller than a pointer)
        // and then have some implicit storage scheme that puts the halfedge and its opposite next to each other, saving more space
        // and then even have a scheme for arranging the edges.
        
        // For now not optimising for performance (time or space).
        
        // NOTE: Remember that for a large Mesh with lots of pointers, the Garbage Collector has to do a lot of work
        //       to check what's alive. For arrays and indices, perhaps not.
        
        readonly List<Edge> _edges = new List<Edge>();
        readonly List<Face> _faces = new List<Face>();
        readonly List<Halfedge> _halfedges = new List<Halfedge>();
        readonly List<Vertex> _vertices = new List<Vertex>();

        // Expose read-only collections 
        // CONSIDER: There are some new types for this in .NET 4.5?
        // CONSIDER: Should this return type be IList<> or ReadOnlyCollection<>?
        public ReadOnlyCollection<Edge> Edges { get { return _edges.AsReadOnly(); } }
        public ReadOnlyCollection<Face> Faces { get { return _faces.AsReadOnly(); } }
        public ReadOnlyCollection<Halfedge> Halfedges { get { return _halfedges.AsReadOnly(); } }
        public ReadOnlyCollection<Vertex> Vertices { get { return _vertices.AsReadOnly(); } }


        // CONSIDER: Update events
        //           Topology_Changed
        //           Geometry_Changed
        //           How to deal with additional changes triggered in derived classes?
        // CONSIDER: Selection management - (inside traits?)

        #region Core mesh update routines

        // CONSIDER: What is needed to keep a small undo stack? (IEditableObject?)
        //           Some algorithms want to try a few options one after another,
        //           e.g. check which edge to collapse, and then do only one.


        /// <summary>
        /// The most basic operation - create an isolated triangle
        /// VertexTraits are supplied in ccw direction
        /// Halfedges and vertices can be recovered from the face.
        /// Traits for edges and halfedges need to be set after.
        /// </summary>
        /// <param name="vt1">Traits of the first vertex. Recover the vertex as face.Bounding.Source</param>
        /// <param name="vt2">Traits of the second vertex. Recover the vertex as face.Bounding.Target</param>
        /// <param name="vt3"></param>
        /// <param name="ft">Newly created face.</param>
        /// <returns>The newly created face</returns>
        // [Euler Ops: makeVertexEdgeFaceShell - make three vertices and the face between them, return the face.] 
        public Face CreateTriangle(TVertexTraits vt1 = default(TVertexTraits),
                                    TVertexTraits vt2 = default(TVertexTraits),
                                    TVertexTraits vt3 = default(TVertexTraits),
                                    TFaceTraits ft = default(TFaceTraits))
        {
            var v1 = CreateVertex(vt1);
            var v2 = CreateVertex(vt2);

            var e12 = CreateEdgeInvalid(v1, v2);
            var e21 = e12.Opposite;
            v1.Outgoing = e12;
            v2.Outgoing = e21;

            // Hook up to self - to create an 'isolated edge' 
            // - invalid but expected by the CreateTriangle.
            e12.Next = e21;
            e12.Previous = e21;
            e21.Next = e12;
            e21.Previous = e12;

            var f = CreateTriangle(v1, v2, vt3, ft);

            // This is an isolated triangle
            AssertValid();
            return f;
        }

        // v1 and v2 successive vertices must be along a boundary, with the Halfedge from v1 to v2 having no face (on the left).
        // Then we create a new vertex 'outside' the boundary, create new edges and a face
        //   \  /        /    
        //    \/  old e / old edge
        //  v2 o <-----o v1     
        //      \ new /
        //   new \ f / new edge 
        //  edge  \ /    no face here...
        //         o v3
        public Face CreateTriangle(Vertex v1,
                                    Vertex v2,
                                    TVertexTraits vt3 = default(TVertexTraits),
                                    TFaceTraits ft = default(TFaceTraits))
        {
            Halfedge e12 = v1.HalfedgeTo(v2);
            Halfedge e21 = e12.Opposite;

            // Checks adjacency condition
            if (e12 == null) throw new InvalidOperationException("v1 and v2 must be neighbors - but no Halfedge found!");
            if (e12.Face != null) throw new InvalidOperationException("Halfedge from v1 to v2 must be a boundary - but a face was found for the Halfedge!");

            var v3 = CreateVertex(vt3);

            var e23 = CreateEdgeInvalid(v2, v3);
            var e32 = e23.Opposite;
            var e31 = CreateEdgeInvalid(v3, v1);
            var e13 = e31.Opposite;

            // New vertex won't have an Outgoing yet - e32 is good - it will be a boundary Halfedge
            v3.Outgoing = e32;
            // Fix v1 Outgoing to maintain Vertex boundary condition
            v1.Outgoing = e13;

            // This is the tricky part - updating the Halfedge adjacencies
            e32.Next = e12.Next;
            e13.Previous = e12.Previous;
            e12.Previous.Next = e13;
            e12.Next.Previous = e32;

            e12.Next = e23;
            e12.Previous = e31;

            e32.Previous = e13;
            e13.Next = e32;

            e23.Previous = e12;
            e23.Next = e31;

            e31.Previous = e23;
            e31.Next = e12;

            // This will go around fixing up the faces along the ring of halfedges.
            var f = CreateFace(e12, ft);

            e12.AssertValid();
            e21.AssertValid();
            e23.AssertValid();
            e32.AssertValid();
            e31.AssertValid();
            e13.AssertValid();
            AssertValid();

            return f;
        }

        // In a face, adds a new edge between two vertices, 
        // splitting the face in two.
        // Condition is that he1.Face == he2.Face

        // Returns the new halfedge from he1.Target to he2.Target, 
        // which has the new face on its left
        // [Euler Ops: makeEF(e0, e1, p) - where the edges define the left and right faces ???]
        //     v2    he2
        //      o<--------o
        //     /  ^  old    .
        //    .     \ face .      <<--(inside)-- e12: new edge between old face and new face
        //   . new f  \   /
        //     o-------> o new He.Source
        //       he1      v1
        //

        // old face might be null (outside the boundary)
        // Returns e12
        public Halfedge SplitFace(Halfedge he1, Halfedge he2, TFaceTraits ft = default(TFaceTraits))
        {
            if (he1.Face != he2.Face) throw new InvalidOperationException("Halfedges must be from the same Face");
            if (he1.Target.HalfedgeTo(he2.Target) != null) throw new InvalidOperationException("There is already an edge between the target vertices");

            var v1 = he1.Target;
            var v2 = he2.Target;

            // make a new edge, leaving a hold where the new face will go
            var e12 = CreateEdgeInvalid(v1, v2);
            var e21 = e12.Opposite;

            e21.Next = he1.Next;
            he1.Next.Previous = e21;
            he1.Next = e12;
            e21.Previous = he2;

            e12.Next = he2.Next;
            he2.Next.Previous = e12;
            he2.Next = e21;
            e12.Previous = he1;

            e21.Face = he2.Face; // Might be null for boundary.
            var f = CreateFace(e12, ft);

            return e12;
        }


        // Combines the faces adjacent to this halfedge into a single face
        // Returns the face that remains (it was to the left of he)
        // [Euler Ops: killEF(eNew)]
        //
        //             \     /
        //              \   /
        //               \ /
        //                o
        //                ^
        //                |
        //                |
        //             he |
        //                |
        //                |
        //                o   he.Source
        //               / \
        //              /   \
        //             /     \
        //
        public Face JoinFaces(Halfedge he)
        {
            if (he == null) throw new ArgumentNullException("he");
            if (he.Face == he.Opposite.Face) throw new ArgumentException("Cannot join face to itself"); // Maybe...
            if (he.IsBoundary) throw new ArgumentException("Halfedge cannot be a boundary edeg - it must have two adjacent faces to join.", "he");

            Face f = he.Face;

            // Fix the left Face to not point to the disppearing Halfedge
            if (he.Face.Bounding == he) he.Face.Bounding = he.Next;

            // Delete the right Face and re-direct the ring of Halfedges
            //  CONSIDER: Do the delete before or after?
            DeleteFace(he.Opposite.Face);
            foreach (var heRight in he.Opposite.Ring)
            {
                heRight.Face = he.Face;
            }

            // Fix up the Vertices
            // NOTE: he is not a boundary, so if he.Source is a Boundary vertex, then its Outgoing would not be he.
            if (he.Source.Outgoing == he) he.Source.Outgoing = he.NextAtSource;
            if (he.Target.Outgoing == he.Opposite) he.Target.Outgoing = he.NextAtTarget;

            // Fix up the Halfedges previous and next
            he.Previous.Next = he.NextAtSource;
            he.NextAtSource.Previous = he.Previous;

            he.Next.Previous = he.PreviousAtTarget;
            he.PreviousAtTarget.Next = he.Next;

            // And delete Edge
            DeleteEdge(he.Edge);

            AssertValid();
            return f;
        }

        // leftFace and rightFace (defined by he1 and he2) must be faces adjacent to vertex v. 
        // The may be adjacent or may be the same.
        // Creates a new target vertex, and an edge from source to target.
        // leftFace will be to the left of the new edge, rightFace to the right
        // and other faces distributed as needed.
        // If leftFace == rightFace (!= null) creates a vertex in the middle of the face 
        // with an edge pointing from sourceVertex into the face. (used by Catmull-Clark subdivision, apparently)
        //
        // Some inspiration from the blue/red pictures here: http://fgiesen.wordpress.com/2012/03/24/half-edge-based-mesh-representations-practice/
        // Going CCW, we take the first blue edge as he1, and after that the first black edge as he2.
        // For the degenerate case, we pass the first black edge after the (new) red edge for both he1 and he2.
        //
        // [Euler Ops: makeEV(e0, e1, p) - where the edges define the left and right faces ???]
        //
        //     \    /  
        //      \  / (v2)
        //    ----o<-----   
        //        |  he2 <-------- the face to the left of this Halfedge will be to the right of the new halfedge
        //        |
        //        |<-- new edge (e12 / e21)
        //        |
        //        o <-- new vertex (v1)
        //        ^ \
        //        |  \ <--  another edge that went to v2, but will now go to v1
        //        |   \
        //        |
        //        |<-- he1 (was pointing to v2) The face to the left of this Halfedge will be to the left of the new halfedge
        //
        // 
        //  Degenerate case:
        //
        //     \    /  
        //      \  / (v2)
        //    ----o<-----   
        //        |  he1 = he2
        //        |
        //        |<-- new edge (e12 / e21)
        //    ^   |
        //    |   o <-- new vertex (v1)        ^
        //    |                                |
        //    | ...   ....    ... ... ... ...  |
        //   ... all part of the existing face (!= null)
        //    
        // (Consider 'Restricted Vertex Split' PMP p 117 / Hoppe et al '93 - Mesh Optimization.)
        public Halfedge SplitVertex(Halfedge he1, Halfedge he2, TVertexTraits vt = default(TVertexTraits))
        {
            if (he1.Target != he2.Target) throw new ArgumentException("Halfedge Targets should be the same vertex.");
            if (he1 == he2 && he1.IsBoundary) throw new ArgumentException("For degenerate halfedges case , Halfedge cannot be on boundary.");

            var v2 = he1.Target;
            var v1 = CreateVertex(vt);
            var e12 = CreateEdgeInvalid(v1, v2);
            var e21 = e12.Opposite;

            v1.Outgoing = e12;

            // This happens to work right for the degenerate case too! 
            // (because he1 == he2, and the second batch of updates depends on the first.)
            e12.Next = he1.Next;
            he1.Next.Previous = e12;
            he1.Next = e12;
            e12.Previous = he1;

            e21.Next = he2.Next;
            he2.Next.Previous = e21;
            he2.Next = e21;
            e21.Previous = he2;

            // Target must change to v1 for the halfedges in [he1, he2) around v2
            // (or none in the degenerate case).
            var movedIncomings = v2.Incomings.Circulate(he1, he2);
            foreach (var he in movedIncomings)
            {
                he.Target = v1;
            }

            AssertValid();
            return e12;
        }

        // TODO: Need torus-making glue operation

        //// CONSIDER: Rather called collapse?
        //// If the either of the faces adjoining the edge is a triangle, the faces are deleted.
        /// Check conditions - PMP p 118
        //public virtual void CollapseEdge(Halfedge h)
        //{
        //}

        #endregion

        #region Private creation and deletion of components
        Vertex CreateVertex(TVertexTraits traits = default(TVertexTraits))
        {
            var v = new Vertex();
            _vertices.Add(v);

            v.Traits = traits;

            // OnVertexCreated(v);
            return v;
        }

        Halfedge CreateHalfedge(THalfedgeTraits traits = default(THalfedgeTraits))
        {
            var he = new Halfedge();
            _halfedges.Add(he);
            he.Traits = traits;

            // OnHalfedgeCreated(he);
            return he;
        }

        Edge CreateEdge(TEdgeTraits traits = default(TEdgeTraits))
        {
            var e = new Edge();
            _edges.Add(e);
            e.Traits = traits;

            // OnEdgeCreated(e);
            return e;
        }

        Face CreateFace(TFaceTraits traits = default(TFaceTraits))
        {
            var f = new Face();
            _faces.Add(f);
            f.Traits = traits;

            // OnFaceCreated(f);
            return f;
        }

        void DeleteEdge(Edge e)
        {
            // TODO: Check if this is OK.
            // OnEdgeDeleted(e);
            _halfedges.Remove(e.Half1);
            _halfedges.Remove(e.Half2);
            _edges.Remove(e);
        }


        void DeleteFace(Face f)
        {
            // TODO: Check if this is OK.
            // OnFaceDeleted(f);
            _faces.Remove(f);
        }

        //// TODO: Allow delete of non-isolated vertices???
        //public virtual void DeleteVertex(Vertex v)
        //{
        //    if (v == null) throw new ArgumentNullException("v");
        //    if (!v.IsIsolated()) throw new InvalidOperationException("Vertex must be isolated to delete");

        //    _vertices.Remove(v);

        //    OnVertexDeleted(v);
        //}

        // Takes two vertices that are NOT neighbours, creates an edge between them,
        // returns the Halfedge created from source to target.
        // Creates an edge and sets some of the adjacencies 
        // - does not leave the mesh Valid :
        //   **Next / Previous is not set**
        Halfedge CreateEdgeInvalid(Vertex source, Vertex target)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (target == null) throw new ArgumentNullException("target");

            // Check that target is not already connected to source

            //// Check - during construction might be isolated
            //// CONSIDER: Collect into an "UnderConstruction' method?
            //if (source.Outgoing != null && source.Outgoing.Target != null)
            //{
            //    foreach (var v in source.Neighbors)
            //    {
            //        if (v == target) throw new InvalidOperationException("target is already a neighbor of source");
            //    }
            //}

            var he1 = CreateHalfedge();
            he1.Target = target;

            var he2 = CreateHalfedge();
            he2.Target = source;

            he1.Opposite = he2;
            he2.Opposite = he1;

            var edge = CreateEdge();
            edge.Half1 = he1;
            he1.Edge = edge;
            he2.Edge = edge;

            return he1;
        }

        // From a halfedge that has no face, makes a face and attaches to the whole ring of halfedges

        /// <summary>
        /// Fill a boundary with a Face
        /// </summary>
        /// <param name="halfedge"></param>
        /// <param name="ft"> </param>
        /// <returns></returns>
        public Face CreateFace(Halfedge halfedge, TFaceTraits ft = default(TFaceTraits))
        {
            // !!! Check manifold conditions???

            if (halfedge == null) throw new ArgumentNullException("halfedge");
            if (halfedge.Face != null) throw new InvalidOperationException("Halfedge may not have a face already");

            Face face = new Face();
            face.Traits = ft;
            face.Bounding = halfedge;
            _faces.Add(face);

            foreach (var he in halfedge.Ring)
            {
                he.Face = face;
            }

            // OnFaceCreated(face);

            AssertValid();

            return face;
        }

        // Check for jutting out edges made by SplitVertex - not allowed 'outside'
        //public virtual void DeleteFace(Face face)
        //{
        //    // Makes a hole where a face was, fixing all halfedges along the perimeter.
        //    // If the face is isolated, deletes everything.
        //    // !!! Check manifold conditions
        //}



        #endregion

        #region Derived operations


        // Create a new triangle from v1 to v2 to v3, inserting a new edge from v1 to v2 
        // and a new face next to e12.
        //                    
        //   \  new edge           
        //  v2 o <-----o v1     
        //   /  \ new / \
        //   old \ f / old edge 
        //  edge  \ /    
        //         o v3

        // we do this by splitting the 'outside' region with a new edge from v1 to v2, giving the new face.
        // For this we need the edge from v3 to v1, and the Previous of v2 to v3
        public virtual Face CreateTriangle(Vertex v1,
                                           Vertex v2,
                                           Vertex v3,
                                           TFaceTraits ft = default(TFaceTraits))
        {
            var e23 = v2.HalfedgeTo(v3);
            var e31 = v3.HalfedgeTo(v1);
            var e12 = v1.HalfedgeTo(v2);

            if (e23 == null) throw new InvalidOperationException("v3 must be a neighbor of v2");
            if (e31 == null) throw new InvalidOperationException("v1 must be a neighbor of v3");
            if (e12 != null) throw new InvalidOperationException("v1 must not be a neighbor of v2 - edge exists already!");
            if (e23.Face != null) throw new InvalidOperationException("v2 and v3 must be on a boundary");
            if (e31.Face != null) throw new InvalidOperationException("v3 and v1 must be on a boundary");

            e12 = SplitFace(e31, e23.Previous, ft);
            return e12.Face;
        }

        // CONSIDER: This might be the better primitive.
        //   \     he           
        //  v2 o <-----o v1     
        //   /  \ new / \
        //   old \ f / old edge 
        //  edge  \ /    
        //         o v3 (new vertex)
        // Create a new triangle in the boundary region to the left of Halfedge he.
        public Face CreateTriangle(Halfedge he,
                                    TVertexTraits vt3 = default(TVertexTraits),
                                    TFaceTraits ft = default(TFaceTraits))
        {
            if (!he.IsBoundary) throw new ArgumentException("Halfedge must be a boundary", "he");

            var v1 = he.Source;
            var v2 = he.Target;
            return CreateTriangle(v1, v2, vt3, ft);
        }

        // CONSIDER: More directed insertion
        // Could be called SplitEdge??
        public Vertex InsertVertex(Edge e)
        {
            return null;
        }

        public Vertex InsertVertex(Face face)
        {
            return null;
        }

        public bool IsEmpty()
        {
            return Vertices.Count == 0;
        }


        #endregion



        public virtual void DoStuff(Vertex v) { }

        [Conditional("DEBUG")]
        void AssertValid()
        {
            foreach (var v in Vertices)
            {
                v.AssertValid();
            }

            foreach (var he in Halfedges)
            {
                he.AssertValid();

                // Check for unwell ring iteration.
                int count = 0;
                foreach (var her in he.Ring)
                {
                    count++;
                    Debug.Assert(count <= Halfedges.Count / 2);
                }
            }
        }

    }

    public struct NullTraits { }

    // EXPERIMENTS
    // TODO: Move...?
    public enum AdvancingFrontState
    {
        FarAway,
        Near,
        Asssigned
    }

    public class DynamicTraits<TEdgeTraits, TFaceTraits, THalfedgeBase, TVertexTraits> where TVertexTraits : IPoint
    {
        public Dictionary<PointTriangleMesh<TEdgeTraits, TFaceTraits, THalfedgeBase, TVertexTraits>.Vertex, AdvancingFrontState> frontStates =
            new Dictionary<PointTriangleMesh<TEdgeTraits, TFaceTraits, THalfedgeBase, TVertexTraits>.Vertex, AdvancingFrontState>();

    }

    public static class CirculatorExtensions
    {
        public static IEnumerable<T> Circulate<T>(this IEnumerable<T> items)
        {
            while (true)
            {
                foreach (var item in items)
                {
                    yield return item;
                }
            }
        }

        public static IEnumerable<T> Circulate<T>(this IEnumerable<T> items, T startInclusive, T endExclusive)
        {
            return items.Circulate()
                    .SkipWhile(item => !EqualityComparer<T>.Default.Equals(item, startInclusive))
                    .TakeWhile(item => !EqualityComparer<T>.Default.Equals(item, endExclusive));
        }
    }
}
