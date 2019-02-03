using System.Collections.Generic;
using System.Xml;
using Windows.Foundation;
using SvgConverter.SvgParse.Brushes;
using SvgConverter.SvgParse.SvgAttributesHelper;

namespace SvgConverter.SvgParse.Utilities
{
    public static class SvgDefsNodeHelper
    {
        public static Dictionary<string, ISvgBrush> GetDefBrushes(XmlElement xmlElement, Size refSize)
        {
            var defBrushes = new Dictionary<string, ISvgBrush>();
            var linearBrushList = xmlElement.GetElementsByTagName("linearGradient");
            foreach (XmlElement item in linearBrushList)
            {
                var id = item.Attributes["id"]?.Value;
                if (!string.IsNullOrWhiteSpace(id))
                {
                    var brush = SvgBrushHelper.ParseLinearGradientBrush(item, refSize);
                    if (brush != null)
                        defBrushes[id] = brush;
                }
            }

            var radialBrushList = xmlElement.GetElementsByTagName("radialGradient");
            foreach (XmlElement item in radialBrushList)
            {
                var id = item.Attributes["id"]?.Value;
                if (!string.IsNullOrWhiteSpace(id))
                {
                    var brush = SvgBrushHelper.ParseRadialGradientBrush(item, refSize);
                    if (brush != null)
                        defBrushes[id] = brush;
                }
            }

            return defBrushes;
        }

        public static Dictionary<string, SvgNodeStyle> GetCssStyles(XmlElement xmlElement,
            Dictionary<string, ISvgBrush> defBrushes, Dictionary<string, SvgNodeGroup> defClipPath, Size refSize)
        {
            var cssStyles = new Dictionary<string, SvgNodeStyle>();
            var styleList = xmlElement.GetElementsByTagName("style");
            foreach (XmlNode item in styleList)
            {
                var styleStr = item.InnerText.Replace(" ", string.Empty)
                    .Replace("\n", string.Empty)
                    .Replace("\t", string.Empty);
                if (!string.IsNullOrWhiteSpace(styleStr))
                {
                    var styles = styleStr.Split('}');
                    foreach (var style in styles)
                    {
                        if (string.IsNullOrWhiteSpace(style)) continue;
                        var idEnd = style.IndexOf('{');
                        if (idEnd == -1) continue;
                        var keyString = style.Substring(0, idEnd);
                        var styleString = style.Substring(idEnd + 1);
                        if (keyString.Length > 1)
                        {
                            var keys = keyString.Split(',');
                            foreach (var keyItem in keys)
                            {
                                if (string.IsNullOrWhiteSpace(keyItem))
                                    continue;
                                var key = keyItem.Trim().Substring(1);
                                if (cssStyles.ContainsKey(key))
                                {
                                    cssStyles[key].MergeStyle(styleString, defBrushes, refSize, defClipPath);
                                }
                                else
                                {
                                    var svgStyle = SvgNodeStyle.Parse(styleString, defBrushes, refSize, defClipPath);
                                    cssStyles[key] = svgStyle;
                                }
                            }
                        }
                    }
                }
            }

            return cssStyles;
        }

        public static Dictionary<string, SvgNodeGroup> GetDefClipPath(XmlElement xmlElement,
            Dictionary<string, SvgNode> defNodes, Size refSize)
        {
            var defClipPath = new Dictionary<string, SvgNodeGroup>();
            var clipPathList = xmlElement.GetElementsByTagName("clipPath");
            foreach (XmlNode item in clipPathList)
            {
                var group = SvgLoadHelper.GetGeometryGroupFromXml(item, null, null, refSize, defNodes);
                if (group?.Id != null)
                    defClipPath[group.Id] = group;
            }

            return defClipPath;
        }

        public static Dictionary<string, SvgNode> GetDefSvgNodes(XmlElement xmlElement,
            Dictionary<string, ISvgBrush> defBrushes, Size refSize)
        {
            var defNodes = new Dictionary<string, SvgNode>();
            var defNodeList = xmlElement.GetElementsByTagName("defs");
            foreach (XmlNode item in defNodeList)
            foreach (XmlNode ele in item.ChildNodes)
                if (ele.NodeType == XmlNodeType.Element)
                    switch (ele.Name.ToLower())
                    {
                        case "g":
                            var group = SvgLoadHelper.GetGeometryGroupFromXml(ele, null, defBrushes,
                                refSize);
                            if (group?.Id != null)
                                defNodes[group.Id] = group;
                            break;
                        default:
                            var geo = SvgLoadHelper.GetGeometryFromXml(ele, null, defBrushes,
                                refSize);
                            if (geo?.Id != null)
                                defNodes[geo.Id] = geo;
                            break;
                    }

            return defNodes;
        }
    }
}