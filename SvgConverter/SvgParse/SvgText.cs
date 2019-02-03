using Windows.Foundation;
using SvgConverter.SvgParse.Attributes;

namespace SvgConverter.SvgParse
{
    public class SvgText : SvgNode
    {
        public Point Position { get; set; }
        public string Content { get; set; }
        public double FontSize { get; set; }
        public string FontFamily { get; set; }
        public TextAnchor TextAnchor { get; set; }

        public override SvgNode Clone()
        {
            var cloneNode = new SvgText()
            {
                RenderOpacity = RenderOpacity,
                RenderTransform = RenderTransform,
                Style = Style.Clone(),
                Position = Position,
                Content = Content,
                FontSize = FontSize,
                FontFamily = FontFamily,
                TextAnchor = TextAnchor
            };
            return cloneNode;
        }
    }
}