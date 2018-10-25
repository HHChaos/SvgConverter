using Windows.Foundation.Metadata;
using Windows.UI.Xaml.Controls;
using SvgConverter.SampleApp.ViewModels;

namespace SvgConverter.SampleApp.Views
{
    // TODO WTS: Change the icons and titles for all NavigationViewItems in ShellPage.xaml.
    public sealed partial class ShellPage : Page
    {
        public ShellPage()
        {
            InitializeComponent();
            HideNavViewBackButton();
            DataContext = ViewModel;
            ViewModel.Initialize(shellFrame, navigationView);
        }

        private ShellViewModel ViewModel => DataContext as ShellViewModel;

        private void HideNavViewBackButton()
        {
            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 6))
                navigationView.IsBackButtonVisible = NavigationViewBackButtonVisible.Collapsed;
        }
    }
}
