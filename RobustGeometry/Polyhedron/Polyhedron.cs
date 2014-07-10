using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RobustGeometry.Polyhedron
{
    // Super-simple BSP-based JavaScript: http://evanw.github.com/csg.js
    // Remember the 'carve' project - http://carve.googlecode.com/hg/doc/carve.pdf
    // https://github.com/mcneel/rhinocommon
    // http://stackoverflow.com/questions/2660053/net-geometry-library
    // More pointers here: http://stackoverflow.com/questions/2002976/constructive-solid-geometry-mesh

    // A Polyhedron can be open or closed, and if closed can be the finite enclosed volume or the infinite unenclosed volume.
    // Polyhedron does not allow self-intersection of faces.

    // We want to allow basic Polyhedron operations like
    // UNION, INTERSECTION, DIFFERENCE (A_MINUS_B), SYMMETRIC_DIFFERENCE?

    // We should consider how to allow attribute (trait) interpolation - a combined verteex might take on interpolated values.
    class Polyhedron
    {
    }
}
