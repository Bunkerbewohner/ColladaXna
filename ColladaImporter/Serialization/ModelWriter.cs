using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Omi.Xna.Collada.Importer;
using Microsoft.Xna.Framework;
using Omi.Xna.Collada.Model.Materials;
using Omi.Xna.Collada.Model.Geometry;
using Omi.Xna.Collada.Importer.Data;
using Omi.Xna.Collada.Model.Lighting;
using Omi.Xna.Collada.Model.Animation;
using System.Diagnostics;
using System.IO;
using Microsoft.Xna.Framework.Content.Pipeline;
using Omi.Xna.Collada.Model;

namespace Omi.Xna.Collada.Importer.Serialization
{
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content Pipeline
    /// to write the specified data type into binary .xnb format.
    ///
    /// This should be part of a Content Pipeline Extension Library project.
    /// </summary>
    [ContentTypeWriter]
    public class ModelWriter : ContentTypeWriter<IntermediateModel>
    {
        protected override void Write(ContentWriter output, IntermediateModel value)
        {            
            // Name
            output.Write(Path.GetFileNameWithoutExtension(value.SourceFilename));

            // 1. Materials (should have been processed before)
            var processedMaterials = from m in value.Materials select m as CompiledMaterial;
            output.WriteObject<List<CompiledMaterial>>(processedMaterials.ToList());

            // 2. Meshes
            output.WriteObject<List<Mesh>>(value.Meshes);

            // 3. MeshInstances
            output.WriteObject<List<MeshInstance>>(value.MeshInstances);       
     
            // 4. Lights
            output.WriteObject<List<Light>>(value.Lights);

            // 5. Skeleton (if there is one)                        
            output.WriteObject<JointList>(value.Joints);

            // 6. Joint Animations (if there are any)
            output.WriteObject<JointAnimationList>(value.JointAnimations);
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "Omi.Xna.Collada.Model.Deserialization.ModelReader, OmiXnaColladaModel";
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {            
            return typeof(ModelData).AssemblyQualifiedName;
        }
    }
}
