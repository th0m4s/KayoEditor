using System;

namespace KayoEditor
{
    class Program
    {
        static void Main(string[] args)
        {
            new ImagePSI("coco.bmp").Flip(FlipMode.FlipBoth).Save("output.bmp");
        }
    }
}
