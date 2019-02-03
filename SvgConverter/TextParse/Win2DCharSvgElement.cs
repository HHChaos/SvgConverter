using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Windows.Foundation;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.Text;
using SvgConverter.SvgParseForWin2D;

namespace SvgConverter.TextParse
{
    public class Win2DCharSvgElement : Win2DSvgElement
    {
        internal Win2DCharSvgElement()
        {
        }

        public char Char { get; private set; }
        public string FontFamily { get; private set; }
        public Color Foreground { get; private set; }
        public double FontSize { get; private set; }

        public void ChangeFontColor(Color newFontColor)
        {
            if (Foreground != newFontColor && Device != null)
            {
                Foreground = newFontColor;
                if (SvgNodeList?.Count > 0 &&
                    SvgNodeList[0] is Win2DSvgGeometry geo)
                {
                    geo.Fill = new CanvasSolidColorBrush(Device, newFontColor);
                    geo.Stroke = new CanvasSolidColorBrush(Device, newFontColor);
                }
            }
        }

        public static Win2DCharSvgElement Parse(ICanvasResourceCreator resourceCreator, CharGeometry charGeo,
            Color foreground, float thickness)
        {
            var textLayout = new CanvasTextLayout(resourceCreator, new string(new[] {charGeo.Char}),
                new CanvasTextFormat
                {
                    FontFamily = charGeo.FontFamily,
                    FontSize = charGeo.FontSize,
                    WordWrapping = CanvasWordWrapping.NoWrap
                }, 0, 0);
            var win2DSvg = new Win2DCharSvgElement
            {
                Device = resourceCreator.Device,
                Char = charGeo.Char,
                Foreground = foreground,
                FontFamily = charGeo.FontFamily,
                FontSize = charGeo.FontSize,
                ViewBox =
                    new Rect(new Point(), new Size(textLayout.LayoutBounds.Width, textLayout.LayoutBounds.Height)),
                SvgNodeList = new List<Win2DSvgNode>()
            };
            var drawList = charGeo.GetGeometrys(resourceCreator).Select(path => new Win2DSvgGeometry
            {
                SourceCanvasGeometry = path,
                PathLength = path.ComputePathLength(),
                RenderOpacity = 1,
                Stroke = new CanvasSolidColorBrush(resourceCreator, Colors.Black),
                Fill = new CanvasSolidColorBrush(resourceCreator, Colors.Transparent),
                StrokeThickness = thickness,
                RenderMethod = RenderMethod.Mark,
                CanvasStrokeStyle = new CanvasStrokeStyle
                {
                    LineJoin = CanvasLineJoin.Round,
                    StartCap = CanvasCapStyle.Round,
                    EndCap = CanvasCapStyle.Round
                },
                RenderTransform =
                    Matrix3x2.CreateTranslation(new Point(textLayout.DrawBounds.X, textLayout.DrawBounds.Y)
                        .ToVector2())
            }).ToList();
            var win2DGeo = new Win2DSvgGeometry
            {
                SourceCanvasGeometry = CanvasGeometry.CreateText(textLayout),
                CanvasStrokeStyle = new CanvasStrokeStyle(),
                RenderOpacity = 1,
                Stroke = new CanvasSolidColorBrush(resourceCreator, foreground),
                Fill = new CanvasSolidColorBrush(resourceCreator, foreground),
                StrokeThickness = 1,
                RenderMethod = RenderMethod.Composite
            };
            win2DSvg.TotalLength = drawList
                .Aggregate<Win2DSvgGeometry, double>(0,
                    (current, item) => current + item.PathLength);
            win2DSvg.SvgNodeList.Add(win2DGeo);
            win2DSvg.SvgNodeList.AddRange(drawList);
            win2DSvg.DrawNodeList = drawList;
            return win2DSvg;
        }
    }
}