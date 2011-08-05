using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace ColladaXna.Base.Geometry
{
    public class VertexData
    {
        public static int GetStride(VertexDataType type)
        {
            switch (type)
            {
                case VertexDataType.Position:
                    return 3;
                case VertexDataType.Color:
                    return 4;
                case VertexDataType.Normal:
                    return 3;
                case VertexDataType.Tangent:
                    return 3;
                case VertexDataType.Binormal:
                    return 3;
                case VertexDataType.TexCoord:
                    return 2;
                case VertexDataType.TexCoord2:
                    return 2;
                case VertexDataType.JointIndices:
                    return 4;
                case VertexDataType.JointWeights:
                    return 4;
                default:
                    return 0;
            }
        }        
    }

    public enum VertexDataType
    {
        Position,
        Color,
        Normal,
        Tangent,
        Binormal,
        TexCoord,
        TexCoord2,
        JointIndices,
        JointWeights
    }
}
