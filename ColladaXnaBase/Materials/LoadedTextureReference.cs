using Microsoft.Xna.Framework.Graphics;

namespace ColladaXna.Base.Materials
{
    /// <summary>
    /// A reference to a loaded texture (during runtime),
    /// consisting of filename, texture channel and 
    /// a texture object.
    /// </summary>
    public class LoadedTextureReference : TextureReference
    {
        private Texture2D _texture;

        ///<summary>
        ///</summary>
        ///<param name="filename"></param>
        ///<param name="texChannel"></param>
        ///<param name="texture"></param>
        public LoadedTextureReference(string filename, string texChannel, Texture2D texture)
        {
            _texture = texture;
            _filename = filename;
            _textureChannel = texChannel;
        }

        public Texture2D Texture { get { return _texture; } }
    }
}
