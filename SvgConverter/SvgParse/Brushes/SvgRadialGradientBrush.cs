using System.Linq;
using System.Numerics;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;

namespace SvgConverter.SvgParse.Brushes
{
    public class SvgRadialGradientBrush : ISvgBrush
    {
        public SvgRadialGradientBrush(SvgGradientStop[] gradientStops)
        {
            Stops = gradientStops;
        }

        public SvgGradientStop[] Stops { get; }

        //
        // 摘要:
        //     Specifies the vertical radius of the brush's radial gradient.
        public float RadiusY { get; set; }

        //
        // 摘要:
        //     Specifies the horizontal radius of the brush's radial gradient.
        public float RadiusX { get; set; }

        //
        // 摘要:
        //     Specifies a displacement from Center, used to form the brush's radial gradient.
        public Vector2 OriginOffset { get; set; }

        //
        // 摘要:
        //     Specifies the center of the brush's radial gradient
        public Vector2 Center { get; set; }
        public float Opacity { get; set; } = 1;
        public Matrix3x2 Transform { get; set; }

        public ICanvasBrush Parse(ICanvasResourceCreator resourceCreator)
        {
            var stops = Stops.Select(o => new CanvasGradientStop
            {
                Position = o.Position,
                Color = o.Color
            });
            var radialGra = new CanvasRadialGradientBrush(resourceCreator, stops.ToArray())
            {
                Center = Center,
                OriginOffset = OriginOffset,
                RadiusX = RadiusX,
                RadiusY = RadiusY,
                Transform = Transform,
                Opacity = Opacity
            };
            return radialGra;
        }
    }
}