using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace KayoEditor
{
    public static class Convolution
    {
        private static Dictionary<Kernel, float[,]> kernels = new Dictionary<Kernel, float[,]>
        {
            { 
                Kernel.EdgeDetection1, new float[,]
                {
                    {  1,  0, -1 },
                    {  0,  0,  0 },
                    { -1, 0 ,  1 }
                }
            },
            {
                Kernel.EdgeDetection2, new float[,]
                {
                    {  0,  1,  0 },
                    {  1, -4,  1 },
                    {  0,  1,  0 }
                }
            },
            {
                Kernel.EdgeDetection3, new float[,]
                {
                     { -1, -1, -1 },
                     { -1,  8, -1 },
                     { -1, -1, -1 }
                }
            },
            {
                Kernel.Sharpen, new float[,]
                {
                    {  0, -1,  0 },
                    { -1,  5, -1 },
                    {  0, -1,  0 }
                }
            },
            {
                Kernel.BoxBlur, new float[,]
                {
                    { 1/9f, 1/9f, 1/9f },
                    { 1/9f, 1/9f, 1/9f },
                    { 1/9f, 1/9f, 1/9f }
                }
            },
            {
                Kernel.GaussianBlur3x3, new float[,]
                {
                    { 1/16f, 2/16f, 1/16f },
                    { 2/16f, 4/16f, 2/16f },
                    { 1/16f, 2/16f, 1/16f }
                }
            },
            {
                Kernel.GaussianBlur5x5, new float[,]
                {
                    {  1/256f,  4/256f,  6/256f,  4/256f, 1/256f },
                    {  4/256f, 16/256f, 24/256f, 16/256f, 4/256f },
                    {  6/256f, 24/256f, 36/256f, 24/256f, 6/256f },
                    {  4/256f, 16/256f, 24/256f, 16/256f, 4/256f },
                    {  1/256f,  4/256f,  6/256f,  4/256f, 1/256f }
                }
            }
        };

        public static ImagePSI ApplyKernel(this ImagePSI image, Kernel kernel, KernelOrigin origin = KernelOrigin.Center, EdgeProcessing edgeProcessing = EdgeProcessing.KernelCrop)
        {
            return ApplyKernel(image, kernels[kernel], origin, edgeProcessing);
        }

        public static ImagePSI ApplyKernel(this ImagePSI image, float[,] kernel, KernelOrigin origin = KernelOrigin.Center, EdgeProcessing edgeProcessing = EdgeProcessing.KernelCrop)
        {
            if (kernel == null)
                throw new ArgumentException("kernel", "kernel cannot be null!");

            int kernelH = kernel.GetLength(0);
            int kernelW = kernel.GetLength(1);

            int imageHeight = image.Height;
            int imageWidth = image.Width;

            if (origin == KernelOrigin.Center && (kernelH % 2 != 1 || kernelW % 2 != 1))
            {
                origin = KernelOrigin.TopLeft;
            }

            int halfKerH = kernelH / 2;
            int halfKerW = kernelW / 2;

            int resultH = imageHeight;
            int resultW = imageWidth;
            if (edgeProcessing == EdgeProcessing.Crop)
            {
                resultH -= kernelH - 1;
                resultW -= kernelW - 1;
            }

            if (resultH <= 0 || resultW <= 0)
                throw new ArithmeticException("result of negative size!");

            ImagePSI result = new ImagePSI(resultW, resultH);

            for (int y = 0; y < resultH; y++)
            {
                for (int x = 0; x < resultW; x++)
                {
                    int acc_r = 0;
                    int acc_g = 0;
                    int acc_b = 0;

                    for (int dy = 0; dy < kernelH; dy++)
                    {
                        for (int dx = 0; dx < kernelW; dx++)
                        {
                            int pos_dy = y + dy, pos_dx = x + dx;
                            if (origin == KernelOrigin.Center && edgeProcessing != EdgeProcessing.Crop)
                            {
                                pos_dy -= halfKerH;
                                pos_dx -= halfKerW;
                            }

                            if (pos_dy < 0 || pos_dy >= imageHeight)
                            {
                                switch (edgeProcessing)
                                {
                                    // no need for crop
                                    case EdgeProcessing.Extend:
                                        if (pos_dy < 0)
                                        {
                                            pos_dy = 0;
                                        }
                                        else
                                        {
                                            pos_dy = imageHeight - 1;
                                        }
                                        break;
                                    case EdgeProcessing.KernelCrop:
                                        continue; // skip this kernel position
                                    case EdgeProcessing.Mirror:
                                        if (pos_dy < 0)
                                        {
                                            pos_dy = -pos_dy + 1;
                                        }
                                        else
                                        {
                                            pos_dy = imageHeight - (pos_dy % imageHeight);
                                        }
                                        break;
                                    case EdgeProcessing.Wrap:
                                        while (pos_dy < 0)
                                            pos_dy += imageHeight;
                                        pos_dy %= imageHeight;
                                        break;
                                }
                            }

                            if (pos_dx < 0 || pos_dx >= imageWidth)
                            {
                                switch (edgeProcessing)
                                {
                                    // no need for crop
                                    case EdgeProcessing.Extend:
                                        if (pos_dx < 0)
                                        {
                                            pos_dx = 0;
                                        }
                                        else
                                        {
                                            pos_dx = imageWidth - 1;
                                        }
                                        break;
                                    case EdgeProcessing.KernelCrop:
                                        continue; // skip this kernel position
                                    case EdgeProcessing.Mirror:
                                        if (pos_dx < 0)
                                        {
                                            pos_dx = -pos_dx + 1;
                                        }
                                        else
                                        {
                                            pos_dx = imageWidth - (pos_dx % imageWidth);
                                        }
                                        break;
                                    case EdgeProcessing.Wrap:
                                        while (pos_dx < 0)
                                            pos_dx += imageWidth;
                                        pos_dx %= imageWidth;
                                        break;
                                }
                            }

                            float factor = kernel[dy, dx];
                            Pixel pixel = image[pos_dx, pos_dy];

                            acc_r += (int)(factor * pixel.R);
                            acc_g += (int)(factor * pixel.G);
                            acc_b += (int)(factor * pixel.B);
                        }
                    }

                    acc_r = Math.Min(255, Math.Max(0, acc_r));
                    acc_g = Math.Min(255, Math.Max(0, acc_g));
                    acc_b = Math.Min(255, Math.Max(0, acc_b));

                    result[x, y] = new Pixel((byte)acc_r, (byte)acc_g, (byte)acc_b);
                }
            }

            return result;
        }

        public enum EdgeProcessing
        {
            Extend, Wrap, Mirror, Crop, KernelCrop
        }

        public enum KernelOrigin
        {
            Center, TopLeft
        }

        public enum Kernel
        {
            [Description("Détection des contours 1")]
            EdgeDetection1,

            [Description("Détection des contours 2")]
            EdgeDetection2,

            [Description("Détection des contours 3")]
            EdgeDetection3,
            
            [Description("Amélioration de la netteté")]
            Sharpen,
            
            [Description("Flou simple")]
            BoxBlur,
            
            [Description("Flou de Gauss 3x3")]
            GaussianBlur3x3,
            
            [Description("Flou de Gauss 5x5")]
            GaussianBlur5x5
        }
    }
}
