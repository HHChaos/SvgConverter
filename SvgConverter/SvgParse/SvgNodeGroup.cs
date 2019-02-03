using System.Collections.Generic;
using System.Linq;

namespace SvgConverter.SvgParse
{
    public class SvgNodeGroup : SvgNode
    {
        public List<SvgNode> Children { get; private set; } = new List<SvgNode>();

        public override SvgNode Clone()
        {
            var cloneNode = new SvgNodeGroup()
            {
                RenderOpacity = RenderOpacity,
                RenderTransform = RenderTransform,
                Style = Style.Clone(),
                Children = Children.Select(o => o.Clone()).ToList()
            };
            return cloneNode;
        }
    }
}