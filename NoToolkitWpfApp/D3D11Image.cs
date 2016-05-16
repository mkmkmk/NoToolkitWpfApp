using System;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using SharpDX.Direct3D11;
using SharpDX.Direct3D9;

namespace NoToolkitWpfApp
{
    public class D3D11Image : D3DImage, IDisposable
    {
        private Texture _texture;
        private IntPtr _textureSurfaceHandle;

        private static readonly ThreadLocal<D3D9> D3D9 = new ThreadLocal<D3D9>(() => new D3D9());

        public D3D11Image(Texture2D renderTarget)
        {
            DeviceEx device = D3D9.Value.Device;

            using (var resource = renderTarget.QueryInterface<SharpDX.DXGI.Resource>())
            {
                IntPtr sharedHandle = resource.SharedHandle;
                _texture =
                    new Texture(
                        device,
                        renderTarget.Description.Width,
                        renderTarget.Description.Height,
                        1,
                        Usage.RenderTarget,
                        Format.A8R8G8B8,
                        Pool.Default,
                        ref sharedHandle);
            }

            using (Surface surfaceLevel = _texture.GetSurfaceLevel(0))
            {
                _textureSurfaceHandle = surfaceLevel.NativePointer;
                TrySetBackbufferPointer(_textureSurfaceHandle);
            }

            IsFrontBufferAvailableChanged += OnIsFrontBufferAvailableChanged;
        }


        public void Invalidate()
        {
            if (_texture == null) return;
            Lock();
            try
            {
                AddDirtyRect(new Int32Rect(0, 0, PixelWidth, PixelHeight));
            }
            finally
            {
                Unlock();
            }
        }

        private void TrySetBackbufferPointer(IntPtr ptr)
        {
            Lock();
            try
            {
                SetBackBuffer(D3DResourceType.IDirect3DSurface9, ptr);
            }
            finally
            {
                Unlock();
            }
        }

        public void Dispose()
        {
            if (_texture == null) return;
            Dispatcher.BeginInvoke(new Action(() =>
            {
                IsFrontBufferAvailableChanged -= OnIsFrontBufferAvailableChanged;
                _texture.Dispose();
                _texture = (Texture)null;
                _textureSurfaceHandle = IntPtr.Zero;
                TrySetBackbufferPointer(IntPtr.Zero);
            }), DispatcherPriority.Send);
        }

        private void OnIsFrontBufferAvailableChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!IsFrontBufferAvailable) return;
            TrySetBackbufferPointer(_textureSurfaceHandle);
        }
    }
}
