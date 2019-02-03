using Windows.UI;

namespace SvgConverter.SvgParse.Brushes
{
    public struct SvgGradientStop
    {
        //
        // 摘要:
        //     The position of the gradient stop. Expected to be between 0 and 1, inclusive.
        public float Position;
        public Color Color;
    }
}