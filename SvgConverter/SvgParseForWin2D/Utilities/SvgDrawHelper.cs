using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas.Geometry;

namespace SvgConverter.SvgParseForWin2D.Utilities
{
    public static class SvgDrawHelper
    {
        public static  CanvasStrokeStyle GetPartPathStyle(this Win2DSvgGeometry geo, float drawLength)
        {
            if (drawLength > geo.PathLength)
                return geo.CanvasStrokeStyle;
            var noneLength = geo.PathLength - drawLength + geo.StrokeThickness;
            var actualDrawLength = drawLength / geo.StrokeThickness;
            var actualNoneLength = noneLength / geo.StrokeThickness;
            var style = new CanvasStrokeStyle
            {
                MiterLimit = geo.CanvasStrokeStyle.MiterLimit,
                DashOffset = geo.CanvasStrokeStyle.DashOffset,
                DashCap = geo.CanvasStrokeStyle.DashCap,
                StartCap = geo.CanvasStrokeStyle.StartCap,
                EndCap = geo.CanvasStrokeStyle.EndCap,
                LineJoin = geo.CanvasStrokeStyle.LineJoin
            };
            if (geo.CanvasStrokeStyle.CustomDashStyle?.Length > 0)
            {
                actualDrawLength += style.DashOffset;
                actualNoneLength += style.DashOffset;
                var dashList = new List<float>();
                var i = 0;
                float sum = 0;
                do
                {
                    sum += geo.CanvasStrokeStyle.CustomDashStyle[i];
                    if (sum > actualDrawLength)
                    {
                        var needLength = geo.CanvasStrokeStyle.CustomDashStyle[i] -
                                         (sum - actualDrawLength);
                        dashList.Add(needLength);
                        break;
                    }

                    dashList.Add(geo.CanvasStrokeStyle.CustomDashStyle[i]);
                    i = ++i % geo.CanvasStrokeStyle.CustomDashStyle.Length;
                } while (true);

                if (dashList.Count % 2 == 0)
                    dashList.Add(0);
                dashList.Add(actualNoneLength);
                style.CustomDashStyle = dashList.ToArray();
                return style;
            }

            style.CustomDashStyle = new[] { actualDrawLength, actualNoneLength };
            return style;
        }
    }
}
