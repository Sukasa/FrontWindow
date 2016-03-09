using STOMP.Frames;
using System;
using System.Drawing;

namespace OpenGLFrontWindow.Panes
{
    class RejectedPane : PaneBase
    {
        private static bool _Visible;
        private static DateTime? _ShowTime;
        private static Bitmap _Window;
        private static int _WindowTextureId;

        protected internal override bool Init()
        {
            RectangleF Rect = new RectangleF(10, 300, Program.Configuration.Width - 220, Program.Configuration.Height - 280);
            _Window = CreateWindow((int)Rect.Width, (int)Rect.Height);

            Bitmap Checkbox = new Bitmap("Textures\\Rejected.png");
            Graphics G = Graphics.FromImage(_Window);
            G.DrawImage(Checkbox, (_Window.Width - Checkbox.Width) / 2, (_Window.Height - Checkbox.Height) / 2);
            G.Dispose();

            _WindowTextureId = LoadTexture(_Window);
            CreateWindowVertices(Rect);

            return true;
        }

        protected internal override void Tick()
        {
            if (_ShowTime.HasValue && (DateTime.Now - _ShowTime.Value).TotalSeconds > 4)
            {
                _Visible = false;
                _ShowTime = null;
            }
        }

        protected internal override void Render()
        {
            if (_Visible)
            {
                BindTexture(_WindowTextureId);
                RenderVertices(OpenTK.Graphics.OpenGL.PrimitiveType.Quads);
            }
        }

        protected internal override void Event(STOMP.StompMessageEventArgs EventData)
        {
            if (((StompMessageFrame)EventData.Frame).BodyText.Contains("Rejected"))
            {
                _Visible = true;
                _ShowTime = DateTime.Now;
            }
        }
    }
}
