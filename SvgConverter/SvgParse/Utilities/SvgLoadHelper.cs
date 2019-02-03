using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Windows.Foundation;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using SvgConverter.SvgParse.Attributes;
using SvgConverter.SvgParse.Brushes;
using SvgConverter.SvgParse.SvgAttributesHelper;

namespace SvgConverter.SvgParse.Utilities
{
    public static class SvgLoadHelper
    {
        public static SvgNodeGroup GetGeometryGroupFromXml(XmlNode node, Dictionary<string, SvgNodeStyle> refCssStyles,
            Dictionary<string, ISvgBrush> defBrushes, Size refSize, Dictionary<string, SvgNode> defNodes = null,
            Dictionary<string, SvgNodeGroup> defClipPath = null)
        {
            var group = new SvgNodeGroup
            {
                Id = node.Attributes["id"]?.Value,
                Style = SvgNodeStyle.Parse(node.Attributes, refCssStyles, defBrushes, refSize, defClipPath)
            };
            var ele = node.FirstChild;
            while (ele != null)
            {
                if (ele.NodeType == XmlNodeType.Element)
                    switch (ele.Name.ToLower())
                    {
                        case "g":
                            var g = GetGeometryGroupFromXml(ele, refCssStyles, defBrushes, refSize, defNodes,
                                defClipPath);
                            if (g != null)
                                group.Children.Add(g);
                            break;
                        default:
                            var geo = GetGeometryFromXml(ele, refCssStyles, defBrushes, refSize, defNodes, defClipPath);
                            if (geo != null)
                                group.Children.Add(geo);
                            break;
                    }

                ele = ele.NextSibling;
            }

            return group;
        }


        public static SvgNode GetGeometryFromXml(XmlNode node, Dictionary<string, SvgNodeStyle> refCssStyles,
            Dictionary<string, ISvgBrush> defBrushes, Size refSize, Dictionary<string, SvgNode> defNodes = null,
            Dictionary<string, SvgNodeGroup> defClipPath = null)
        {
            switch (node.Name.ToLower())
            {
                case "path":
                    var d = node.Attributes["d"]?.Value;
                    var geometry =
                        (PathGeometry)
                        XamlReader.Load(
                            $"<Geometry xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>{d}</Geometry>");
                    var pathStyle = SvgNodeStyle.Parse(node.Attributes, refCssStyles, defBrushes, refSize, defClipPath);
                    geometry.FillRule = pathStyle.FillRule;
                    return new SvgGeometry
                    {
                        Id = node.Attributes["id"]?.Value,
                        Geometry = geometry,
                        Style = pathStyle
                    };
                case "line":
                    var startPoint = new Point(
                        SvgLengthHelper.ParseLength(node.Attributes["x1"]?.Value, refSize.Width),
                        SvgLengthHelper.ParseLength(node.Attributes["y1"]?.Value, refSize.Height));
                    var endPoint = new Point(
                        SvgLengthHelper.ParseLength(node.Attributes["x2"]?.Value, refSize.Width),
                        SvgLengthHelper.ParseLength(node.Attributes["y2"]?.Value, refSize.Height));
                    var linePath =
                        (PathGeometry)
                        XamlReader.Load(
                            $"<Geometry xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>M{startPoint.X},{startPoint.Y}L{endPoint.X},{endPoint.Y}</Geometry>");
                    return new SvgGeometry
                    {
                        Id = node.Attributes["id"]?.Value,
                        Geometry = linePath,
                        Style = SvgNodeStyle.Parse(node.Attributes, refCssStyles, defBrushes, refSize, defClipPath)
                    };
                case "rect":
                    var rectRect =
                        new Rect(
                            new Point(
                                SvgLengthHelper.ParseLength(node.Attributes["x"]?.Value, refSize.Width),
                                SvgLengthHelper.ParseLength(node.Attributes["y"]?.Value, refSize.Height)),
                            new Size(
                                SvgLengthHelper.ParseLength(node.Attributes["width"]?.Value, refSize.Width),
                                SvgLengthHelper.ParseLength(node.Attributes["height"]?.Value, refSize.Height)));

                    return new SvgRectangle
                    {
                        Id = node.Attributes["id"]?.Value,
                        Rect = rectRect,
                        RadiusX = (float) SvgLengthHelper.ParseLength(node.Attributes["rx"]?.Value, refSize.Width),
                        RadiusY = (float) SvgLengthHelper.ParseLength(node.Attributes["ry"]?.Value, refSize.Width),
                        Style = SvgNodeStyle.Parse(node.Attributes, refCssStyles, defBrushes, refSize, defClipPath)
                    };
                case "circle":
                case "ellipse":
                    var ellipse = new EllipseGeometry
                    {
                        Center = new Point(SvgLengthHelper.ParseLength(node.Attributes["cx"]?.Value, refSize.Width),
                            SvgLengthHelper.ParseLength(node.Attributes["cy"]?.Value, refSize.Height))
                    };
                    if (node.Name.ToLower().Equals("circle"))
                    {
                        ellipse.RadiusX =
                            ellipse.RadiusY =
                                SvgLengthHelper.ParseLength(node.Attributes["r"]?.Value, refSize.Width);
                    }
                    else
                    {
                        ellipse.RadiusX = SvgLengthHelper.ParseLength(node.Attributes["rx"]?.Value, refSize.Width);
                        ellipse.RadiusY = SvgLengthHelper.ParseLength(node.Attributes["ry"]?.Value, refSize.Height);
                    }

                    return new SvgGeometry
                    {
                        Id = node.Attributes["id"]?.Value,
                        Geometry = ellipse,
                        Style = SvgNodeStyle.Parse(node.Attributes, refCssStyles, defBrushes, refSize, defClipPath)
                    };
                case "polyline":
                case "polygon":
                    var points = node.Attributes["points"]?.Value;
                    if (string.IsNullOrWhiteSpace(points)) break;
                    points = points.Replace(",", " ");
                    var strs = points.Split(' ');
                    var polylineStr = string.Empty;
                    strs = (from str in strs
                        where !string.IsNullOrWhiteSpace(str)
                        select str.Trim()).ToArray();
                    if (strs.Length == 0) return null;
                    for (var i = 0; i < strs.Length / 2; i++)
                    {
                        if (polylineStr.Equals(string.Empty))
                            polylineStr += "M";
                        else
                            polylineStr += "L";

                        polylineStr += $"{strs[i * 2]},{strs[i * 2 + 1]}";
                    }

                    if (polylineStr.Equals(string.Empty)) break;
                    if (node.Name.ToLower().Equals("polygon"))
                        polylineStr += "Z";
                    var polyPath =
                        (PathGeometry)
                        XamlReader.Load(
                            $"<Geometry xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>{polylineStr}</Geometry>");
                    var polyStyle = SvgNodeStyle.Parse(node.Attributes, refCssStyles, defBrushes, refSize, defClipPath);
                    polyPath.FillRule = polyStyle.FillRule;
                    return new SvgGeometry
                    {
                        Id = node.Attributes["id"]?.Value,
                        Geometry = polyPath,
                        Style = polyStyle
                    };
                case "text":
                    var textContent = node.InnerText;
                    var fontSize = SvgLengthHelper.ParseLength(node.Attributes["font-size"]?.Value, refSize.Width);
                    if (string.IsNullOrWhiteSpace(textContent) || fontSize < 0.01) break;
                    Enum.TryParse<TextAnchor>(node.Attributes["text-anchor"]?.Value, true, out var anchor);
                    return new SvgText
                    {
                        Id = node.Attributes["id"]?.Value,
                        Content = textContent,
                        Position = new Point(
                            SvgLengthHelper.ParseLength(node.Attributes["x"]?.Value, refSize.Width),
                            SvgLengthHelper.ParseLength(node.Attributes["y"]?.Value, refSize.Height)),
                        FontSize = fontSize,
                        FontFamily = node.Attributes["font-family"]?.Value?.Replace("'", string.Empty),
                        Style = SvgNodeStyle.Parse(node.Attributes, refCssStyles, defBrushes, refSize, defClipPath),
                        TextAnchor = anchor
                    };
                case "image":
                    var href = node.Attributes["xlink:href"]?.Value;
                    foreach (var header in SvgImage.Base64DateHeaders)
                        if (href?.StartsWith(header, StringComparison.CurrentCultureIgnoreCase) ==
                            true)
                        {
                            var content = Convert.FromBase64String(href.Substring(header.Length));
                            var rect =
                                new Rect(
                                    new Point(
                                        SvgLengthHelper.ParseLength(node.Attributes["x"]?.Value, refSize.Width),
                                        SvgLengthHelper.ParseLength(node.Attributes["y"]?.Value, refSize.Height)),
                                    new Size(
                                        SvgLengthHelper.ParseLength(node.Attributes["width"]?.Value, refSize.Width),
                                        SvgLengthHelper.ParseLength(node.Attributes["height"]?.Value, refSize.Height)));
                            var imgStyle = SvgNodeStyle.Parse(node.Attributes, refCssStyles, defBrushes, refSize,
                                defClipPath);
                            return new SvgImage
                            {
                                Id = node.Attributes["id"]?.Value,
                                ViewRect = rect,
                                ImageBytes = content,
                                Style = imgStyle
                            };
                        }

                    break;
                case "use":
                    if (defNodes != null)
                    {
                        var id = node.Attributes["xlink:href"]?.Value;
                        if (!string.IsNullOrEmpty(id))
                        {
                            id = id.Replace("#", string.Empty).Trim();
                            if (defNodes.ContainsKey(id))
                            {
                                var svgNode = defNodes[id].Clone();
                                svgNode.Style.MergeStyle(node.Attributes, refCssStyles, defBrushes, refSize,
                                    defClipPath);
                                return svgNode;
                            }
                        }
                    }

                    break;
            }

            return null;
        }
    }
}