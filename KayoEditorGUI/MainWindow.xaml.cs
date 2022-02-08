using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
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
        DisplayedImagePSI resultImageDisplay = null;

        public MainWindow()
        {
            InitializeComponent();

            MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
            ShowScreen_Welcome();

            resultImageDisplay = new DisplayedImagePSI(ResultImage, ResultImageDetails);
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
                        LoadImage(loadingImage, Path.GetFileName(filename));
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

        private void LoadImage(ImagePSI image, string name = "Nouvelle image")
        {
            loadedImage = image;
            resultImage = loadedImage.Copy();

            ShowScreen_Editor();

            resultImageDisplay.UpdateImage(resultImage);
            OpenedImageName.Text = name;
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
            loadedImage = null;
            resultImage = null;

            ResultImage.Source = null;

            GC.Collect();

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

        private void SaveImage_Click(object sender, RoutedEventArgs e)
        {
            OpenSaveImage();
        }

        #endregion

        private void TransformGreyscale_Click(object sender, RoutedEventArgs e)
        {
            resultImage = resultImage.Greyscale();
            resultImageDisplay.UpdateImage(resultImage);
        }

        private void TransformBlackWhite_Click(object sender, RoutedEventArgs e)
        {
            resultImage = resultImage.BlackAndWhite();
            resultImageDisplay.UpdateImage(resultImage);
        }

        private void TransformScale_Click(object sender, RoutedEventArgs e)
        {
            QuestionPopup popup = new QuestionPopup("Facteur d'agrandissement :");

            float scale = popup.AskFloat(true);
            if(popup.Confirmed)
            {
                if(scale > 0)
                {
                    try
                    {
                        resultImage = resultImage.Scale(scale);
                        resultImageDisplay.UpdateImage(resultImage);
                    } catch(Exception exception)
                    {
                        MessagePopup.Show("Impossible d'agrandir l'image : " + exception.Message + " (" + exception.GetType().Name + ")\n" + exception.StackTrace);
                    }
                } else
                {
                    MessagePopup.Show("Le facteur d'agrandissement doit être un nombre non nul (>0) !");
                }
            }
        }

        private void TransformRotate_Click(object sender, RoutedEventArgs e)
        {
            QuestionPopup popup = new QuestionPopup("Angle (en degrés) :");

            int angle = popup.AskValue(new int[] { 90, 180, 270 }, new string[] { "90°", "180°", "270°" });
            if(popup.Confirmed)
            {
                MessagePopup.Show("angle " + angle);
                resultImageDisplay.UpdateImage(resultImage);
            }
        }

        private void TransformFlip_Click(object sender, RoutedEventArgs e)
        {
            QuestionPopup popup = new QuestionPopup("Direction de l'effet miroir :");

            FlipMode mode = popup.AskEnum<FlipMode>();
            if(popup.Confirmed)
            {
                try
                {
                    resultImage = resultImage.Flip(mode);
                    resultImageDisplay.UpdateImage(resultImage);
                } catch(Exception exception)
                {
                    MessagePopup.Show("Impossible de retourner l'image : " + exception.Message + " (" + exception.GetType().Name + ")\n" + exception.StackTrace);
                }
            }
        }

        private void BackMenu_Click(object sender, RoutedEventArgs e)
        {
            ShowScreen_Welcome();
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            resultImage = loadedImage.Copy();
            resultImageDisplay.UpdateImage(resultImage);
        }

        private void CreateQRC_Clicked(object sender, RoutedEventArgs e)
        {
            QuestionPopup popup = new QuestionPopup("Message (47 caractères alphanumériques max.) :");
            string message = popup.AskText(QRCodeGenerator.ExtendedRegex).ToUpper();

            if(popup.Confirmed)
            {
                try
                {
                    LoadImage(QRCodeGenerator.GenerateQRCode(message));
                } catch(Exception exception)
                {
                    MessagePopup.Show("Impossible de générer le QR code : " + exception.Message + " (" + exception.GetType().Name + ")\n" + exception.StackTrace);
                }
            }
        }

        private void TransformNegative_Click(object sender, RoutedEventArgs e)
        {
            resultImage = resultImage.Negative();
            resultImageDisplay.UpdateImage(resultImage);
        }

        private void TransformInvert_Click(object sender, RoutedEventArgs e)
        {

        }

        private void TransformKernel_Click(object sender, RoutedEventArgs e)
        {

        }

        private void TransformReadQR_Click(object sender, RoutedEventArgs e)
        {

        }

        private void TransformHisto_Click(object sender, RoutedEventArgs e)
        {

        }

        private void TransformEncode_Click(object sender, RoutedEventArgs e)
        {

        }

        private void TransformDecode_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
