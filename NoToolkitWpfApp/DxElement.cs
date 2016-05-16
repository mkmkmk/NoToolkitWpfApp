using System;
using System.Windows;
using System.Windows.Threading;
using SharpDX;
using SharpDX.Direct3D11;
using Point = System.Windows.Point;

namespace NoToolkitWpfApp
{
    public sealed class DxElement : FrameworkElement
    {

        private D3D11Image _image;
        private readonly DispatcherTimer _resizeDelayTimer;

        public event EventHandler OnResized;



        public DxElement()
        {

            _resizeDelayTimer = new DispatcherTimer(DispatcherPriority.Normal)
            {
                Interval = TimeSpan.FromMilliseconds(33)
            };

            _resizeDelayTimer.Tick += (sender, e) =>
            {
                _resizeDelayTimer.Stop();
                if (OnResized != null) OnResized(this, EventArgs.Empty);
            };

            Focusable = true;

            SizeChanged += (sender, e) =>
            {
                _resizeDelayTimer.Stop();
                _resizeDelayTimer.Start();
            };

        }

        public void SetBackbuffer(Texture2D backbuffer)
        {
            Utilities.Dispose(ref _image);
            _image = new D3D11Image(backbuffer);
            InvalidateVisual();

        }

        public void Close()
        {
            Utilities.Dispose(ref _image);
        }


        protected override void OnRender(System.Windows.Media.DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (_image != null)
                _image.Invalidate();

            if (_image != null && _image.IsFrontBufferAvailable)
                drawingContext.DrawImage(_image, new Rect(new Point(), RenderSize));

        }

     }
}
