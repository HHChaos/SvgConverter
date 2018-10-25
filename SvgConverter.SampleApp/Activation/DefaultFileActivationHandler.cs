using System;
using System.Globalization;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using HHChaosToolkit.UWP.Services.Navigation;

namespace SvgConverter.SampleApp.Activation
{
    internal class DefaultFileActivationHandler : ActivationHandler<FileActivatedEventArgs>
    {
        private readonly string _navElement;

        public DefaultFileActivationHandler(Type navElement)
        {
            _navElement = navElement.FullName;
        }

        private NavigationService NavigationService => NavigationServiceList.Instance.Default;

        protected override async Task HandleInternalAsync(FileActivatedEventArgs args)
        {
            NavigationService.Navigate(_navElement, args.Files[0]);
            await Task.CompletedTask;
        }

        protected override bool CanHandleInternal(FileActivatedEventArgs args)
        {
            if (args.Files?.Count > 0)
            {
                var file = args.Files[0];
                if (file.IsOfType(StorageItemTypes.File) &&
                    file.Name.EndsWith(".svg", true, CultureInfo.CurrentCulture))
                    return true;
            }

            return false;
        }
    }
}
