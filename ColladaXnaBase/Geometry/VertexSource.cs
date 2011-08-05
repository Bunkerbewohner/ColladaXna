using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace ColladaXna.Base.Geometry
{
    /// <summary>
    /// Represents one group of vertex data, e.g. positions, normals etc.
    /// </summary>
    public class VertexSource
    {
        public String GlobalID;        

        public int Stride;

        public float[] Data;

        public int Count { get { return Data.Length / Stride; } }    
    }   
}
