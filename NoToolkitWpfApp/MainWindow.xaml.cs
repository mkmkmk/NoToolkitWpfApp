using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using Buffer = SharpDX.Direct3D11.Buffer;
using Matrix = SharpDX.Matrix;

namespace NoToolkitWpfApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DxManager _manager;
        private Matrix _view;
        private Matrix _proj;
        private Buffer _sphereVertBuffer;
        private Buffer _sphereIndBuffer;
        private Stopwatch _clock;
        private ConstantBuffer _cbuf;
        private Buffer _constantBuffer;
        private Sphere _sphere;


        public MainWindow()
        {
            InitializeComponent();

            Loaded += (sender, args) =>
                Catch(() =>
                {
                    _manager = new DxManager(SharpDxElement);
                    Prepare();
                });


            SharpDxElement.OnResized +=
                (source, e) =>
                    Catch(
                        () =>
                            _proj =
                                Matrix.PerspectiveFovLH(
                                    (float)Math.PI / 4.0f,
                                    (float)SharpDxElement.ActualWidth / (float)SharpDxElement.ActualHeight,
                                    0.1f,
                                    100.0f)
                    );


            new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(15),
                IsEnabled = true,
            }.Tick += (sender, args) => Draw();

            Closing += (sender, args) => Utilities.Dispose(ref _manager);

        }


        private void Catch(Action todo)
        {
            try
            {
                todo();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }


        private void Prepare()
        {

            var vertexShaderByteCode = ShaderBytecode.CompileFromFile("shaders.fx", "VS", "vs_4_0");
            var vertexShader = new VertexShader(_manager.Device, vertexShaderByteCode);

            var pixelShaderByteCode = ShaderBytecode.CompileFromFile("shaders.fx", "PS", "ps_4_0");
            var pixelShader = new PixelShader(_manager.Device, pixelShaderByteCode);

            var signature = ShaderSignature.GetInputSignature(vertexShaderByteCode);

            var layout = new InputLayout(_manager.Device, signature, new[]
                    {
                        new InputElement("POSITION", 0, SharpDX.DXGI.Format.R32G32B32A32_Float, 0, 0),
                        new InputElement("COLOR", 0, SharpDX.DXGI.Format.R32G32B32A32_Float, 16, 0),
                        new InputElement("NORMAL",0, SharpDX.DXGI.Format.R32G32B32A32_Float, 32, 0),
                    });

            _sphere = new Sphere(1f, 4);

            var sphereVert = 
                _sphere.Vertices.SelectMany(el => 
                    new[]
                    {
                        new Vector4(el.Position, 1f), 
                        Color.LawnGreen.ToVector4(), 
                        new Vector4(el.Normal, 0f)
                    }).ToArray();

            _sphereVertBuffer = Buffer.Create(_manager.Device, BindFlags.VertexBuffer, sphereVert);
            _sphereIndBuffer = Buffer.Create(_manager.Device, BindFlags.IndexBuffer, _sphere.Indices);


            _constantBuffer = new Buffer(_manager.Device, Utilities.SizeOf<ConstantBuffer>(), ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);

            var context = _manager.Device.ImmediateContext;
            context.InputAssembler.InputLayout = layout;
            context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

            context.VertexShader.SetConstantBuffer(0, _constantBuffer);
            context.VertexShader.Set(vertexShader);
            context.PixelShader.SetConstantBuffer(0, _constantBuffer);
            context.PixelShader.Set(pixelShader);

            _view = Matrix.LookAtLH(new Vector3(0, 0, -5), new Vector3(0, 0, 0), Vector3.UnitY);
            _proj = Matrix.Identity;

            _clock = new Stopwatch();
            _clock.Start();

            _cbuf = new ConstantBuffer();


        }


        private void Draw()
        {
            _manager.Clear(Color.Black);


            var context = _manager.Device.ImmediateContext;

            var time = (float)_clock.Elapsed.TotalSeconds;
            var viewProj = Matrix.Multiply(_view, _proj);

            var world =
                Matrix.RotationX(time) *
                Matrix.RotationY(time * 1.5f) *
                Matrix.RotationZ(time * .9f) *
                Matrix.Translation(0, 0, 0);

            var worldViewProj = world * viewProj;

            Vector3 lightDirection = new Vector3(-0.3f, 0.3f, +1);
            lightDirection.Normalize();

            _cbuf.WorldViewProj = worldViewProj;
            _cbuf.World = world;
            _cbuf.LightDir = new Vector4(lightDirection, 1);
            _cbuf.Light = (CheckBoxLight.IsChecked ?? false) ? 1 : 0;

            context.UpdateSubresource(ref _cbuf, _constantBuffer);

            context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(_sphereVertBuffer, Utilities.SizeOf<Vector4>() * 3, 0));
            context.InputAssembler.SetIndexBuffer(_sphereIndBuffer, SharpDX.DXGI.Format.R16_UInt, 0);
            context.DrawIndexed(_sphere.Indices.Length, 0, 0);

            _manager.Present();

        }


        [StructLayout(LayoutKind.Sequential, Size = 64 + 64 + 16 + 16)]
        struct ConstantBuffer
        {
            public Matrix WorldViewProj;
            public Matrix World;
            public Vector4 LightDir;
            public int Light;
        }


    }


}
