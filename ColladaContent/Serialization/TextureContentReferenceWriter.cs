using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

using TWrite = Seafarer.Xna.Collada.Content.Data.TextureContentReference;
using Seafarer.Xna.Collada.Importer.Materials;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace Seafarer.Xna.Collada.Content.Serialization
{
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content Pipeline
    /// to write the specified data type into binary .xnb format.
    ///
    /// This should be part of a Content Pipeline Extension Library project.
    /// </summary>
    [ContentTypeWriter]
    public class TextureContentReferenceWriter : ContentTypeWriter<TWrite>
    {
        protected override void Write(ContentWriter output, TWrite value)
        {
            output.Write(value.Filename);
            output.Write(value.TextureChannel);
            output.WriteExternalReference(value.ExternalReference);
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {            
            return "Seafarer.Xna.Collada.Importer.Serialization.TextureContentReferenceReader, ColladaImporter";
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return typeof(LoadedTextureReference).AssemblyQualifiedName;
        }
    }
}
