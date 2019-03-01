using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Windows.UI;
using HHChaosToolkit.UWP.Mvvm;
using Microsoft.Graphics.Canvas.Text;
using SvgConverter.SampleApp.Helpers;
using SvgConverter.SampleApp.Models;
using SvgConverter.SampleApp.Controls;
using HHChaosToolkit.UWP.Services.Navigation;

namespace SvgConverter.SampleApp.ViewModels
{
    public class TextSvgPlayerViewModel : ViewModelBase
    {
        private string _content;
        private TextSvgInfo _currentTextSvgInfo;
        private ColorInfo _selectedFontColor;
        private string _selectedFontName;
        private AnimationPlayer _animationPlayer;

        public TextSvgPlayerViewModel()
        {
            foreach (var font in CanvasFontSet.GetSystemFontSet().Fonts.Where(font => !font.IsSymbolFont).Select(font => font.FamilyNames?.First().Value)
                .Distinct())
                FontNames.Add(font);
            foreach (var p in typeof(Colors).GetProperties())
                if (p.GetValue(null) is Color color)
                {
                    if (color.Equals(Colors.Transparent))
                        continue;
                    AllColors.Add(new ColorInfo
                    {
                        Name = p.Name,
                        Color = color
                    });
                }

            if (FontNames.Count > 0) SelectedFontName = FontNames[0];
            if (AllColors.Count > 0) SelectedFontColor = AllColors.FirstOrDefault(c => c.Color == Colors.Black);
            Content = "TextToSvgContentSample".GetLocalized();
            _currentTextSvgInfo = new TextSvgInfo
            {
                Content = Content ?? "Test Text",
                FontName = _selectedFontName ?? "Arial",
                FontColor = _selectedFontColor.Color
            };
        }

        public async void InitPlayer(AnimationPlayer animationPlayer)
        {
            _animationPlayer = animationPlayer;
            if (_currentTextSvgInfo != null)
            {
                await _animationPlayer.SetAnimationItem(_currentTextSvgInfo);
            }
        }

        public override void OnNavigatedFrom(NavigatedFromEventArgs e)
        {
            _animationPlayer = null;
            base.OnNavigatedFrom(e);
        }

        public string Content
        {
            get => _content;
            set => Set(ref _content, value);
        }

        public string SelectedFontName
        {
            get => _selectedFontName;
            set => Set(ref _selectedFontName, value);
        }

        public ColorInfo SelectedFontColor
        {
            get => _selectedFontColor;
            set => Set(ref _selectedFontColor, value);
        }

        public ObservableCollection<string> FontNames { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<ColorInfo> AllColors { get; set; } = new ObservableCollection<ColorInfo>();

        public ICommand ParseTextCommand
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    if (!string.IsNullOrWhiteSpace(Content))
                    {
                        _currentTextSvgInfo = new TextSvgInfo
                        {
                            Content = Content,
                            FontName = _selectedFontName ?? "Arial",
                            FontColor = _selectedFontColor.Color
                        };
                        if (_animationPlayer != null)
                        {
                            await _animationPlayer.SetAnimationItem(_currentTextSvgInfo);
                        }
                    }
                });
            }
        }
    }
}
