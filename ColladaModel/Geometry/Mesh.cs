using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Omi.Xna.Collada.Model.Geometry
{
    /// <summary>
    /// Design-time mesh representation
    /// </summary>
    public class Mesh
    {
        /// <summary>
        /// Name of this mesh. Is used as identifier.
        /// </summary>
        public String Name;

        /// <summary>
        /// List of vertex containers (design-time equivalent of vertex buffers).
        /// Mesh parts hold references to one of these. It's also possible that
        /// all mesh parts share the samer container and thus will be using
        /// the same vertex buffer. This is only possible if all mesh parts
        /// share the same vertex type.
        /// </summary>
        public VertexContainer[] VertexContainers;

        /// <summary>
        /// Stores mesh different mesh parts. Mesh parts are parts of the same
        /// mesh that may use different materials or even different vertex types.
        /// For example one mesh part might use normal mapping and thus needs
        /// tangents, while an other doesn't. In this case each mesh part 
        /// uses its own vertex container rather than sharing the same.
        /// A mesh that uses only one material will contain only one mesh part.
        /// </summary>
        public MeshPart[] MeshParts;

        /// <summary>
        /// Bounds of the mesh in form of a box that contains all of its geometry
        /// </summary>
        public BoundingBox Bounds;
    }
}
