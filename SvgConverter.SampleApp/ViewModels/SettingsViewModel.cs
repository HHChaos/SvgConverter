using System.Windows.Input;
using Windows.ApplicationModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;
using HHChaosToolkit.UWP.Mvvm;
using SvgConverter.SampleApp.Helpers;
using SvgConverter.SampleApp.Services;

namespace SvgConverter.SampleApp.ViewModels
{
    // TODO WTS: Add other settings as necessary. For help see https://github.com/Microsoft/WindowsTemplateStudio/blob/master/docs/pages/settings.md
    public class SettingsViewModel : ViewModelBase
    {
        private ElementTheme _elementTheme = ThemeSelectorService.Theme;

        private ICommand _switchThemeCommand;

        private string _versionDescription;

        public ElementTheme ElementTheme
        {
            get => _elementTheme;

            set => Set(ref _elementTheme, value);
        }

        public string VersionDescription
        {
            get => _versionDescription;

            set => Set(ref _versionDescription, value);
        }

        public ICommand SwitchThemeCommand
        {
            get
            {
                if (_switchThemeCommand == null)
                    _switchThemeCommand = new RelayCommand<ElementTheme>(
                        async param =>
                        {
                            ElementTheme = param;
                            await ThemeSelectorService.SetThemeAsync(param);
                        });

                return _switchThemeCommand;
            }
        }

        public override void OnNavigatedTo(NavigationEventArgs e)
        {
            VersionDescription = GetVersionDescription();
        }

        private string GetVersionDescription()
        {
            var appName = "AppDisplayName".GetLocalized();
            var package = Package.Current;
            var packageId = package.Id;
            var version = packageId.Version;

            return $"{appName} - {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        }
    }
}
