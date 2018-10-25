using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using SvgConverter.SampleApp.ViewModels;

namespace SvgConverter.SampleApp.Views
{
    public sealed partial class InkToSvgPage : Page
    {
        public InkToSvgPage()
        {
            InitializeComponent();
        }

        private InkToSvgViewModel ViewModel => DataContext as InkToSvgViewModel;

        private async void InkBtnSave_Click(object sender, RoutedEventArgs e)
        {
            var svgContent = await InkStrokeCollection.GetFullSvgContent();
            ViewModel.SaveSvgCommand.Execute(svgContent);
        }
    }
}
