using System.Windows;
using System.Windows.Input;

namespace KayoEditorGUI
{
    /// <summary>
    /// Interaction logic for MessagePopup.xaml
    /// </summary>
    public partial class MessagePopup : Window
    {
        MessageResult lastResult = MessageResult.Continue;

        public MessagePopup(string message, bool canContinue = true, bool canCancel = false, bool canRetry = false)
        {
            InitializeComponent();

            MessageContent.Text = message;

            int count = 0;

            if(canCancel)
            {
                ButtonCancel.Visibility = Visibility.Visible;
                count++;
            }

            if(canRetry)
            {
                ButtonRetry.Visibility = Visibility.Visible;
                count++;
            }

            if(canContinue || count == 0)
            {
                ButtonContinue.Visibility = Visibility.Visible;
                count++;
            }

            ButtonsGrid.Columns = count;
        }

        public MessageResult ShowMessage()
        {
            ShowDialog();
            return lastResult;
        }

        public static MessageResult Show(string message, bool canContinue = true, bool canCancel = false, bool canRetry = false)
        {
            return new MessagePopup(message, canContinue, canCancel, canRetry).ShowMessage();
        }

        private void TopBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        public enum MessageResult
        {
            Continue, Cancel, Retry
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            lastResult = MessageResult.Cancel;
            Close();
        }

        private void ButtonContinue_Click(object sender, RoutedEventArgs e)
        {
            lastResult = MessageResult.Continue;
            Close();
        }

        private void ButtonRetry_Click(object sender, RoutedEventArgs e)
        {
            lastResult = MessageResult.Retry;
            Close();
        }
    }
}
