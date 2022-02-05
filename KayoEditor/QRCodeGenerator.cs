using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace KayoEditor
{
    public static class QRCodeGenerator
    {
        private static readonly Regex extentedRegex = new Regex(@"^[a-zA-Z0-9$%*+\-./: ]{0,47}$");
        private static readonly Regex strictRegex = new Regex(@"^[A-Z0-9$%*+\-./: ]{0,47}$");

        public static Regex ExtendedRegex => extentedRegex;
        public static Regex StrictRegex => strictRegex;

        public static ImagePSI GenerateQRCode(string text)
        {
            throw new NotImplementedException("Cannot generate a QR code yet!");
        }
    }
}
