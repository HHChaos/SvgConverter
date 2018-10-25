using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using HHChaosToolkit.UWP.Services.Navigation;
using SvgConverter.SampleApp.ViewModels;

namespace SvgConverter.SampleApp.Activation
{
    internal class DefaultLaunchActivationHandler : ActivationHandler<LaunchActivatedEventArgs>
    {
        private readonly string _navElement;

        public DefaultLaunchActivationHandler(Type navElement)
        {
            _navElement = navElement.FullName;
        }

        public NavigationService NavigationService => ShellViewModel.NavigationService;

        protected override async Task HandleInternalAsync(LaunchActivatedEventArgs args)
        {
            NavigationService.Navigate(_navElement, args.Arguments);
            await Task.CompletedTask;
        }

        protected override bool CanHandleInternal(LaunchActivatedEventArgs args)
        {
            return NavigationService.Frame.Content == null;
        }
    }
}
