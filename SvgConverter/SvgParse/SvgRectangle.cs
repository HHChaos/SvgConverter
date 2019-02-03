using Windows.Foundation;

namespace SvgConverter.SvgParse
{
    public class SvgRectangle : SvgNode
    {
        public Rect Rect { get; set; }
        public float RadiusX { get; set; }
        public float RadiusY { get; set; }

        public override SvgNode Clone()
        {
            var cloneNode = new SvgRectangle()
            {
                RenderOpacity = RenderOpacity,
                RenderTransform = RenderTransform,
                Style = Style.Clone(),
                Rect = Rect,
                RadiusX = RadiusX,
                RadiusY = RadiusY
            };
            return cloneNode;
        }
    }
}