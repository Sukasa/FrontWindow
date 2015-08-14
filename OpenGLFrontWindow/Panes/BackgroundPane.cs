using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Astronomy;
using System.Threading.Tasks;
using OpenGLFrontWindow.Properties;

namespace OpenGLFrontWindow.Panes
{
    class BackgroundPane : PaneBase
    {
        static SunCalculator Sun;
        static SunAndMoonData Data;
        int Texture;
        float LerpAmt = 0;

        public override bool Init()
        {
            Vertexes = new Vertex[12];

            for (int i = 0; i < 12; i++)
            {
                int Height = Program.Configuration.Height;

                Vertexes[i].Position.X = ((i & 1) == 1) ^ ((i & 2) == 2) ? Program.Configuration.Width : 0;

                if (i > 2)
                    Vertexes[i].Position.Y = Height;
                else
                    Vertexes[i].Position.Y = 0;

                Vertexes[i].TextureCoords.X = 0.0f;
                Vertexes[i].TextureCoords.Y = 0.0f;
            }

            Texture = LoadTexture("Textures/White.png");

            Sun = new SunCalculator(Settings.Default.Lng, Settings.Default.Lat, 0, TimeZoneInfo.Local.IsDaylightSavingTime(DateTime.Now));
            ZIndex = 0;
            return true;
        }

        public override void Tick()
        {

            Twilight2 T = new Twilight2();

            Data = T.GetData(DateTime.Now, Settings.Default.Lat, Settings.Default.Lng, (DateTime.Now - DateTime.UtcNow).TotalHours);


            int Current = DateTime.Now.Hour * 60 + DateTime.Now.Minute;
            int Sunrise = Data.SunRise.Hour * 60 + Data.SunRise.Minute;
            int Sunset = Data.SunSet.Hour * 60 + Data.SunSet.Minute;

            // Messy, but this will lerp the background from night->day->night, with one-hour transitions centered on the sunrise/sunset times.
            if (Current < Sunrise - 30)
            {
                LerpAmt = 0;
            }
            else if (Math.Abs(Current - Sunrise) <= 30)
            {
                int Test = Current - (Sunrise - 30);
                LerpAmt = (float)Test / 60;
            }
            else if (Current < Sunset - 30)
            {
                LerpAmt = 1;
            }
            else if (Math.Abs(Current - Sunset) <= 30)
            {
                int Test = Current - (Sunrise - 30);
                LerpAmt = 1 - ((float)Test / 60);
            }
            else
            {
                LerpAmt = 0;
            }

            // Lerp all positions and colours from day to night (or back)
            float Position1 = Lerp(56, 76, LerpAmt);
            float Position2 = Lerp(20, 30, LerpAmt);

            float Colour1r = Lerp(0, 44, LerpAmt);
            float Colour1g = Lerp(0, 95, LerpAmt);
            float Colour1b = Lerp(0, 237, LerpAmt);

            float Colour2r = Lerp(29, 98, LerpAmt);
            float Colour2g = Lerp(5, 196, LerpAmt);
            float Colour2b = Lerp(26, 245, LerpAmt);

            float Colour3r = Lerp(51, 65, LerpAmt);
            float Colour3g = Lerp(22, 209, LerpAmt);
            float Colour3b = Lerp(49, 243, LerpAmt);

            // Assemble colour vectors
            Vector4 Colour1 = new Vector4(Colour1r, Colour1g, Colour1b, 256.0f);
            Vector4 Colour2 = new Vector4(Colour2r, Colour2g, Colour2b, 256.0f);
            Vector4 Colour3 = new Vector4(Colour3r, Colour3g, Colour3b, 256.0f);

            Colour1 /= 256f;
            Colour2 /= 256f;
            Colour3 /= 256f;

            // Now set vertex heights/colours
            Vertexes[6].Position.Y = Vertexes[7].Position.Y = Vertexes[8].Position.Y = Vertexes[9].Position.Y = Program.Configuration.Height * Position1 / 100;
            Vertexes[2].Position.Y = Vertexes[3].Position.Y = Vertexes[4].Position.Y = Vertexes[5].Position.Y = Program.Configuration.Height * Position2 / 100;


            Vertexes[0].Color = Vertexes[1].Color = Colour3;
            Vertexes[2].Color = Vertexes[3].Color = Vertexes[4].Color = Vertexes[5].Color = Colour2;
            Vertexes[6].Color = Vertexes[7].Color = Vertexes[8].Color = Vertexes[9].Color = Vertexes[10].Color = Vertexes[11].Color = Colour1;
        }

        public override void Event(dynamic EventData)
        {
            // Do nothing - background doesn't give a crap
        }

        public override void Render()
        {
            BindTexture(Texture);
            UpdateVertices();
            RenderVertices(PrimitiveType.Quads);
        }

        public int Lerp(int P0, int P1, float Amt)
        {
            return P0 + (int)((P1 - P0) * Amt);
        }

        public float Lerp(float P0, float P1, float Amt)
        {
            return P0 + (P1 - P0) * Amt;
        }
    }
}
