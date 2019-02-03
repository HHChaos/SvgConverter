using System;
using System.Numerics;
using Microsoft.Graphics.Canvas.Geometry;

namespace SvgConverter.SvgParseForWin2D
{
    public enum RenderMethod
    {
        Draw,
        Fill,
        Mark,
        Composite,
        MarkAndFill
    }

    public abstract class Win2DSvgNode : IDisposable
    {
        public string Id { get; set; }
        public RenderMethod RenderMethod { get; set; }
        public double RenderOpacity { get; set; } = 1;
        public Matrix3x2 RenderTransform { get; set; } = Matrix3x2.Identity;
        public CanvasGeometry ClipGeometry { get; set; }
        public abstract void Dispose();
    }
}