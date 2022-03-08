using System;
using System.Linq;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace KayoEditorGUI
{
    /// <summary>
    /// Interaction logic for QuestionPopup.xaml
    /// </summary>
    public partial class QuestionPopup : Window
    {
        AllowedTextType allowedTextType = AllowedTextType.Text;
        Regex customRegex = null;

        bool confirmed = false;

        public bool Confirmed => confirmed;

        public QuestionPopup(string question)
        {
            InitializeComponent();

            QuestionContent.Text = question;
        }

        public T AskEnum<T>(int defaultIndex = -1) where T : Enum {
            Input_ComboBox.Visibility = Visibility.Visible;

            T[] values = (T[])Enum.GetValues(typeof(T));
            Input_ComboBox.ItemsSource = values.Select((T x) => new Item<T>
            {
                Value = x,
                Display = x.GetDescription()
            });

            Input_ComboBox.SelectedIndex = defaultIndex;
            ShowDialog();

            object item = Input_ComboBox.SelectedItem;
            if (item == null) return values[0];

            return ((Item<T>)item).Value;
        }

        public T AskValue<T>(T[] values, string[] display = null, int defaultIndex = -1)
        {
            Input_ComboBox.Visibility = Visibility.Visible;
            Input_ComboBox.ItemsSource = values.Select((T x, int i) => new Item<T>
            {
                Value = x,
                Display = display != null && display.Length > i ? display[i] : x.ToString()
            });

            Input_ComboBox.SelectedIndex = defaultIndex;
            ShowDialog();

            object item = Input_ComboBox.SelectedItem;
            if (item == null) return values[0];

            return ((Item<T>)item).Value;
        }

        private void ShowTextInput()
        {
            Input_Text.Visibility = Visibility.Visible;
        }

        public string AskText(Regex customRegex = null)
        {
            ShowTextInput();

            allowedTextType = AllowedTextType.Text;
            if (customRegex != null)
            {
                allowedTextType = AllowedTextType.Custom;
                this.customRegex = customRegex;
            }

            ShowDialog();

            return Input_Text.Text;
        }

        public float AskFloat(bool onlyPositive = false)
        {
            ShowTextInput();
            allowedTextType = onlyPositive ? AllowedTextType.PositiveFloat : AllowedTextType.Float;

            ShowDialog();

            string val = Input_Text.Text;
            if (val == "") return 0;

            string separator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            if (separator == ".") val = val.Replace(',', '.');
            else if (separator == ",") val = val.Replace('.', ',');

            return float.Parse(val);
        }

        public int AskInt(bool onlyPositive = false)
        {
            ShowTextInput();
            allowedTextType = onlyPositive ? AllowedTextType.PositiveInt : AllowedTextType.Int;

            ShowDialog();

            string val = Input_Text.Text;
            if (val == "") return 0;

            return int.Parse(val);
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Input_Text.MaxWidth = ActualWidth - 10;
            e.Handled = !IsTextAllowed(Input_Text.Text + e.Text);
        }

        private void TextBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                if (!IsTextAllowed(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private static readonly Regex regexFloat = new Regex(@"^[-+]{0,1}\d+((\.|,)\d*){0,1}$");
        private static readonly Regex regexInt = new Regex(@"^[-+]{0,1}\d+$");
        private static readonly Regex regexPositiveFloat = new Regex(@"^\+{0,1}\d+((\.|,)\d*){0,1}$");
        private static readonly Regex regexPositiveInt = new Regex(@"^\+{0,1}\d+$");

        private bool IsTextAllowed(string text)
        {
            switch(allowedTextType)
            {
                case AllowedTextType.Float:
                    return regexFloat.IsMatch(text);
                case AllowedTextType.Int:
                    return regexInt.IsMatch(text);
                case AllowedTextType.PositiveFloat:
                    return regexPositiveFloat.IsMatch(text);
                case AllowedTextType.PositiveInt:
                    return regexPositiveInt.IsMatch(text);
                case AllowedTextType.Custom:
                    return customRegex.IsMatch(text);
                default:
                    return true;
            }
        }

        public enum AllowedTextType
        {
            Text, Float, Int, PositiveFloat, PositiveInt, Custom
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            confirmed = false;
            Close();
        }

        private void ButtonConfirm_Click(object sender, RoutedEventArgs e)
        {
            confirmed = true;
            Close();
        }

        private void TopBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private struct Item<T>
        {
            public T Value;
            public string Display;

            public override string ToString()
            {
                return Display;
            }
        }
    }
}
