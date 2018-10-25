using System;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using SvgConverter.SampleApp.Services;
using SvgConverter.SampleApp.ViewModels;

namespace SvgConverter.SampleApp
{
    public sealed partial class App : Application
    {
        private readonly Lazy<ActivationService> _activationService;

        public App()
        {
            InitializeComponent();
            _activationService = new Lazy<ActivationService>(CreateActivationService);
        }

        private ActivationService ActivationService => _activationService.Value;

        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            if (!args.PrelaunchActivated) await ActivationService.ActivateAsync(args);
        }

        protected override async void OnFileActivated(FileActivatedEventArgs args)
        {
            await ActivationService.ActivateAsync(args);
        }

        protected override async void OnActivated(IActivatedEventArgs args)
        {
            await ActivationService.ActivateAsync(args);
        }

        private ActivationService CreateActivationService()
        {
            return new ActivationService(this, typeof(SvgViewerViewModel));
        }
    }
}
