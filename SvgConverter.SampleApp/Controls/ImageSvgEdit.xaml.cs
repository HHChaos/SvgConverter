using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using HHChaosToolkit.UWP.Mvvm;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace SvgConverter.SampleApp.Controls
{
    public sealed partial class ImageSvgEdit : UserControl
    {
        // Using a DependencyProperty as the backing store for StrokeSize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StrokeSizeProperty =
            DependencyProperty.Register("StrokeSize", typeof(double), typeof(ImageSvgEdit),
                new PropertyMetadata(8d, StrokeSizePropertyChangedCallback));

        // Using a DependencyProperty as the backing store for BackImageFile.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BackImageFileProperty =
            DependencyProperty.Register("BackImageFile", typeof(StorageFile), typeof(ImageSvgEdit),
                new PropertyMetadata(null, BackImageFilePropertyChangedCallback));

        private readonly List<InkStroke> _redoList = new List<InkStroke>();
        private readonly List<InkStroke> _undoList = new List<InkStroke>();
        private Size _imageSize;
        private RelayCommand _redoCommand;
        private Matrix3x2 _scaleMatrix;

        private RelayCommand _undoCommand;

        public ImageSvgEdit()
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
                Size = new Size(8, 8)
            });
            InkCanvas.InkPresenter.StrokesCollected += InkPresenter_StrokesCollected;
            InkCanvas.InkPresenter.StrokesErased += InkPresenter_StrokesErased;
        }


        public StorageFile BackImageFile
        {
            get => (StorageFile) GetValue(BackImageFileProperty);
            set => SetValue(BackImageFileProperty, value);
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
                    }

                    _undoCommand?.OnCanExecuteChanged();
                    _redoCommand?.OnCanExecuteChanged();
                }, () => _undoList.Count > 0));
            }
        }

        public InkInputProcessingMode InkInputMode
        {
            get => InkCanvas.InkPresenter.InputProcessingConfiguration.Mode;
            set => InkCanvas.InkPresenter.InputProcessingConfiguration.Mode = value;
        }

        public double StrokeSize
        {
            get => (double) GetValue(StrokeSizeProperty);
            set => SetValue(StrokeSizeProperty, value);
        }

        private static async void BackImageFilePropertyChangedCallback(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            if (d is ImageSvgEdit control) await control.SetImageAsync((StorageFile) e.NewValue);
        }

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
        }

        private void InkPresenter_StrokesCollected(InkPresenter sender, InkStrokesCollectedEventArgs args)
        {
            foreach (var item in args.Strokes) _redoList.Add(item);
            _undoList.Clear();
            _undoCommand?.OnCanExecuteChanged();
            _redoCommand?.OnCanExecuteChanged();
        }

        private static void StrokeSizePropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ImageSvgEdit control)
            {
                var size = (double) e.NewValue;
                if (size > 0)
                    control.InkCanvas.InkPresenter.UpdateDefaultDrawingAttributes(new InkDrawingAttributes
                    {
                        IgnorePressure = true,
                        Color = Colors.Black,
                        PenTip = PenTipShape.Circle,
                        Size = new Size(size, size)
                    });
            }
        }

        public async Task SetImageAsync(StorageFile imageFile)
        {
            if (imageFile == null || !imageFile.IsAvailable)
            {
                _scaleMatrix = Matrix3x2.Identity;
                _imageSize = new Size();
                BackImage.Source = null;
                BackImage.RenderTransform = null;
                return;
            }

            var image = new BitmapImage();
            using (var stream = await imageFile.OpenReadAsync())
            {
                image.SetSource(stream);
            }

            _imageSize = new Size(image.PixelWidth, image.PixelHeight);
            BackImage.Source = image;
            var scaleX = CanvasGrid.ActualWidth / image.PixelWidth;
            var scaleY = CanvasGrid.ActualHeight / image.PixelHeight;
            var scale = Math.Min(scaleX, scaleY);
            _scaleMatrix = Matrix3x2.CreateScale((float) scale);
            _scaleMatrix.Translation = scale.Equals(scaleX)
                ? new Point(0, image.PixelHeight * (scaleY - scaleX) / 2).ToVector2()
                : new Point(image.PixelWidth * (scaleX - scaleY) / 2, 0).ToVector2();
            BackImage.RenderTransform = new MatrixTransform
            {
                Matrix = new Matrix
                {
                    M11 = _scaleMatrix.M11,
                    M12 = _scaleMatrix.M12,
                    M21 = _scaleMatrix.M21,
                    M22 = _scaleMatrix.M22,
                    OffsetX = _scaleMatrix.M31,
                    OffsetY = _scaleMatrix.M32
                }
            };
        }

        public async Task<string> SaveAsSvgAsync()
        {
            var path = ParseInk(InkCanvas.InkPresenter.StrokeContainer.GetStrokes());
            var image = await ParseImage(BackImageFile, _imageSize, _scaleMatrix);
            var svgContent = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                             "<svg version=\"1.1\" xmlns=\"http://www.w3.org/2000/svg\" xmlns:xlink=\"http://www.w3.org/1999/xlink\" xml:space=\"preserve\" " +
                             $"viewBox=\"0 0 {CanvasGrid.ActualWidth} {CanvasGrid.ActualHeight}\">\n" +
                             $"{image}{path}" +
                             "</svg>";
            return svgContent;
        }

        private async Task<string> ParseImage(StorageFile imageFile, Size imageSize, Matrix3x2 scaleMatrix)
        {
            if (imageFile == null)
                return string.Empty;
            byte[] dataBytes;
            using (var stream = (await imageFile.OpenReadAsync()).AsStream())
            {
                dataBytes = new byte[stream.Length];
                await stream.ReadAsync(dataBytes, 0, dataBytes.Length);
            }

            return
                "    <image overflow=\"visible\" " +
                $"width=\"{imageSize.Width}\" height=\"{imageSize.Height}\" " +
                $"xlink:href=\"data:image/{imageFile.FileType.Substring(1).ToLower()};base64,{Convert.ToBase64String(dataBytes)}\" " +
                $"transform=\"matrix({scaleMatrix.M11} {scaleMatrix.M12} {scaleMatrix.M21} {scaleMatrix.M22} {scaleMatrix.M31} {scaleMatrix.M32})\">" +
                "</image>\n";
        }

        private string ParseInk(IEnumerable<InkStroke> strokes)
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
                        $"    <path fill=\"none\" opacity=\"0\" stroke-width=\"{size.Width}\" d=\"M{path.ToString().Substring(2)}\"/>\n");
            }

            return pathBuilder.ToString();
        }
    }
}
