using System;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml.Data;

namespace SvgConverter.SampleApp.Converters
{
    public class ErasingModeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var isCheck = value as bool?;
            if (isCheck == true)
                return InkInputProcessingMode.Erasing;
            return InkInputProcessingMode.Inking;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
