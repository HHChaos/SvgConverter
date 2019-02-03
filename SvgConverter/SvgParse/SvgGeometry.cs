using Windows.UI.Xaml.Media;

namespace SvgConverter.SvgParse
{
    public class SvgGeometry : SvgNode
    {
        public Geometry Geometry { get; set; }

        public override SvgNode Clone()
        {
            var cloneNode = new SvgGeometry()
            {
                RenderOpacity = RenderOpacity,
                RenderTransform = RenderTransform,
                Style = Style.Clone(),
                Geometry = Geometry
            };
            return cloneNode;
        }
    }
}