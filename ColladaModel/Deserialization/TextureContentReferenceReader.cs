using Omi.Xna.Collada.Model.Materials;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Omi.Xna.Collada.Model.Deserialization
{
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content
    /// Pipeline to read the specified data type from binary .xnb format.
    /// 
    /// Unlike the other Content Pipeline support classes, this should
    /// be a part of your main game project, and not the Content Pipeline
    /// Extension Library project.
    /// </summary>
    public class TextureContentReferenceReader : ContentTypeReader<LoadedTextureReference>
    {
        protected override LoadedTextureReference Read(ContentReader input, LoadedTextureReference existingInstance)
        {            
            string filename = input.ReadString();
            string textureChannel = input.ReadString();
            Texture2D texture = input.ReadExternalReference<Texture2D>();

            return new LoadedTextureReference(filename, textureChannel, texture);
        }
    }
}
