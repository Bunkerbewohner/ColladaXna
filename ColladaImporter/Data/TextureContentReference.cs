using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Omi.Xna.Collada.Model.Materials;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace Omi.Xna.Collada.Importer.Data
{
    public class TextureContentReference : TextureReference
    {
        public ExternalReference<TextureContent> ExternalReference;

        public TextureContentReference(TextureReference texture,
            ExternalReference<TextureContent> externalReference)
        {
            this.Filename = texture.Filename;
            this.TextureChannel = texture.TextureChannel;
            this.ExternalReference = externalReference;
        }

    }
}
