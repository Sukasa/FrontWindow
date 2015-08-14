using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;

namespace OpenGLFrontWindow
{
    [StructLayout(LayoutKind.Explicit)]
    public struct Vertex
    {
        [FieldOffset(0)]
        public Vector2 Position;

        [FieldOffset(8)]
        public Vector4 Color;
        
        [FieldOffset(24)]
        public Vector2 TextureCoords;
    }
}
