using OpenTK.Graphics.OpenGL;
using OpenTK;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using STOMP;

namespace OpenGLFrontWindow.Panes
{
    public abstract class PaneBase
    {
        // OpenGL and ordering information
        protected internal Vertex[] Vertexes;
        protected internal Int32 ZIndex;
        protected internal int VertexBuffer;

        // Internal bitmap components for window creation
        private static Bitmap[] WindowComponents;

        // Functions to be implemented by derived classes
        protected internal abstract bool Init();
        protected internal abstract void Tick();
        protected internal abstract void Render();
        protected internal abstract void Event(StompMessageEventArgs EventData);

        // Texture handling
        protected int LoadTexture(string Filename, bool Repeating = false)
        {
            return LoadTexture(new Bitmap(Filename), Repeating);
        }
        protected int LoadTexture(Bitmap Image, bool Repeating = false)
        {
            int TextureId = GL.GenTexture();
            BitmapData BD = Image.LockBits(new Rectangle(0, 0, Image.Width, Image.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.BindTexture(TextureTarget.Texture2D, TextureId);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Image.Width, Image.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, BD.Scan0);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);

            if (Repeating)
            {
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            }
            else
            {
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Clamp);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Clamp);
            }


            return TextureId;
        }

        protected void CreateRectangle(RectangleF Rect, int Offset = 0)
        {
            // Set up array if needed
            if (Vertexes == null)
            {
                Vertexes = new Vertex[Offset + 4];
            }
            else if (Vertexes.Length < Offset + 4)
            {
                Vertex[] New = new Vertex[Offset + 4];
                Array.Copy(Vertexes, New, Vertexes.Length);
                Vertexes = New;
            }

            // Create Vertices
            Vertexes[Offset].Position = new Vector2(Rect.Left, Program.Configuration.Height - Rect.Bottom);
            Vertexes[Offset + 1].Position = new Vector2(Rect.Right, Program.Configuration.Height - Rect.Bottom);
            Vertexes[Offset + 2].Position = new Vector2(Rect.Left, Program.Configuration.Height - Rect.Top);
            Vertexes[Offset + 3].Position = new Vector2(Rect.Right, Program.Configuration.Height - Rect.Top);

            Vertexes[Offset].TextureCoords = new Vector2(0.0f, 1.0f);
            Vertexes[Offset + 1].TextureCoords = new Vector2(1.0f, 1.0f);
            Vertexes[Offset + 2].TextureCoords = new Vector2(1.0f, 0.0f);
            Vertexes[Offset + 3].TextureCoords = new Vector2(0.0f, 0.0f);

        }

        // Rendering utility functions
        protected void UpdateVertices()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(Vertexes.Length * Marshal.SizeOf(typeof(Vertex))), Vertexes, BufferUsageHint.StreamDraw);
        }
        protected void RenderVertices(PrimitiveType RenderMode, int Offset = 0, int Count = -1)
        {
            if (Count < 0)
                Count = Vertexes.Length;

            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBuffer);
            Program.BindVertexAndUniforms();

            GL.DrawArrays(RenderMode, Offset, Count);
        }
        protected void BindTexture(int TextureId)
        {
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, TextureId);
            GL.BindSampler(0, Program.SamplerId);
        }

        // Text rendering functions
        protected SizeF MeasureText(string Text, Bitmap Canvas, string FontName = "Calibri", int FontSize = 12)
        {
            Graphics Renderer = Graphics.FromImage(Canvas);

            Renderer.SmoothingMode = SmoothingMode.AntiAlias;
            Renderer.InterpolationMode = InterpolationMode.HighQualityBicubic;
            Renderer.PixelOffsetMode = PixelOffsetMode.HighQuality;

            SizeF Size = Renderer.MeasureString(Text, new Font(FontName, FontSize));
            Renderer.Dispose();

            return Size;
        }
        protected void RenderText(string Text, Bitmap Canvas, Brush DrawBrush, string FontName = "Calibri", int FontSize = 12, int XOffset = 0, int YOffset = 0)
        {
            RectangleF Bounds = new RectangleF(XOffset, YOffset, Canvas.Width - XOffset, Canvas.Height - YOffset);

            Graphics Renderer = Graphics.FromImage(Canvas);

            Renderer.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            Renderer.SmoothingMode = SmoothingMode.HighQuality;
            Renderer.InterpolationMode = InterpolationMode.HighQualityBicubic;

            Renderer.PixelOffsetMode = PixelOffsetMode.HighQuality;
            Renderer.DrawString(Text, new Font(FontName, FontSize), DrawBrush, Bounds);

            Renderer.Flush();
            Renderer.Dispose();
        }

        // Themed window creation function
        protected Bitmap CreateWindow(int Width, int Height)
        {
            Bitmap Canvas = new Bitmap(Width, Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Graphics Renderer = Graphics.FromImage(Canvas);

            Brush DrawColour = Brushes.Gray;

            Renderer.FillPie(DrawColour, 0, 0, 40, 40, 180, 90);
            Renderer.FillPie(DrawColour, Width - 40, 0, 40, 40, 270, 90);
            Renderer.FillPie(DrawColour, 0, Height - 40, 40, 40, 90, 90);
            Renderer.FillPie(DrawColour, Width - 40, Height - 40, 40, 40, 0, 90);

            Renderer.FillRectangle(DrawColour, 20, 0, Width - 40, 20);
            Renderer.FillRectangle(DrawColour, 0, 20, Width, Height - 40);
            Renderer.FillRectangle(DrawColour, 20, Height - 20, Width - 40, 20);

            Renderer.Dispose();
            return Canvas;
        }

        protected int CreateWindowVertices(RectangleF Position, int VertexOffset = 0)
        {
            CreateRectangle(Position, VertexOffset);

            return 4;
        }

        // Internal framework
        internal bool BaseInit()
        {
            VertexBuffer = GL.GenBuffer();
            try
            {
                return Init();
            }
            catch
            {
                return false;
            }
        }

        internal static void LoadWindowResources()
        {
            Bitmap[] Components = new Bitmap[1];

            WindowComponents = Components;
        }
    }
}
