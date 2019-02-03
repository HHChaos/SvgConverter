using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Effects;
using SvgConverter.SvgParse;
using SvgConverter.SvgParseForWin2D.Utilities;

namespace SvgConverter.SvgParseForWin2D
{
    public class Win2DSvgElement : AnimationBase
    {
        private readonly object _lockobj = new object();
        private int _drawIndex;

        private float _progress;

        internal Win2DSvgElement()
        {
        }

        public List<Win2DSvgNode> SvgNodeList { get; protected set; }
        protected List<Win2DSvgGeometry> DrawNodeList { get; set; }
        public double TotalLength { get; protected set; }

        public override float Progress
        {
            get => _progress;
            set
            {
                var oldValue = _progress;
                _progress = value > 1 ? 1 : value;
                _progress = _progress < 0 ? 0 : _progress;
                RefreshDrawProgress(_progress - oldValue);
            }
        }

        public double DrewLength => TotalLength * _progress;

        /// <summary>
        ///     刷新绘制进度
        /// </summary>
        /// <param name="dValue">进度差值</param>
        private void RefreshDrawProgress(double dValue)
        {
            lock (_lockobj)
            {
                var dLength = TotalLength * dValue;
                if (dValue == 0)
                    return;
                if (_progress <= 0)
                {
                    foreach (var item in DrawNodeList)
                    {
                        item.Progress = 0;
                        _drawIndex = 0;
                    }

                    return;
                }

                if (_progress >= 1)
                {
                    foreach (var item in DrawNodeList)
                    {
                        item.Progress = 1;
                        _drawIndex = DrawNodeList.Count - 1;
                    }

                    return;
                }

                var step = dValue >= 0 ? 1 : -1;
                do
                {
                    if (_drawIndex > DrawNodeList.Count - 1)
                    {
                        _drawIndex = DrawNodeList.Count - 1;
                        break;
                    }

                    if (_drawIndex < 0)
                    {
                        _drawIndex = 0;
                        break;
                    }

                    var geo = DrawNodeList[_drawIndex];
                    if (dLength > 0)
                    {
                        var leftLength = geo.PathLength * (1 - geo.Progress);
                        if (dLength > leftLength)
                        {
                            dLength -= leftLength;
                            geo.Progress = 1;
                        }
                        else
                        {
                            geo.Progress += dLength / geo.PathLength;
                            break;
                        }
                    }
                    else
                    {
                        var drawLength = geo.PathLength * geo.Progress;
                        if (drawLength > -dLength)
                        {
                            geo.Progress += dLength / geo.PathLength;
                            break;
                        }

                        dLength += drawLength;
                        geo.Progress = 0;
                    }

                    _drawIndex += step;
                } while (true);
            }
        }

        /// <summary>
        ///     按进度绘制svg图像
        /// </summary>
        /// <param name="drawingSession"></param>
        /// <param name="drawProgress">本次绘制的进度</param>
        /// <returns></returns>
        public override Vector2? Draw(
            CanvasDrawingSession drawingSession, float drawProgress)
        {
            lock (_lockobj)
            {
                Progress = _progress + drawProgress;
                if (Progress <= 0)
                    return null;
                var canFill = Progress >= 1 || TotalLength < 0.01;
                var pathCommandList = new CanvasCommandList(drawingSession);
                var pathDrawSession = pathCommandList.CreateDrawingSession();
                CanvasCommandList compositeCommandList = null;
                CanvasDrawingSession compositeDrawSession = null;
                CanvasCommandList markCommandList = null;
                CanvasDrawingSession markDrawingSession = null;
                var sourceTransform = drawingSession.Transform;
                if (!canFill)
                {
                    markCommandList = new CanvasCommandList(drawingSession);
                    markDrawingSession = markCommandList.CreateDrawingSession();
                    compositeCommandList = new CanvasCommandList(drawingSession);
                    compositeDrawSession = compositeCommandList.CreateDrawingSession();
                    drawingSession.Transform = Matrix3x2.Identity;
                }

                try
                {
                    foreach (var item in SvgNodeList)
                        if (item is Win2DSvgImage svgimg)
                        {
                            if (canFill)
                                svgimg.Draw(pathDrawSession);
                            else
                                using (var imgCommandList = new CanvasCommandList(compositeDrawSession))
                                {
                                    using (var imgDrawSession = imgCommandList.CreateDrawingSession())
                                    {
                                        svgimg.Draw(imgDrawSession);
                                    }

                                    compositeDrawSession.Transform = sourceTransform;
                                    compositeDrawSession.DrawImage(imgCommandList);
                                }
                        }
                        else if (item is Win2DSvgGeometry svgGeo)
                        {
                            switch (item.RenderMethod)
                            {
                                case RenderMethod.Draw:
                                    pathDrawSession.Transform = canFill
                                        ? svgGeo.RenderTransform
                                        : svgGeo.RenderTransform * sourceTransform;
                                    var p = svgGeo.RenderStroke(pathDrawSession, svgGeo.Stroke);
                                    if (p.HasValue) return Vector2.Transform(p.Value, drawingSession.Transform);
                                    if (canFill)
                                    {
                                        pathDrawSession.FillGeometry(svgGeo.CanvasGeometry, svgGeo.Fill);
                                    }
                                    else
                                    {
                                        compositeDrawSession.Transform = svgGeo.RenderTransform * sourceTransform;
                                        compositeDrawSession.DrawGeometry(svgGeo.CanvasGeometry, svgGeo.Stroke,
                                            svgGeo.StrokeThickness,
                                            svgGeo.CanvasStrokeStyle);
                                    }

                                    break;
                                case RenderMethod.Mark:
                                    if (!canFill)
                                    {
                                        markDrawingSession.Transform = svgGeo.RenderTransform * sourceTransform;
                                        var pm = svgGeo.RenderStroke(markDrawingSession,
                                            new CanvasSolidColorBrush(drawingSession, Colors.Black));
                                        if (pm.HasValue) return Vector2.Transform(pm.Value, drawingSession.Transform);
                                    }

                                    break;
                                case RenderMethod.Composite:
                                    if (canFill)
                                    {
                                        pathDrawSession.Transform = svgGeo.RenderTransform;
                                        pathDrawSession.DrawGeometry(svgGeo.CanvasGeometry, svgGeo.Stroke,
                                            svgGeo.StrokeThickness, svgGeo.CanvasStrokeStyle);
                                        pathDrawSession.FillGeometry(svgGeo.CanvasGeometry, svgGeo.Fill);
                                    }
                                    else
                                    {
                                        compositeDrawSession.Transform = svgGeo.RenderTransform * sourceTransform;
                                        compositeDrawSession.DrawGeometry(svgGeo.CanvasGeometry, svgGeo.Stroke,
                                            svgGeo.StrokeThickness, svgGeo.CanvasStrokeStyle);
                                        compositeDrawSession.FillGeometry(svgGeo.CanvasGeometry, svgGeo.Fill);
                                    }

                                    break;
                                case RenderMethod.Fill:
                                    if (canFill)
                                    {
                                        pathDrawSession.Transform = svgGeo.RenderTransform;
                                        pathDrawSession.FillGeometry(svgGeo.CanvasGeometry, svgGeo.Fill);
                                    }
                                    else
                                    {
                                        compositeDrawSession.Transform = svgGeo.RenderTransform * sourceTransform;
                                        compositeDrawSession.FillGeometry(svgGeo.CanvasGeometry, svgGeo.Fill);
                                    }

                                    break;
                                case RenderMethod.MarkAndFill:
                                    if (!canFill)
                                    {
                                        markDrawingSession.Transform = svgGeo.RenderTransform * sourceTransform;
                                        var pm = svgGeo.RenderStroke(markDrawingSession,
                                            new CanvasSolidColorBrush(drawingSession, Colors.Black));
                                        compositeDrawSession.Transform = svgGeo.RenderTransform * sourceTransform;
                                        compositeDrawSession.FillGeometry(svgGeo.CanvasGeometry, svgGeo.Fill);
                                        if (pm.HasValue) return Vector2.Transform(pm.Value, drawingSession.Transform);
                                    }
                                    else
                                    {
                                        pathDrawSession.Transform = svgGeo.RenderTransform;
                                        pathDrawSession.FillGeometry(svgGeo.CanvasGeometry, svgGeo.Fill);
                                    }

                                    break;
                            }
                        }
                }
                finally
                {
                    pathDrawSession.Dispose();
                    drawingSession.DrawImage(pathCommandList);
                    pathCommandList.Dispose();
                    if (!canFill)
                    {
                        compositeDrawSession.Dispose();
                        markDrawingSession.Dispose();
                        var effect = new AlphaMaskEffect
                        {
                            Source = compositeCommandList,
                            AlphaMask = markCommandList
                        };
                        drawingSession.DrawImage(effect);
                        markCommandList.Dispose();
                        compositeCommandList.Dispose();
                        effect.Dispose();
                    }
                }

                return null;
            }
        }

        /// <summary>
        ///     将SvgElement转换为Win2DSvgElement对象
        /// </summary>
        /// <param name="resourceCreator"></param>
        /// <param name="svg"></param>
        /// <returns></returns>
        public static async Task<Win2DSvgElement> Parse(ICanvasResourceCreator resourceCreator, SvgElement svg)
        {
            var win2DSvg = new Win2DSvgElement
            {
                ViewBox = svg.ViewBox
            };
            var list = await Win2DSvgParseHelper.GetWin2DGeometrys(resourceCreator, svg);
            var drawList = list.Where(
                    item => item.RenderMethod.Equals(RenderMethod.Draw) || item.RenderMethod.Equals(RenderMethod.Mark))
                .Cast<Win2DSvgGeometry>().ToList();
            win2DSvg.TotalLength =
                drawList.Aggregate<Win2DSvgGeometry, double>(0, (current, item) => current + item.PathLength);
            if (Math.Abs(win2DSvg.TotalLength) < 0.01)
            {
                drawList = new List<Win2DSvgGeometry>();
                foreach (var item in list)
                    if (item.RenderMethod.Equals(RenderMethod.Fill) && ((Win2DSvgGeometry) item).StrokeThickness > 0)
                    {
                        item.RenderMethod = RenderMethod.MarkAndFill;
                        drawList.Add((Win2DSvgGeometry) item);
                        win2DSvg.TotalLength += ((Win2DSvgGeometry) item).PathLength;
                    }
            }

            win2DSvg.SvgNodeList = list;
            win2DSvg.DrawNodeList = drawList;
            win2DSvg.Device = resourceCreator.Device;
            return win2DSvg;
        }

        /// <summary>
        ///     保存svg文件图像到指定的流中
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="svg"></param>
        /// <returns></returns>
        public static async Task RenderImage(IRandomAccessStream stream, SvgElement svg)
        {
            var device = CanvasDevice.GetSharedDevice();
            using (var win2DSvg = await Parse(device, svg))
            {
                win2DSvg.Progress = 1;
                using (
                    var offScreen = new CanvasRenderTarget(device, (float) svg.ViewBox.Width,
                        (float) svg.ViewBox.Height,
                        96))
                {
                    using (var session = offScreen.CreateDrawingSession())
                    {
                        session.Clear(Colors.Transparent);
                        var m = Matrix3x2.CreateTranslation(-(float) svg.ViewBox.X, -(float) svg.ViewBox.Y);
                        session.Transform = m;
                        win2DSvg.Draw(session, 0);
                    }

                    await offScreen.SaveAsync(stream, CanvasBitmapFileFormat.Png);
                }
            }
        }

        public override void Dispose()
        {
            lock (_lockobj)
            {
                foreach (var node in SvgNodeList) node.Dispose();
                SvgNodeList.Clear();
                DrawNodeList.Clear();
            }

            GC.SuppressFinalize(this);
        }
    }
}