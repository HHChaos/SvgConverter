using Windows.UI.Xaml.Controls;
using SvgConverter.SampleApp.ViewModels;

namespace SvgConverter.SampleApp.Views
{
    public sealed partial class TextSvgPlayerPage : Page
    {
        public TextSvgPlayerPage()
        {
            InitializeComponent();
        }

        private TextSvgPlayerViewModel ViewModel => DataContext as TextSvgPlayerViewModel;
    }
}
