using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Media;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.Text;
using SvgConverter.SvgParse;
using SvgConverter.SvgParse.Attributes;
using SvgConverter.SvgParse.Utilities;

namespace SvgConverter.SvgParseForWin2D.Utilities
{
    public static class Win2DSvgParseHelper
    {
        public static CanvasPathBuilder GetPathBuilder(ICanvasResourceCreator resourceCreator,
            PathGeometry geo, FillRule filleRule)
        {
            var builder = new CanvasPathBuilder(resourceCreator);
            if (filleRule.Equals(FillRule.Nonzero))
                builder.SetFilledRegionDetermination(CanvasFilledRegionDetermination.Winding);
            foreach (var item in geo.Figures)
            {
                builder.BeginFigure(item.StartPoint.ToVector2());
                foreach (var seg in item.Segments)
                    if (seg is BezierSegment bez1)
                    {
                        builder.AddCubicBezier(bez1.Point1.ToVector2(), bez1.Point2.ToVector2(),
                            bez1.Point3.ToVector2());
                    }
                    else if (seg is QuadraticBezierSegment bez2)
                    {
                        builder.AddQuadraticBezier(bez2.Point1.ToVector2(), bez2.Point2.ToVector2());
                    }
                    else if (seg is LineSegment line)
                    {
                        builder.AddLine(line.Point.ToVector2());
                    }
                    else if (seg is PolyLineSegment poly)
                    {
                        foreach (var p in poly.Points) builder.AddLine(p.ToVector2());
                    }
                    else if (seg is ArcSegment arc)
                    {
                        var dir = arc.SweepDirection == SweepDirection.Clockwise
                            ? CanvasSweepDirection.Clockwise
                            : CanvasSweepDirection.CounterClockwise;
                        var size = arc.IsLargeArc ? CanvasArcSize.Large : CanvasArcSize.Small;
                        builder.AddArc(arc.Point.ToVector2(),
                            (float) arc.Size.Width, (float) arc.Size.Height, (float) arc.RotationAngle, dir,
                            size);
                    }
                    else if (seg is PolyBezierSegment)
                    {
                        //实际转换出的path对象中不包含PolyBezierSegment
                    }

                builder.EndFigure(item.IsClosed ? CanvasFigureLoop.Closed : CanvasFigureLoop.Open);
            }

            return builder;
        }

        public static List<Vector2> GetFillPoints(Rect viewRect, int count)
        {
            var list = new List<Vector2>();
            var step = (float) (viewRect.Width + viewRect.Height) / count;
            var turn = true;
            var width = (float) viewRect.X;
            var heigh = (float) viewRect.Y;
            for (var i = 0; i < 2 * count; i++)
            {
                if (turn)
                {
                    width += step;
                    list.Add(new Vector2(width, (float) viewRect.Y));
                }
                else
                {
                    heigh += step;
                    list.Add(new Vector2((float) viewRect.X, heigh));
                }

                turn = !turn;
            }

            return list;
        }

        public static CanvasStrokeStyle ParseStrokeStyle(SvgNodeStyle nodeStyle)
        {
            var style = new CanvasStrokeStyle
            {
                MiterLimit = (float) nodeStyle.StrokeMiterLimit,
                DashOffset = (float) (nodeStyle.StrokeDashOffset / nodeStyle.StrokeThickness),
                DashCap = (CanvasCapStyle) nodeStyle.StrokeLineCap,
                StartCap = (CanvasCapStyle) nodeStyle.StrokeLineCap,
                EndCap = (CanvasCapStyle) nodeStyle.StrokeLineCap,
                LineJoin = (CanvasLineJoin) nodeStyle.StrokeLineJoin
            };
            if (nodeStyle.StrokeDashArray.Count > 0)
            {
                var array = nodeStyle.StrokeDashArray.ToArray();
                var darray = new float[array.Length];
                for (var i = 0; i < array.Length; i++)
                    //array[i]/nodeStyle.StrokeThickness为Win2D中图形忽略线宽后实际虚线应表现的形式
                    darray[i] = (float) (array[i] / nodeStyle.StrokeThickness);
                style.CustomDashStyle = darray;
            }

            return style;
        }

        public static CanvasGeometry ParseGeometry(ICanvasResourceCreator resourceCreator, SvgNode node)
        {
            CanvasGeometry geometry = null;
            if (node is SvgGeometry geo)
            {
                if (geo.Geometry is PathGeometry pathGeometry)
                {
                    var builder = GetPathBuilder(resourceCreator, pathGeometry,
                        geo.Style.FillRule);
                    geometry =
                        CanvasGeometry.CreatePath(builder);
                    builder.Dispose();
                }
                else if (geo.Geometry is EllipseGeometry ellipseGeometry)
                {
                    geometry = CanvasGeometry.CreateEllipse(resourceCreator, ellipseGeometry.Center.ToVector2(),
                        (float) ellipseGeometry.RadiusX, (float) ellipseGeometry.RadiusY);
                }
            }
            else if (node is SvgRectangle rect)
            {
                geometry = CanvasGeometry.CreateRoundedRectangle(resourceCreator, rect.Rect, rect.RadiusX,
                    rect.RadiusY);
            }

            return geometry;
        }

        public static CanvasGeometry ParseClipGeometry(ICanvasResourceCreator resourceCreator, SvgNodeGroup clipPath,
            SvgNodeGroup parentClipPath)
        {
            CanvasGeometry clipGeo = null;
            if (clipPath?.Children?.Count > 0)
                foreach (var clip in clipPath.Children)
                {
                    var geo = ParseGeometry(resourceCreator, clip);
                    if (geo != null)
                        clipGeo = clipGeo != null
                            ? clipGeo.CombineWith(geo, clip.RenderTransform, CanvasGeometryCombine.Union)
                            : geo;
                }

            CanvasGeometry parentClipGeo = null;
            if (parentClipPath?.Children?.Count > 0)
                foreach (var clip in parentClipPath.Children)
                {
                    var geo = ParseGeometry(resourceCreator, clip);
                    if (geo != null)
                        parentClipGeo = parentClipGeo != null
                            ? parentClipGeo.CombineWith(geo, clip.RenderTransform, CanvasGeometryCombine.Union)
                            : geo;
                }

            if (clipGeo == null && parentClipGeo == null)
                return null;
            if (clipGeo != null && parentClipGeo != null)
                return clipGeo.CombineWith(parentClipGeo, Matrix3x2.Identity, CanvasGeometryCombine.Intersect);
            if (clipGeo != null)
                return clipGeo;
            return parentClipGeo;
        }

        public static async Task<List<Win2DSvgNode>> GetWin2DGeometrys(ICanvasResourceCreator resourceCreator,
            SvgElement svg)
        {
            var svgGeometrys = SvgParseHelper.GetSvgGeometrys(svg);
            var pathDrawList = new List<Win2DSvgNode>();
            foreach (var item in svgGeometrys)
                if (item is SvgGeometry || item is SvgRectangle)
                {
                    var geometry = ParseGeometry(resourceCreator, item);
                    if (geometry != null)
                    {
                        var win2DGeo = new Win2DSvgGeometry
                        {
                            Id = item.Id,
                            ClipGeometry =
                                ParseClipGeometry(resourceCreator, item.Style.ClipPath, item.Style.ParentClipPath),
                            SourceCanvasGeometry = geometry,
                            CanvasStrokeStyle = ParseStrokeStyle(item.Style),
                            RenderOpacity = item.RenderOpacity,
                            RenderTransform = item.RenderTransform,
                            StrokeThickness = (float) item.Style.StrokeThickness,
                            Stroke = item.Style.Stroke.Parse(resourceCreator),
                            Fill = item.Style.Fill.Parse(resourceCreator)
                        };
                        win2DGeo.PathLength = win2DGeo.CanvasGeometry.ComputePathLength();
                        win2DGeo.Stroke.Opacity *= (float) item.RenderOpacity;
                        win2DGeo.Fill.Opacity *= (float) item.RenderOpacity;
                        if (item.RenderOpacity.Equals(0) && win2DGeo.StrokeThickness > 0)
                            win2DGeo.RenderMethod = RenderMethod.Mark;
                        else if (win2DGeo.StrokeThickness < 0.01 || win2DGeo.Stroke.Opacity.Equals(0))
                            win2DGeo.RenderMethod = RenderMethod.Fill;
                        pathDrawList.Add(win2DGeo);
                    }
                }
                else if (item is SvgText text)
                {
                    var layout = new CanvasTextLayout(resourceCreator, text.Content,
                        new CanvasTextFormat
                        {
                            FontSize = (float) text.FontSize,
                            FontFamily = text.FontFamily,
                            WordWrapping = CanvasWordWrapping.NoWrap
                        }, 0, 0);
                    var transform = Matrix3x2.Identity;
                    switch (text.TextAnchor)
                    {
                        case TextAnchor.Start:
                            transform = Matrix3x2.CreateTranslation(
                                            new Point(0, -layout.LayoutBounds.Height).ToVector2()) *
                                        text.RenderTransform;
                            break;
                        case TextAnchor.Middle:
                            transform = Matrix3x2.CreateTranslation(
                                            new Point(-layout.LayoutBounds.Width / 2, -layout.LayoutBounds.Height)
                                                .ToVector2()) * text.RenderTransform;
                            break;
                        case TextAnchor.End:
                            transform = Matrix3x2.CreateTranslation(
                                            new Point(-layout.LayoutBounds.Width, -layout.LayoutBounds.Height)
                                                .ToVector2()) * text.RenderTransform;
                            break;
                    }

                    transform.Translation += text.Position.ToVector2();
                    var win2DGeo = new Win2DSvgGeometry
                    {
                        Id = item.Id,
                        ClipGeometry =
                            ParseClipGeometry(resourceCreator, text.Style.ClipPath, text.Style.ParentClipPath),
                        SourceCanvasGeometry = CanvasGeometry.CreateText(layout),
                        CanvasStrokeStyle = ParseStrokeStyle(text.Style),
                        RenderOpacity = text.RenderOpacity,
                        RenderTransform = transform,
                        StrokeThickness = (float) text.Style.StrokeThickness,
                        RenderMethod = RenderMethod.Composite,
                        Stroke = text.Style.Stroke.Parse(resourceCreator),
                        Fill = text.Style.Fill.Parse(resourceCreator)
                    };
                    win2DGeo.Stroke.Opacity *= (float) text.RenderOpacity;
                    win2DGeo.Fill.Opacity *= (float) text.RenderOpacity;
                    pathDrawList.Add(win2DGeo);
                }
                else if (item is SvgImage img)
                {
                    var win2DImg = await Win2DSvgImage.GetImage(resourceCreator, img.ViewRect, img.ImageBytes);
                    win2DImg.Id = img.Id;
                    win2DImg.RenderOpacity = img.RenderOpacity;
                    win2DImg.RenderTransform = img.RenderTransform;
                    win2DImg.ClipGeometry =
                        ParseClipGeometry(resourceCreator, img.Style.ClipPath, img.Style.ParentClipPath);
                    pathDrawList.Add(win2DImg);
                }

            return pathDrawList;
        }
    }
}