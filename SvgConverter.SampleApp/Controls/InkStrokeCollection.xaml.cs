using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml.Controls;
using HHChaosToolkit.UWP.Mvvm;
using Microsoft.Graphics.Canvas;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace SvgConverter.SampleApp.Controls
{
    public sealed partial class InkStrokeCollection : UserControl
    {
        private readonly List<InkStroke> _redoList = new List<InkStroke>();
        private readonly List<InkStroke> _undoList = new List<InkStroke>();
        private RelayCommand _redoCommand;
        private RelayCommand _undoCommand;

        public InkStrokeCollection()
        {
            InitializeComponent();
            InkCanvas.InkPresenter.InputDeviceTypes = CoreInputDeviceTypes.Mouse |
                                                      CoreInputDeviceTypes.Pen |
                                                      CoreInputDeviceTypes.Touch;
            InkCanvas.InkPresenter.UpdateDefaultDrawingAttributes(new InkDrawingAttributes
            {
                IgnorePressure = true,
                Color = Colors.Black,
                PenTip = PenTipShape.Circle,
                Size = new Size(2, 2)
            });
            InkCanvas.InkPresenter.StrokesCollected += InkPresenter_StrokesCollected;
            InkCanvas.InkPresenter.StrokesErased += InkPresenter_StrokesErased;
        }

        public RelayCommand UndoCommand
        {
            get
            {
                return _undoCommand ?? (_undoCommand = new RelayCommand(() =>
                {
                    if (_redoList.Count > 0)
                    {
                        var item = _redoList.Last();
                        _redoList.Remove(item);
                        _undoList.Add(item.Clone());
                        item.Selected = true;
                        InkCanvas.InkPresenter.StrokeContainer.DeleteSelected();
                        StrokesChanged?.Invoke(InkCanvas.InkPresenter, EventArgs.Empty);
                    }

                    _undoCommand?.OnCanExecuteChanged();
                    _redoCommand?.OnCanExecuteChanged();
                }, () => _redoList.Count > 0));
            }
        }

        public RelayCommand RedoCommand
        {
            get
            {
                return _redoCommand ?? (_redoCommand = new RelayCommand(() =>
                {
                    if (_undoList.Count > 0)
                    {
                        var item = _undoList.Last();
                        _undoList.Remove(item);
                        InkCanvas.InkPresenter.StrokeContainer.AddStroke(item);
                        _redoList.Add(item);
                        StrokesChanged?.Invoke(InkCanvas.InkPresenter, EventArgs.Empty);
                    }

                    _undoCommand?.OnCanExecuteChanged();
                    _redoCommand?.OnCanExecuteChanged();
                }, () => _undoList.Count > 0));
            }
        }

        public InkCanvas RootInkCanvas => InkCanvas;

        public string SvgPathContent
        {
            get
            {
                var rect = InkCanvas.InkPresenter.StrokeContainer.BoundingRect;
                var path = ParseInk(InkCanvas.InkPresenter.StrokeContainer.GetStrokes());
                var svgContent = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                                 "<svg version=\"1.1\" xmlns=\"http://www.w3.org/2000/svg\" xmlns:xlink=\"http://www.w3.org/1999/xlink\" xml:space=\"preserve\" " +
                                 $"viewBox=\"{(float) rect.X} {(float) rect.Y} {(float) rect.Width} {(float) rect.Height}\">\n" +
                                 $"{path}" +
                                 "</svg>";
                return svgContent;
            }
        }

        public event TypedEventHandler<InkPresenter, EventArgs> StrokesChanged;

        private void InkPresenter_StrokesErased(InkPresenter sender, InkStrokesErasedEventArgs args)
        {
            foreach (var item in args.Strokes)
            {
                var strokeBuilder = new InkStrokeBuilder();
                strokeBuilder.SetDefaultDrawingAttributes(item.DrawingAttributes);
                var stroke = strokeBuilder.CreateStrokeFromInkPoints(item.GetInkPoints(), item.PointTransform);
                _undoList.Add(stroke);
                if (_redoList.Contains(item))
                    _redoList.Remove(item);
            }

            _undoCommand?.OnCanExecuteChanged();
            _redoCommand?.OnCanExecuteChanged();
            StrokesChanged?.Invoke(sender, EventArgs.Empty);
        }

        private void InkPresenter_StrokesCollected(InkPresenter sender, InkStrokesCollectedEventArgs args)
        {
            foreach (var item in args.Strokes) _redoList.Add(item);
            _undoList.Clear();
            _undoCommand?.OnCanExecuteChanged();
            _redoCommand?.OnCanExecuteChanged();
            StrokesChanged?.Invoke(sender, EventArgs.Empty);
        }

        public async Task<string> GetFullSvgContent()
        {
            var strokes = InkCanvas.InkPresenter.StrokeContainer.GetStrokes();
            var rect = InkCanvas.InkPresenter.StrokeContainer.BoundingRect;
            var path = ParseInk(strokes, 0);
            var image = await ParseInkToImgBase64(strokes);
            var svgContent = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                             "<svg version=\"1.1\" xmlns=\"http://www.w3.org/2000/svg\" xmlns:xlink=\"http://www.w3.org/1999/xlink\" xml:space=\"preserve\" " +
                             $"viewBox=\"{(float) rect.X} {(float) rect.Y} {(float) rect.Width} {(float) rect.Height}\">\n" +
                             $"{image}{path}" +
                             "</svg>";
            return svgContent;
        }

        private string ParseInk(IEnumerable<InkStroke> strokes, float opacity = 1)
        {
            var pathBuilder = new StringBuilder();
            foreach (var stroke in strokes)
            {
                var path = new StringBuilder();
                var size = stroke.DrawingAttributes.Size;
                var inkPoints = stroke.GetInkPoints().Select(point => point.Position).ToList();
                foreach (var p in inkPoints) path.Append($" L{p.X},{p.Y}");
                if (path.Length != 0)
                    pathBuilder.Append(
                        $"    <path fill=\"none\" opacity=\"{opacity}\" stroke=\"black\" stroke-width=\"{size.Width}\" stroke-linecap=\"round\" d=\"M{path.ToString().Substring(2)}\"/>\n");
            }

            return pathBuilder.ToString();
        }

        private async Task<string> ParseInkToImgBase64(IEnumerable<InkStroke> strokes)
        {
            string base64;
            var rect = InkCanvas.InkPresenter.StrokeContainer.BoundingRect;
            var device = CanvasDevice.GetSharedDevice();
            using (
                var offScreen = new CanvasRenderTarget(device, (float) rect.Width,
                    (float) rect.Height,
                    96))
            {
                using (var session = offScreen.CreateDrawingSession())
                {
                    session.Clear(Colors.Transparent);
                    var m = Matrix3x2.CreateTranslation(-(float) rect.X, -(float) rect.Y);
                    session.Transform = m;
                    session.DrawInk(strokes);
                }

                using (var stream = new InMemoryRandomAccessStream())
                {
                    await offScreen.SaveAsync(stream, CanvasBitmapFileFormat.Png);
                    await stream.FlushAsync();
                    var imageBytes = new byte[stream.Size];
                    var buffer = CryptographicBuffer.CreateFromByteArray(imageBytes);
                    await stream.ReadAsync(buffer, (uint) stream.Size, InputStreamOptions.None);
                    CryptographicBuffer.CopyToByteArray(buffer, out imageBytes);
                    var scaleMatrix = Matrix3x2.CreateTranslation((float) rect.X, (float) rect.Y);
                    base64 = "    <image overflow=\"visible\" " +
                             $"width=\"{(float) rect.Width}\" height=\"{(float) rect.Height}\" " +
                             $"xlink:href=\"data:image/png;base64,{Convert.ToBase64String(imageBytes)}\" " +
                             $"transform=\"matrix({scaleMatrix.M11} {scaleMatrix.M12} {scaleMatrix.M21} {scaleMatrix.M22} {scaleMatrix.M31} {scaleMatrix.M32})\">" +
                             "</image>\n";
                }
            }

            return base64;
        }
    }
}
