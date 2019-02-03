using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace SvgConverter.SvgParse
{
    public sealed class SvgImage : SvgNode
    {
        public static readonly List<string> Base64DateHeaders = new List<string>
        {
            "data:image/png;base64,",
            "data:image/jpeg;base64,",
            "data:image/jpg;base64,",
            "data:image/x-icon;base64,"
        };

        public Rect ViewRect { get; set; }
        public byte[] ImageBytes { get; set; }

        public async Task<ImageSource> GetImageSource()
        {
            if (ImageBytes == null) return null;
            var img = new BitmapImage();
            using (var randomAccessStream = new InMemoryRandomAccessStream())
            {
                var buffer = CryptographicBuffer.CreateFromByteArray(ImageBytes);
                await randomAccessStream.WriteAsync(buffer);
                randomAccessStream.Seek(0);
                await img.SetSourceAsync(randomAccessStream);
            }

            return img;
        }

        public override SvgNode Clone()
        {
            var cloneNode = new SvgImage()
            {
                RenderOpacity = RenderOpacity,
                RenderTransform = RenderTransform,
                Style = Style.Clone(),
                ViewRect = ViewRect,
                ImageBytes = ImageBytes
            };
            return cloneNode;
        }
    }
}