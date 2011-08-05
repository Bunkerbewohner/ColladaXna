using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using System.Drawing;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using System.Drawing.Imaging;

namespace Omi.Xna.Collada.Importer
{

    [ContentImporter(".tif", ".tiff", DisplayName = "Image Importer", DefaultProcessor = "TextureProcessor")]
    public class ImageImporter : ContentImporter<Texture2DContent>
    {
        public override Texture2DContent Import(string filename, ContentImporterContext context)
        {
            Bitmap bitmap = Image.FromFile(filename) as Bitmap;
            var bitmapContent = new PixelBitmapContent<Microsoft.Xna.Framework.Color>(bitmap.Width, bitmap.Height);

            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height; j++)
                {
                    System.Drawing.Color from = bitmap.GetPixel(i, j);
                    Microsoft.Xna.Framework.Color to = new Microsoft.Xna.Framework.Color(from.R, from.G, from.B, from.A);
                    bitmapContent.SetPixel(i, j, to);
                }
            }

            return new Texture2DContent()
            {
                Mipmaps = new MipmapChain(bitmapContent)
            };
        }

        public static Texture2D LoadImage(string path, GraphicsDevice graphicsDevice)
        {
            Bitmap bitmap = Image.FromFile(path) as Bitmap;
            int bufferSize = bitmap.Height * bitmap.Width * 4;
            Texture2D texture = null;

            using (MemoryStream memStream = new MemoryStream(bufferSize))
            {
                bitmap.Save(memStream, ImageFormat.Png);
                texture = Texture2D.FromStream(graphicsDevice, memStream); 
            }

            return texture;
        }

        public static Texture2D LoadImage(Stream stream, GraphicsDevice graphicsDevice)
        {
            Bitmap bitmap = Image.FromStream(stream) as Bitmap;
            int bufferSize = bitmap.Height * bitmap.Width * 4;
            Texture2D texture = null;

            using (MemoryStream memStream = new MemoryStream(bufferSize))
            {
                bitmap.Save(memStream, ImageFormat.Png);
                texture = Texture2D.FromStream(graphicsDevice, memStream);
            }

            return texture;
        }
    }
}
