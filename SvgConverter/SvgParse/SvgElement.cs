using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using Windows.Foundation;
using SvgConverter.SvgParse.Brushes;
using SvgConverter.SvgParse.SvgAttributesHelper;
using SvgConverter.SvgParse.Utilities;

namespace SvgConverter.SvgParse
{
    public class SvgElement
    {
        public Dictionary<string, SvgNodeStyle> CssStyles { get; private set; }
        public Dictionary<string, ISvgBrush> DefBrushes { get; private set; }
        public Dictionary<string, SvgNodeGroup> DefClipPath { get; private set; }
        public Dictionary<string, SvgNode> DefSvgNodes { get; private set; }
        public Rect ViewBox => new Rect(Offset, Size);
        public Point Offset { get; private set; }
        public Size Size { get; private set; }
        public List<SvgNode> Children { get; } = new List<SvgNode>();

        private static Rect GetRect(string xml)
        {
            if (string.IsNullOrWhiteSpace(xml))
                return new Rect();
            var strs = xml.Split(' ');
            if (strs.Length == 4)
            {
                var x = SvgLengthHelper.ParseLength(strs[0], 0);
                var y = SvgLengthHelper.ParseLength(strs[1], 0);
                var width = SvgLengthHelper.ParseLength(strs[2], 0);
                var height = SvgLengthHelper.ParseLength(strs[3], 0);
                return new Rect(new Point(x, y), new Size(width, height));
            }

            return new Rect();
        }

        private void ParseBaseStyle(Dictionary<string, string> dic)
        {
            if (dic == null) return;
            foreach (var item in dic)
                switch (item.Key.ToLower())
                {
                    case "viewbox":
                        var viewBox = GetRect(item.Value);
                        Offset = new Point(viewBox.X, viewBox.Y);
                        Size = new Size(viewBox.Width, viewBox.Height);
                        break;
                    case "width":
                        Size = new Size(SvgLengthHelper.ParseLength(item.Value, Size.Width), Size.Height);
                        break;
                    case "height":
                        Size = new Size(Size.Width, SvgLengthHelper.ParseLength(item.Value, Size.Height));
                        break;
                    case "x":
                    case "top":
                        Offset = new Point(Offset.X, SvgLengthHelper.ParseLength(item.Value, 0));
                        break;
                    case "y":
                    case "left":
                        Offset = new Point(SvgLengthHelper.ParseLength(item.Value, 0), Offset.Y);
                        break;
                    case "style":
                        var styleDic = new Dictionary<string, string>();
                        var strs = item.Value.Split(';');
                        foreach (var str in strs)
                        {
                            var keyValue = str.Split(':');
                            if (keyValue?.Length == 2) styleDic[keyValue[0].Trim()] = keyValue[1].Trim();
                        }

                        ParseBaseStyle(styleDic);
                        break;
                }
        }

        /// <summary>
        ///     从xml字符串中加载svg文档
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        public static SvgElement LoadFromXml(string xml)
        {
            var svg = new SvgElement();
            var xmlDoc = new XmlDocument();
            var settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Ignore,
                IgnoreComments = true,
                IgnoreProcessingInstructions = true,
                IgnoreWhitespace = true
            }; 
            xml = Regex.Replace(xml, @"\s+", " ");
            var str = xml.Replace("&ns_extend;", "http://ns.adobe.com/Extensibility/1.0/")
                .Replace("&ns_ai;", "http://ns.adobe.com/AdobeIllustrator/10.0/")
                .Replace("&ns_graphs;", "http://ns.adobe.com/Graphs/1.0/")
                .Replace("&ns_vars;", "http://ns.adobe.com/Variables/1.0/")
                .Replace("&ns_imrep;", "http://ns.adobe.com/ImageReplacement/1.0/")
                .Replace("&ns_sfw;", "http://ns.adobe.com/SaveForWeb/1.0/")
                .Replace("&ns_custom;", "http://ns.adobe.com/GenericCustomNamespace/1.0/")
                .Replace("&ns_adobe_xpath;", "http://ns.adobe.com/XPath/1.0/")
                .Replace("<switch", "<g")
                .Replace("</switch", "</g");
            var reader = XmlReader.Create(new StringReader(str), settings);
            xmlDoc.Load(reader);
            var root = xmlDoc.DocumentElement;
            if (root.Name.ToLower().Equals("svg") && root.HasChildNodes)
            {
                var dic = new Dictionary<string, string>();
                foreach (XmlAttribute item in root.Attributes) dic[item.Name] = item.Value;
                svg.ParseBaseStyle(dic);
                if (svg.Size == new Size())
                    svg.Size = new Size(500, 500);

                svg.DefBrushes = SvgDefsNodeHelper.GetDefBrushes(root, svg.Size);
                svg.DefSvgNodes = SvgDefsNodeHelper.GetDefSvgNodes(root, svg.DefBrushes, svg.Size);
                svg.DefClipPath = SvgDefsNodeHelper.GetDefClipPath(root, svg.DefSvgNodes, svg.Size);
                svg.CssStyles = SvgDefsNodeHelper.GetCssStyles(root, svg.DefBrushes, svg.DefClipPath, svg.Size);

                var ele = root.FirstChild;

                while (ele != null)
                {
                    if (ele.NodeType == XmlNodeType.Element)
                        switch (ele.Name.ToLower())
                        {
                            case "g":
                                var group = SvgLoadHelper.GetGeometryGroupFromXml(ele, svg.CssStyles, svg.DefBrushes,
                                    svg.Size, svg.DefSvgNodes, svg.DefClipPath);
                                if (group != null)
                                    svg.Children.Add(group);
                                break;
                            default:
                                var geo = SvgLoadHelper.GetGeometryFromXml(ele, svg.CssStyles, svg.DefBrushes,
                                    svg.Size, svg.DefSvgNodes, svg.DefClipPath);
                                if (geo != null)
                                    svg.Children.Add(geo);
                                break;
                        }

                    ele = ele.NextSibling;
                }
            }

            return svg;
        }
    }
}