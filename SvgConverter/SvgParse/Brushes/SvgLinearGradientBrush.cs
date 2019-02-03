using System.Linq;
using System.Numerics;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;

namespace SvgConverter.SvgParse.Brushes
{
    public class SvgLinearGradientBrush : ISvgBrush
    {
        public SvgLinearGradientBrush(SvgGradientStop[] gradientStops)
        {
            Stops = gradientStops;
        }

        public SvgGradientStop[] Stops { get; }

        //
        // 摘要:
        //     The point on the canvas on which the gradient starts.
        public Vector2 StartPoint { get; set; }

        //
        // 摘要:
        //     The point on the canvas on which the gradient stops.
        public Vector2 EndPoint { get; set; }
        public float Opacity { get; set; } = 1;
        public Matrix3x2 Transform { get; set; }

        public ICanvasBrush Parse(ICanvasResourceCreator resourceCreator)
        {
            var stops = Stops.Select(o => new CanvasGradientStop
            {
                Position = o.Position,
                Color = o.Color
            });
            var linearGra = new CanvasLinearGradientBrush(resourceCreator, stops.ToArray())
            {
                StartPoint = StartPoint,
                EndPoint = EndPoint,
                Transform = Transform,
                Opacity = Opacity
            };
            return linearGra;
        }
    }
}