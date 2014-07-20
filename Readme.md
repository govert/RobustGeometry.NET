# RobustGeometry.NET

RobustGeometry.NET is a computational geometry software project with the following goals:

* Implement basic geometric primitives and polygon mesh processing.
* Based on a robust numerical foundation.
* Target the .NET platform (and Mono).
* Published under a permissive open-source license allowing commercial use.

## Current status

* A C# port of the robust predicates of Shewchuk is done. It is complete but needs a bit more testing. To test this and understand the status of floating-point computation in .NET, there are some utility classes in the test project to access the Intel x86 FPU control states, and manipulate floating-point binary representations of doubles.

* A first implementation of the Halfedge data structure is done.

## License

RobustGeometry.NET is published under the MIT license.

The original C code for the robust geometric predicates was placed in the public domain by Jonathan Richard Shewchuk.

The elegant .NET generics design for the halfedge data structure is [described here](http://www.dgp.toronto.edu/~alexk/) by Alexander Kolliopoulos and implemented in his Lydos library, is published under the zlib license.

## Roadmap

The following are required for my own near-term use, so I will be spending some time on them.

* Expand operations and tests on Halfedge-based meshes.
* Implement basic surface mesh processing.
* Add polyhedron geometry and CSG operations.
* WPF (3D) integration with the [Helix 3D Toolkit](http://helixtoolkit.codeplex.com) for visualisation.
* Delaunay and constrainted Delaunay triangulations, perhaps based on Triangle.NET (https://triangle.codeplex.com/).

To give some idea of the type of application I require this for, I deal with meshes with 50,000 - 100,000 vertices, and need not do any real-time processing. For me the focus is on an implementation that is robust enough to use in practice (no unexpected numerical problems), yet simple enough to maintain and occasionally grow over the long term (not too much magic).

Targeting the .NET platform is an explicit goal; while the initial work is in C#, I would welcome having parts of the library implemented in F#. Configuring as a .NET portable library, or targeting the Mono runtime, would be nice but are not priorities for me.

In principle, I would prefer a smaller selection of simple algorithms implemented in .NET, rather than a set of wrapper around extensive, complex or highly optimised C++ libraries.

## Getting involved

I think there is a real need for a library like this. I have limited time and expertise to devote to it, so progress from my side is likely to be sporadic. But this is stuff I really need and use, so I have a long-term interest in the continuity and success of this project. 

Your interest and help in this project would be greatly appreciated. Get in touch with me, use the library, fork the source, and submit pull request. I'm new to github, so be a bit patient with me too ;-)

I would really like for this to become a community project. Any advice or input on improvement that would stimulate involvement would be very welcome.

The following is a vague list of future directions for work that would enhance the project or complement the project and my interests.

* Improved testing and documentation throughout.
* Performance measurement and optimisation throughout.
* Improved build and release infrastructure, publish to NuGet.
* Experiment with halfedge structure with indexing / handles instead of references (for 64-bit).
* Import / export of meshes from standard formats. 
* Level set methods like fast marching based on the mesh data structure.
* Triangle mesh refinement.
* Geometric primitives, basic distance intersections etc.
* General polygon mesh processing.
* Solid modelling and polyhedron primitives.

### Contributor guidelines

Please be mindful of the license conditions of other software. To keep this library available under a permissive license that allows free commercial use, code licensed under GPL, LGPL or similar licensed must be avoided. This includes the code from libraries like CGAL and OpenMesh.

I believe BSD and MS-PL licenses would be compatible with this project, which means code from the [ROS robotics library](http://www.ros.org) or the [Point Cloud Library](http://pointclouds.org) could be ported or used here with proper attribution.
