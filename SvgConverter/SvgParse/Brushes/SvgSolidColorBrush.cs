using System.Numerics;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;

namespace SvgConverter.SvgParse.Brushes
{
    public class SvgSolidColorBrush : ISvgBrush
    {
        public SvgSolidColorBrush(Color color)
        {
            Color = color;
        }

        public Color Color { get; set; }
        public float Opacity { get; set; } = 1;
        public Matrix3x2 Transform { get; set; }

        public ICanvasBrush Parse(ICanvasResourceCreator resourceCreator)
        {
            return new CanvasSolidColorBrush(resourceCreator, Color)
            {
                Opacity = Opacity
            };
        }
    }
}