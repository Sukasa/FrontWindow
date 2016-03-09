using Astronomy;
using OpenGLFrontWindow.Properties;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;

namespace OpenGLFrontWindow.Panes
{
    class BackgroundPane : PaneBase
    {
        static SunCalculator Sun;
        static SunAndMoonData Data;
        int Texture;
        float LerpAmt = 0;

        protected internal override bool Init()
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

        protected internal override void Tick()
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
                int Test = Current - (Sunset - 30);
                LerpAmt = 1 - ((float)Test / 60);
            }
            else
            {
                LerpAmt = 0;
            }

            Vector4 Colour1Day = new Vector4(0.171875f, 0.37109375f, 0.92578125f, 1.0f);
            Vector4 Colour1Night = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);

            Vector4 Colour2Day = new Vector4(0.3828125f, 0.765625f, 0.95703125f, 1.0f);
            Vector4 Colour2Night = new Vector4(0.11328125f, 0.01953125f, 0.1015625f, 1.0f);

            Vector4 Colour3Day = new Vector4(0.23828125f, 0.81640625f, 0.94921875f, 1.0f);
            Vector4 Colour3Night = new Vector4(0.19921875f, 0.0859375f, 0.19140625f, 1.0f);

            // Lerp all positions and colours from day to night (or back)
            float Position1 = Lerp(56f, 76f, LerpAmt);
            float Position2 = Lerp(20f, 30f, LerpAmt);

            Vector4 Colour1 = Colour1Night + (Colour1Day - Colour1Night) * LerpAmt;
            Vector4 Colour2 = Colour2Night + (Colour2Day - Colour2Night) * LerpAmt;
            Vector4 Colour3 = Colour3Night + (Colour3Day - Colour3Night) * LerpAmt;

            // Now set vertex heights/colours
            Vertexes[6].Position.Y = Vertexes[7].Position.Y = Vertexes[8].Position.Y = Vertexes[9].Position.Y = Program.Configuration.Height * Position1 / 100;
            Vertexes[2].Position.Y = Vertexes[3].Position.Y = Vertexes[4].Position.Y = Vertexes[5].Position.Y = Program.Configuration.Height * Position2 / 100;


            Vertexes[0].Color = Vertexes[1].Color = Colour3;
            Vertexes[2].Color = Vertexes[3].Color = Vertexes[4].Color = Vertexes[5].Color = Colour2;
            Vertexes[6].Color = Vertexes[7].Color = Vertexes[8].Color = Vertexes[9].Color = Vertexes[10].Color = Vertexes[11].Color = Colour1;
        }

        protected internal override void Event(STOMP.StompMessageEventArgs EventData)
        {
            // Do nothing - background doesn't give a crap
        }

        protected internal override void Render()
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
