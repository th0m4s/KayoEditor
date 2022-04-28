using KayoEditor;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace KayoEditorTests
{
    [TestClass]
    public class UnitTest
    {
        private static Pixel RED => new Pixel(255, 0, 0);
        private static Pixel GREEN => new Pixel(0, 255, 0);
        private static Pixel BLUE => new Pixel(0, 0, 255);

        private static Random random = new Random();

        [TestMethod]
        public void TestConstructor()
        {
            int size = random.Next(1, 10);
            ImagePSI image = new ImagePSI(size, size * 2);

            Assert.AreEqual(size, image.Width);
            Assert.AreEqual(size * 2, image.Height);

            Assert.AreEqual("BM", image.Type);
            Assert.AreEqual(24, image.ColorDepth);
        }

        [TestMethod]
        public void TestPixels()
        {
            ImagePSI image = new ImagePSI(2, 2);
            image[1, 1] = RED;

            Assert.IsTrue(image[1, 1] == RED);
            Assert.IsFalse(image[0, 0] == RED);
        }

        [TestMethod]
        public void TestGreyscale()
        {
            ImagePSI image = new ImagePSI(2, 2);

            image[0, 0] = new Pixel(0, 0, 0);
            image[0, 1] = new Pixel(255, 255, 255);

            image[1, 0] = new Pixel(6, 44, 148);
            image[1, 1] = new Pixel(36, 50, 92);

            ImagePSI gs = image.Greyscale();

            Assert.IsTrue(gs[0, 0] == new Pixel(0));
            Assert.IsTrue(gs[0, 1] == new Pixel(255));
            Assert.IsTrue(gs[1, 0] == new Pixel(66));
            Assert.IsTrue(gs[1, 1] == new Pixel(59));
        }

        [TestMethod]
        public void TestBlackAndWhite()
        {
            ImagePSI image = new ImagePSI(2, 2);

            image[0, 0] = new Pixel(0, 0, 0);
            image[0, 1] = new Pixel(255, 255, 255);

            image[1, 0] = new Pixel(6, 44, 148);
            image[1, 1] = new Pixel(153, 100, 249);

            ImagePSI bw = image.BlackAndWhite();

            Assert.IsTrue(bw[0, 0] == new Pixel(0));
            Assert.IsTrue(bw[0, 1] == new Pixel(255));
            Assert.IsTrue(bw[1, 0] == new Pixel(0));
            Assert.IsTrue(bw[1, 1] == new Pixel(255));
        }

        [TestMethod]
        public void TestNegative()
        {
            ImagePSI image = new ImagePSI(2, 2);

            image[0, 0] = new Pixel(0, 0, 0);
            image[0, 1] = new Pixel(255, 255, 255);
            image[1, 0] = new Pixel(6, 44, 148);
            image[1, 1] = new Pixel(36, 50, 92);

            ImagePSI neg = image.Negative();

            Assert.IsFalse(neg == image);
            Assert.IsTrue(neg.Negative() == image);
        }

        [TestMethod]
        public void TestInvert()
        {
            ImagePSI image = new ImagePSI(2, 2);

            image[0, 0] = new Pixel(0, 0, 0);
            image[0, 1] = new Pixel(255, 255, 255);
            image[1, 0] = new Pixel(6, 44, 148);
            image[1, 1] = new Pixel(36, 50, 92);

            ImagePSI inverted = image.Invert();

            Assert.IsFalse(inverted == image);
            Assert.IsTrue(inverted.Invert() == image);
        }

        [TestMethod]
        public void TestFlip()
        {
            ImagePSI image = new ImagePSI(2, 2);

            image[0, 0] = new Pixel(0, 0, 0);
            image[0, 1] = new Pixel(255, 255, 255);
            image[1, 0] = new Pixel(6, 44, 148);
            image[1, 1] = new Pixel(36, 50, 92);

            ImagePSI flipX = image.Flip(FlipMode.FlipX);
            ImagePSI flipY = image.Flip(FlipMode.FlipY);              

            Assert.IsFalse(flipX == image);
            Assert.IsFalse(flipY == image);

            Assert.IsTrue(flipX.Flip(FlipMode.FlipX) == image);
            Assert.IsTrue(flipY.Flip(FlipMode.FlipY) == image);

            ImagePSI both = image.Flip(FlipMode.FlipBoth);

            Assert.IsFalse(both == image);
            Assert.IsTrue(both.Flip(FlipMode.FlipBoth) == image);
        }

        [TestMethod]
        public void TestQRCode()
        {
            string v1 = "HELLO WORLD";
            string v2 = "ESILV POLE LEONARD DE VINCI $%*+-./:";

            Assert.AreEqual(v1, QRCode.ReadQRCode(QRCode.GenerateQRCode(v1)));
            Assert.AreEqual(v2, QRCode.ReadQRCode(QRCode.GenerateQRCode(v2)));
        }
    }
}
