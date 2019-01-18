using Windows.UI.Xaml.Data;
using CommonServiceLocator;
using Unity.ServiceLocation;
using Unity;
using Unity.Lifetime;
using HHChaosToolkit.UWP.Mvvm;
using HHChaosToolkit.UWP.Services;
using HHChaosToolkit.UWP.Services.Navigation;
using SvgConverter.SampleApp.Views;

namespace SvgConverter.SampleApp.ViewModels
{
    [Bindable]
    public class ViewModelLocator
    {
        public ViewModelLocator()
        {
            if (!ServiceLocator.IsLocationProviderSet) InitViewModelLocator();
        }

        public ObjectPickerService ObjectPickerService => ServiceLocator.Current.GetInstance<ObjectPickerService>();
        public SubWindowsService SubWindowsService => ServiceLocator.Current.GetInstance<SubWindowsService>();

        public SettingsViewModel SettingsViewModel => ServiceLocator.Current.GetInstance<SettingsViewModel>();

        public InkToSvgViewModel InkToSvgViewModel => ServiceLocator.Current.GetInstance<InkToSvgViewModel>();

        public ImageToSvgViewModel ImageToSvgViewModel => ServiceLocator.Current.GetInstance<ImageToSvgViewModel>();

        public TextSvgPlayerViewModel TextSvgPlayerViewModel =>
            ServiceLocator.Current.GetInstance<TextSvgPlayerViewModel>();

        public SvgViewerViewModel SvgViewerViewModel => ServiceLocator.Current.GetInstance<SvgViewerViewModel>();

        public ShellViewModel ShellViewModel => ServiceLocator.Current.GetInstance<ShellViewModel>();

        private IUnityContainer _container;
        public void InitViewModelLocator()
        {
            _container = new UnityContainer();
            var _serviceLocator = new UnityServiceLocator(_container);
            ServiceLocator.SetLocatorProvider(() => _serviceLocator);

            _container.RegisterType<ObjectPickerService>(new ContainerControlledLifetimeManager())
                      .RegisterType<SubWindowsService>(new ContainerControlledLifetimeManager());

            RegisterNavigationService<ShellViewModel, ShellPage>(NavigationServiceList.DefaultNavigationServiceKey);
            RegisterNavigationService<SvgViewerViewModel, SvgViewerPage>(NavigationServiceList
                .DefaultNavigationServiceKey);

            RegisterNavigationService<SvgViewerViewModel, SvgViewerPage>(ShellViewModel.ContentNavigationServiceKey);
            RegisterNavigationService<TextSvgPlayerViewModel, TextSvgPlayerPage>(ShellViewModel
                .ContentNavigationServiceKey);
            RegisterNavigationService<ImageToSvgViewModel, ImageToSvgPage>(ShellViewModel.ContentNavigationServiceKey);
            RegisterNavigationService<InkToSvgViewModel, InkToSvgPage>(ShellViewModel.ContentNavigationServiceKey);
            RegisterNavigationService<SettingsViewModel, SettingsPage>(ShellViewModel.ContentNavigationServiceKey);
        }

        public void RegisterNavigationService<VM, V>(string nsKey)
            where VM : ViewModelBase
        {
            _container.RegisterType<VM>(new ContainerControlledLifetimeManager());
            if (!NavigationServiceList.Instance.IsRegistered(nsKey))
                NavigationServiceList.Instance.Register(nsKey, new NavigationService());
            var contentService = NavigationServiceList.Instance[nsKey];
            contentService.Configure(typeof(VM).FullName, typeof(V));
        }

        public void RegisterObjectPicker<T, VM, V>()
            where VM : ObjectPickerBase<T>
        {
            _container.RegisterType<VM>(new ContainerControlledLifetimeManager());
            ObjectPickerService.Configure(typeof(T).FullName, typeof(VM).FullName, typeof(V));
        }

        public void RegisterSubWindow<VM, V>()
            where VM : SubWindowBase
        {
            _container.RegisterType<VM>(new ContainerControlledLifetimeManager());
            SubWindowsService.Configure(typeof(VM).FullName, typeof(V));
        }
    }
}
