using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

using TWrite = Omi.Xna.Collada.Importer.Data.CompiledMaterial;
using Omi.Xna.Collada.Model.Materials;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

namespace Omi.Xna.Collada.Importer.Serialization
{
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content Pipeline
    /// to write the specified data type into binary .xnb format.
    ///
    /// This should be part of a Content Pipeline Extension Library project.
    /// </summary>
    [ContentTypeWriter]
    public class MaterialWriter : ContentTypeWriter<TWrite>
    {
        protected override void Write(ContentWriter output, TWrite value)
        {
            // Name
            output.Write(value.Name);

            // Properties
            output.WriteObject<List<MaterialProperty>>(value.Properties);

            output.WriteExternalReference<CompiledEffectContent>(value.Effect);
            output.WriteObject<Dictionary<String,Object>>(value.EffectParameters);
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "Omi.Xna.Collada.Model.Deserialization.MaterialReader, OmiXnaColladaModel";
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {            
            return typeof (Omi.Xna.Collada.Model.Materials.EffectMaterial).AssemblyQualifiedName;            
        }
    }
}
