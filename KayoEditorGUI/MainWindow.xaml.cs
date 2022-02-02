using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using KayoEditor;
using Microsoft.Win32;

namespace KayoEditorGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ImagePSI loadedImage = null;
        ImagePSI resultImage = null;

        public MainWindow()
        {
            InitializeComponent();

            MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
            ShowScreen_Welcome();
        }

        #region Topbar handlers

        Brush topBarHoverBrush = new SolidColorBrush(Color.FromRgb(40, 43, 46));

        private void TopBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            switch(e.ClickCount)
            {
                case 1:
                    DragMove();
                    break;
                case 2:
                    if(WindowState == WindowState.Normal)
                    {
                        WindowState = WindowState.Maximized;
                    } else
                    {
                        WindowState = WindowState.Normal;
                    }
                    break;
            }
        }

        private void TopButton_MouseEnter(object sender, MouseEventArgs e)
        {
            ((Grid)sender).Background = topBarHoverBrush;
        }

        private void TopButton_MouseLeave(object sender, MouseEventArgs e)
        {
            ((Grid)sender).Background = Brushes.Transparent;
        }

        private void CloseButton_MouseEnter(object sender, MouseEventArgs e)
        {
            ((Grid)sender).Background = Brushes.Red;
        }

        private void MinimizeButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            } else
            {
                WindowState = WindowState.Maximized;
            }
        }

        private void CloseButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                MainWindowBorder.BorderThickness = new Thickness(6);
            }
            else
            {
                MainWindowBorder.BorderThickness = new Thickness(0);
            }
        }

        #endregion

        #region Image load/save

        private void Contents_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0)
                {
                    string filename = files[0];
                    LoadImage(filename);
                }
            }
        }

        private void OpenLoadImage()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "Ouvrir un fichier bitmap 24-bit...";
            dialog.DefaultExt = ".bmp";
            dialog.Filter = "Image Bitmap|*.bmp";

            if (dialog.ShowDialog() == true)
                LoadImage(dialog.FileName);
        }

        private void LoadImage(string filename)
        {
            if(File.Exists(filename))
            {
                try
                {
                    ImagePSI loadingImage = new ImagePSI(filename);

                    if(loadedImage == null || MessagePopup.Show("Une image est déjà ouverte dans Kayo Editor !\nVoulez-vous charger cette nouvelle image ?", true, true) 
                        == MessagePopup.MessageResult.Continue)
                    {
                        loadedImage = loadingImage;
                        resultImage = loadedImage.Copy();

                        ShowScreen_Editor();

                        UpdateDisplayed_LoadedImage();
                        UpdateDisplayed_ResultImage();
                    }
                } catch(Exception e)
                {
                    MessagePopup.Show("Impossible de charger ce fichier : " + e.Message + " (" + e.GetType().Name + ")\n" + e.StackTrace);
                }
            } else
            {
                MessagePopup.Show("Ce fichier n'existe pas !");
            }
        }

        private void OpenSaveImage()
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Title = "Enregistrer au format bitmap 24-bit...";
            dialog.AddExtension = true;
            dialog.DefaultExt = ".bmp";
            dialog.Filter = "Image Bitmap|*.bmp";

            if (dialog.ShowDialog() == true)
                SaveImage(dialog.FileName);
        }

        private void SaveImage(string filename)
        {
            if(resultImage != null)
            {
                try
                {
                    resultImage.Save(filename);
                    MessagePopup.Show("Image sauvegardée !");
                }
                catch (Exception e)
                {
                    MessagePopup.Show("Impossible de sauvegarder l'image : " + e.Message + " (" + e.GetType().Name + ")\n" + e.StackTrace);
                }
            }
        }

        #endregion

        #region Welcome screen

        public void ShowScreen_Welcome()
        {
            Grid_WelcomeScreen.Visibility = Visibility.Visible;
            Grid_EditorScreen.Visibility = Visibility.Collapsed;
        }

        private void OpenImage_Clicked(object sender, RoutedEventArgs e)
        {
            OpenLoadImage();
        }

        #endregion

        #region Editor screen

        public void ShowScreen_Editor()
        {
            Grid_WelcomeScreen.Visibility = Visibility.Collapsed;
            Grid_EditorScreen.Visibility = Visibility.Visible;
        }

        public void UpdateDisplayed_LoadedImage()
        {
            if(loadedImage != null)
            {
                WriteableBitmap writeableBitmap = new WriteableBitmap(loadedImage.Width, loadedImage.Height, 96, 96, PixelFormats.Bgr24, null);
                writeableBitmap.WritePixels(new Int32Rect(0, 0, loadedImage.Width, loadedImage.Height), loadedImage.RawPixels.ToArray(), loadedImage.Stride, 0, 0);

                DisplayLoadedImage.Source = writeableBitmap;

                LoadedImageDetails.Text = loadedImage.Width + "x" + loadedImage.Height + " pixels (" + loadedImage.Width * loadedImage.Height + " pixels)";
            }
        }

        public void UpdateDisplayed_ResultImage()
        {
            if (resultImage != null)
            {
                WriteableBitmap writeableBitmap = new WriteableBitmap(resultImage.Width, resultImage.Height, 96, 96, PixelFormats.Bgr24, null);
                writeableBitmap.WritePixels(new Int32Rect(0, 0, resultImage.Width, resultImage.Height), resultImage.RawPixels.ToArray(), resultImage.Stride, 0, 0);

                DisplayResultImage.Source = writeableBitmap;

                ResultImageDetails.Text = resultImage.Width + "x" + resultImage.Height + " pixels (" + resultImage.Width * resultImage.Height + " pixels)";
            }
        }

        private void SaveImage_Click(object sender, RoutedEventArgs e)
        {
            OpenSaveImage();
        }

        #endregion

        private void TransformGreyscale_Click(object sender, RoutedEventArgs e)
        {
            resultImage = resultImage.Greyscale();
            UpdateDisplayed_ResultImage();
        }

        private void TransformBlackWhite_Click(object sender, RoutedEventArgs e)
        {
            resultImage = resultImage.BlackAndWhite();
            UpdateDisplayed_ResultImage();
        }

        private void TransformScale_Click(object sender, RoutedEventArgs e)
        {

        }

        private void TransformRotate_Click(object sender, RoutedEventArgs e)
        {

        }

        private void TransformFlip_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MoreTransforms_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BackMenu_Click(object sender, RoutedEventArgs e)
        {
            ShowScreen_Welcome();
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            resultImage = loadedImage.Copy();
            UpdateDisplayed_ResultImage();
        }
    }
}
