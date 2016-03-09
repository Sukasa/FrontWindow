using System;
using System.Drawing;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenGLFrontWindow.Panes
{
    class EventsPane : PaneBase
    {
        private int WindowTextureId;
        private Bitmap EventsText;

        protected internal override bool Init()
        {
            int Verts = CreateWindowVertices(new RectangleF(Program.Configuration.Width - 200, 300, 180, Program.Configuration.Height - 320));


            CreateRectangle(new RectangleF(0, 0, 0, 0), Verts); // Text
            WindowTextureId = LoadTexture(CreateWindow(Program.Configuration.Width - 30, 180));

            


            return false;
        }

        protected void MakeEventsTextBitmap()
        {
            // Get events text


            // For each header / body component, get the respective heights
            Size MeasuredSize = GetTextSize(170, "", false);

            // Add up heights + spacing + buffer, create bitmap

            // Render text to bitmap

            // Upload to GPU

        }

        protected Size GetTextSize(int MaxWidth, string Text, bool Header)
        {
            Graphics G = Graphics.FromImage(EventsText);

            Font F = new Font("Arial", 12, Header ? FontStyle.Bold : FontStyle.Regular);

            G.MeasureString(Text, F, MaxWidth);
            return new Size(MaxWidth, 0);
        }

        protected internal override void Tick()
        {
            // TODO Scroll Text

            // TODO update every 6h
        }

        protected internal override void Render()
        {
            BindTexture(WindowTextureId);
            RenderVertices(PrimitiveType.Quads, 0, 4);
        }

        protected internal override void Event(STOMP.StompMessageEventArgs EventData)
        {
            
        }
    }
}
