using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;

namespace SvgConverter.TextParse
{
    public class TextSvgElement : AnimationBase
    {
        private readonly Dictionary<int, Tuple<float, CharGeometry>> _charGeometrys =
            new Dictionary<int, Tuple<float, CharGeometry>>();

        private readonly List<Tuple<Rect, Win2DCharSvgElement>> _charSvgElements =
            new List<Tuple<Rect, Win2DCharSvgElement>>();

        private readonly object _lockobj = new object();
        private float _progress;

        private TextSvgElement()
        {
        }

        public double TotalLength { get; private set; }
        public double DrewLength => TotalLength * _progress;

        public override float Progress
        {
            get => _progress;
            set
            {
                _progress = value >= 1 ? 1 : value;
                RefreshDrawProgress();
            }
        }

        public string Text { get; private set; }
        public string FontFamily { get; private set; }
        public Color Foreground { get; private set; }
        public double FontSize { get; private set; }

        /// <summary>
        ///     刷新绘制进度
        /// </summary>
        private void RefreshDrawProgress()
        {
            lock (_lockobj)
            {
                var length = DrewLength;
                foreach (var item in _charSvgElements)
                    if (length >= item.Item2.TotalLength)
                    {
                        length -= item.Item2.TotalLength;
                        item.Item2.Progress = 1;
                    }
                    else
                    {
                        item.Item2.Progress = (float) (length / item.Item2.TotalLength);
                        length = 0;
                    }
            }
        }

        public override Vector2? Draw(
            CanvasDrawingSession drawingSession, float drawProgress)
        {
            lock (_lockobj)
            {
                Progress += drawProgress;
                var sourceTransform = drawingSession.Transform;
                drawingSession.Transform = Matrix3x2.Identity;
                var needDrawLenght = TotalLength * drawProgress;
                var charCommandList = new CanvasCommandList(drawingSession);
                var charDrawingSession = charCommandList.CreateDrawingSession();
                try
                {
                    foreach (var item in _charSvgElements)
                    {
                        charDrawingSession.Transform =
                            Matrix3x2.CreateTranslation(new Point(item.Item1.X, item.Item1.Y)
                                .ToVector2());
                        charDrawingSession.Transform = charDrawingSession.Transform * sourceTransform;
                        if (item.Item2.Progress >= 1)
                        {
                            item.Item2.Draw(charDrawingSession, 0);
                        }
                        else
                        {
                            var leftLength = item.Item2.TotalLength - item.Item2.DrewLength;
                            if (needDrawLenght > leftLength)
                            {
                                item.Item2.Draw(charDrawingSession, 1 - item.Item2.Progress);
                                needDrawLenght -= leftLength;
                            }
                            else
                            {
                                var p = item.Item2.Draw(charDrawingSession,
                                    (float) (needDrawLenght / item.Item2.TotalLength));
                                if (p.HasValue) return Vector2.Transform(p.Value, drawingSession.Transform);
                            }
                        }
                    }

                    return null;
                }
                finally
                {
                    charDrawingSession.Dispose();
                    drawingSession.DrawImage(charCommandList);
                    charCommandList.Dispose();
                }
            }
        }

        public override void Dispose()
        {
            lock (_lockobj)
            {
                foreach (var node in _charSvgElements) node.Item2.Dispose();
            }

            GC.SuppressFinalize(this);
        }

        public void ChangeFontColor(Color newFontColor)
        {
            if (Foreground != newFontColor)
                lock (_lockobj)
                {
                    Foreground = newFontColor;
                    foreach (var item in _charSvgElements) item.Item2.ChangeFontColor(newFontColor);
                }
        }

        public static async Task<TextSvgElement> Parse(ICanvasResourceCreator resourceCreator, string text,
            string fontFamily, Color foreground,
            float fontSize = 72f)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;
            var chars = text.ToCharArray();
            var strLayout = new CanvasTextLayout(CanvasDevice.GetSharedDevice(), text,
                new CanvasTextFormat
                {
                    FontFamily = fontFamily,
                    FontSize = fontSize,
                    WordWrapping = CanvasWordWrapping.NoWrap
                }, 0, 0);
            var textGeo = new TextSvgElement
            {
                Text = text,
                FontFamily = fontFamily,
                Foreground = foreground,
                FontSize = fontSize,
                ViewBox = strLayout.LayoutBounds
            };
            var taskDic = new Dictionary<char, Task>();
            var lockobj = new object();
            for (var i = 0; i < chars.Length; i++)
            {
                if (taskDic.Keys.Contains(chars[i]) || chars[i].Equals(' ')) continue;
                var tuple = new Tuple<Dictionary<int, Tuple<float, CharGeometry>>, int, char, string>(
                    textGeo._charGeometrys, i,
                    chars[i], fontFamily);
                var task =
                    new Task(o =>
                    {
                        if (!(o is Tuple<Dictionary<int, Tuple<float, CharGeometry>>, int, char, string> t))
                            return;
                        var charGeometrys = t.Item1;
                        var index = t.Item2;
                        var letter = t.Item3;
                        var font = t.Item4;
                        int rowCount = 72, colCount = 72, pathWidthLimit = 6;
                        var thickness = fontSize / 10f;
                        if (char.IsNumber(letter))
                        {
                            rowCount = 48;
                            colCount = 16;
                            pathWidthLimit = 8;
                            thickness *= 2f;
                        }
                        else if (char.IsUpper(letter))
                        {
                            rowCount = 48;
                            colCount = 16;
                            pathWidthLimit = 8;
                            thickness *= 2f;
                        }
                        else if (char.IsLower(letter))
                        {
                            rowCount = 48;
                            colCount = 16;
                            pathWidthLimit = 8;
                            thickness *= 1.5f;
                        }
                        else if (char.IsPunctuation(letter) || char.IsSymbol(letter))
                        {
                            rowCount = 16;
                            colCount = 16;
                            pathWidthLimit = 4;
                        }

                        var charGeo = TextParseHelper.GetGeometrys(rowCount, colCount, pathWidthLimit, letter, font,
                            fontSize);
                        lock (lockobj)
                        {
                            charGeometrys[index] = new Tuple<float, CharGeometry>(thickness, charGeo);
                        }
                    }, tuple);
                taskDic[chars[i]] = task;
                task.Start();
            }

            await Task.WhenAll(taskDic.Values.ToArray());
            for (var i = 0; i < chars.Length; i++)
            {
                if (chars[i].Equals(' ')) continue;
                var rect = strLayout.GetCharacterRegions(i, 1)[0].LayoutBounds;
                if (textGeo._charGeometrys.TryGetValue(text.IndexOf(chars[i]), out var charGeo))
                {
                    var charSvg =
                        Win2DCharSvgElement.Parse(resourceCreator, charGeo.Item2, foreground, charGeo.Item1);
                    textGeo._charSvgElements.Add(new Tuple<Rect, Win2DCharSvgElement>(rect, charSvg));
                    textGeo.TotalLength += charSvg.TotalLength;
                }
            }

            strLayout.Dispose();
            return textGeo;
        }
    }
}