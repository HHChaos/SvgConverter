using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace SvgConverter.SampleApp.Helpers
{
    public class NavHelper
    {
        public static readonly DependencyProperty NavigateToProperty =
            DependencyProperty.RegisterAttached("NavigateTo", typeof(string), typeof(NavHelper),
                new PropertyMetadata(null));

        public static string GetNavigateTo(NavigationViewItem item)
        {
            return (string) item.GetValue(NavigateToProperty);
        }

        public static void SetNavigateTo(NavigationViewItem item, string value)
        {
            item.SetValue(NavigateToProperty, value);
        }
    }
}
