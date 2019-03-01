using Windows.UI.Xaml.Controls;
using SvgConverter.SampleApp.ViewModels;

namespace SvgConverter.SampleApp.Views
{
    public sealed partial class SvgViewerPage : Page
    {
        public SvgViewerPage()
        {
            InitializeComponent();
            this.Loaded += SvgViewerPage_Loaded;
        }

        private void SvgViewerPage_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ViewModel?.InitPlayer(AnimationPlayer);
        }

        private SvgViewerViewModel ViewModel => DataContext as SvgViewerViewModel;
    }
}
