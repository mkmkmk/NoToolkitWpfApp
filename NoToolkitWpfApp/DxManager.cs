using System;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Device11 = SharpDX.Direct3D11.Device;
using Buffer11 = SharpDX.Direct3D11.Buffer;

namespace NoToolkitWpfApp
{


    public class DxManager : IDisposable
    {
        private Device11 _device;
        private readonly DeviceContext _deviceContext;

        private RenderTargetView _backbufferView;
        private DepthStencilView _zbufferView;


        private readonly DxElement _element;

        public Device11 Device { get { return _device; } }


        public DxManager(DxElement form)
        {
            _element = form;

            FeatureLevel[] levels = { FeatureLevel.Level_11_1, FeatureLevel.Level_11_0 };
            DeviceCreationFlags flag = DeviceCreationFlags.SingleThreaded;

            _device = new SharpDX.Direct3D11.Device(DriverType.Hardware, flag, levels);
            _deviceContext = Device.ImmediateContext;

            _element.OnResized += Resize;

            Resize();
        }


        public void Resize(object source = null, EventArgs e = null)
        {
            Utilities.Dispose(ref _backbufferView);
            Utilities.Dispose(ref _zbufferView);


            if (_element.ActualWidth <= 1e-3 || _element.ActualHeight <= 1e-3)
                return;

            int width = (int)_element.ActualWidth;
            int height = (int)_element.ActualHeight;


            var backBufDescr = new Texture2DDescription
            {
                Width = width,
                Height = height,
                ArraySize = 1,
                SampleDescription = new SampleDescription(1, 0),
                BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget,
                Format = Format.B8G8R8A8_UNorm,
                MipLevels = 1,
                Usage = ResourceUsage.Default,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.Shared
            };

            Utilities.Dispose(ref _backbufferView);

            using (var backbuffer = new Texture2D(_device, backBufDescr))
            {
                _element.SetBackbuffer(backbuffer);
                _backbufferView = new RenderTargetView(Device, backbuffer);

            }

            var zBufDescription = new Texture2DDescription
            {
                Width = width,
                Height = height,
                ArraySize = 1,
                SampleDescription = new SampleDescription(1, 0),
                BindFlags = BindFlags.DepthStencil,
                Format = Format.D16_UNorm,
                MipLevels = 1,
                Usage = ResourceUsage.Default,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            };

            Utilities.Dispose(ref _zbufferView);
            using (var zbufferTexture = new Texture2D(Device, zBufDescription))
            {
                _zbufferView = new DepthStencilView(Device, zbufferTexture);
            }

            _deviceContext.Rasterizer.SetViewport(0, 0, (int)_element.ActualWidth, (int)_element.ActualHeight);
            _deviceContext.OutputMerger.SetTargets(_zbufferView, _backbufferView);

        }


        public void Dispose()
        {
            _element.OnResized -= Resize;
            _element.Close();

            Utilities.Dispose(ref _backbufferView);
            Utilities.Dispose(ref _zbufferView);
            Utilities.Dispose(ref _device);

        }


        public void Clear(Color4 color)
        {
            _deviceContext.ClearRenderTargetView(_backbufferView, color);
            _deviceContext.ClearDepthStencilView(_zbufferView, DepthStencilClearFlags.Depth, 1.0F, 0);
        }


        public void Present()
        {
            _deviceContext.Flush();
            _element.InvalidateVisual();
        }


        public static bool IsDirectX11Supported()
        {
            return SharpDX.Direct3D11.Device.GetSupportedFeatureLevel() == FeatureLevel.Level_11_0;
        }


    }
}
