using System;
using System.Linq;
using System.Windows.Input;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using HHChaosToolkit.UWP.Mvvm;
using HHChaosToolkit.UWP.Services.Navigation;
using SvgConverter.SampleApp.Helpers;
using SvgConverter.SampleApp.Views;

namespace SvgConverter.SampleApp.ViewModels
{
    public class ShellViewModel : ViewModelBase
    {
        public static readonly string ContentNavigationServiceKey = "HomeShellViewModel_Content";
        private ICommand _itemInvokedCommand;
        private NavigationView _navigationView;


        private NavigationViewItem _selected;

        public static NavigationService NavigationService =>
            NavigationServiceList.Instance.IsRegistered(ContentNavigationServiceKey)
                ? NavigationServiceList.Instance[ContentNavigationServiceKey]
                : null;


        public NavigationViewItem Selected
        {
            get => _selected;
            set => Set(ref _selected, value);
        }

        public ICommand ItemInvokedCommand => _itemInvokedCommand ??
                                              (_itemInvokedCommand =
                                                  new RelayCommand<NavigationViewItemInvokedEventArgs>(OnItemInvoked));

        public void Initialize(Frame frame, NavigationView navigationView)
        {
            _navigationView = navigationView;
            NavigationServiceList.Instance.RegisterOrUpdateFrame(ContentNavigationServiceKey, frame);
            NavigationService.Navigated += Frame_Navigated;
            if (NavigationService?.Frame?.Content == null)
                NavigationService?.Navigate(typeof(SvgViewerViewModel).FullName);
        }

        private void OnItemInvoked(NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                NavigationService.Navigate(typeof(SettingsViewModel).FullName);
                return;
            }

            var item = _navigationView.MenuItems
                .OfType<NavigationViewItem>()
                .First(menuItem => (string) menuItem.Content == (string) args.InvokedItem);
            var pageKey = item.GetValue(NavHelper.NavigateToProperty) as string;
            NavigationService.Navigate(pageKey);
        }

        private void Frame_Navigated(object sender, NavigationEventArgs e)
        {
            if (e.SourcePageType == typeof(SettingsPage))
            {
                Selected = _navigationView.SettingsItem as NavigationViewItem;
                return;
            }

            Selected = _navigationView.MenuItems
                .OfType<NavigationViewItem>()
                .FirstOrDefault(menuItem => IsMenuItemForPageType(menuItem, e.SourcePageType));
        }

        private bool IsMenuItemForPageType(NavigationViewItem menuItem, Type sourcePageType)
        {
            var navigatedPageKey = NavigationService.GetNameOfRegisteredPage(sourcePageType);
            var pageKey = menuItem.GetValue(NavHelper.NavigateToProperty) as string;
            return pageKey == navigatedPageKey;
        }
    }
}
