using System.Collections.Generic;
using System.Numerics;
using System.Xml;
using Windows.Foundation;
using Windows.UI;
using SvgConverter.SvgParse.Brushes;

namespace SvgConverter.SvgParse.SvgAttributesHelper
{
    public static class SvgBrushHelper
    {
        public static ISvgBrush Parse(string brush, Dictionary<string, ISvgBrush> defBrushes)
        {
            if (string.IsNullOrWhiteSpace(brush) || brush.ToLower().Equals("none"))
                return new SvgSolidColorBrush(Colors.Transparent);
            if (brush.StartsWith("url(") && defBrushes != null)
            {
                brush =
                    brush.Replace("url(", string.Empty)
                        .Replace("#", string.Empty)
                        .Replace(")", string.Empty)
                        .Replace(" ", string.Empty);
                if (defBrushes.ContainsKey(brush)) return defBrushes[brush];
                return new SvgSolidColorBrush(Colors.Transparent);
            }

            return new SvgSolidColorBrush(SvgColorHelper.ParseColor(brush));
        }

        public static SvgLinearGradientBrush ParseLinearGradientBrush(XmlElement brush, Size refSize)
        {
            if (brush != null && brush.Name == "linearGradient")
            {
                var stopList = brush.GetElementsByTagName("stop");
                var list = new List<SvgGradientStop>();
                foreach (XmlNode item in stopList)
                {
                    var offset = SvgLengthHelper.ParseLength(item.Attributes["offset"]?.Value, 1);
                    var styles = ParseStyle(item.Attributes);
                    if (styles != null)
                    {
                        styles.TryGetValue("stop-color", out var stopColorStr);
                        var color = string.IsNullOrWhiteSpace(stopColorStr)
                            ? Colors.Black
                            : SvgColorHelper.ParseColor(stopColorStr);
                        if (styles.TryGetValue("stop-opacity", out var stopOpacityStr))
                            color.A = (byte) (SvgLengthHelper.ParseLength(stopOpacityStr, 1) * 255);
                        list.Add(new SvgGradientStop
                        {
                            Position = (float) offset,
                            Color = color
                        });
                    }
                }

                if (list.Count == 0)
                    return null;
                var linearGra = new SvgLinearGradientBrush(list.ToArray())
                {
                    StartPoint =
                        new Point(SvgLengthHelper.ParseLength(brush.Attributes["x1"]?.Value, refSize.Width),
                            SvgLengthHelper.ParseLength(brush.Attributes["y1"]?.Value, refSize.Height)).ToVector2(),
                    EndPoint =
                        new Point(
                            SvgLengthHelper.ParseLength(
                                brush.Attributes["x2"] == null ? "100%" : brush.Attributes["x2"]?.Value, refSize.Width),
                            SvgLengthHelper.ParseLength(brush.Attributes["y2"]?.Value, refSize.Height)).ToVector2(),
                    Transform = SvgTransformHelper.ParseTransform(brush.Attributes["gradientTransform"]?.Value)
                };
                //if (brush.Attributes["gradientUnits"]?.Value != "userSpaceOnUse")
                //{
                //    linearGra.StartPoint =
                //        new Point(linearGra.StartPoint.X * refSize.Width, linearGra.StartPoint.Y * refSize.Height)
                //            .ToVector2();
                //    linearGra.EndPoint =
                //        new Point(linearGra.EndPoint.X * refSize.Width, linearGra.EndPoint.Y * refSize.Height)
                //            .ToVector2();
                //}
                return linearGra;
            }

            return null;
        }

        public static SvgRadialGradientBrush ParseRadialGradientBrush(XmlElement brush, Size refSize)
        {
            if (brush != null && brush.Name == "radialGradient")
            {
                var stopList = brush.GetElementsByTagName("stop");
                var list = new List<SvgGradientStop>();
                foreach (XmlNode item in stopList)
                {
                    var offset = SvgLengthHelper.ParseLength(item.Attributes["offset"]?.Value, 1);
                    var styles = ParseStyle(item.Attributes);
                    if (styles != null)
                    {
                        styles.TryGetValue("stop-color", out var stopColorStr);
                        var color = string.IsNullOrWhiteSpace(stopColorStr)
                            ? Colors.Black
                            : SvgColorHelper.ParseColor(stopColorStr);
                        if (styles.TryGetValue("stop-opacity", out var stopOpacityStr))
                            color.A = (byte) (SvgLengthHelper.ParseLength(stopOpacityStr, 1) * 255);
                        list.Add(new SvgGradientStop
                        {
                            Position = (float) offset,
                            Color = color
                        });
                    }
                }

                if (list.Count == 0)
                    return null;
                var radialGra = new SvgRadialGradientBrush(list.ToArray())
                {
                    Center =
                        new Point(
                                SvgLengthHelper.ParseLength(
                                    brush.Attributes["cx"] == null ? "50%" : brush.Attributes["cx"]?.Value,
                                    refSize.Width),
                                SvgLengthHelper.ParseLength(
                                    brush.Attributes["cy"] == null ? "50%" : brush.Attributes["cy"]?.Value,
                                    refSize.Height))
                            .ToVector2(),
                    OriginOffset = new Point(SvgLengthHelper.ParseLength(brush.Attributes["fx"]?.Value, refSize.Width),
                        SvgLengthHelper.ParseLength(brush.Attributes["fy"]?.Value, refSize.Height)).ToVector2(),
                    RadiusX =
                        (float)
                        SvgLengthHelper.ParseLength(
                            brush.Attributes["r"] == null ? "50%" : brush.Attributes["r"]?.Value, refSize.Width),
                    RadiusY =
                        (float)
                        SvgLengthHelper.ParseLength(
                            brush.Attributes["r"] == null ? "50%" : brush.Attributes["r"]?.Value, refSize.Width),
                    Transform = SvgTransformHelper.ParseTransform(brush.Attributes["gradientTransform"]?.Value)
                };
                //if (brush.Attributes["gradientUnits"]?.Value != "userSpaceOnUse")
                //{
                //    radialGra.Center =
                //        new Point(radialGra.Center.X * refSize.Width, radialGra.Center.Y * refSize.Height)
                //            .ToVector2();
                //    radialGra.OriginOffset =
                //        new Point(radialGra.OriginOffset.X * refSize.Width, radialGra.OriginOffset.Y * refSize.Height)
                //            .ToVector2();
                //    radialGra.RadiusX = radialGra.RadiusY = (float) (radialGra.RadiusX * refSize.Width);
                //}
                return radialGra;
            }

            return null;
        }

        private static Dictionary<string, string> ParseStyle(XmlAttributeCollection style)
        {
            if (style != null && style.Count > 0)
            {
                var dic = new Dictionary<string, string>();
                foreach (XmlAttribute item in style)
                    if (item.Name == "style")
                    {
                        var strs = item.Value?.Split(';');
                        if (strs != null)
                            foreach (var str in strs)
                            {
                                var keyValue = str.Split(':');
                                if (keyValue?.Length == 2) dic[keyValue[0].Trim()] = keyValue[1].Trim();
                            }
                    }
                    else
                    {
                        dic[item.Name] = item.Value;
                    }

                return dic;
            }

            return null;
        }
    }
}