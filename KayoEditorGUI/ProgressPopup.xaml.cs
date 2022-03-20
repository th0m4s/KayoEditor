﻿using KayoEditor;
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
        private Func<ImagePSI> computeMethod;
        private bool canceled = false;

        private ProgressPopup(Func<ImagePSI> method, string message)
        {
            InitializeComponent();

            computeMethod = method;
            MessageContent.Text = message;
        }

        public static ImagePSI ComputeImage(Func<ImagePSI> method, string message = "Veuillez patienter...")
        {
            ProgressPopup popup = new ProgressPopup(method, message);
            return popup.Compute();
        }

        private ImagePSI Compute()
        {
            ImagePSI result = null;
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
                throw exception;

            return result;
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
}