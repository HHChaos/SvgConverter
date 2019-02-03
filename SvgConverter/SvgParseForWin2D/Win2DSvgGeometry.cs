using System;
using System.Collections.Generic;
using System.Numerics;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Geometry;
using SvgConverter.SvgParseForWin2D.Utilities;

namespace SvgConverter.SvgParseForWin2D
{
    public class Win2DSvgGeometry : Win2DSvgNode
    {
        public CanvasGeometry CanvasGeometry
        {
            get
            {
                if (ClipGeometry != null)
                    return SourceCanvasGeometry.CombineWith(ClipGeometry, RenderTransform,
                        CanvasGeometryCombine.Intersect);
                return SourceCanvasGeometry;
            }
        }

        public CanvasGeometry SourceCanvasGeometry { get; set; }
        public CanvasStrokeStyle CanvasStrokeStyle { get; set; }
        public ICanvasBrush Stroke { get; set; }
        public float StrokeThickness { get; set; }
        public float PathLength { get; set; }
        public double Progress { get; set; }
        public bool IsDrewCompleted => Progress >= 1;
        public ICanvasBrush Fill { get; set; }

        public Vector2? RenderStroke(CanvasDrawingSession drawingSession, ICanvasBrush stroke)
        {
            if (drawingSession == null) return null;
            if (IsDrewCompleted)
            {
                drawingSession.DrawGeometry(CanvasGeometry, stroke, StrokeThickness, CanvasStrokeStyle);
            }
            else
            {
                var drawLenght = (float) (PathLength * Progress);
                Progress = drawLenght / PathLength;
                var style = this.GetPartPathStyle(drawLenght);
                drawingSession.DrawGeometry(CanvasGeometry, stroke,
                    StrokeThickness, style);
                return Vector2.Transform(CanvasGeometry.ComputePointOnPath(drawLenght),
                    drawingSession.Transform);
            }

            return null;
        }


        public override void Dispose()
        {
            ClipGeometry?.Dispose();
            SourceCanvasGeometry?.Dispose();
            CanvasStrokeStyle?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}