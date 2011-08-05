using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Xna.Framework.Content;

namespace Omi.Xna.Collada.Model.Materials
{
    ///<summary>
    ///</summary>    
    public class TextureReference
    {
        protected string _filename;
        protected string _textureChannel;

        /// <summary>
        /// Relative Path to texture file (relative from model)
        /// </summary>
        public string Filename
        {
            get
            {
                return _filename;
            }
            set
            {
                _filename = value.Replace("file:///", "");
            }
        }

        /// <summary>
        /// Texture Channel, usually CHANNEL1
        /// </summary>
        public string TextureChannel
        {
            get
            {
                return _textureChannel;
            }
            set
            {
                _textureChannel = value;
            }
        }

        public TextureReference()
        {
        }

        public TextureReference(string filename)
        {
            Filename = filename;
            TextureChannel = "CHANNEL1";
        }

        public TextureReference(string filename, string channel)            
        {
            Filename = filename;
            TextureChannel = channel;
        }

        public bool IsEmpty { get { return Filename == null || Filename.Length == 0; } }
    }    
}
