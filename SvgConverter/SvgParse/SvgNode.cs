using System.Numerics;

namespace SvgConverter.SvgParse
{
    public abstract class SvgNode
    {
        public string Id { get; set; }
        public double RenderOpacity { get; set; } = 1;
        public Matrix3x2 RenderTransform { get; set; } = Matrix3x2.Identity;
        public SvgNodeStyle Style { get; set; } = new SvgNodeStyle();
        public abstract SvgNode Clone();
    }
}