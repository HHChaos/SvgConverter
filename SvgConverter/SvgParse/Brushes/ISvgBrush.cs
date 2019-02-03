using System.Numerics;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;

namespace SvgConverter.SvgParse.Brushes
{
    public interface ISvgBrush
    {
        float Opacity { get; set; }
        Matrix3x2 Transform { get; set; }
        ICanvasBrush Parse(ICanvasResourceCreator resourceCreator);
    }
}