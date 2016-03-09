using OpenGLFrontWindow.Panes;
using OpenGLFrontWindow.Properties;
using STOMP.Frames;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using STOMP;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace OpenGLFrontWindow
{
    class Program
    {
        public static ConfigurationData Configuration = new ConfigurationData();
        public static GameWindow Window;
        public static STOMPClient StompClient;
        public static MqttClient MQTTClient;

        private int UseScreen = Settings.Default.Monitor;
        private List<PaneBase> Panes;

        static internal int SamplerId;
        static private int ShaderProgramId;
        static private Matrix4 ProjectionMatrix;

        void Run(string[] args)
        {
            // Read command line + set config
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-s":
                        UseScreen = Int32.Parse(args[++i]) - 1;
                        break;

                }
            }

            // Init rest of configuration
            Configuration.Width = Screen.AllScreens[UseScreen].Bounds.Width;
            Configuration.Height = Screen.AllScreens[UseScreen].Bounds.Height;

            //Init window resources
            PaneBase.LoadWindowResources();
            
            // Init OpenGL
            Window = new GameWindow(Configuration.Width, Configuration.Height, new GraphicsMode(), "", GameWindowFlags.Fullscreen, DisplayDevice.GetDisplay((DisplayIndex)UseScreen));
            Window.RenderFrame += Render;
            Window.UpdateFrame += Tick;

            InitRenderer();

            // Init Panes
            Panes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
                                                           .Where(x => typeof(PaneBase).IsAssignableFrom(x) && !x.IsAbstract)
                                                           .Select<Type, PaneBase>(x => (PaneBase)Activator.CreateInstance(x))
                                                           .Where(x => x.BaseInit()) // BaseInit() == false means the pane isn't production-ready
                                                           .OrderBy(x => x.ZIndex)
                                                           .ToList();

            // Init event bus protocol(s)
            MQTTClient = new MqttClient("mqtt://MQTTHost");
            MQTTClient.Connect("frontWindowDisplay");
            MQTTClient.Subscribe(new string[] { "token/events/string/here" }, new byte[] { 0 });
            MQTTClient.MqttMsgPublishReceived += MQTT_MessageReceived;

            StompClient = new STOMPClient("stomp://StompHost");
            StompClient.Subscribe("Token Events");
            StompClient.MessageReceived += StompClient_MessageReceived;

            // Start operation
            Window.Run(60, 60);
        }

        void MQTT_MessageReceived(object sender, MqttMsgPublishEventArgs e)
        {
            //Panes.ForEach(x => x.Event(e));
        }

        void StompClient_MessageReceived(object sender, StompMessageEventArgs e)
        {
            Panes.ForEach(x => x.Event(e));
        }

        void Render(object sender, FrameEventArgs e)
        {
            Window.MakeCurrent();

            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);
            GL.Viewport(0, 0, Program.Configuration.Width, Program.Configuration.Height);

            // Clear main colour buffer
            GL.ClearColor(Color.Black);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            // Bind shader program
            GL.UseProgram(ShaderProgramId);

            Panes.ForEach(x => x.Render());

            Window.SwapBuffers();
        }

        void Tick(object sender, FrameEventArgs e)
        {
            Panes.ForEach(x => x.Tick());
        }

        void InitRenderer()
        {
            // Make sure we're current
            Window.MakeCurrent();

            // Generate texture sampler
            SamplerId = GL.GenSampler();
            GL.SamplerParameter(SamplerId, SamplerParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.SamplerParameter(SamplerId, SamplerParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.SamplerParameter(SamplerId, SamplerParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.SamplerParameter(SamplerId, SamplerParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            // Create projection matrix
            ProjectionMatrix = new Matrix4();

            ProjectionMatrix[0, 0] = 2.0f / (float)(Program.Configuration.Width);    // X Scale
            ProjectionMatrix[1, 1] = 2.0f / (float)(Program.Configuration.Height);   // Y Scale
            ProjectionMatrix[2, 2] = 1.0f;                          // Z Scale
            ProjectionMatrix[3, 3] = 1.0f;                          // W
            ProjectionMatrix[3, 0] = -1.0f;                         // X Translation
            ProjectionMatrix[3, 1] = -1.0f;                         // Y Translation

            // Set up blending
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.Enable(EnableCap.Blend);

            // Compile shaders
            ShaderProgramId = BuildProgram(File.ReadAllText("Shaders/Shader.vert"), File.ReadAllText("Shaders/Shader.frag"));
        }

        private int CompileShader(string ShaderCode, ShaderType Type)
        {
            int shaderId = GL.CreateShader(Type);
            GL.ShaderSource(shaderId, ShaderCode);
            GL.CompileShader(shaderId);

            int CompileSuccess;
            GL.GetShader(shaderId, ShaderParameter.CompileStatus, out CompileSuccess);

            if (CompileSuccess == 0)
            {
                string ErrorLog = GL.GetShaderInfoLog(shaderId);

                GL.DeleteShader(shaderId);
                throw new ArgumentException("Invalid " + (Type == ShaderType.FragmentShader ? "fragment" : "vertex") + " shader program: \r\n" + ErrorLog);
            }

            return shaderId;
        }

        private int BuildProgram(string VertexShader, string FragmentShader)
        {
            try
            {
                // First allocate the program
                int ProgramId = GL.CreateProgram();

                // Now try and compile the individual shader stages
                int VertexShaderId = CompileShader(VertexShader, ShaderType.VertexShader);
                int FragmentShaderId = CompileShader(FragmentShader, ShaderType.FragmentShader);

                // Now link
                GL.AttachShader(ProgramId, VertexShaderId);
                GL.AttachShader(ProgramId, FragmentShaderId);

                GL.BindAttribLocation(ProgramId, 0, "in_position");
                GL.BindAttribLocation(ProgramId, 1, "in_color");
                GL.BindAttribLocation(ProgramId, 2, "in_texture");
                GL.BindFragDataLocation(ProgramId, 0, "fragColor");

                GL.LinkProgram(ProgramId);

                // Delete the unneeded shader objects (they're duplicated into the final program)
                GL.DeleteShader(FragmentShaderId);
                GL.DeleteShader(VertexShaderId);

                // And lastly, check to make sure the program linked properly.
                int LinkSuccess;
                GL.GetProgram(ProgramId, GetProgramParameterName.LinkStatus, out LinkSuccess);

                if (LinkSuccess == 0)
                {
                    string Error = GL.GetProgramInfoLog(ProgramId);
                    throw new InvalidOperationException("Failed to compile program: " + Error);
                }

                return ProgramId;
            }
            catch (ArgumentException aex)
            {
                throw new InvalidOperationException("Failed to create program", aex);
            }
        }

        public static void BindVertexAndUniforms()
        {
            int Stride = Marshal.SizeOf(typeof(Vertex));

            // Position
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, Stride, 0);
            GL.EnableVertexAttribArray(0);

            // Colour
            GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, Stride, 8);
            GL.EnableVertexAttribArray(1);

            // Texture
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, Stride, 24);
            GL.EnableVertexAttribArray(2);


            // Set up uniforms
            int UniformLocation;

            // Bind projection matrix uniform
            UniformLocation = GL.GetUniformLocation(ShaderProgramId, "projection");
            GL.UniformMatrix4(UniformLocation, false, ref ProjectionMatrix);

            UniformLocation = GL.GetUniformLocation(Program.ShaderProgramId, "renderTexture");
            GL.Uniform1(UniformLocation, 0);
        }

        static void Main(string[] args)
        {
            (new Program()).Run(args);
        }
    }
}
