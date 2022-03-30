using KayoEditor;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace KayoEditorGUI
{
    /// <summary>
    /// Interaction logic for ProgressPopup.xaml
    /// </summary>
    public partial class ProgressPopup : Window
    {
        private Thread computeThread = null;
        private Func<object> computeMethod;
        private bool canceled = false;

        private ProgressPopup(Func<object> method, string message)
        {
            InitializeComponent();

            computeMethod = method;
            MessageContent.Text = message;
        }

        public static T Compute<T>(Func<T> method, string message = "Veuillez patienter...") where T : class
        {
            ProgressPopup popup = new ProgressPopup(method, message);
            return popup.Compute<T>();
        }

        private T Compute<T>()
        {
            object result = null;
            Exception exception = null;

            computeThread = new Thread(() =>
            {
                try
                {
                    result = computeMethod.Invoke();
                } catch (Exception e)
                {
                    exception = e;
                } finally
                {
                    Close();
                }
            });

            computeThread.Start();
            ShowDialog();

            computeThread = null;
            GC.Collect();

            if (canceled)
                throw new OperationCanceledException();

            if (exception != null)
                throw new ImageComputeException(exception);

            return (T)result;
        }
        
        private new void Close()
        {
            Dispatcher.Invoke(() => base.Close());
        }

        private void TopBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: real progress class, with progress value and cancelation like cancelationtoken
            canceled = true;

            try
            {
#pragma warning disable SYSLIB0006
                computeThread.Abort();
#pragma warning restore SYSLIB0006
            } catch (Exception) {

            } finally
            {
                Close();
            }
        }
    }

    public class ImageComputeException : Exception
    {
        public ImageComputeException(Exception inner) : base("", inner) { }
    }
}
