using Windows.UI.Xaml.Controls;
using SvgConverter.SampleApp.ViewModels;

namespace SvgConverter.SampleApp.Views
{
    public sealed partial class TextSvgPlayerPage : Page
    {
        public TextSvgPlayerPage()
        {
            InitializeComponent();
            this.Loaded += TextSvgPlayerPage_Loaded;
        }

        private void TextSvgPlayerPage_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ViewModel?.InitPlayer(AnimationPlayer);
        }

        private TextSvgPlayerViewModel ViewModel => DataContext as TextSvgPlayerViewModel;
    }
}
