using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Seafarer.Xna.Collada.Importer.Geometry
{
    /// <summary>
    /// A part of a mesh which uses a specific material which is 
    /// different from the material used by other mesh parts.
    /// </summary>
    public class MeshPart
    {
        /// <summary>
        /// Reference to a vertex container which contains at least the used
        /// vertices of this mesh part. The mesh container may contain more
        /// vertices if it is shared among different mesh parts.
        /// </summary>
        public VertexContainer Vertices;

        /// <summary>
        /// Indices refering to vertices in the used vertex container.
        /// Every three indices describe one triangle of the mesh part.
        /// </summary>
        public int[] Indices;

        /// <summary>
        /// Name of the material used by this mesh part
        /// </summary>
        public String MaterialName;
    }
}
