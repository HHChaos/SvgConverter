using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using HHChaosToolkit.UWP.Controls;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;
using SvgConverter.SampleApp.Models;
using SvgConverter.SvgParse;
using SvgConverter.SvgParseForWin2D;
using SvgConverter.TextParse;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace SvgConverter.SampleApp.Controls
{
    public sealed partial class AnimationPlayer : UserControl, INotifyPropertyChanged
    {
        // Using a DependencyProperty as the backing store for SvgElement.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AnimationItemProperty =
            DependencyProperty.Register("SvgElement", typeof(SvgElement), typeof(AnimationPlayer),
                new PropertyMetadata(null, SvgElementPropertyChangedCallback));

        // Using a DependencyProperty as the backing store for TextSvgInfo.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextSvgInfoProperty =
            DependencyProperty.Register("TextSvgInfo", typeof(TextSvgInfo), typeof(AnimationPlayer),
                new PropertyMetadata(null, TextSvgInfoPropertyChangedCallback));

        private readonly SynchronizationContext _context = SynchronizationContext.Current;
        private readonly List<Action<ICanvasResourceCreator>> _lazyTasks = new List<Action<ICanvasResourceCreator>>();
        private readonly object _lockobj = new object();

        private AnimationBase _animationItem;
        private float _drawGap;
        private CanvasBitmap _handBitmap;
        private PlayState _playState = PlayState.Stopped;
        private int _pointerId = -1;
        private Point _prevPoint;
        private float _scale = 1f;
        private Matrix3x2 _scaleMatrix;

        public AnimationPlayer()
        {
            InitializeComponent();
            TimeBox.Value = 12;
            if (!DesignMode.DesignModeEnabled)
            {
                Canvas.CreateResources += Canvas_CreateResources;
                SizeChanged += UserControl_SizeChanged;
                Unloaded += UserControl_Unloaded;
            }
        }

        public HandInfo Hand { get; set; } = new HandInfo
        {
            Source = new Uri("ms-appx:///Assets/hand1.png"),
            PenOffect = new Point(17, 91)
        };

        public TextSvgInfo TextSvgInfo
        {
            get => (TextSvgInfo) GetValue(TextSvgInfoProperty);
            set => SetValue(TextSvgInfoProperty, value);
        }

        public SvgElement SvgElement
        {
            get => (SvgElement) GetValue(AnimationItemProperty);
            set => SetValue(AnimationItemProperty, value);
        }

        public bool IsPlayReverse { get; set; }

        public ICanvasResourceCreator ResourceCreator
        {
            get
            {
                try
                {
                    var device = Canvas.Device;
                    return device;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public PlayState PlayState
        {
            get => _playState;
            set
            {
                _playState = value;
                switch (value)
                {
                    case PlayState.Playing:
                        Icon.Symbol = Symbol.Pause;
                        break;
                    case PlayState.Paused:
                        Icon.Symbol = Symbol.Play;
                        break;
                    case PlayState.Stopped:
                        Icon.Symbol = Symbol.Play;
                        break;
                }
            }
        }

        public float Progress
        {
            get => _animationItem?.Progress ?? 0;
            set
            {
                if (_animationItem != null)
                    _animationItem.Progress = value;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private static async void TextSvgInfoPropertyChangedCallback(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            if (d is AnimationPlayer control) await control.UpdateAnimationItem((TextSvgInfo) e.NewValue);
        }

        private async Task UpdateAnimationItem(TextSvgInfo textSvgInfo)
        {
            if (string.IsNullOrWhiteSpace(textSvgInfo?.Content))
                return;
            try
            {
                if (ResourceCreator != null)
                {
                    var testSvg = await TextSvgElement.Parse(ResourceCreator, textSvgInfo.Content, textSvgInfo.FontName,
                        textSvgInfo.FontColor);
                    SetAnimationObj(testSvg);
                }
                else
                {
                    _lazyTasks.Add(async c =>
                    {
                        var testSvg = await TextSvgElement.Parse(ResourceCreator, textSvgInfo.Content,
                            textSvgInfo.FontName, textSvgInfo.FontColor);
                        SetAnimationObj(testSvg);
                    });
                }
            }
            catch (Exception exception)
            {
                var toast = new Toast(exception.Message);
                toast.Show();
            }
        }

        private async Task UpdateAnimationItem(SvgElement svg)
        {
            if (svg == null)
                return;
            try
            {
                if (ResourceCreator != null)
                {
                    var animationItem = await Win2DSvgElement.Parse(ResourceCreator, svg);
                    SetAnimationObj(animationItem);
                }
                else
                {
                    _lazyTasks.Add(async c =>
                    {
                        var animationItem = await Win2DSvgElement.Parse(ResourceCreator, svg);
                        SetAnimationObj(animationItem);
                    });
                }
            }
            catch (Exception exception)
            {
                var toast = new Toast(exception.Message);
                toast.Show();
            }
        }

        private static async void SvgElementPropertyChangedCallback(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            if (d is AnimationPlayer control) await control.UpdateAnimationItem((SvgElement) e.NewValue);
        }

        private async void Canvas_CreateResources(CanvasAnimatedControl sender, CanvasCreateResourcesEventArgs args)
        {
            InitedEvent?.Invoke(this, EventArgs.Empty);
            foreach (var t in _lazyTasks) t.Invoke(ResourceCreator);
            _lazyTasks.Clear();
            if (Hand != null)
                using (var randomAccessStream = new InMemoryRandomAccessStream())
                {
                    var handFile =
                        await StorageFile.GetFileFromApplicationUriAsync(Hand.Source);
                    var buffer = await FileIO.ReadBufferAsync(handFile);
                    await randomAccessStream.WriteAsync(buffer);
                    randomAccessStream.Seek(0);
                    _handBitmap = await CanvasBitmap.LoadAsync(ResourceCreator, randomAccessStream);
                }
        }

        public event EventHandler InitedEvent;

        public void SetAnimationObj(AnimationBase animationBase)
        {
            if (animationBase == null) return;
            PlayState = PlayState.Stopped;
            lock (_lockobj)
            {
                _animationItem?.Dispose();
                _animationItem = animationBase;
                var panelWidth = Canvas.ActualWidth - 120;
                var panelHeight = Canvas.ActualHeight - 120;
                var scaleX = panelWidth / animationBase.ViewBox.Width;
                var scaleY = panelHeight / animationBase.ViewBox.Height;
                _scale = (float) Math.Min(scaleX, scaleY);
                _scaleMatrix = Matrix3x2.CreateScale(_scale);
                _scaleMatrix.Translation = new Point(
                        (panelWidth - _animationItem.ViewBox.Width * _scale) / 2 - _animationItem.ViewBox.X * _scale +
                        60,
                        (panelHeight - _animationItem.ViewBox.Height * _scale) / 2 - _animationItem.ViewBox.Y * _scale +
                        60)
                    .ToVector2();
                _animationItem.Progress = 1;
            }

            PlayState = PlayState.Paused;
        }

        private void Canvas_Draw(ICanvasAnimatedControl sender,
            CanvasAnimatedDrawEventArgs args)
        {
            if (PlayState == PlayState.Stopped || _animationItem == null)
                return;
            args.DrawingSession.Transform = _scaleMatrix;
            var needDrawLenght = PlayState == PlayState.Paused ? 0 : (IsPlayReverse ? -_drawGap : _drawGap);
            lock (_lockobj)
            {
                var handPosition = _animationItem?.Draw(args.DrawingSession, needDrawLenght);
                if (Hand != null && handPosition != null && _handBitmap != null)
                {
                    var position = handPosition.Value - Hand.PenOffect.ToVector2();
                    args.DrawingSession.Transform = Matrix3x2.Identity;
                    args.DrawingSession.DrawImage(_handBitmap, position);
                }
            }

            _context.Post(_ =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Progress"));
                if (_animationItem?.Progress >= 1 && !IsPlayReverse || _animationItem?.Progress <= 0 && IsPlayReverse)
                    PlayState = PlayState.Paused;
            }, null);
        }

        private void Button_Play_Click(object sender, RoutedEventArgs e)
        {
            if (!TimeBox.Value.HasValue)
            {
                var toast = new Toast("Please set duration!");
                toast.Show();
                return;
            }

            switch (PlayState)
            {
                case PlayState.Playing:
                    PlayState = PlayState.Paused;
                    break;
                case PlayState.Paused:
                    var totaltime = TimeBox.Value.Value;
                    _drawGap = 1 / totaltime * (float) Canvas.TargetElapsedTime.TotalSeconds;
                    if (_animationItem != null)
                        _animationItem.Progress = IsPlayReverse ? 1 : 0;
                    PlayState = PlayState.Playing;
                    break;
                case PlayState.Stopped:
                    return;
            }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            PlayState = PlayState.Stopped;
            lock (_lockobj)
            {
                _animationItem?.Dispose();
                _animationItem = null;
                _handBitmap?.Dispose();
                _handBitmap = null;
            }
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_animationItem == null)
                return;
            var panelWidth = Canvas.ActualWidth - 120;
            var panelHeight = Canvas.ActualHeight - 120;
            var scaleX = panelWidth / _animationItem.ViewBox.Width;
            var scaleY = panelHeight / _animationItem.ViewBox.Height;
            _scale = (float) Math.Min(scaleX, scaleY);
            _scaleMatrix = Matrix3x2.CreateScale(_scale);
            _scaleMatrix.Translation = new Point(
                    (panelWidth - _animationItem.ViewBox.Width * _scale) / 2 - _animationItem.ViewBox.X * _scale + 60,
                    (panelHeight - _animationItem.ViewBox.Height * _scale) / 2 - _animationItem.ViewBox.Y * _scale + 60)
                .ToVector2();
        }

        private void Canvas_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            var translation = _scaleMatrix.Translation;
            var center = Vector2.Transform(e.GetCurrentPoint(Canvas).Position.ToVector2() - translation,
                Matrix3x2.CreateScale(1 / _scale));
            var offset = e.GetCurrentPoint(Canvas).Properties.MouseWheelDelta / 800f;
            if (_scale + offset < 0.05)
                return;
            _scale += offset;
            _scaleMatrix = Matrix3x2.CreateScale(_scale);
            _scaleMatrix.Translation = translation - Vector2.Transform(center, Matrix3x2.CreateScale(offset));
        }

        private void Canvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            _prevPoint = e.GetCurrentPoint(Canvas).Position;
            Canvas.PointerMoved += Canvas_PointerMoved;
            _pointerId = (int) e.Pointer.PointerId;
        }

        private void Canvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerId != _pointerId)
                return;
            var pos = e.GetCurrentPoint(Canvas).Position;
            _scaleMatrix.Translation = new Point(_scaleMatrix.Translation.X + pos.X - _prevPoint.X,
                _scaleMatrix.Translation.Y + pos.Y - _prevPoint.Y).ToVector2();
            _prevPoint = pos;
        }

        private void Canvas_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerId != _pointerId)
                return;
            Canvas.PointerMoved -= Canvas_PointerMoved;
            _pointerId = -1;
        }
    }
}
