using System.Collections.Generic;
using System.Numerics;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;

namespace SvgConverter.TextParse
{
    public class CharGeometry
    {
        public char Char { get; set; }
        public string FontFamily { get; set; }
        public float FontSize { get; set; }
        public List<Vector2[]> PathList { get; set; }

        public List<CanvasGeometry> GetGeometrys(ICanvasResourceCreator resourceCreator)
        {
            if (PathList == null || PathList.Count <= 0) return new List<CanvasGeometry>();
            var list = new List<CanvasGeometry>();
            foreach (var pathPoints in PathList)
            {
                if (pathPoints.Length < 2) continue;
                var builder = new CanvasPathBuilder(resourceCreator);
                builder.BeginFigure(pathPoints[0]);
                for (var i = 1; i < pathPoints.Length; i++) builder.AddLine(pathPoints[i]);
                builder.EndFigure(CanvasFigureLoop.Open);
                list.Add(CanvasGeometry.CreatePath(builder));
                builder.Dispose();
            }

            return list;
        }
    }
}