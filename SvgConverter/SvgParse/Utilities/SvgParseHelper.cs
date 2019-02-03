using System.Collections.Generic;

namespace SvgConverter.SvgParse.Utilities
{
    public static class SvgParseHelper
    {
        public static List<SvgNode> GetSvgGeometrys(SvgElement svg)
        {
            var list = new List<SvgNode>();
            foreach (var item in svg.Children)
            {
                if (item.Style.Display == Attributes.DisplayMode.None)
                    continue;

                item.RenderOpacity = item.Style.Opacity;
                item.RenderTransform = item.Style.Transform;
                if (item is SvgNodeGroup group)
                    list.AddRange(GetSvgGeometrys(group));
                else
                    list.Add(item);
            }

            return list;
        }

        public static List<SvgNode> GetSvgGeometrys(SvgNodeGroup grop)
        {
            var list = new List<SvgNode>();
            foreach (var item in grop.Children)
            {
                if (grop.Style?.ClipPath != null)
                    item.Style.ParentClipPath = grop.Style.ClipPath;
                item.RenderOpacity = item.Style.Opacity * grop.RenderOpacity;
                item.RenderTransform = item.Style.Transform * grop.RenderTransform;
                if (item is SvgNodeGroup group)
                    list.AddRange(GetSvgGeometrys(group));
                else
                    list.Add(item);
            }

            return list;
        }

        //public static async void Draw(List<SvgNode> items, Panel panel)
        //{
        //    panel.Children.Clear();
        //    while (items.Count != 0)
        //    {
        //        var item = items[0];
        //        items.RemoveAt(0);
        //        if (item is SvgGeometry)
        //        {
        //            var geo = (SvgGeometry) item;
        //            var path = new Path()
        //            {
        //                Data = geo.Geometry,
        //                Opacity = geo.RenderOpacity,
        //                Stretch = geo.Style.Stretch,
        //                Stroke = geo.Style.Stroke,
        //                StrokeThickness = geo.Style.StrokeThickness,
        //                StrokeMiterLimit = geo.Style.StrokeMiterLimit,
        //                Fill = geo.Style.Fill,
        //                StrokeDashArray = geo.Style.StrokeDashArray,
        //                StrokeDashOffset = geo.Style.StrokeDashOffset,
        //                StrokeDashCap = geo.Style.StrokeLineCap,
        //                StrokeStartLineCap = geo.Style.StrokeLineCap,
        //                StrokeEndLineCap = geo.Style.StrokeLineCap,
        //                StrokeLineJoin = geo.Style.StrokeLineJoin
        //            };
        //            //path.RenderTransform = new MatrixTransform()
        //            //{
        //            //    Matrix =
        //            //        new Matrix(geo.RenderTransform.M11, geo.RenderTransform.M12,
        //            //            geo.RenderTransform.M21, geo.RenderTransform.M22,
        //            //            geo.RenderTransform.M31, geo.RenderTransform.M32)
        //            //};
        //            path.Data = geo.Geometry;
        //            panel.Children.Add(path);
        //        }
        //        else if (item is SvgRectangle)
        //        {
        //            var rect = (SvgRectangle)item;
        //            var rectangle = new Rectangle
        //            {
        //                Width = rect.Rect.Width,
        //                Height = rect.Rect.Height,
        //                Margin = new Thickness(rect.Rect.X, rect.Rect.Y, 0, 0),
        //                RadiusX = rect.radiusX,
        //                RadiusY = rect.radiusY,
        //                Opacity = rect.RenderOpacity,
        //                Stretch = rect.Style.Stretch,
        //                Stroke = rect.Style.Stroke,
        //                StrokeThickness = rect.Style.StrokeThickness,
        //                StrokeMiterLimit = rect.Style.StrokeMiterLimit,
        //                Fill = rect.Style.Fill,
        //                StrokeDashArray = rect.Style.StrokeDashArray,
        //                StrokeDashOffset = rect.Style.StrokeDashOffset,
        //                StrokeDashCap = rect.Style.StrokeLineCap,
        //                StrokeStartLineCap = rect.Style.StrokeLineCap,
        //                StrokeEndLineCap = rect.Style.StrokeLineCap,
        //                StrokeLineJoin = rect.Style.StrokeLineJoin
        //            };
        //            panel.Children.Add(rectangle);
        //        }
        //        else if (item is SvgImage)
        //        {
        //            var svgImg = (SvgImage) item;
        //            var img = new Image
        //            {
        //                Opacity = svgImg.RenderOpacity,
        //                Margin = new Thickness(svgImg.ViewRect.X, svgImg.ViewRect.Y, 0, 0),
        //                Width = svgImg.ViewRect.Width,
        //                Height = svgImg.ViewRect.Height,
        //                Source = await svgImg.GetImageSource()
        //            };
        //            //img.RenderTransform = new MatrixTransform()
        //            //{
        //            //    Matrix =
        //            //        new Matrix(svgImg.RenderTransform.M11, svgImg.RenderTransform.M12,
        //            //            svgImg.RenderTransform.M21, svgImg.RenderTransform.M22,
        //            //            svgImg.RenderTransform.M31, svgImg.RenderTransform.M32)
        //            //};
        //            panel.Children.Add(img);
        //        }

        //    }
        //}
    }
}