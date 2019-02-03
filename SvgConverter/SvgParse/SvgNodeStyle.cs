using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Xml;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml.Media;
using SvgConverter.SvgParse.Attributes;
using SvgConverter.SvgParse.Brushes;
using SvgConverter.SvgParse.SvgAttributesHelper;

namespace SvgConverter.SvgParse
{
    public class SvgNodeStyle
    {
        public DisplayMode Display { get; set; }

        public ISvgBrush Stroke { get; set; } = new SvgSolidColorBrush(
            Colors.Transparent) {Opacity = 0};

        public double StrokeThickness { get; set; } = 1;
        public double StrokeMiterLimit { get; set; }
        public double Opacity { get; set; } = 1;
        public Stretch Stretch { get; set; }

        public ISvgBrush Fill { get; set; } = new SvgSolidColorBrush(Colors.Black);

        public FillRule FillRule { get; set; } = FillRule.Nonzero;
        public Matrix3x2 Transform { get; set; } = Matrix3x2.Identity;
        public DoubleCollection StrokeDashArray { get; set; } = new DoubleCollection();
        public double StrokeDashOffset { get; set; }
        public PenLineCap StrokeLineCap { get; set; }
        public PenLineJoin StrokeLineJoin { get; set; } = PenLineJoin.Bevel;

        public SvgNodeGroup ParentClipPath { get; set; }
        public SvgNodeGroup ClipPath { get; set; }

        public SvgNodeStyle Clone()
        {
            return new SvgNodeStyle
            {
                Display=Display,
                Stroke = Stroke,
                StrokeThickness = StrokeThickness,
                Fill = Fill,
                FillRule = FillRule,
                Opacity = Opacity,
                Stretch = Stretch,
                StrokeDashArray = StrokeDashArray,
                StrokeDashOffset = StrokeDashOffset,
                StrokeLineCap = StrokeLineCap,
                StrokeLineJoin = StrokeLineJoin,
                StrokeMiterLimit = StrokeMiterLimit,
                Transform = Transform,
                ClipPath = ClipPath,
                ParentClipPath = ParentClipPath
            };
        }

        public void MergeStyle(string cssStyle, Dictionary<string, ISvgBrush> defBrushes, Size refSize,
            Dictionary<string, SvgNodeGroup> defClipPath)
        {
            if (!string.IsNullOrWhiteSpace(cssStyle))
            {
                var dic = new Dictionary<string, string>();
                var strs = cssStyle.Split(';');
                foreach (var str in strs)
                {
                    var keyValue = str.Split(':');
                    if (keyValue?.Length == 2) dic[keyValue[0].Trim()] = keyValue[1].Trim();
                }

                MergeStyle(dic, defBrushes, refSize, defClipPath);
            }
        }

        public void MergeStyle(XmlAttributeCollection attr, Dictionary<string, SvgNodeStyle> refCssStyles,
            Dictionary<string, ISvgBrush> defBrushes, Size refSize,
            Dictionary<string, SvgNodeGroup> defClipPath)
        {
            if (attr != null)
            {
                var dic = new Dictionary<string, string>();
                foreach (XmlAttribute item in attr) dic[item.Name] = item.Value;
                MergeStyle(dic, defBrushes, refSize, defClipPath);
            }
        }

        public void MergeStyle(Dictionary<string, string> dic, Dictionary<string, ISvgBrush> defBrushes, Size refSize,
            Dictionary<string, SvgNodeGroup> defClipPath)
        {
            if (dic == null) return;
            foreach (var item in dic)
                switch (item.Key.ToLower())
                {
                    case "display":
                        if (!string.IsNullOrWhiteSpace(item.Value))
                            switch (item.Value)
                            {
                                case "inline":
                                    Display = DisplayMode.Inline;
                                    break;
                                case "none":
                                    Display = DisplayMode.None;
                                    break;
                                default:
                                    Display = DisplayMode.Other;
                                    break;
                            }
                        break;
                    case "opacity":
                        if (!string.IsNullOrWhiteSpace(item.Value))
                        {
                            double.TryParse(item.Value, out var opacity);
                            Opacity = opacity;
                        }

                        break;
                    case "fill":
                        if (!string.IsNullOrWhiteSpace(item.Value)) Fill = SvgBrushHelper.Parse(item.Value, defBrushes);
                        break;
                    case "stroke":
                        if (!string.IsNullOrWhiteSpace(item.Value))
                            Stroke = SvgBrushHelper.Parse(item.Value, defBrushes);
                        break;
                    case "stroke-width":
                        if (!string.IsNullOrWhiteSpace(item.Value))
                            StrokeThickness = SvgLengthHelper.ParseLength(item.Value, refSize.Width);
                        break;
                    case "stroke-miterlimit":
                        if (!string.IsNullOrWhiteSpace(item.Value))
                            StrokeMiterLimit = SvgLengthHelper.ParseLength(item.Value, refSize.Width);
                        break;
                    case "fill-opacity":
                        if (!string.IsNullOrWhiteSpace(item.Value))
                        {
                            double.TryParse(item.Value, out var opacity);
                            Fill.Opacity = (float) opacity;
                        }

                        break;
                    case "stroke-opacity":
                        if (!string.IsNullOrWhiteSpace(item.Value))
                        {
                            double.TryParse(item.Value, out var opacity);
                            Stroke.Opacity = (float) opacity;
                        }

                        break;
                    case "fill-rule":
                        if (!string.IsNullOrWhiteSpace(item.Value))
                        {
                            if (item.Value.ToLower().Equals("nonzero"))
                                FillRule = FillRule.Nonzero;
                            else if (item.Value.ToLower().Equals("evenodd")) FillRule = FillRule.EvenOdd;
                        }

                        break;
                    case "stroke-dasharray":
                        if (!string.IsNullOrWhiteSpace(item.Value))
                        {
                            var str = item.Value.Replace(',', ' ');
                            var dashs = str.Split(' ');
                            if (dashs?.Length != 0)
                                foreach (var dash in dashs)
                                    StrokeDashArray.Add(SvgLengthHelper.ParseLength(dash, refSize.Width));
                            var valid = StrokeDashArray.Sum() > 0;
                            if (!valid)
                                StrokeDashArray = new DoubleCollection();
                        }

                        break;
                    case "stroke-dashoffset":
                        if (!string.IsNullOrWhiteSpace(item.Value))
                            StrokeDashOffset = SvgLengthHelper.ParseLength(item.Value, refSize.Width);
                        break;
                    case "stroke-linecap":
                        if (!string.IsNullOrWhiteSpace(item.Value))
                            switch (item.Value)
                            {
                                case "butt":
                                    StrokeLineCap = PenLineCap.Flat;
                                    break;
                                case "square":
                                    StrokeLineCap = PenLineCap.Square;
                                    break;
                                case "round":
                                    StrokeLineCap = PenLineCap.Round;
                                    break;
                            }
                        break;
                    case "stroke-linejoin":
                        if (!string.IsNullOrWhiteSpace(item.Value))
                            switch (item.Value)
                            {
                                case "miter":
                                    StrokeLineJoin = PenLineJoin.Miter;
                                    break;
                                case "round":
                                    StrokeLineJoin = PenLineJoin.Round;
                                    break;
                                case "bevel":
                                    StrokeLineJoin = PenLineJoin.Bevel;
                                    break;
                            }
                        break;
                    case "transform":
                        if (!string.IsNullOrWhiteSpace(item.Value))
                            Transform = SvgTransformHelper.ParseTransform(item.Value);
                        break;
                    case "clip-path":
                        if (!string.IsNullOrWhiteSpace(item.Value))
                        {
                            var clipPathId = item.Value;
                            if (clipPathId.StartsWith("url("))
                            {
                                clipPathId =
                                    clipPathId.Replace("url(", string.Empty)
                                        .Replace("#", string.Empty)
                                        .Replace(")", string.Empty)
                                        .Replace(" ", string.Empty);
                                if (defClipPath?.ContainsKey(clipPathId) == true)
                                    ClipPath = defClipPath[clipPathId];
                            }
                        }

                        break;
                    case "style":
                        var styleDic = new Dictionary<string, string>();
                        var strs = item.Value.Split(';');
                        foreach (var str in strs)
                        {
                            var keyValue = str.Split(':');
                            if (keyValue?.Length == 2) styleDic[keyValue[0].Trim()] = keyValue[1].Trim();
                        }

                        MergeStyle(styleDic, defBrushes, refSize, defClipPath);
                        break;
                }
        }

        public static SvgNodeStyle Parse(string cssStyle, Dictionary<string, ISvgBrush> defBrushes, Size refSize,
            Dictionary<string, SvgNodeGroup> defClipPath)
        {
            var style = new SvgNodeStyle();
            if (!string.IsNullOrWhiteSpace(cssStyle))
            {
                var dic = new Dictionary<string, string>();
                var strs = cssStyle.Split(';');
                foreach (var str in strs)
                {
                    var keyValue = str.Split(':');
                    if (keyValue?.Length == 2) dic[keyValue[0].Trim()] = keyValue[1].Trim();
                }

                style.MergeStyle(dic, defBrushes, refSize, defClipPath);
            }

            return style;
        }

        public static SvgNodeStyle Parse(XmlAttributeCollection attr, Dictionary<string, SvgNodeStyle> refCssStyles,
            Dictionary<string, ISvgBrush> defBrushes,
            Size refSize, Dictionary<string, SvgNodeGroup> defClipPath)
        {
            var style = new SvgNodeStyle();
            if (attr != null)
            {
                var dic = new Dictionary<string, string>();
                foreach (XmlAttribute item in attr) dic[item.Name] = item.Value;
                if (dic.ContainsKey("class") && refCssStyles != null)
                {
                    refCssStyles.TryGetValue(dic["class"], out var cssStyle);
                    if (cssStyle != null)
                        style = cssStyle.Clone();
                }

                style.MergeStyle(dic, defBrushes, refSize, defClipPath);
            }

            return style;
        }
    }
}