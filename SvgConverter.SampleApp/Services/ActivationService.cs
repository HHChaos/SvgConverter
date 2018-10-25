using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using HHChaosToolkit.UWP.Services.Navigation;
using SvgConverter.SampleApp.Activation;
using SvgConverter.SampleApp.ViewModels;

namespace SvgConverter.SampleApp.Services
{
    // For more information on application activation see https://github.com/Microsoft/WindowsTemplateStudio/blob/master/docs/activation.md
    internal class ActivationService
    {
        private readonly App _app;
        private readonly Type _defaultNavItem;

        public ActivationService(App app, Type defaultNavItem)
        {
            _app = app;
            _defaultNavItem = defaultNavItem;
        }

        public async Task ActivateAsync(object activationArgs)
        {
            if (IsInteractive(activationArgs))
            {
                await InitializeAsync();
                if (Window.Current.Content == null)
                {
                    var rootFrame = new Frame();
                    Window.Current.Content = rootFrame;
                    NavigationServiceList.Instance.Default.Navigate(typeof(ShellViewModel).FullName);
                }


                var activationHandler = GetActivationHandlers()
                    .FirstOrDefault(h => h.CanHandle(activationArgs));

                if (activationHandler != null) await activationHandler.HandleAsync(activationArgs);

                // Ensure the current window is active
                Window.Current.Activate();

                // Tasks after activation
                await StartupAsync();
            }
        }

        private async Task InitializeAsync()
        {
            await ThemeSelectorService.InitializeAsync();
        }

        private async Task StartupAsync()
        {
            await ThemeSelectorService.SetRequestedThemeAsync();
        }

        private IEnumerable<ActivationHandler> GetActivationHandlers()
        {
            yield return new DefaultLaunchActivationHandler(_defaultNavItem);
            yield return new DefaultFileActivationHandler(typeof(SvgViewerViewModel));
        }

        private bool IsInteractive(object args)
        {
            return args is IActivatedEventArgs;
        }
    }
}
