using System;
using System.Collections.Generic;
using Omi.Xna.Collada.Model.Materials;
using Microsoft.Xna.Framework;

namespace Omi.Xna.Collada.Model
{
    public class ModelMesh
    {
        public String Name { get; set; }

        public List<ModelMeshPart> Parts { get; set; }

        public List<EffectMaterial> Materials { get; set; }

        /// <summary>
        /// List of instances that use this mesh.
        /// </summary>
        public List<ModelMeshInstance> Instances { get; set; }

        public BoundingBox Bounds { get; set; }
    }
}
