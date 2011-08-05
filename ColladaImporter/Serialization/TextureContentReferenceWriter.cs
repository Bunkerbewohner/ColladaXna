using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

using TWrite = Omi.Xna.Collada.Importer.Data.TextureContentReference;
using Omi.Xna.Collada.Model.Materials;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace Omi.Xna.Collada.Importer.Serialization
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
            return "Omi.Xna.Collada.Model.Deserialization.TextureContentReferenceReader, OmiXnaColladaModel";
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return typeof(LoadedTextureReference).AssemblyQualifiedName;
        }
    }
}
