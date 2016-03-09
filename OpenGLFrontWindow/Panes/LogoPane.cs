using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Drawing.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using System.Threading.Tasks;

namespace OpenGLFrontWindow.Panes
{
    class LogoPane : PaneBase
    {
        protected internal override void Event(STOMP.StompMessageEventArgs EventData)
        {
            // Do nothing
        }

        private Bitmap Text;
        private Bitmap Window;
        private Bitmap Logo;

        private int WindowTextureId;
        private int TextTextureId;
        private int LogoTextureId;

        protected internal override bool Init()
        {
            Vertexes = new Vertex[12];

            Window = CreateWindow(Program.Configuration.Width - 30, 180);
            Text = new Bitmap(Program.Configuration.Width - 30, 180);
            Logo = new Bitmap("Textures/logo_small.png");

            SizeF Size = MeasureText("MakerSpace Nanaimo", Text, "Calibri", 81);
            RenderText("MakerSpace Nanaimo", Text, Brushes.WhiteSmoke, "Calibri", 81, (int)(Text.Width - Size.Width - Logo.Width) / 2 + Logo.Width, (int)(180 - Size.Height) / 2);

            WindowTextureId = LoadTexture(Window);
            TextTextureId = LoadTexture(Text);
            LogoTextureId = LoadTexture(Logo);

            Vertexes[0].Position = Vertexes[4].Position = Vertexes[8].Position = new Vector2(15f, (float)Program.Configuration.Height - 210f);
            Vertexes[1].Position = Vertexes[5].Position = new Vector2((float)Program.Configuration.Width - 15f, (float)Program.Configuration.Height - 210f);
            Vertexes[2].Position = Vertexes[6].Position = new Vector2((float)Program.Configuration.Width - 15f, (float)Program.Configuration.Height - 10f);
            Vertexes[3].Position = Vertexes[7].Position = Vertexes[11].Position = new Vector2(15, (float)Program.Configuration.Height - 10f);

            Vertexes[9].Position = new Vector2(215f, (float)Program.Configuration.Height - 210f);
            Vertexes[10].Position = new Vector2(215f, (float)Program.Configuration.Height - 10f);

            Vertexes[0].TextureCoords = Vertexes[4].TextureCoords = Vertexes[8].TextureCoords = new Vector2(0.0f, 1.0f);
            Vertexes[1].TextureCoords = Vertexes[5].TextureCoords = Vertexes[9].TextureCoords = new Vector2(1.0f, 1.0f);
            Vertexes[2].TextureCoords = Vertexes[6].TextureCoords = Vertexes[10].TextureCoords = new Vector2(1.0f, 0.0f);
            Vertexes[3].TextureCoords = Vertexes[7].TextureCoords = Vertexes[11].TextureCoords = new Vector2(0.0f, 0.0f);

            for (int i = 0; i < 12; i++)
                Vertexes[i].Color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);

            UpdateVertices();

            return true;
        }

        protected internal override void Render()
        {
            BindTexture(WindowTextureId);
            RenderVertices(PrimitiveType.Quads, 0, 4);

            BindTexture(TextTextureId);
            RenderVertices(PrimitiveType.Quads, 4, 4);

            BindTexture(LogoTextureId);
            RenderVertices(PrimitiveType.Quads, 8, 4);
        }

        protected internal override void Tick()
        {
            // Do nothing
        }
    }
}
