using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using SvgConverter.SampleApp.ViewModels;

namespace SvgConverter.SampleApp.Views
{
    public sealed partial class ImageToSvgPage : Page
    {
        public ImageToSvgPage()
        {
            InitializeComponent();
        }

        private ImageToSvgViewModel ViewModel => DataContext as ImageToSvgViewModel;

        private async void Button_Save_Click(object sender, RoutedEventArgs e)
        {
            var svgContent = await ImageSvgEdit.SaveAsSvgAsync();
            ViewModel.SaveSvgCommand.Execute(svgContent);
        }
    }
}
