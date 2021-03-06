﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ColladaXna.Base.Geometry
{
    /// <summary>
    /// Index Buffer with variable data: It may contain
    /// 16-bit or 32-bit indices (short or int)
    /// </summary>
    public class VariableIndexBuffer
    {
        private short[] _data16;
        private int[] _data32;

        public short[] Data16 { get { return _data16; } }
        public int[] Data32 { get { return _data32; } }

        public bool IsShort { get { return _data16 != null; } }

        public VariableIndexBuffer(int[] indices)
        {
            _data32 = new int[indices.Length];
            Array.Copy(indices, _data32, indices.Length);
        }

        public VariableIndexBuffer(short[] indices)
        {
            _data16 = new short[indices.Length];
            Array.Copy(indices, _data16, indices.Length);
        }

    }
}
