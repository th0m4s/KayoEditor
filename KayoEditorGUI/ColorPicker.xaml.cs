using KayoEditor;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace KayoEditorGUI
{
    /// <summary>
    /// Interaction logic for MessagePopup.xaml
    /// </summary>
    public partial class ColorPicker : Window
    {
        private SelectedType selectedType = SelectedType.None;

        int h; // 0-360
        int s; // 0-100
        int v; // 0-100

        private bool confirmed = false;
        public bool Confirmed => confirmed;

        public ColorPicker(Pixel currentColor)
        {
            InitializeComponent();

            LoadRGB(currentColor.R, currentColor.G, currentColor.B);
            OriginalColorPreview.Fill = new SolidColorBrush(Color.FromRgb(currentColor.R, currentColor.G, currentColor.B));

            confirmed = false;
        }

        public ColorPicker() : this(new Pixel()) { }

        public Pixel AskColor()
        {
            ShowDialog();
            SolidColorBrush brush = (SolidColorBrush)NewColorPreview.Fill;

            return new Pixel(brush.Color.R, brush.Color.G, brush.Color.B);
        }

        private void LoadRGB(byte r, byte g, byte b)
        {
            System.Drawing.Color color = System.Drawing.Color.FromArgb(r, g, b);
            h = (int)color.GetHue();
            s = (int)(color.GetSaturation() * 100);
            v = (int)(Math.Max(r, Math.Max(g, b)) / 255f * 100);
        }

        bool listenToInputChanges = true;

        private void UpdateDisplay(bool updateRGBInputs = true, bool updateHexInput = true)
        {
            double S = s / 100d;
            double V = v / 100d;
            double hf = h / 60d;

            int i = (int)Math.Floor(hf);
            double f = hf - i;

            double pv = V * (1 - S);
            double qv = V * (1 - S * f);
            double tv = V * (1 - S * (1 - f));

            double r = 0, g = 0, b = 0, mr = 1, mg = 0, mb = 0;
            switch(i)
            {
                case 0:
                    r = V;
                    g = tv;
                    b = pv;

                    mg = hf;
                    break;
                case 1:
                    r = qv;
                    g = V;
                    b = pv;

                    mr = 2 - hf;
                    mg = 1;
                    break;
                case 2:
                    r = pv;
                    g = V;
                    b = tv;

                    mr = 0;
                    mg = 1;
                    mb = hf - 2;
                    break;
                case 3:
                    r = pv;
                    g = qv;
                    b = V;

                    mr = 0;
                    mg = 4 - hf;
                    mb = 1;
                    break;
                case 4:
                    r = tv;
                    g = pv;
                    b = V;

                    mr = hf - 4;
                    mb = 1;
                    break;
                case 5:
                    r = V;
                    g = pv;
                    b = qv;

                    mr = 1;
                    mb = 6 - hf;
                    break;
                case 6:
                    r = V;
                    g = tv;
                    b = pv;
                    break;
            }
            byte R = (byte)(r * 255), G = (byte)(g * 255), B = (byte)(b * 255);

            NewColorPreview.Fill = new SolidColorBrush(Color.FromRgb(R, G, B));
            SatValColor.Color = Color.FromRgb((byte)(mr * 255), (byte)(mg * 255), (byte)(mb * 255));

            TranslateSliderHue.X = h / 360f * HueGrid.ActualWidth;
            TranslateSliderSatVal.X = s / 100f * ValueSaturationGrid.ActualWidth;
            TranslateSliderSatVal.Y = (100-v) / 100f * ValueSaturationGrid.ActualHeight;

            listenToInputChanges = false;

            if (updateRGBInputs)
            {
                InputR.Text = "" + R;
                InputG.Text = "" + G;
                InputB.Text = "" + B;
            }

            if(updateHexInput)
            {
                string res = "";

                if (R < 16) res += "0";
                res += Convert.ToString(R, 16);

                if (G < 16) res += "0";
                res += Convert.ToString(G, 16);

                if (B < 16) res += "0";
                res += Convert.ToString(B, 16);

                InputHex.Text = "#" + res.ToUpper();
            }

            listenToInputChanges = true;
        }

        private void TopBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        bool canMove = true;

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (!canMove) return;
            canMove = false;

            if(selectedType != SelectedType.None)
            {
                if(selectedType == SelectedType.Hue)
                {
                    UpdateMouseHue();
                } else if(selectedType == SelectedType.SaturationValue)
                {
                    UpdateMouseSatVal();
                }

                UpdateDisplay();
            }

            canMove = true;
        }

        private void UpdateMouseHue()
        {
            Point point = Mouse.GetPosition(HueGrid);
            h = (int)(Math.Max(0, Math.Min(1, point.X / HueGrid.ActualWidth)) * 360);
        }

        private void UpdateMouseSatVal()
        {
            Point point = Mouse.GetPosition(ValueSaturationGrid);
            s = (int)(Math.Max(0, Math.Min(1, point.X / ValueSaturationGrid.ActualWidth)) * 100);
            v = (int)((1 - Math.Max(0, Math.Min(1, point.Y / ValueSaturationGrid.ActualHeight))) * 100);
        }

        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            selectedType = SelectedType.None;
            ((UIElement)sender).ReleaseMouseCapture();
        }

        private void SliderSatVal_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (selectedType != SelectedType.None) return;
            selectedType = SelectedType.SaturationValue;
            
            ((UIElement)sender).CaptureMouse();
        }

        private void SatValDisplay_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (selectedType != SelectedType.None) return;
            selectedType = SelectedType.SaturationValue;
            
            SliderSatVal.CaptureMouse();
            UpdateMouseSatVal();
        }        

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateDisplay();
        }

        private void SliderHue_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (selectedType != SelectedType.None) return;
            selectedType = SelectedType.Hue;
            
            ((UIElement)sender).CaptureMouse();
        }

        private void HueDisplay_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (selectedType != SelectedType.None) return;
            selectedType = SelectedType.Hue;

            SliderHue.CaptureMouse();
            UpdateMouseHue();
        }        

        private static readonly Regex RegexHex = new Regex(@"^(#|)[a-fA-F0-9]{0,6}$");
        private static readonly Regex RegexRGB = new Regex(@"^([0-9]{0,2}|[01][0-9]{1,2}|2(5[0-5]|[0-4][0-9]))$"); // nombre entre 0 et 255

        private bool IsTextAllowed(string text, bool isHex)
        {
            return (isHex ? RegexHex : RegexRGB).IsMatch(text);
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox input = (TextBox)sender;
            e.Handled = !IsTextAllowed(input.Text + e.Text, input.Name.Contains("Hex"));
        }

        private void TextBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                if (!IsTextAllowed(text, ((TextBox)sender).Name.Contains("Hex")))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private enum SelectedType
        {
            None, SaturationValue, Hue
        }

        private void InputRGB_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!listenToInputChanges) return;

            byte r = 0, g = 0, b = 0;

            byte.TryParse(InputR.Text, out r);
            byte.TryParse(InputG.Text, out g);
            byte.TryParse(InputB.Text, out b);

            LoadRGB(r, g, b);

            UpdateDisplay(false, true);
        }

        private void InputHex_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!listenToInputChanges) return;

            string input = InputHex.Text;
            if (input.StartsWith("#")) input = input.Substring(1);

            while (input.Length < 6)
                input += "0";

            byte R = byte.Parse(input.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            byte G = byte.Parse(input.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            byte B = byte.Parse(input.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);

            LoadRGB(R, G, B);

            UpdateDisplay(true, false);
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
    }
}
