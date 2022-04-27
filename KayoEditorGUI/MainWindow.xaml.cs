using System;
using System.ComponentModel;
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

        Pixel currentPaintColor = new Pixel(255, 0, 0);

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
            DragDropView.Visibility = Visibility.Collapsed;
            Grid_WelcomeScreen.IsHitTestVisible = true;

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

        private OpenFileDialog GetOpenDialog()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "Ouvrir un fichier bitmap 24 bits...";
            dialog.DefaultExt = ".bmp";
            dialog.Filter = "Image Bitmap|*.bmp";

            return dialog;
        }

        private void OpenLoadImage()
        {
            OpenFileDialog dialog = GetOpenDialog();

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
            try
            {
                resultImage = ProgressPopup.Compute(() => resultImage.Greyscale(), "Transformation en nuances de gris...");
                resultImageDisplay.UpdateImage(resultImage);
            } catch(Exception exception)
            {
                ShowException(exception, "Impossible de transformer l'image en nuances de gris");
            }
        }

        private void TransformBlackWhite_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                resultImage = ProgressPopup.Compute(() => resultImage.BlackAndWhite(), "Mise en noir et blanc...");
                resultImageDisplay.UpdateImage(resultImage);
            } catch(Exception exception)
            {
                ShowException(exception, "Impossible de transformer l'image en noir et blanc");
            }
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
                        resultImage = ProgressPopup.Compute(() => resultImage.Scale(scale), (scale < 1 ? "Rétrécissement" : "Agrandissement") + " de l'image...");
                        resultImageDisplay.UpdateImage(resultImage);
                    } catch(Exception exception)
                    {
                        ShowException(exception, "Impossible d'agrandir l'image");
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

            int angle = popup.AskInt() % 360;
            while(angle < 0) angle += 360; // modulo doesn't work with negative numbers

            if(popup.Confirmed)
            {
                try
                {
                    resultImage = ProgressPopup.Compute(() => resultImage.Rotate(angle), "Rotation de l'image...");
                    resultImageDisplay.UpdateImage(resultImage);
                }
                catch (Exception exception)
                {
                    ShowException(exception, "Impossible de tourner l'image");
                }
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
                    resultImage = ProgressPopup.Compute(() => resultImage.Flip(mode), "Application de l'effet miroir...");
                    resultImageDisplay.UpdateImage(resultImage);
                } catch(Exception exception)
                {
                    ShowException(exception, "Impossible de retourner l'image");
                }
            }
        }

        private void BackMenu_Click(object sender, RoutedEventArgs e)
        {
            ShowScreen_Welcome();
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                resultImage = ProgressPopup.Compute(() => loadedImage.Copy(), "Réinitialisation de l'image...");
                resultImageDisplay.UpdateImage(resultImage);
            } catch(Exception exception)
            {
                ShowException(exception, "Impossible de charger la photo originale");
            }
        }

        private void CreateQRC_Clicked(object sender, RoutedEventArgs e)
        {
            QuestionPopup popup = new QuestionPopup("Message (47 caractères alphanumériques max.) :");
            string message = popup.AskText(QRCode.ExtendedRegex).ToUpper();

            if(popup.Confirmed)
            {
                try
                {
                    LoadImage(ProgressPopup.Compute(() => QRCode.GenerateQRCode(message).Scale(10), "Création du QR code..."));
                } catch(Exception exception)
                {
                    ShowException(exception, "Impossible de générer le QR code");
                }
            }
        }

        private void CreateFractal_Clicked(object sender, RoutedEventArgs e)
        {
            QuestionPopup popup = new QuestionPopup("Formule du complexe c :");
            Complex c = popup.AskValue(new Complex[] { new Complex(0.3, 0.5), new Complex(0.285, 0.01), new Complex(-1.417022285618, 0.0099534), new Complex(-0.038088, 0.9754633), new Complex(0.285, 0.013), new Complex(0.285, 0.01), new Complex(-1.476,0), new Complex(-0.4, 0.6), new Complex(-0.8, 0.156) });

            if(popup.Confirmed)
            {
                try
                {
                    LoadImage(ProgressPopup.Compute(() => FractalGenerator.GenerateFractal(1280, 720, c), "Génération de la fractale..."));
                } catch(Exception exception)
                {
                    ShowException(exception, "Impossible de générer la fractale");
                }
            }
        }

        private void TransformNegative_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                resultImage = ProgressPopup.Compute(() => resultImage.Negative(), "Transformation en négatif...");
                resultImageDisplay.UpdateImage(resultImage);
            } catch(Exception exception)
            {
                ShowException(exception, "Impossible d'obtenir le négatif de l'image");
            }
        }

        private void TransformInvert_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                resultImage = ProgressPopup.Compute(() => resultImage.Invert(), "Inversion des intensités...");
                resultImageDisplay.UpdateImage(resultImage);
            } catch(Exception exception)
            {
                ShowException(exception, "Impossible d'inverser les intensités");
            }
        }

        private void TransformKernel_Click(object sender, RoutedEventArgs e)
        {
            QuestionPopup popup = new QuestionPopup("Filtre à appliquer :");

            Convolution.Kernel kernel = popup.AskEnum<Convolution.Kernel>();
            if(popup.Confirmed)
            {
                Convolution.KernelOrigin kernelOrigin = Convolution.KernelOrigin.Center;
                Convolution.EdgeProcessing edgeProcessing = Convolution.EdgeProcessing.Extend;

                try
                {
                    resultImage = ProgressPopup.Compute(() => resultImage.ApplyKernel(kernel, kernelOrigin, edgeProcessing), "Application du filtre...");
                    resultImageDisplay.UpdateImage(resultImage);
                } catch(Exception exception)
                {
                    ShowException(exception, "Impossible d'appliquer le filtre");
                }
            }
        }

        private void TransformReadQR_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessagePopup.Show("Message : " + ProgressPopup.Compute(() => QRCode.ReadQRCode(resultImage), "Lecture du QR code..."));
            }
            catch (Exception exception)
            {
                ShowException(exception, "Impossible de lire le QR code");
            }
        }

        private void TransformHisto_Click(object sender, RoutedEventArgs e)
        {
            QuestionPopup popup = new QuestionPopup("Quelle(s) couleur(s) utiliser pour l'histogramme ?");

            HistogramChannel channel = popup.AskEnum<HistogramChannel>(0);
            if(popup.Confirmed)
            {
                try
                {
                    bool r = channel == HistogramChannel.Red || channel == HistogramChannel.All;
                    bool g = channel == HistogramChannel.Green || channel == HistogramChannel.All;
                    bool b = channel == HistogramChannel.Blue || channel == HistogramChannel.All;

                    resultImage = ProgressPopup.Compute(() => resultImage.Histogram(r, g, b), "Création de l'histogramme...");
                    resultImageDisplay.UpdateImage(resultImage);
                } catch(Exception exception)
                {
                    ShowException(exception, "Impossible de calculer l'histogramme");
                }
            }
        }

        private void TransformEncode_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = GetOpenDialog();
            dialog.Title = "Choisir un fichier bitmap 24 bits à cacher...";

            if(dialog.ShowDialog() == true)
            {
                try
                {
                    ImagePSI imageToHide = new ImagePSI(dialog.FileName);
                    resultImage = ProgressPopup.Compute(() => resultImage.HideImageInside(imageToHide));
                    resultImageDisplay.UpdateImage(resultImage);
                } catch(Exception exception)
                {
                    ShowException(exception, "Impossible de cacher l'image");
                }
            }
        }

        private void TransformDecode_Click(object sender, RoutedEventArgs e)
        {
            resultImage = ProgressPopup.Compute(() => resultImage.GetHiddenImage(), "Décodage de l'image...");
            resultImageDisplay.UpdateImage(resultImage);
        }

        private void PaintColorPreview_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ColorPicker picker = new ColorPicker(currentPaintColor);
            Pixel pixel = picker.AskColor();

            if(picker.Confirmed)
            {
                currentPaintColor = pixel;
                PaintColorPreview.Fill = new SolidColorBrush(Color.FromRgb(pixel.R, pixel.G, pixel.B));
            }
        }

        private void ShowException(Exception exception, string message = "Une erreur est survenue", bool ignoreCanceled = true)
        {
            Type type = exception.GetType();
            if(!ignoreCanceled || type != typeof(OperationCanceledException))
            {
                if(type == typeof(ImageComputeException))
                    exception = exception.InnerException;
                
                MessagePopup.Show(message + " : " + exception.Message + " (" + exception.GetType().Name + ")\n" + exception.StackTrace);
            }
        }

        private void Contents_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0)
                {
                    DragDropView.Visibility = Visibility.Visible;
                    Grid_WelcomeScreen.IsHitTestVisible = false;
                }
            }
        }

        private void Contents_DragLeave(object sender, DragEventArgs e)
        {
            DragDropView.Visibility = Visibility.Collapsed;
            Grid_WelcomeScreen.IsHitTestVisible = true;
        }

        private void TransformFilter_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ImagePSI Filtre = new ImagePSI("Filtre_coeurs.bmp"); // Filtre_TEST
                resultImage = ProgressPopup.Compute(() => resultImage.Innovation(Filtre), "Appliquation du filtre...");
                resultImageDisplay.UpdateImage(resultImage);
            }
            catch (Exception exception)
            {
                ShowException(exception, "Impossible d'appliquer le filtre sur l'image");
            }
        } 
    }

    public enum HistogramChannel
    {
        [Description("RGB (toutes)")]
        All,

        [Description("Rouge")]
        Red,

        [Description("Vert")]
        Green,

        [Description("Bleu")]
        Blue,
    }
}
