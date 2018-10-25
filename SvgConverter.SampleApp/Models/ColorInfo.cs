using Windows.UI;
using Windows.UI.Xaml.Media;

namespace SvgConverter.SampleApp.Models
{
    public class ColorInfo
    {
        public string Name { get; set; }
        public Color Color { get; set; }

        public SolidColorBrush Brushify()
        {
            return new SolidColorBrush(Color);
        }

        public SolidColorBrush Invert()
        {
            return new SolidColorBrush(Color.FromArgb(0xFF, (byte) (255 - Color.R), (byte) (255 - Color.G),
                (byte) (255 - Color.B)));
        }
    }
}
