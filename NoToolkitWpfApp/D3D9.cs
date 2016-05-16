using System;
using System.Runtime.InteropServices;
using SharpDX;

namespace NoToolkitWpfApp
{
    internal sealed class D3D9 : IDisposable
    {
        private SharpDX.Direct3D9.Direct3DEx _direct3D;
        private SharpDX.Direct3D9.DeviceEx _device;

        public D3D9()
        {
            var presentparams = new SharpDX.Direct3D9.PresentParameters
            {
                Windowed = true,
                SwapEffect = SharpDX.Direct3D9.SwapEffect.Discard,
                DeviceWindowHandle = GetDesktopWindow(),
                PresentationInterval = SharpDX.Direct3D9.PresentInterval.Default
            };

            const SharpDX.Direct3D9.CreateFlags deviceFlags = 
                SharpDX.Direct3D9.CreateFlags.HardwareVertexProcessing | 
                SharpDX.Direct3D9.CreateFlags.Multithreaded | 
                SharpDX.Direct3D9.CreateFlags.FpuPreserve;

            _direct3D = new SharpDX.Direct3D9.Direct3DEx();
            _device = new SharpDX.Direct3D9.DeviceEx(_direct3D, 0, SharpDX.Direct3D9.DeviceType.Hardware, IntPtr.Zero, deviceFlags, presentparams);
        }

        ~D3D9()
        {
            Dispose();
        }

        public SharpDX.Direct3D9.DeviceEx Device { get { return _device; } }

        public void Dispose()
        {
            Utilities.Dispose(ref _direct3D);
            Utilities.Dispose(ref _device);

            GC.SuppressFinalize(this);
        }

        [DllImport("user32.dll", SetLastError = false)]
        private static extern IntPtr GetDesktopWindow();
    }
}