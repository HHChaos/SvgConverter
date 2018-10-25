using System;
using System.Globalization;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace SvgConverter.SampleApp.Controls
{
    public class NumberTextBox : TextBox
    {
        private bool _isChangingTextWithCode;
        private bool _isChangingValueWithCode;
        private float? _value;

        public NumberTextBox()
        {
            Loaded += NumberTextBox_Loaded;
        }

        public float? Value
        {
            get => _value;
            set
            {
                if (_value == value)
                    return;
                _value = value;
                UpdateValueText();
                ValueChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler ValueChanged;

        private void NumberTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            TextChanged += NumberTextBox_TextChanged;
            KeyDown += NumberTextBox_KeyDown;
            GotFocus += NumberTextBox_GotFocus;
            LostFocus += NumberTextBox_LostFocus;
            Unloaded += NumberTextBox_Unloaded;
        }

        private void NumberTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            SelectAll();
        }

        private void NumberTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdateValueText();
        }

        private void NumberTextBox_Unloaded(object sender, RoutedEventArgs e)
        {
            TextChanged -= NumberTextBox_TextChanged;
            KeyDown -= NumberTextBox_KeyDown;
            GotFocus -= NumberTextBox_GotFocus;
            LostFocus -= NumberTextBox_LostFocus;
            Loaded -= NumberTextBox_Loaded;
            Unloaded -= NumberTextBox_Unloaded;
        }

        private void NumberTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                UpdateValueText();
                SelectAll();
                e.Handled = true;
            }
        }

        private void NumberTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateValueFromText();
        }

        private bool UpdateValueFromText()
        {
            if (_isChangingTextWithCode) return false;
            if (float.TryParse(Text, NumberStyles.Any, CultureInfo.CurrentUICulture, out var val))
            {
                if (val >= 0)
                {
                    _isChangingValueWithCode = true;
                    Value = val;
                    _isChangingValueWithCode = false;
                    return true;
                }
            }
            else if (string.IsNullOrEmpty(Text))
            {
                Value = null;
                return true;
            }

            return false;
        }

        private void UpdateValueText()
        {
            if (_isChangingValueWithCode) return;
            _isChangingTextWithCode = true;
            Text = Value?.ToString() ?? string.Empty;
            _isChangingTextWithCode = false;
        }
    }
}
