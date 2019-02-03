using System;
using System.Numerics;
using Windows.Foundation;
using Microsoft.Graphics.Canvas;

namespace SvgConverter
{
    public abstract class AnimationBase : IDisposable
    {
        public Rect ViewBox { get; protected set; }
        public abstract float Progress { get; set; }
        public CanvasDevice Device { get; protected set; }

        public abstract void Dispose();

        public abstract Vector2? Draw(
            CanvasDrawingSession drawingSession, float drawProgress);
    }
}