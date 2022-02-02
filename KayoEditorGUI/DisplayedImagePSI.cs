using KayoEditor;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace KayoEditorGUI
{
    class DisplayedImagePSI
    {
        Image screenImage;
        TextBlock screenDetails;

        public DisplayedImagePSI(Image screenImage, TextBlock screenDetails)
        {
            this.screenImage = screenImage;
            this.screenDetails = screenDetails;
        }

        public DisplayedImagePSI(Image screenImage) : this(screenImage, null) { }

        public void UpdateImage(ImagePSI image)
        {
            if (image != null)
            {
                WriteableBitmap writeableBitmap = new WriteableBitmap(image.Width, image.Height, 96, 96, PixelFormats.Bgr24, null);
                writeableBitmap.WritePixels(new Int32Rect(0, 0, image.Width, image.Height), image.RawPixels.ToArray(), image.Stride, 0, 0);

                screenImage.Source = writeableBitmap;
                
                if(screenDetails != null)
                {
                    screenDetails.Text = image.Width + "x" + image.Height + " pixels (" + image.Width * image.Height + " pixels)";
                }

                GC.Collect();
            }
        }
    }
}
