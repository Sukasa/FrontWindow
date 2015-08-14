using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace OpenGLFrontWindow.Panes
{
    public abstract class PaneBase
    {
        public Rectangle Position;
        public Vertex[] Vertexes;
        public Int32 ZIndex;
        public bool Ready;

        private int VertexBuffer;

        public abstract bool Init();

        public abstract void Tick();

        public abstract void Render();

        public abstract void Event(dynamic EventData);

        public int LoadTexture(string Filename)
        {
            int TextureId = GL.GenTexture();
            Bitmap Image = new Bitmap(Filename);
            BitmapData BD = Image.LockBits(new Rectangle(0, 0, Image.Width, Image.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.BindTexture(TextureTarget.Texture2D, TextureId);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Image.Width, Image.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Rgba, PixelType.UnsignedByte, BD.Scan0);
            
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);

            return TextureId;
        }

        public void UpdateVertices()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(Vertexes.Length * Marshal.SizeOf(typeof(Vertex))), Vertexes, BufferUsageHint.StreamDraw);
        }

        public void RenderVertices(PrimitiveType RenderMode)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBuffer);
            Program.BindVertexAndUniforms();

            GL.DrawArrays(RenderMode, 0, Vertexes.Length);
        }

        public void BindTexture(int TextureId)
        {
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, TextureId);
            GL.BindSampler(0, Program.SamplerId);
        }

        public bool BaseInit()
        {
            VertexBuffer = GL.GenBuffer();

            return Init();
        }
    }
}
