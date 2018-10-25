using Windows.UI.Xaml.Controls;
using SvgConverter.SampleApp.ViewModels;

namespace SvgConverter.SampleApp.Views
{
    public sealed partial class SvgViewerPage : Page
    {
        public SvgViewerPage()
        {
            InitializeComponent();
        }

        private SvgViewerViewModel ViewModel => DataContext as SvgViewerViewModel;
    }
}
