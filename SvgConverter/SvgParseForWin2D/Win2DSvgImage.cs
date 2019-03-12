using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;

namespace SvgConverter.SvgParseForWin2D
{
    public class Win2DSvgImage : Win2DSvgNode
    {
        public Win2DSvgImage(Rect viewRect, CanvasBitmap bitmap)
        {
            RenderMethod = RenderMethod.Composite;
            ViewRect = viewRect;
            SourceCanvasBitmap = bitmap;
        }

        public Rect ViewRect { get; set; }
        public CanvasBitmap SourceCanvasBitmap { get; set; }

        public void Draw(CanvasDrawingSession targetDrawSession)
        {
            if (SourceCanvasBitmap == null || ViewRect.Width == 0 || ViewRect.Height == 0)
            {
                return;
            }

            if (ClipGeometry != null)
            {
                using (var offScreen = new CanvasCommandList(SourceCanvasBitmap.Device))
                {
                    using (var drawSession = offScreen.CreateDrawingSession())
                    {
                        drawSession.Clear(Colors.Transparent);
                        var imgCommandList = new CanvasCommandList(SourceCanvasBitmap.Device);
                        var markCommandList = new CanvasCommandList(SourceCanvasBitmap.Device);
                        using (var imgDrawSession = imgCommandList.CreateDrawingSession())
                        {
                            imgDrawSession.Clear(Colors.Transparent);
                            imgDrawSession.DrawImage(SourceCanvasBitmap, ViewRect);
                        }

                        using (var markDrawSession = markCommandList.CreateDrawingSession())
                        {
                            markDrawSession.Clear(Colors.Transparent);
                            markDrawSession.FillGeometry(ClipGeometry, Colors.Black);
                        }

                        var effect = new AlphaMaskEffect
                        {
                            Source = imgCommandList,
                            AlphaMask = markCommandList
                        };
                        drawSession.DrawImage(effect);
                        imgCommandList.Dispose();
                        markCommandList.Dispose();
                    }
                    targetDrawSession.Transform = RenderTransform;
                    targetDrawSession.DrawImage(offScreen);
                }
            }
            else
            {
                targetDrawSession.Transform = RenderTransform;
                targetDrawSession.DrawImage(SourceCanvasBitmap, ViewRect);
            }
        }

        public static async Task<Win2DSvgImage> GetImage(ICanvasResourceCreator device, Rect viewRect,
            byte[] imageBytes)
        {
            if (imageBytes == null) return null;
            using (var randomAccessStream = new InMemoryRandomAccessStream())
            {
                var buffer = CryptographicBuffer.CreateFromByteArray(imageBytes);
                await randomAccessStream.WriteAsync(buffer);
                randomAccessStream.Seek(0);
                var win2DSvgImg = new Win2DSvgImage(viewRect, await CanvasBitmap.LoadAsync(device, randomAccessStream));
                return win2DSvgImg;
            }
        }

        public override void Dispose()
        {
            ClipGeometry?.Dispose();
            SourceCanvasBitmap?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}